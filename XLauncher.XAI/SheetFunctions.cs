using System;
using System.Linq;

using ExcelDna.Integration;

namespace XLauncher.XAI
{

  [ExcelFunction(Prefix = "XLauncher.")]
  public static class XLauncherFunctions
  {

    readonly static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static object GetParam(string Framework, string ParamName, object DefaultValue, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      Framework = Framework?.Trim();
      if (String.IsNullOrEmpty(Framework))
        return $"#ERROR: parameter 'Framework' is empty.";

      ParamName = ParamName?.Trim();
      if (String.IsNullOrEmpty(ParamName))
        return $"#ERROR: parameter 'ParamName' is empty.";

      try {

        var ctx = Addin.Session.Contexts
          .Where(c => c.Name.Equals(Framework, StringComparison.OrdinalIgnoreCase))
          .FirstOrDefault()
        ;

        if (ctx != null) {

          var param = ctx.Params
            .Where(p => p.Name.Equals(ParamName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault()
          ;

          if (param != null) {
            return Convert.ChangeType(param.Value, param.TypeCode);
          } else {
            logger.Debug($"Parameter '{ParamName}' not found.");
          }

        } else {
          logger.Debug($"Framework '{Framework}' not found.");
        }

        return (DefaultValue is ExcelMissing || DefaultValue is ExcelEmpty) ? ExcelError.ExcelErrorNA : DefaultValue;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot find parameter '{Framework}::{ParamName}'");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object GetParamTable(object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        var pars = Addin.Session.Contexts
          .SelectMany(f => f.Params.Select(p => (f.Name, p)))
          .ToArray();

        var ans = new object[pars.Length, 4];
        for (var i = 0; i < pars.Length; ++i) {
          var (f, p) = pars[i];
          ans[i, 0] = f;
          ans[i, 1] = p.Name;
          ans[i, 2] = p.Type.ToString();
          try {
            ans[i, 3] = Convert.ChangeType(p.Value, p.TypeCode);
          }
          catch {
            ans[i, 3] = $"#CONVERT: {p.Value}";
          }
        }

        return ans;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot build parameter table");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object GetVersion(object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      return typeof(XLauncherFunctions).Assembly.GetName().Version.ToString();

    }

  }
}
