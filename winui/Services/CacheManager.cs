namespace StarSorter.Services
{
	public class CacheManager
	{
		private readonly ImageCacheService _imageCacheService;
		private readonly DataManagerService _dataManagerService;

		public CacheManager(ImageCacheService imageCacheService, DataManagerService dataManagerService)
		{
			_imageCacheService = imageCacheService;
			_dataManagerService = dataManagerService;
		}

		public async Task ClearAllCacheAsync()
		{
			await _imageCacheService.ClearCacheAsync();
			await _dataManagerService.ClearAllDataAsync();
		}

		public async Task CleanupExpiredFilesAsync()
		{
			await _imageCacheService.CleanupExpiredFilesAsync();
			await _dataManagerService.CleanupCacheAsync();
		}
	}
}