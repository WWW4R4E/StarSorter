using Microsoft.Windows.Storage;
using StarSorter.Helpers;
using StarSorter.ViewModels;
using System.Diagnostics;
using System.Text.Json;

namespace StarSorter.Services
{
	public class DataManagerService
	{
		private readonly string _dataPath;
		private readonly string _cachePath;
		private readonly string _configPath;
		private readonly string _imagesCachePath;
		private readonly string _apiCachePath;

		public DataManagerService()
		{
			var appData = ApplicationData.GetDefault();
			var localFolder = appData.LocalFolder;

			_dataPath = Path.Combine(localFolder.Path, "data");
			_cachePath = Path.Combine(localFolder.Path, "cache");
			_configPath = Path.Combine(localFolder.Path, "config");
			_imagesCachePath = Path.Combine(_cachePath, "images");
			_apiCachePath = Path.Combine(_cachePath, "api");

			Directory.CreateDirectory(_dataPath);
			Directory.CreateDirectory(_imagesCachePath);
			Directory.CreateDirectory(_apiCachePath);
			Directory.CreateDirectory(_configPath);

			// 调试输出缓存目录位置
			Debug.WriteLine($"[DataManagerService] Data directory: {_dataPath}");
			Debug.WriteLine($"[DataManagerService] Cache directory: {_cachePath}");
			Debug.WriteLine($"[DataManagerService] Images cache directory: {_imagesCachePath}");
			Debug.WriteLine($"[DataManagerService] API cache directory: {_apiCachePath}");
			Debug.WriteLine($"[DataManagerService] Config directory: {_configPath}");
			Debug.WriteLine($"[DataManagerService] Local folder path: {localFolder.Path}");
		}

		public async Task SaveStarsDataAsync(List<StarRepository> stars, List<NavLink> navLinks)
		{
			var starsJson = JsonSerializer.Serialize(stars, SerializationContext.CamelCase.ListStarRepository);
			var categoriesJson = JsonSerializer.Serialize(navLinks, SerializationContext.CamelCase.ListNavLink);

			var starsPath = Path.Combine(_dataPath, "stars.json");
			var categoriesPath = Path.Combine(_dataPath, "categories.json");

			await File.WriteAllTextAsync(starsPath, starsJson);
			await File.WriteAllTextAsync(categoriesPath, categoriesJson);
		}

		public async Task<(List<StarRepository> Stars, List<NavLink> Categories)> LoadStarsDataAsync()
		{
			var starsPath = Path.Combine(_dataPath, "stars.json");
			var categoriesPath = Path.Combine(_dataPath, "categories.json");

			try
			{
				List<StarRepository> stars = new();
				List<NavLink> categories = new();

				if (File.Exists(starsPath))
				{
					var starsJson = await File.ReadAllTextAsync(starsPath);
					var loadedStars = (List<StarRepository>?)JsonSerializer.Deserialize(starsJson, SerializationContext.CamelCase.ListStarRepository);
					stars = loadedStars ?? new List<StarRepository>();
				}

				if (File.Exists(categoriesPath))
				{
					var categoriesJson = await File.ReadAllTextAsync(categoriesPath);
					var loadedCategories = (List<NavLink>?)JsonSerializer.Deserialize(categoriesJson, SerializationContext.CamelCase.ListNavLink);
					categories = loadedCategories ?? new List<NavLink>();
				}
				return (stars, categories);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[DataManagerService] Error loading stars data: {ex.Message}");
				return (new List<StarRepository>(), new List<NavLink>());
			}
		}

		public async Task SaveNotificationsDataAsync(List<NotificationItem> notifications)
		{
			var json = JsonSerializer.Serialize(notifications, SerializationContext.CamelCase.ListNotificationItem);

			var filePath = Path.Combine(_dataPath, "notifications.json");
			await File.WriteAllTextAsync(filePath, json);
		}

		public async Task<List<NotificationItem>> LoadNotificationsDataAsync()
		{
			var filePath = Path.Combine(_dataPath, "notifications.json");

			try
			{
				if (!File.Exists(filePath))
				{
					System.Diagnostics.Debug.WriteLine($"[DataManagerService] Notifications file not found: {filePath}");
					return new List<NotificationItem>();
				}

				var json = await File.ReadAllTextAsync(filePath);

				var result = (List<NotificationItem>?)JsonSerializer.Deserialize(json, SerializationContext.CamelCase.ListNotificationItem) ?? new List<NotificationItem>();

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[DataManagerService] Error loading notifications from {filePath}: {ex.Message}");
				return new List<NotificationItem>();
			}
		}

		public async Task SaveUserProfileAsync<T>(T userProfile)
		{
			var json = JsonSerializer.Serialize(userProfile, SerializationContext.CamelCase.GetTypeInfo(typeof(T))!);

			var filePath = Path.Combine(_dataPath, "user_profile.json");
			await File.WriteAllTextAsync(filePath, json);

			System.Diagnostics.Debug.WriteLine($"[DataManagerService] Saved user profile data to {filePath}");
		}

		public async Task<T?> LoadUserProfileAsync<T>() where T : class
		{
			try
			{
				var filePath = Path.Combine(_dataPath, "user_profile.json");
				if (!File.Exists(filePath))
				{
					return null;
				}

				var json = await File.ReadAllTextAsync(filePath);

				return (T?)JsonSerializer.Deserialize(json, SerializationContext.CamelCase.GetTypeInfo(typeof(T))!);
			}
			catch
			{
				return null;
			}
		}

		public async Task SaveSettingsAsync<T>(T settings)
		{
			var json = JsonSerializer.Serialize(settings, SerializationContext.CamelCase.GetTypeInfo(typeof(T))!);

			var filePath = Path.Combine(_configPath, "settings.json");
			await File.WriteAllTextAsync(filePath, json);
		}

		public async Task<T?> LoadSettingsAsync<T>()
		{
			var filePath = Path.Combine(_configPath, "settings.json");

			try
			{
				if (!File.Exists(filePath))
				{
					Debug.WriteLine($"[DataManagerService] Settings file not found: {filePath}");
					return default;
				}

				var json = await File.ReadAllTextAsync(filePath);

				var result = (T?)JsonSerializer.Deserialize(json, SerializationContext.CamelCase.GetTypeInfo(typeof(T))!);

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[DataManagerService] Error loading settings from {filePath}: {ex.Message}");
				return default(T);
			}
		}

		public async Task SaveApiCacheAsync(string key, string jsonContent)
		{
			var filePath = Path.Combine(_apiCachePath, $"{SanitizeFileName(key)}.json");
			var doc = JsonDocument.Parse(jsonContent);
			var formatted = JsonSerializer.Serialize(doc.RootElement, SerializationContext.CamelCase.JsonElement);
			await File.WriteAllTextAsync(filePath, formatted);
		}

		public async Task<string?> LoadApiCacheAsync(string key)
		{
			var filePath = Path.Combine(_apiCachePath, $"{SanitizeFileName(key)}.json");
			try
			{
				if (!File.Exists(filePath))
					return null;

				var fileInfo = new FileInfo(filePath);
				// 缓存有效期1小时
				if (DateTime.Now.Subtract(fileInfo.LastWriteTime).TotalHours > 1)
				{
					File.Delete(filePath);
					return null;
				}

				return await File.ReadAllTextAsync(filePath);
			}
			catch
			{
				return null;
			}
		}

		private static string SanitizeFileName(string name)
		{
			var invalid = Path.GetInvalidFileNameChars();
			return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
		}

		public async Task ClearAllDataAsync()
		{
			try
			{
				// 清理数据目录
				var dataFiles = Directory.GetFiles(_dataPath, "*.json");
				foreach (var file in dataFiles)
				{
					File.Delete(file);
				}

				// 清理API缓存
				var apiCacheFiles = Directory.GetFiles(_apiCachePath, "*.json");
				foreach (var file in apiCacheFiles)
				{
					File.Delete(file);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[DataManagerService] Error clearing data: {ex.Message}");
			}
		}

		public async Task CleanupCacheAsync()
		{
			// 清理过期的API缓存文件
			try
			{
				var apiCacheFiles = Directory.GetFiles(_apiCachePath, "*.json");
				foreach (var file in apiCacheFiles)
				{
					var fileInfo = new FileInfo(file);
					// 如果文件超过24小时，则删除它
					if (DateTime.Now.Subtract(fileInfo.LastWriteTime).TotalHours > 24)
					{
						File.Delete(file);
					}
				}

				Debug.WriteLine("[DataManagerService] Completed cache cleanup operation");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[DataManagerService] Error during cache cleanup: {ex.Message}");
			}
		}
	}
}