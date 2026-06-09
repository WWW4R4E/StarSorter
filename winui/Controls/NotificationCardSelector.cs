using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Models;

namespace StarSorter.Controls
{
    public partial class NotificationCardSelector : DataTemplateSelector
    {
        public DataTemplate? IssueTemplate { get; set; }
        public DataTemplate? PullRequestTemplate { get; set; }
        public DataTemplate? ReleaseTemplate { get; set; }
        public DataTemplate? CommitTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is NotificationThread thread)
            {
                return (thread.Subject.Type switch
                {
                    "Issue" => IssueTemplate ?? DefaultTemplate,
                    "PullRequest" => PullRequestTemplate ?? DefaultTemplate,
                    "Release" => ReleaseTemplate ?? DefaultTemplate,
                    "Commit" => CommitTemplate ?? DefaultTemplate,
                    _ => DefaultTemplate ?? IssueTemplate ?? PullRequestTemplate ?? ReleaseTemplate ?? CommitTemplate
                })!;
            }

            return (DefaultTemplate ?? base.SelectTemplateCore(item))!;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}