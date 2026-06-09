using System;
using System.Collections.Generic;

namespace StarSorter.ViewModels
{
    public class NotificationItem
    {
        public string Id { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Unknown;
        public NotificationAction Action { get; set; } = NotificationAction.Unknown;
        public GitHubUser Developer { get; set; } = new();
        public Repository RepositoryInfo { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public bool HasValidAvatar => !string.IsNullOrEmpty(Developer?.AvatarUrl);
        public bool HasNoValidAvatar => string.IsNullOrEmpty(Developer?.AvatarUrl);

        public string ActionDescription => Action switch
        {
            NotificationAction.Create => "创建",
            NotificationAction.Update => "更新",
            NotificationAction.Delete => "删除",
            NotificationAction.Subscribe => "订阅",
            NotificationAction.Unsubscribe => "取消订阅",
            NotificationAction.Mention => "提及",
            NotificationAction.Open => "打开",
            NotificationAction.Close => "关闭",
            NotificationAction.Comment => "评论",
            _ => "未知操作"
        };

        public string TypeDescription => Type switch
        {
            NotificationType.Release => "发布通知",
            NotificationType.Star => "Star动态",
            NotificationType.Fork => "Fork通知",
            NotificationType.PullRequest => "Pull Request",
            NotificationType.Issue => "Issue",
            NotificationType.Repository => "仓库更新",
            NotificationType.SubscriptionDev => "订阅开发者",
            NotificationType.SubscriptionRepo => "订阅仓库",
            NotificationType.Commit => "提交",
            _ => "未知类型"
        };
    }
}
