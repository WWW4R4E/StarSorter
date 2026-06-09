namespace StarSorter.Native
{
    public enum GhError
    {
        GH_SUCCESS = 0,
        GH_NETWORK_ERROR = -1,
        GH_UNAUTHORIZED = -2,
        GH_NOT_FOUND = -3,
        GH_RATE_LIMITED = -4,
        GH_SERVER_ERROR = -5,
        GH_PARSE_ERROR = -6,
        GH_FORBIDDEN = -7,
        GH_HTTP_ERROR = -8,
        GH_UNKNOWN = -99,
    }
}
