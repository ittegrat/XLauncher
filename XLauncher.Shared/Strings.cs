using System;
using System.IO;
using System.Linq;
using System.Text;

namespace XLauncher.Common
{
  internal static class Strings
  {

    static readonly char[] illegals = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();
    static readonly char[] reserved = "!#$%&()-.@^_~".ToArray();

    public const string APP_NAME = "XLauncher";
    public const string CONFIG_SECTION = APP_NAME;
    public const string SETUP_CONFIG_SECTION = APP_NAME + "Setup";

    public const string MTX_APPLICATION = APP_NAME + ".App";
    public const string MTX_UPDATER = APP_NAME + ".Setup";
    public const string UPDATER_ARGS = "-upgrade";

    public const string XLSESSION_EVAR = "XLAUNCHER_SESSION";
    public const string XLSESSION_BASENAME = "XLSession";
    public const string XLSESSION_FILENAME = XLSESSION_BASENAME + ".xml";

    public static bool IsReserved(char c) { return Array.IndexOf(reserved, c) >= 0; }

    public static string IfNull(this string str, string other) {
      if (String.IsNullOrWhiteSpace(str)) {
        if (String.IsNullOrWhiteSpace(other))
          throw new ArgumentException("IfNull 'other' must be non-empty.");
        return other;
      }
      return str;
    }
    public static string Standardize(this string str) { return str.Trim().ToUpperInvariant(); }
    public static string ToPascalCase(this string str, string separator = null) {

      var sb = new StringBuilder();
      var sum = 0;

      foreach (var c in str) {
        sb.Append(Char.IsLetterOrDigit(c) ? Char.ToLower(c) : ' ');
        sum += (int)c;
      }

      if (separator != null)
        sb.Append(' ').Append(separator).Append(sum.ToString());

      var tf = System.Globalization.CultureInfo.GetCultureInfo("en-us").TextInfo;
      var pcs = tf.ToTitleCase(sb.ToString()).Replace(" ", String.Empty);

      return pcs;

    }
    public static string ValidateChars(this string str) {
      if (str.Any(c => Array.IndexOf(illegals, c) >= 0))
        throw new ArgumentException($"String '{str}' contains illegal characters.");
      return str;
    }

  }
}
