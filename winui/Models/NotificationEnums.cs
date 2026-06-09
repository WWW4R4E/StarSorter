namespace StarSorter.ViewModels
{
    public enum NotificationType
    {
        Unknown,
        SubscriptionRepo,
        SubscriptionDev,
        Release,
        Star,
        Fork,
        PullRequest,
        Issue,
        Repository,
        Commit
    }

    public enum NotificationAction
    {
        Unknown,
        Create,
        Update,
        Delete,
        Subscribe,
        Unsubscribe,
        Mention,
        Open,
        Close,
        Comment
    }
}
