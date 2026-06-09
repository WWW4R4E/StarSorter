using System;

namespace StarSorter.Helpers
{
	public static class Converters
	{
		public static Uri? ToUri(string? value)
		{
			return !string.IsNullOrEmpty(value) ? new Uri(value) : null;
		}
	}
}
