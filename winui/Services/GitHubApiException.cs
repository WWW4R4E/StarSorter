using StarSorter.Helpers;
using StarSorter.Native;

namespace StarSorter.Services
{
    public class GitHubApiException : Exception
    {
        public GhError ErrorCode { get; }
        public string? ServerMessage { get; }

        public GitHubApiException(GhError error)
            : base(GetErrorMessage(error))
        {
            ErrorCode = error;
            ServerMessage = null;
        }

        public GitHubApiException(GhError error, string? serverMessage)
            : base(FormatMessage(error, serverMessage))
        {
            ErrorCode = error;
            ServerMessage = serverMessage;
        }

        public GitHubApiException(GhError error, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = error;
            ServerMessage = null;
        }

        private static string FormatMessage(GhError error, string? serverMessage)
        {
            return GetErrorMessage(error);
        }

        private static string GetErrorMessage(GhError error) => error switch
        {
            GhError.GH_SUCCESS => "Success",
            GhError.GH_NETWORK_ERROR => LocalizationHelper.GetLocalizedString("GhNetworkError"),
            GhError.GH_UNAUTHORIZED => LocalizationHelper.GetLocalizedString("GhUnauthorized"),
            GhError.GH_NOT_FOUND => LocalizationHelper.GetLocalizedString("GhNotFound"),
            GhError.GH_RATE_LIMITED => LocalizationHelper.GetLocalizedString("GhRateLimited"),
            GhError.GH_SERVER_ERROR => LocalizationHelper.GetLocalizedString("GhServerError"),
            GhError.GH_PARSE_ERROR => LocalizationHelper.GetLocalizedString("GhParseError"),
            GhError.GH_FORBIDDEN => LocalizationHelper.GetLocalizedString("GhForbidden"),
            GhError.GH_HTTP_ERROR => LocalizationHelper.GetLocalizedString("GhHttpError"),
            _ => LocalizationHelper.GetLocalizedString("GhUnknownError", ((int)error).ToString())
        };
    }
}
