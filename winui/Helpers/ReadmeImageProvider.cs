using MarkWin2D;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.Svg;
using Microsoft.UI;
using Microsoft.UI.Text;
using StarSorter.Services;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI;

namespace StarSorter.Helpers;

public partial class ReadmeImageProvider : IImageProvider
{
    private readonly ImageCacheService _cacheService;
    private readonly HttpClient _httpClient;
    private readonly string? _repositoryFullName;
    private readonly Dictionary<string, CanvasBitmap> _bitmapCache = new();
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public event EventHandler? ImagesInvalidated;

    public ReadmeImageProvider(ImageCacheService cacheService, string? repositoryFullName = null)
    {
        _cacheService = cacheService;
        _repositoryFullName = repositoryFullName;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHub-Stars-Manager");
        _httpClient.DefaultRequestHeaders.Add("Accept", "image/*,*/*");
    }

    public CanvasBitmap? GetImage(string url, CanvasDevice device)
    {
        var resolvedUrl = ResolveUrl(url);

        if (string.IsNullOrEmpty(resolvedUrl))
            return null;

        if (_bitmapCache.TryGetValue(resolvedUrl, out var bmp))
            return bmp;

        _ = LoadImageAsync(resolvedUrl, device);
        return null;
    }

    private string ResolveUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return url;

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return url;

        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var blobMatch = GitHubBlobRegex().Match(url);
            if (blobMatch.Success)
                return $"https://raw.githubusercontent.com/{blobMatch.Groups[1].Value}/{blobMatch.Groups[2].Value}";

            return url;
        }

        if (url.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            return "https:" + url;

        if (!string.IsNullOrEmpty(_repositoryFullName))
        {
            var relativePath = url.TrimStart('/');
            return $"https://raw.githubusercontent.com/{_repositoryFullName}/HEAD/{relativePath}";
        }

        return url;
    }

    [GeneratedRegex(@"^https://github\.com/([^/]+/[^/]+)/blob/(.+)$")]
    private static partial Regex GitHubBlobRegex();

    private async Task LoadImageAsync(string url, CanvasDevice device)
    {
        await _loadLock.WaitAsync();
        try
        {
            if (_bitmapCache.ContainsKey(url))
                return;

            CanvasBitmap? bmp = null;

            try
            {
                var cachedPath = await _cacheService.GetCachedImagePathAsync(url);
                var fromCache = !string.IsNullOrEmpty(cachedPath) &&
                                !cachedPath.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase) &&
                                File.Exists(cachedPath);

                byte[] imageBytes;
                if (fromCache)
                    imageBytes = await File.ReadAllBytesAsync(cachedPath);
                else
                    imageBytes = await _httpClient.GetByteArrayAsync(url);

                if (IsSvgContent(imageBytes))
                {
                    var svgXml = Encoding.UTF8.GetString(imageBytes);
                    ParseSvgSize(svgXml, out var svgW, out var svgH);

                    svgXml = Regex.Replace(svgXml,
                        @"font-family\s*=\s*""[^""]*DejaVu Sans[^""]*""",
                        "font-family='Segoe UI'",
                        RegexOptions.IgnoreCase);

                    var svgBytes = Encoding.UTF8.GetBytes(svgXml);
                    using var ms = new MemoryStream(svgBytes);
                    using var svgDoc = await CanvasSvgDocument.LoadAsync(device, ms.AsRandomAccessStream());
                    var rt = new CanvasRenderTarget(device, svgW, svgH, 96);

                    using (var ds = rt.CreateDrawingSession())
                    {
                        ds.Clear(Colors.Transparent);
                        ds.DrawSvg(svgDoc, new Size(svgW, svgH));
                    }

                    using (var ds = rt.CreateDrawingSession())
                    {
                        RenderSvgText(ds, svgXml);
                    }

                    bmp = rt;
                }
                else
                {
                    using var ms = new MemoryStream(imageBytes);
                    bmp = await CanvasBitmap.LoadAsync(device, ms.AsRandomAccessStream());
                }
            }
            catch
            {
                return;
            }

            if (bmp != null)
            {
                _bitmapCache[url] = bmp;
                ImagesInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private static bool IsSvgContent(byte[] bytes)
    {
        if (bytes.Length < 5) return false;
        var header = Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 200)).AsSpan().TrimStart();
        return header.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
               header.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) ||
               header.StartsWith("<!DOCTYPE svg", StringComparison.OrdinalIgnoreCase);
    }

    private static void ParseSvgSize(string svgXml, out float width, out float height)
    {
        width = 800f;
        height = 600f;

        var vbMatch = Regex.Match(svgXml, @"viewBox\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);
        if (vbMatch.Success)
        {
            var parts = vbMatch.Groups[1].Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4 &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var vw) &&
                float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var vh))
            {
                width = vw;
                height = vh;
                return;
            }
        }

        var wMatch = Regex.Match(svgXml, @"\bwidth\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);
        var hMatch = Regex.Match(svgXml, @"\bheight\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);
        if (wMatch.Success)
            TryParseSvgLength(wMatch.Groups[1].Value, out width);
        if (hMatch.Success)
            TryParseSvgLength(hMatch.Groups[1].Value, out height);
    }

    private static void TryParseSvgLength(string raw, out float value)
    {
        value = 0;
        var trimmed = raw.Trim();
        var unitIdx = trimmed.IndexOfAny(['p', 'e', 'm', '%', 'c', 'i', 'v', 'x']);
        if (unitIdx > 0)
            trimmed = trimmed[..unitIdx].Trim();
        float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void RenderSvgText(CanvasDrawingSession ds, string svgXml)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(svgXml);
        }
        catch
        {
            return;
        }

        var ns = doc.Root?.Attribute("xmlns")?.Value ?? "";
        var textEls = doc.Descendants(XName.Get("text", ns)).ToList();
        if (textEls.Count == 0) return;

        var originalTransform = ds.Transform;

        foreach (var textEl in textEls)
        {
            var firstTspan = textEl.Descendants(XName.Get("tspan", ns)).FirstOrDefault();
            var ariaHidden = textEl.Attribute("aria-hidden")?.Value
                             ?? firstTspan?.Attribute("aria-hidden")?.Value;
            if (ariaHidden == "true") continue;

            var xStr = firstTspan?.Attribute("x")?.Value ?? textEl.Attribute("x")?.Value ?? "0";
            var yStr = firstTspan?.Attribute("y")?.Value ?? textEl.Attribute("y")?.Value ?? "0";
            if (!float.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) x = 0f;
            if (!float.TryParse(yStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) y = 0f;

            var fontSizeStr = GetInheritedAttrib(textEl, "font-size") ?? "14";
            fontSizeStr = Regex.Match(fontSizeStr, @"^([\d.]+)").Groups[1].Value;
            float.TryParse(fontSizeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var fontSize);
            if (fontSize <= 0) fontSize = 14f;

            var dyStr = firstTspan?.Attribute("dy")?.Value ?? textEl.Attribute("dy")?.Value;
            float dy = 0f;
            if (!string.IsNullOrEmpty(dyStr))
            {
                dyStr = dyStr.Trim();
                if (dyStr.EndsWith("em", StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(dyStr[..^2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture,
                            out var dyEm))
                    {
                        dy = dyEm * fontSize;
                    }
                }
                else
                {
                    TryParseSvgLength(dyStr, out dy);
                }
            }

            var fontFamily = CleanFontFamily(GetInheritedAttrib(textEl, "font-family") ?? "Segoe UI");
            var fontWeight = GetInheritedAttrib(textEl, "font-weight") ?? "normal";
            var fillStr = GetInheritedAttrib(textEl, "fill") ?? "#000";
            var fillOpacityStr = GetInheritedAttrib(textEl, "fill-opacity") ?? "1";

            var textAnchor = firstTspan?.Attribute("text-anchor")?.Value
                             ?? GetInheritedAttrib(textEl, "text-anchor")
                             ?? "start";

            var dominantBaseline = firstTspan?.Attribute("dominant-baseline")?.Value
                                   ?? GetInheritedAttrib(textEl, "dominant-baseline")
                                   ?? "alphabetic";

            var textContent = string.Concat(textEl.DescendantNodes().OfType<XText>().Select(n => n.Value)).Trim();
            if (string.IsNullOrEmpty(textContent)) continue;

            using var fmt = new CanvasTextFormat
            {
                FontFamily = fontFamily,
                FontSize = fontSize,
                FontWeight = fontWeight.Equals("bold", StringComparison.OrdinalIgnoreCase)
                    ? FontWeights.Bold
                    : FontWeights.Normal,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                WordWrapping = CanvasWordWrapping.NoWrap
            };

            using var layout = new CanvasTextLayout(ds, textContent, fmt, 0f, 0f);

            var textWidth = (float)layout.LayoutBounds.Width;
            float drawX = textAnchor switch
            {
                "middle" => x - textWidth / 2f,
                "end" => x - textWidth,
                _ => x,
            };

            float drawY;
            if (dominantBaseline.Equals("central", StringComparison.OrdinalIgnoreCase) ||
                dominantBaseline.Equals("middle", StringComparison.OrdinalIgnoreCase))
            {
                drawY = (y + dy) - (float)layout.LayoutBounds.Height / 2f;
            }
            else if (dominantBaseline.Equals("hanging", StringComparison.OrdinalIgnoreCase) ||
                     dominantBaseline.Equals("text-before-edge", StringComparison.OrdinalIgnoreCase))
            {
                drawY = y + dy;
            }
            else
            {
                var lineMetrics = layout.LineMetrics;
                float baselineOffset = (lineMetrics != null && lineMetrics.Length > 0)
                    ? (float)lineMetrics[0].Baseline
                    : fontSize * 0.8f;

                drawY = (y + dy) - baselineOffset;
            }

            var combinedMatrix = GetElementTransform(textEl);
            ds.Transform = combinedMatrix * originalTransform;

            var color = ParseSvgColor(fillStr);
            if (float.TryParse(fillOpacityStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var opacity) &&
                opacity < 1f)
                color = Color.FromArgb((byte)(color.A * opacity), color.R, color.G, color.B);

            ds.DrawTextLayout(layout, drawX, drawY, color);

            ds.Transform = originalTransform;
        }
    }

    private static string? GetInheritedAttrib(XElement el, string name)
    {
        var current = el;
        while (current != null)
        {
            var attr = current.Attribute(name);
            if (attr != null) return attr.Value;
            current = current.Parent;
        }

        return null;
    }

    private static Matrix3x2 GetElementTransform(XElement el)
    {
        var matrix = Matrix3x2.Identity;
        var current = el;

        while (current != null)
        {
            var transformAttr = current.Attribute("transform")?.Value;
            if (!string.IsNullOrEmpty(transformAttr))
            {
                var funcs = ParseTransformFunctions(transformAttr);
                foreach (var (name, args) in funcs)
                {
                    var localMatrix = CreateMatrixFromTransform(name, args);
                    matrix = matrix * localMatrix;
                }
            }

            current = current.Parent;
        }

        return matrix;
    }

    private static Matrix3x2 CreateMatrixFromTransform(string name, float[] args)
    {
        switch (name.ToLowerInvariant())
        {
            case "translate":
                float tx = args.Length > 0 ? args[0] : 0;
                float ty = args.Length > 1 ? args[1] : 0;
                return Matrix3x2.CreateTranslation(tx, ty);

            case "scale":
                float sx = args.Length > 0 ? args[0] : 1;
                float sy = args.Length > 1 ? args[1] : sx;
                return Matrix3x2.CreateScale(sx, sy);

            case "rotate":
                float angle = args.Length > 0 ? args[0] : 0;
                float angleRad = angle * (float)Math.PI / 180f;
                if (args.Length >= 3)
                {
                    float cx = args[1];
                    float cy = args[2];
                    return Matrix3x2.CreateRotation(angleRad, new Vector2(cx, cy));
                }

                return Matrix3x2.CreateRotation(angleRad);

            case "matrix":
                if (args.Length >= 6)
                {
                    return new Matrix3x2(args[0], args[1], args[2], args[3], args[4], args[5]);
                }

                break;

            case "skewx":
                if (args.Length > 0)
                {
                    float skewXRad = args[0] * (float)Math.PI / 180f;
                    return new Matrix3x2(1, 0, (float)Math.Tan(skewXRad), 1, 0, 0);
                }

                break;

            case "skewy":
                if (args.Length > 0)
                {
                    float skewYRad = args[0] * (float)Math.PI / 180f;
                    return new Matrix3x2(1, (float)Math.Tan(skewYRad), 0, 1, 0, 0);
                }

                break;
        }

        return Matrix3x2.Identity;
    }

    private static List<(string name, float[] args)> ParseTransformFunctions(string t)
    {
        var result = new List<(string, float[])>();
        var matches = Regex.Matches(t, @"(\w+)\s*\(([^)]*)\)");
        foreach (Match m in matches)
        {
            var name = m.Groups[1].Value;
            var argsStr = m.Groups[2].Value;
            var parts = argsStr.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
            var args = new float[parts.Length];
            for (var i = 0; i < parts.Length; i++)
                float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out args[i]);
            result.Add((name, args));
        }

        return result;
    }

    private static string CleanFontFamily(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "Segoe UI";
        var first = raw.Split(',').First().Trim().Trim('\'');
        return first.Equals("DejaVu Sans", StringComparison.OrdinalIgnoreCase) ? "Segoe UI" : first;
    }

    private static Color ParseSvgColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr) || colorStr == "currentColor" || colorStr.StartsWith("url("))
            return Colors.Black;

        if (colorStr.StartsWith("#"))
        {
            var hex = colorStr.TrimStart('#');
            if (hex.Length == 3)
                hex = new string([hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]]);
            if (uint.TryParse(hex, NumberStyles.HexNumber, null, out var rgb))
                return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        return colorStr.ToLowerInvariant() switch
        {
            "white" => Colors.White,
            "black" => Colors.Black,
            "red" => Colors.Red,
            "blue" => Colors.Blue,
            "green" => Colors.Green,
            "gray" or "grey" => Colors.Gray,
            "transparent" => Colors.Transparent,
            _ => Colors.Black,
        };
    }
}
