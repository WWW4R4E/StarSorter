using System.Text.Json;
using StarSorter.ViewModels;

namespace StarSorter.Helpers
{
    public class ClassifyExportData
    {
        public string Prompt { get; set; } = string.Empty;
        public List<string> ExistingCategories { get; set; } = new();
        public List<ClassifyRepoItem> UncategorizedRepos { get; set; } = new();
    }

    public class ClassifyRepoItem
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ClassifyImportData
    {
        public Dictionary<string, List<string>> Categories { get; set; } = new();
        public List<string> Unassigned { get; set; } = new();
    }

    public static class AIServiceHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string BuildPrompt()
        {
            return """
你是一个 GitHub Stars 管理助手。

现有分类如下（请尽量将仓库分配到已有分类中）：
{{现有分类}}

未分类仓库列表（名称 + GitHub 链接）：
{{未分类仓库}}

要求：
1. 根据仓库名称和项目性质，将每个仓库分配到最合适的分类
2. 对于不适合任何已有分类的仓库，请新建合理的分类名称
3. 保留仓库的原始名称不变

请严格按照以下 JSON 格式返回（不要添加任何额外说明文本）：
{
  "categories": {
    "已有分类名": ["repo1", "repo2"],
    "新建分类名": ["repo3", "repo4"]
  },
  "unassigned": ["无法分类的仓库名"]
}
""";
        }

        public static string BuildExportJson(List<StarRepository> uncategorized, List<NavLink> existingCategories)
        {
            var export = new ClassifyExportData
            {
                Prompt = BuildPrompt(),
                ExistingCategories = existingCategories
                    .Where(n => !n.IsBuiltIn)
                    .Select(n => n.Label)
                    .ToList(),
                UncategorizedRepos = uncategorized
                    .Where(r => string.IsNullOrEmpty(r.Category))
                    .Select(r => new ClassifyRepoItem
                    {
                        Name = r.Name,
                        Url = string.IsNullOrEmpty(r.HtmlUrl)
                            ? $"https://github.com/{r.OwnerName}/{r.Name}"
                            : r.HtmlUrl
                    })
                    .ToList()
            };

            return JsonSerializer.Serialize(export, SerializationContext.CamelCase.ClassifyExportData);
        }

        public static ClassifyImportData? ParseImportJson(string json)
        {
            try
            {
                var result = (ClassifyImportData?)JsonSerializer.Deserialize(json, SerializationContext.CamelCase.ClassifyImportData);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static bool ValidateImportData(ClassifyImportData? data, out string? error)
        {
            if (data == null)
            {
                error = "JSON 解析失败，请检查格式是否正确。";
                return false;
            }

            if (data.Categories == null || data.Categories.Count == 0)
            {
                error = "未找到任何分类数据（categories 字段为空）。";
                return false;
            }

            var hasEmptyCategory = data.Categories.Any(kv => kv.Value == null || kv.Value.Count == 0);
            if (hasEmptyCategory)
            {
                error = "部分分类下没有仓库，请检查。";
                return false;
            }

            error = null;
            return true;
        }
    }
}
