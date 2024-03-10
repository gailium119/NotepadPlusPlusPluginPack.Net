using System;
using System.Collections.Generic;

namespace NppPlugin.DllExport
{
	internal static class Null
	{
		public static int NullSafeCount<T>(this ICollection<T> items)
		{
			if (items != null)
			{
				return items.Count;
			}
			return 0;
		}

		public static string NullSafeTrim(this string input, params char[] trimChars)
		{
			if (input != null)
			{
				return input.Trim(trimChars);
			}
			return null;
		}

		public static string NullSafeToString(this object input)
		{
			if (input != null)
			{
				return input.ToString();
			}
			return null;
		}

		public static string NullSafeToLowerInvariant(this string input)
		{
			if (input != null)
			{
				return input.ToLowerInvariant();
			}
			return null;
		}

		public static string NullSafeToUpperInvariant(this string input)
		{
			if (input != null)
			{
				return input.ToUpperInvariant();
			}
			return null;
		}

		public static string NullSafeTrimStart(this string input, params char[] trimChars)
		{
			if (input != null)
			{
				return input.TrimStart(trimChars);
			}
			return null;
		}

		public static string NullIfEmpty(this string input)
		{
			if (!string.IsNullOrEmpty(input))
			{
				return input;
			}
			return null;
		}

		public static string NullSafeTrimEnd(this string input, params char[] trimChars)
		{
			if (input != null)
			{
				return input.TrimEnd(trimChars);
			}
			return null;
		}

		public static T IfNull<T>(this T input, T replacement)
		{
			if (input != null)
			{
				return input;
			}
			return replacement;
		}

		public static string IfEmpty(this string input, string replacement)
		{
			if (!string.IsNullOrEmpty(input))
			{
				return input;
			}
			return replacement;
		}

		public static TValue NullSafeCall<T, TValue>(this T input, Func<T, TValue> method)
		{
			if (input != null)
			{
				return method(input);
			}
			return default(TValue);
		}

		public static TValue NullSafeCall<T, TValue>(this T input, Func<TValue> method)
		{
			if (input != null)
			{
				return method();
			}
			return default(TValue);
		}
	}
}
