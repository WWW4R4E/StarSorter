using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using StarSorter.Helpers;
using StarSorter.Models;
using StarSorter.Native;
using StarSorter.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarSorter.ViewModels
{
	public partial class NotificationsViewModel : ObservableObject
	{
		private readonly GitHubService _gitHubService;
		private readonly DataManagerService _dataManager;
		private readonly ILogger<NotificationsViewModel> _logger;
		private readonly ShellViewModel _shellVm;

		private const string CacheKey = "notifications_all";
		private const int CacheExpiryHours = 1;

		public ObservableCollection<NavLink> NavLinks { get; set; } = new();

		[ObservableProperty]
		public partial ObservableCollection<NotificationThread> CurrentCategoryNotifications { get; set; } = new();

		[ObservableProperty] public partial string SelectedCategory { get; set; } = string.Empty;

		[ObservableProperty] public partial bool IsLoading { get; set; } = false;

		private List<NotificationThread> _allNotifications = new();

		private string AllNotificationsLabel => LocalizationHelper.GetLocalizedString("AllNotifications");
		private string IssueLabel => LocalizationHelper.GetLocalizedString("Issue");
		private string PullRequestLabel => LocalizationHelper.GetLocalizedString("PullRequest");
		private string ReleaseLabel => LocalizationHelper.GetLocalizedString("Release");
		private string CommitLabel => LocalizationHelper.GetLocalizedString("Commit");

		public NotificationsViewModel(GitHubService gitHubService, DataManagerService dataManager,
			ILogger<NotificationsViewModel> logger, ShellViewModel shellVm)
		{
			_gitHubService = gitHubService;
			_dataManager = dataManager;
			_logger = logger;
			_shellVm = shellVm;

			InitializeNavLinks();
			_ = LoadNotificationsAsync();
		}

		public void InitializeNavLinks()
		{
			NavLinks.Add(new NavLink { Label = AllNotificationsLabel, Glyph = "\uE791" });
			NavLinks.Add(new NavLink { Label = IssueLabel, Glyph = "\uE16F" });
			NavLinks.Add(new NavLink { Label = PullRequestLabel, Glyph = "\uE16F" });
			NavLinks.Add(new NavLink { Label = ReleaseLabel, Glyph = "\uE15B" });
			NavLinks.Add(new NavLink { Label = CommitLabel, Glyph = "\uE1CE" });
		}

		public string GetLocalizedString(string key) => LocalizationHelper.GetLocalizedString(key);

		public async Task LoadNotificationsAsync()
		{
			IsLoading = true;

			try
			{
				var cached = await _dataManager.LoadApiCacheAsync(CacheKey);
				if (cached != null)
				{
					var deserialized = (List<NotificationThread>?)JsonSerializer.Deserialize(cached,
						SerializationContext.SnakeCaseLower.ListNotificationThread);
					if (deserialized != null && deserialized.Count > 0)
					{
						_allNotifications = deserialized;
						LoadNotificationsForCategory(SelectedCategory);
						return;
					}
				}

				var notifications = await _gitHubService.GetNotificationsAsync();
				_allNotifications = notifications ?? new List<NotificationThread>();

				var json = JsonSerializer.Serialize(_allNotifications, SerializationContext.SnakeCaseLower.ListNotificationThread);
				await _dataManager.SaveApiCacheAsync(CacheKey, json);

				LoadNotificationsForCategory(SelectedCategory);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "加载通知失败");

				if (ex is GitHubApiException apiEx)
				{
					var title = apiEx.ErrorCode switch
					{
						GhError.GH_FORBIDDEN => LocalizationHelper.GetLocalizedString("PermissionDenied"),
						GhError.GH_UNAUTHORIZED => LocalizationHelper.GetLocalizedString("AuthFailed"),
						_ => LocalizationHelper.GetLocalizedString("RequestFailed")
					};
					_shellVm.RequestDialog(title, ex.Message);
				}

				if (_allNotifications.Count == 0)
				{
					var cached = await _dataManager.LoadApiCacheAsync(CacheKey);
					if (cached != null)
					{
						var deserialized = (List<NotificationThread>?)JsonSerializer.Deserialize(cached,
							SerializationContext.SnakeCaseLower.ListNotificationThread);
						if (deserialized != null)
						{
							_allNotifications = deserialized;
							LoadNotificationsForCategory(SelectedCategory);
						}
					}
				}
			}
			finally
			{
				IsLoading = false;
			}
		}

		public async Task RefreshNotificationsAsync()
		{
			IsLoading = true;

			try
			{
				var notifications = await _gitHubService.GetNotificationsAsync();
				_allNotifications = notifications ?? new List<NotificationThread>();

				var json = JsonSerializer.Serialize(_allNotifications, SerializationContext.SnakeCaseLower.ListNotificationThread);
				await _dataManager.SaveApiCacheAsync(CacheKey, json);

				LoadNotificationsForCategory(SelectedCategory);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "刷新通知失败");

				if (ex is GitHubApiException apiEx)
				{
					var title = apiEx.ErrorCode switch
					{
						GhError.GH_FORBIDDEN => LocalizationHelper.GetLocalizedString("PermissionDenied"),
						GhError.GH_UNAUTHORIZED => LocalizationHelper.GetLocalizedString("AuthFailed"),
						_ => LocalizationHelper.GetLocalizedString("RequestFailed")
					};
					_shellVm.RequestDialog(title, ex.Message);
				}
			}
			finally
			{
				IsLoading = false;
			}
		}

		public async Task MarkAllReadAsync()
		{
			try
			{
				await _gitHubService.MarkNotificationsReadAsync();
				_allNotifications.Clear();
				CurrentCategoryNotifications.Clear();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "标记已读失败");
			}
		}

		public async Task DeleteNotificationAsync(NotificationThread thread)
		{
			if (thread == null) return;

			try
			{
				await _gitHubService.MarkThreadDoneAsync(thread.Id);
				_allNotifications.Remove(thread);
				LoadNotificationsForCategory(SelectedCategory);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "删除通知失败: {ThreadId}", thread.Id);
			}
		}

		public void LoadNotificationsForCategory(string categoryName)
		{
			SelectedCategory = categoryName;

			if (string.IsNullOrEmpty(categoryName) || categoryName == AllNotificationsLabel)
			{
				CurrentCategoryNotifications = new ObservableCollection<NotificationThread>(_allNotifications);
			}
			else
			{
				var filtered = _allNotifications.Where(n => GetCategoryForType(n.Subject.Type) == categoryName)
					.ToList();
				CurrentCategoryNotifications = new ObservableCollection<NotificationThread>(filtered);
			}
		}

		private string GetCategoryForType(string subjectType)
		{
			return subjectType switch
			{
				"Issue" => IssueLabel,
				"PullRequest" => PullRequestLabel,
				"Release" => ReleaseLabel,
				"Commit" => CommitLabel,
				_ => AllNotificationsLabel
			};
		}
	}
}
