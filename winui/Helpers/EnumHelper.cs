namespace StarSorter.Helpers;

internal static partial class EnumHelper
{
    public static TEnum GetEnum<TEnum>(string text) where TEnum : struct, Enum
    {
        return Enum.Parse<TEnum>(text);
    }
}
