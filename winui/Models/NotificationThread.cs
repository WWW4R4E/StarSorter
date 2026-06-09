using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using StarSorter.Helpers;
using StarSorter.ViewModels;

namespace StarSorter.Models
{
    public class NotificationThread : INotifyPropertyChanged
    {
        private bool _unread;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("repository")]
        public Repository Repository { get; set; } = new();

        [JsonPropertyName("subject")]
        public NotificationSubject Subject { get; set; } = new();

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("unread")]
        public bool Unread
        {
            get => _unread;
            set
            {
                if (_unread != value)
                {
                    _unread = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;

        [JsonPropertyName("last_read_at")]
        public string? LastReadAt { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("subscription_url")]
        public string SubscriptionUrl { get; set; } = string.Empty;

        // Computed display properties
        [JsonIgnore]
        public string TimeAgo
        {
            get
            {
                if (string.IsNullOrEmpty(UpdatedAt))
                    return string.Empty;

                if (DateTime.TryParse(UpdatedAt, out var dt))
                {
                    var span = DateTime.UtcNow - dt.ToUniversalTime();
                    if (span.TotalMinutes < 1) return LocalizationHelper.GetLocalizedString("Notification_TimeAgo_JustNow");
                    if (span.TotalMinutes < 60) return LocalizationHelper.GetLocalizedString("Notification_TimeAgo_MinutesAgo", (int)span.TotalMinutes);
                    if (span.TotalHours < 24) return LocalizationHelper.GetLocalizedString("Notification_TimeAgo_HoursAgo", (int)span.TotalHours);
                    if (span.TotalDays < 7) return LocalizationHelper.GetLocalizedString("Notification_TimeAgo_DaysAgo", (int)span.TotalDays);
                    return dt.ToString("yyyy-MM-dd");
                }
                return UpdatedAt;
            }
        }

        [JsonIgnore]
        public string ReasonLabel => Reason switch
        {
            "mention" => LocalizationHelper.GetLocalizedString("Notification_Reason_Mention"),
            "subscribed" => LocalizationHelper.GetLocalizedString("Notification_Reason_Subscribed"),
            "review_requested" => LocalizationHelper.GetLocalizedString("Notification_Reason_ReviewRequested"),
            "assign" => LocalizationHelper.GetLocalizedString("Notification_Reason_Assign"),
            "state_change" => LocalizationHelper.GetLocalizedString("Notification_Reason_StateChange"),
            "security_alert" => LocalizationHelper.GetLocalizedString("Notification_Reason_SecurityAlert"),
            "ci_activity" => LocalizationHelper.GetLocalizedString("Notification_Reason_CIActivity"),
            "manual" => LocalizationHelper.GetLocalizedString("Notification_Reason_Manual"),
            _ => Reason
        };

        [JsonIgnore]
        public string TypeLabel => Subject.Type switch
        {
            "Issue" => LocalizationHelper.GetLocalizedString("Issue"),
            "PullRequest" => LocalizationHelper.GetLocalizedString("PullRequest"),
            "Release" => LocalizationHelper.GetLocalizedString("Release"),
            "Commit" => LocalizationHelper.GetLocalizedString("Commit"),
            _ => Subject.Type
        };

        [JsonIgnore]
        public string ShortTitle
        {
            get
            {
                var title = Subject.Title ?? string.Empty;
                return title.Length > 80 ? title[..77] + "..." : title;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}