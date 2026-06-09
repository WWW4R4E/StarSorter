using Microsoft.Windows.ApplicationModel.Resources;

namespace StarSorter.Helpers
{
    public static class LocalizationHelper
    {
        private static readonly ResourceLoader _resourceLoader = new();

        public static string GetLocalizedString(string key)
        {
            try
            {
                return _resourceLoader.GetString(key);
            }
            catch
            {
                return key;
            }
        }

        public static string GetLocalizedString(string key, params object[] args)
        {
            var baseString = GetLocalizedString(key);
            try
            {
                return string.Format(baseString, args);
            }
            catch
            {
                return baseString;
            }
        }
    }
}