using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

enum BuildOption
{
    FullBuild = 1,
    WinUIOnly = 2,
    AndroidOnly = 3,
    CoreOnly = 4,
    RunTests = 5,
    RunWinUI = 6,
    ServeDocs = 7,
    CleanOnly = 8,
    Exit = 0
}

class Build : NukeBuild
{
    // 项目路径定义
    AbsolutePath CoreDirectory => RootDirectory / "core";
    AbsolutePath WinUIDirectory => RootDirectory / "winui";
    AbsolutePath ComposeDirectory => RootDirectory / "compose";
    AbsolutePath AndroidAppDirectory => ComposeDirectory / "app";
    AbsolutePath ZigOutputDirectory => RootDirectory / "core" / "zig-out";
    AbsolutePath ZigDocDirectory => CoreDirectory / "doc";
    AbsolutePath ZigDocOutputDirectory => ZigOutputDirectory / "doc";
    
    // 构建配置
    [Parameter("Configuration to build - Default is 'Release'")]
    readonly Configuration Configuration = Configuration.Release;
    
    // 是否构建各平台应用
    [Parameter("Build WinUI app (Windows only)")]
    readonly bool BuildWinUI = true;
    
    [Parameter("Build Android/Compose app")]
    readonly bool BuildAndroid = true;
    
    // 目标 1: 清理所有构建产物
    Target Clean => _ => _
        .Executes(() =>
        {
            // 清理 WinUI 构建产物（直接删除文件，不使用 DotNetClean）
            var winuiBinDir = WinUIDirectory / "bin";
            var winuiObjDir = WinUIDirectory / "obj";
            var winuiNativeDir = WinUIDirectory / "Native";
            var winuiNativeIncludeDir = WinUIDirectory / "Native" / "include";
            if (Directory.Exists(winuiBinDir))
                Directory.Delete(winuiBinDir, true);
            if (Directory.Exists(winuiObjDir))
                Directory.Delete(winuiObjDir, true);
            
            // 只清理 Native 目录下的 dll 和 include 子目录，保留响应文件
            if (Directory.Exists(winuiNativeDir))
            {
                // 删除所有 dll 文件
                foreach (var dllFile in Directory.EnumerateFiles(winuiNativeDir, "*.dll"))
                {
                    File.Delete(dllFile);
                }
                
                // 删除 include 子目录
                if (Directory.Exists(winuiNativeIncludeDir))
                {
                    Directory.Delete(winuiNativeIncludeDir, true);
                }
            }
            
            // 清理 Zig 构建产物
            if (Directory.Exists(ZigOutputDirectory))
            {
                Directory.Delete(ZigOutputDirectory, true);
            }
            
            // 清理 Zig 缓存目录
            var zigCacheDir = CoreDirectory / ".zig-cache";
            if (Directory.Exists(zigCacheDir))
            {
                Directory.Delete(zigCacheDir, true);
            }
            
            // 清理 Compose/Android 构建产物
            var gradleDir = ComposeDirectory / ".gradle";
            if (Directory.Exists(gradleDir))
            {
                Directory.Delete(gradleDir, true);
            }
            
            var androidBuildDir = AndroidAppDirectory / "build";
            if (Directory.Exists(androidBuildDir))
                Directory.Delete(androidBuildDir, true);
                
            Log.Information("✅ Clean completed");
        });
    
    // 目标 2: 构建 Zig Core 核心库
    Target BuildZigCore => _ => _
        .Executes(() =>
        {
            Log.Information("🔨 Building Zig Core Library...");
            
            var zigInstalled = File.Exists(Environment.GetEnvironmentVariable("PATH")
                .Split(Path.PathSeparator)
                .Select(p => Path.Combine(p, "zig.exe"))
                .FirstOrDefault(File.Exists));
            
            if (!zigInstalled)
            {
                Log.Warning("Zig not found in PATH. Please install Zig: https://ziglang.org/download/");
                throw new Exception("Zig is required to build the core library");
            }
            
            var result = ProcessTasks.StartProcess(
                "zig",
                $"build -Doptimize=ReleaseFast -Dcpu=baseline",
                CoreDirectory
            );
            
            result.AssertZeroExitCode();
            
            var zigLibPath = ZigOutputDirectory / "lib";
            if (Directory.Exists(zigLibPath))
            {
                Log.Information($"✅ Zig Core built successfully: {zigLibPath}");
                foreach (var file in Directory.EnumerateFiles(zigLibPath, "*.*"))
                {
                    Log.Information($"📚 Generated: {file}");
                }
            }
            else if (Directory.Exists(ZigOutputDirectory))
            {
                Log.Information($"✅ Zig Core built successfully: {ZigOutputDirectory}");
                foreach (var file in Directory.EnumerateFiles(ZigOutputDirectory, "*.*", SearchOption.AllDirectories))
                {
                    Log.Information($"📚 Generated: {file}");
                }
            }
            else
            {
                Log.Warning("Zig output directory not found. Build may have failed.");
            }
        });
    
    // 目标 4: 复制 Zig 核心库产物到 WinUI3 项目
    Target CopyZigArtifacts => _ => _
        .DependsOn(BuildZigCore)
        .OnlyWhenStatic(() => BuildWinUI && OperatingSystem.IsWindows())
        .Executes(() =>
        {
            Log.Information("📦 Copying Zig artifacts to WinUI project...");
            
            var zigBinDir = ZigOutputDirectory / "bin";
            var winuiNativeDir = WinUIDirectory / "Native";
            var winuiHeadersDir = winuiNativeDir / "include";
            
            Directory.CreateDirectory(winuiNativeDir);
            Directory.CreateDirectory(winuiHeadersDir);
            
            var dllFiles = Directory.EnumerateFiles(zigBinDir, "*.dll").ToList();
            if (dllFiles.Any())
            {
                foreach (var dllPath in dllFiles)
                {
                    var destPath = winuiNativeDir / Path.GetFileName(dllPath);
                    File.Copy(dllPath, destPath, true);
                    Log.Information($"📋 Copied DLL: {Path.GetFileName(dllPath)}");
                }
            }
            else
            {
                Log.Warning($"No DLL files found in Zig output directory: {zigBinDir}");
            }
            
            var headerFile = CoreDirectory / "include" / "github_stars.h";
            if (File.Exists(headerFile))
            {
                var destHeaderPath = winuiHeadersDir / "github_stars.h";
                File.Copy(headerFile, destHeaderPath, true);
                Log.Information($"📋 Copied header: github_stars.h");
            }
            else
            {
                Log.Warning($"Header file not found: {headerFile}");
            }
            
            Log.Information("✅ Zig artifacts copied successfully");
        });
    
    // 目标 5: 通过 ClangSharpPInvokeGenerator 生成 C ABI 绑定
    Target GenerateClangSharpBindings => _ => _
        .DependsOn(CopyZigArtifacts)
        .OnlyWhenStatic(() => BuildWinUI && OperatingSystem.IsWindows())
        .Executes(() =>
        {
            Log.Information("🔗 Generating C ABI bindings with ClangSharpPInvokeGenerator...");
            
            var responseFile = WinUIDirectory / "Native" / "generate.rsp";
            
            if (!File.Exists(responseFile))
            {
                Log.Error($"Response file not found: {responseFile}");
                throw new Exception("Cannot generate bindings without response file");
            }
            
            var generatorPath = Environment.GetEnvironmentVariable("PATH")
                .Split(Path.PathSeparator)
                .Select(p => Path.Combine(p, "ClangSharpPInvokeGenerator.cmd"))
                .FirstOrDefault(File.Exists);
            
            if (string.IsNullOrEmpty(generatorPath))
            {
                Log.Error("ClangSharpPInvokeGenerator.cmd not found in PATH");
                Log.Error("Please install the tool first:");
                Log.Error("dotnet tool install --global ClangSharpPInvokeGenerator");
                throw new Exception("ClangSharpPInvokeGenerator is required to generate bindings");
            }
            
            var result = ProcessTasks.StartProcess(
                generatorPath,
                $"@{responseFile}",
                WinUIDirectory / "Native"
            );
            
            result.AssertZeroExitCode();
            Log.Information("✅ Bindings generated successfully");
        });
    
    // 目标 6: 构建 WinUI 3 应用
    Target BuildWinUIApp => _ => _
        .DependsOn(GenerateClangSharpBindings)
        .OnlyWhenStatic(() => BuildWinUI && OperatingSystem.IsWindows())
        .Executes(() =>
        {
            Log.Information("🏗️ Building WinUI 3 Application...");
            
            var winuiNativeDir = WinUIDirectory / "Native";
            var winUIOutputPath = WinUIDirectory / "bin" / Configuration.ToString();
            var runtimeIdentifier = Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
            var projectPath = WinUIDirectory / "StarSorter.csproj";
            
            DotNetBuild(s => s
                .SetProjectFile(projectPath)
                .SetConfiguration(Configuration)
                .SetNoRestore(true)
                .SetRuntime(runtimeIdentifier));
            
            var dllFiles = Directory.EnumerateFiles(winuiNativeDir, "*.dll").ToList();
            if (dllFiles.Any() && Directory.Exists(winUIOutputPath))
            {
                foreach (var dllPath in dllFiles)
                {
                    var destPath = winUIOutputPath / Path.GetFileName(dllPath);
                    File.Copy(dllPath, destPath, true);
                }
                Log.Information($"📋 Copied {dllFiles.Count} Zig library(ies) to WinUI output");
            }
            
            Log.Information($"✅ WinUI App built: {winUIOutputPath}");
        });
    
    // 目标 7: 构建 Android/Compose 应用（仅 Android）
    Target BuildAndroidApp => _ => _
        .DependsOn(BuildZigCore)
        .OnlyWhenStatic(() => BuildAndroid)
        .Executes(() =>
        {
            Log.Information("🎨 Building Android/Compose Application...");
            
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (string.IsNullOrEmpty(javaHome))
            {
                Log.Warning("JAVA_HOME not set. Using system Java if available.");
            }
            
            var gradlew = ComposeDirectory / (OperatingSystem.IsWindows() ? "gradlew.bat" : "gradlew");
            if (!File.Exists(gradlew))
            {
                Log.Error("Gradle wrapper not found in compose directory. Please run: gradle wrapper");
                throw new Exception("Gradle wrapper is required");
            }
            
            var gradleTask = Configuration == Configuration.Release 
                ? ":app:assembleRelease" 
                : ":app:assembleDebug";
            
            Log.Information($"Running Gradle task: {gradleTask}");
            
            var result = ProcessTasks.StartProcess(
                gradlew,
                gradleTask,
                ComposeDirectory
            );
            
            result.AssertZeroExitCode();
            
            var apkType = Configuration == Configuration.Release ? "release" : "debug";
            var apkDirectory = AndroidAppDirectory / "build" / "outputs" / "apk" / apkType;
            
            if (Directory.Exists(apkDirectory))
            {
                var apks = Directory.EnumerateFiles(apkDirectory, "*.apk").ToList();
                if (apks.Any())
                {
                    Log.Information($"📱 Android APK(s) generated:");
                    foreach (var apk in apks)
                    {
                        Log.Information($"   - {apk}");
                    }
                }
                else
                {
                    Log.Warning("No APK files found in output directory");
                }
            }
            else
            {
                Log.Warning($"APK output directory not found: {apkDirectory}");
            }
            
            Log.Information("✅ Android/Compose App built successfully");
        });
    
    // 目标 8: 运行 WinUI 应用（调试用）
    Target RunWinUI => _ => _
        .DependsOn(BuildWinUIApp)
        .OnlyWhenStatic(() => BuildWinUI && OperatingSystem.IsWindows())
        .Executes(() =>
        {
            var exePath = WinUIDirectory / "bin" / Configuration.ToString() / "net10.0-windows" / "winui.exe";
            
            if (File.Exists(exePath))
            {
                Log.Information($"🚀 Launching WinUI App: {exePath}");
                ProcessTasks.StartProcess(exePath);
            }
            else
            {
                Log.Error($"WinUI executable not found at {exePath}");
            }
        });
    
    // 目标 9: 运行所有测试
    Target Test => _ => _
        .DependsOn(BuildZigCore)
        .Executes(() =>
        {
            Log.Information("🧪 Running all tests...");
            
            var coreSrcDir = CoreDirectory / "src";
            if (Directory.Exists(coreSrcDir))
            {
                Log.Information("Running Zig tests...");
                ProcessTasks.StartProcess("zig", "test src/main.zig", CoreDirectory)
                    .AssertZeroExitCode();
            }
            
            var winuiTestsDir = WinUIDirectory / "tests";
            if (Directory.Exists(winuiTestsDir))
            {
                Log.Information("Running .NET tests...");
                DotNetTest(s => s
                    .SetProjectFile(winuiTestsDir / "Tests.csproj"));
            }
            
            var gradlew = ComposeDirectory / (OperatingSystem.IsWindows() ? "gradlew.bat" : "gradlew");
            if (File.Exists(gradlew))
            {
                Log.Information("Running Android/Kotlin tests...");
                ProcessTasks.StartProcess(gradlew, "test", ComposeDirectory)
                    .AssertZeroExitCode();
            }
            else
            {
                Log.Warning("Gradle wrapper not found, skipping Android tests");
            }
            
            Log.Information("✅ All tests passed");
        });
    
    // 目标 10: 完整构建（默认目标）
    Target FullBuild => _ => _
        .DependsOn(BuildZigCore)
        .DependsOn(BuildWinUIApp)
        .DependsOn(BuildAndroidApp);
    
    // 目标 11: 仅构建核心（不构建 UI）
    Target BuildCoreOnly => _ => _
        .DependsOn(BuildZigCore);
    
    // 目标 12: 构建 Zig 文档
    Target BuildZigDocs => _ => _
        .Executes(() =>
        {
            Log.Information("📚 Building Zig Documentation...");
            
            var zigInstalled = File.Exists(Environment.GetEnvironmentVariable("PATH")
                .Split(Path.PathSeparator)
                .Select(p => Path.Combine(p, "zig.exe"))
                .FirstOrDefault(File.Exists));
            
            if (!zigInstalled)
            {
                Log.Warning("Zig not found in PATH. Please install Zig: https://ziglang.org/download/");
                throw new Exception("Zig is required to build documentation");
            }
            
            var result = ProcessTasks.StartProcess(
                "zig",
                "build docs",
                CoreDirectory
            );
            
            result.AssertZeroExitCode();
            
            if (Directory.Exists(ZigDocOutputDirectory))
            {
                Log.Information($"✅ Zig Documentation built successfully: {ZigDocOutputDirectory}");
                Log.Information($"📄 Generated documentation files:");
                foreach (var file in Directory.EnumerateFiles(ZigDocOutputDirectory, "*.html", SearchOption.AllDirectories))
                {
                    Log.Information($"   - {file}");
                }
            }
            else
            {
                Log.Warning("Documentation output directory not found. Build may have failed.");
            }
        });
    
    // 目标 13: 构建文档并启动服务器
    Target ServeDocs => _ => _
        .DependsOn(BuildZigDocs)
        .Executes(() =>
        {
            Log.Information("🌐 Starting documentation server...");
            
            var port = FindAvailablePort(8080);
            Log.Information($"Using available port: {port}");
            
            if (!Directory.Exists(ZigDocOutputDirectory))
            {
                Log.Error($"Documentation directory not found: {ZigDocOutputDirectory}");
                Log.Information("Please run 'BuildZigDocs' first.");
                return;
            }
            
            Log.Information($"Serving documentation from: {ZigDocOutputDirectory}");
            Log.Information($"Documentation will be available at: http://localhost:{port}");
            Log.Information($"Press Ctrl+C to stop the server");
            
            // 使用 dotnet-serve 或简单的 HTTP 服务器
            try
            {
                // 尝试使用 dotnet-serve
                var serveArgs = new[]
                {
                    "serve",
                    "--port", port.ToString(),
                    "--directory", ZigDocOutputDirectory.ToString(),
                    "--open-browser"
                };
                
                ProcessTasks.StartProcess("dotnet", string.Join(" ", serveArgs.Select(arg => 
                    arg.Contains(" ") ? $"\"{arg}\"" : arg)),
                    ZigDocOutputDirectory
                ).WaitForExit();
            }
            catch
            {
                // 如果 dotnet-serve 不可用，使用简单的 Python HTTP 服务器（如果可用）
                Log.Warning("dotnet-serve not found, trying alternative...");
                
                var pythonFound = File.Exists(Environment.GetEnvironmentVariable("PATH")
                    .Split(Path.PathSeparator)
                    .SelectMany(p => new[] { 
                        Path.Combine(p, "python.exe"), 
                        Path.Combine(p, "python3.exe") 
                    })
                    .FirstOrDefault(File.Exists));
                
                if (pythonFound)
                {
                    Log.Information("Using Python HTTP server...");
                    ProcessTasks.StartProcess(
                        "python",
                        $"-m http.server {port}",
                        ZigDocOutputDirectory
                    ).WaitForExit();
                }
                else
                {
                    Log.Error("No HTTP server tool found. Please install dotnet-serve:");
                    Log.Information("  dotnet tool install --global dotnet-serve");
                    Log.Information($"Or manually open: {ZigDocOutputDirectory}");
                }
            }
        });
    
    // 辅助方法：寻找可用端口
    static int FindAvailablePort(int startPort)
    {
        for (var port = startPort; port < startPort + 100; port++)
        {
            if (IsPortAvailable(port))
            {
                return port;
            }
        }
        throw new Exception("No available port found in the specified range");
    }
    
    // 辅助方法：检查端口是否可用
    static bool IsPortAvailable(int port)
    {
        try
        {
            var tcpListener = new TcpListener(System.Net.IPAddress.Loopback, port);
            tcpListener.Start();
            tcpListener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // 默认目标：交互式菜单
    public static int Main()
    {
        var buildScriptPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(buildScriptPath));
        
        if (Directory.Exists(projectRoot))
        {
            Environment.CurrentDirectory = projectRoot;
        }
        
        if (Environment.GetCommandLineArgs().Length == 1)
        {
            ShowInteractiveMenu();
            return 0;
        }
        
        return Execute<Build>(x => x.FullBuild);
    }
    
    static void ShowInteractiveMenu()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              StarSorter - 构建菜单                 ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  命令行参数格式: .\\build.ps1 --<TargetName>                  ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  1. 完整构建 (WinUI + Android)        --FullBuild           ║");
        Console.WriteLine("║  2. 仅构建 WinUI 3 应用               --BuildWinUIApp       ║");
        Console.WriteLine("║  3. 仅构建 Android/Jetpack Compose    --BuildAndroidApp     ║");
        Console.WriteLine("║  4. 仅构建 Zig 核心库                 --BuildCoreOnly       ║");
        Console.WriteLine("║  5. 运行所有测试                      --Test                ║");
        Console.WriteLine("║  6. 运行 WinUI 应用 (调试)            --RunWinUI            ║");
        Console.WriteLine("║  7. 构建并启动 Zig 文档服务器          --ServeDocs           ║");
        Console.WriteLine("║  8. 清理所有构建产物                  --Clean               ║");
        Console.WriteLine("║  0. 退出                                                    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        Console.Write("请选择操作 [0-8]: ");
        var input = Console.ReadLine();
        
        if (!int.TryParse(input, out var choice))
        {
            Console.WriteLine("❌ 无效输入，请输入数字");
            ShowInteractiveMenu();
            return;
        }
        
        var option = (BuildOption)choice;
        
        Console.WriteLine();
        
        switch (option)
        {
            case BuildOption.FullBuild:
                Console.WriteLine("🚀 开始完整构建...");
                Execute<Build>(x => x.FullBuild);
                break;
                
            case BuildOption.WinUIOnly:
                Console.WriteLine("🚀 开始构建 WinUI 3 应用...");
                Execute<Build>(x => x.BuildWinUIApp);
                break;
                
            case BuildOption.AndroidOnly:
                Console.WriteLine("🚀 开始构建 Android/Compose 应用...");
                Execute<Build>(x => x.BuildAndroidApp);
                break;
                
            case BuildOption.CoreOnly:
                Console.WriteLine("🚀 开始构建 Zig 核心库...");
                Execute<Build>(x => x.BuildCoreOnly);
                break;
                
            case BuildOption.RunTests:
                Console.WriteLine("🧪 开始运行测试...");
                Execute<Build>(x => x.Test);
                break;
                
            case BuildOption.RunWinUI:
                Console.WriteLine("🎮 开始运行 WinUI 应用...");
                Execute<Build>(x => x.RunWinUI);
                break;
                
            case BuildOption.ServeDocs:
                Console.WriteLine("📚 开始构建文档并启动服务器...");
                Execute<Build>(x => x.ServeDocs);
                break;
                
            case BuildOption.CleanOnly:
                Console.WriteLine("🧹 开始清理构建产物...");
                Execute<Build>(x => x.Clean);
                break;
                
            case BuildOption.Exit:
                Console.WriteLine("👋 退出构建工具");
                return;
                
            default:
                Console.WriteLine("❌ 无效选项，请重新选择");
                ShowInteractiveMenu();
                return;
        }
        
        Console.WriteLine();
        Console.WriteLine("✅ 操作完成！按任意键退出...");
        Console.ReadKey();
    }
}