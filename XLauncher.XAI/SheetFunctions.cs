using System;
using System.Linq;
using System.Reflection;

using ExcelDna.Integration;

namespace XLauncher.XAI
{

  [ExcelFunction(Prefix = "XLauncher.")]
  public static class SheetFunctions
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

        if (pars.Length == 0)
          return ExcelError.ExcelErrorNA;

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
    public static object GetSessionTitle(object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      if (String.IsNullOrWhiteSpace(Addin.Session.Title))
        return String.Empty;
      else
        return Addin.Session.Title.Trim();

    }
    public static object GetVersion(object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      if (
        Attribute.GetCustomAttribute(typeof(SheetFunctions).Assembly, typeof(AssemblyInformationalVersionAttribute))
        is AssemblyInformationalVersionAttribute va
      ) return va.InformationalVersion;

      return typeof(SheetFunctions).Assembly.GetName().Version.ToString();

    }

    public static object AddParam(string Framework, string ParamName, object Value) {
      return SetParam(true, Framework, ParamName, Value);
    }
    public static object SetParam(string Framework, string ParamName, object Value) {
      return SetParam(false, Framework, ParamName, Value);
    }

    static object SetParam(bool isNew, string framework, string paramName, object value) {

      framework = framework?.Trim();
      if (String.IsNullOrEmpty(framework))
        return $"#ERROR: parameter 'Framework' is empty.";

      paramName = paramName?.Trim();
      if (String.IsNullOrEmpty(paramName))
        return $"#ERROR: parameter 'ParamName' is empty.";

      string ToDateStr(double d) {
        if (d < 0)
          throw new ArgumentException("Not a legal OleAut date.");
        return DateTime.FromOADate(d).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
      }

      try {

        if (isNew) {

          var ctx = Addin.Session.Contexts.FirstOrDefault(c => c.Name.Equals(framework, StringComparison.OrdinalIgnoreCase));

          if (ctx != null && ctx.Params.Any(p => p.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase)))
            return $"#ERROR: parameter '{paramName}' already exists.";

          var param = new Entities.Session.Param { Name = paramName };

          if (value is bool) {
            param.Value = value.ToString();
            param.Type = Entities.Session.ParamType.Boolean;
          } else if (value is string str) {
            param.Value = str;
            param.Type = Entities.Session.ParamType.String;
          } else if (value is double dbl) {
            try {
              param.Value = ToDateStr(dbl);
              param.Type = Entities.Session.ParamType.DateTime;
            }
            catch (ArgumentException) {
              return $"#ERROR: value '{value}' is not a valid date.";
            }
          } else {
            return $"#ERROR: type '{value.GetType().Name}' is not supported.";
          }

          if (ctx == null) {
            ctx = new Entities.Session.Context { Name = framework };
            ctx.Params = Array.Empty<Entities.Session.Param>();
            Addin.Session.Contexts = Addin.Session.Contexts.Append(ctx).ToArray();
          }

          ctx.Params = ctx.Params.Append(param).ToArray();

        } else {

          try {

            var ctx = Addin.Session.Contexts.First(c => c.Name.Equals(framework, StringComparison.OrdinalIgnoreCase));

            try {

              var param = ctx.Params.First(p => p.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));

              if (param.Type == Entities.Session.ParamType.Boolean && value is bool) {
                param.Value = value.ToString();
              } else if (param.Type == Entities.Session.ParamType.String && value is string str) {
                param.Value = str;
              } else if (param.Type == Entities.Session.ParamType.DateTime && value is double dbl) {
                try {
                  param.Value = ToDateStr(dbl);
                }
                catch (ArgumentException) {
                  return $"#ERROR: value '{value}' is not a valid date.";
                }
              } else {
                return $"#ERROR: value '{value}' is not a '{param.TypeCode}'.";
              }

            }
            catch (InvalidOperationException) {
              return $"#ERROR: parameter '{paramName}' not found.";
            }

          }
          catch (InvalidOperationException) {
            return $"#ERROR: framework '{framework}' not found.";
          }

        }

        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot set parameter '{framework}::{paramName}' to value '{value}'");
        return $"#ERROR: {ex.Message}";
      }

    }

  }
}
