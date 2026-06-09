using Microsoft.Windows.Storage;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace StarSorter.Services
{
	public class ImageCacheService
	{
		private readonly string _imageCachePath;
		private readonly HttpClient _httpClient;
		private readonly ConcurrentDictionary<string, Task<string>> _pendingDownloads = new();

		public ImageCacheService()
		{
			var appData = ApplicationData.GetDefault();
			var localFolder = appData.LocalFolder;

			_imageCachePath = Path.Combine(localFolder.Path, "cache", "images");

			Directory.CreateDirectory(_imageCachePath);

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHub-Stars-Manager");
		}

		public Task<string> GetCachedImagePathAsync(string imageUrl)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				System.Diagnostics.Debug.WriteLine("[ImageCacheService] Image URL is null or empty, returning default image");
				return Task.FromResult("ms-appx:///Assets/default-avatar.png");
			}

			var fileName = GenerateFileName(imageUrl);
			var filePath = Path.Combine(_imageCachePath, fileName);

			if (File.Exists(filePath))
			{
				var fileInfo = new FileInfo(filePath);
				var age = DateTimeOffset.Now - fileInfo.LastWriteTime;

				if (fileInfo.Length > 0 && age.TotalHours <= 24)
				{
					System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Returning cached image: {filePath}");
					return Task.FromResult(filePath);
				}
			}

			return DownloadAndCacheAsync(imageUrl, filePath, fileName);
		}

		private Task<string> DownloadAndCacheAsync(string imageUrl, string filePath, string fileName)
		{
			if (_pendingDownloads.TryGetValue(fileName, out var pending))
				return pending;

			var task = DownloadCoreAsync(imageUrl, filePath);
			_pendingDownloads.TryAdd(fileName, task);

			_ = task.ContinueWith(_ =>
			{
				_pendingDownloads.TryRemove(fileName, out var _);
			}, TaskContinuationOptions.ExecuteSynchronously);

			return task;
		}

		private async Task<string> DownloadCoreAsync(string imageUrl, string filePath)
		{
			System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Downloading image: {imageUrl}");

			try
			{
				var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
				if (imageBytes.Length > 0)
				{
					await File.WriteAllBytesAsync(filePath, imageBytes);
					System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Cached image saved: {filePath}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Downloaded image is empty: {imageUrl}");
				}
				return filePath;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Failed to download image {imageUrl}, error: {ex.Message}");
				return "ms-appx:///Assets/default-avatar.png";
			}
		}

		public async Task ClearCacheAsync()
		{
			if (Directory.Exists(_imageCachePath))
			{
				var files = Directory.GetFiles(_imageCachePath, "*.*");
				int deletedCount = 0;

				foreach (var file in files)
				{
					try
					{
						File.Delete(file);
						deletedCount++;
						System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Deleted cached image: {file}");
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Failed to delete cached image {file}, error: {ex.Message}");
						// 忽略删除失败的文件
					}
				}

				System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Cleared {deletedCount} cached images from {_imageCachePath}");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Image cache directory does not exist: {_imageCachePath}");
			}
		}

		public async Task CleanupExpiredFilesAsync()
		{
			if (Directory.Exists(_imageCachePath))
			{
				var files = Directory.GetFiles(_imageCachePath, "*.*");
				int expiredCount = 0;

				foreach (var file in files)
				{
					var fileInfo = new FileInfo(file);
					var age = DateTimeOffset.Now - fileInfo.CreationTime;

					if (age.TotalDays > 7) // 删除超过7天的文件
					{
						try
						{
							File.Delete(file);
							expiredCount++;
							System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Deleted expired cached image: {file}");
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Failed to delete expired cached image {file}, error: {ex.Message}");
							// 忽略删除失败的文件
						}
					}
				}

				System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Cleaned up {expiredCount} expired cached images from {_imageCachePath}");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Image cache directory does not exist: {_imageCachePath}");
			}
		}

		private string GenerateFileName(string url)
		{
			using var sha256 = SHA256.Create();
			var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(url));
			return BitConverter.ToString(hash).Replace("-", "").ToLower() + ".png";
		}
	}
}