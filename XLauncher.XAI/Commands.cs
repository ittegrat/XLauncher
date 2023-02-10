using System;
using System.Linq;
using ExcelDna.Integration;

namespace XLauncher.XAI
{

  [ExcelCommand(Prefix = "XLauncher.Log.")]
  public static class LogDisplayCommands
  {

    readonly static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static void Clear() {
      ExcelDna.Logging.LogDisplay.Clear();
    }
    public static void Hide() {
      ExcelDna.Logging.LogDisplay.Hide();
    }
    public static void Show() {
      ExcelDna.Logging.LogDisplay.Show();
    }

    public static void SetLogLevels(string Level, string RuleName) {

      try {

        var ruleNames = new string[] { "UI", "File" };

        var level = NLog.LogLevel.FromString(Level.Trim());

        RuleName = RuleName?.Trim();
        if (String.IsNullOrEmpty(RuleName)) {
          foreach (var rule in ruleNames.Select(rn => NLog.LogManager.Configuration.FindRuleByName(rn))) {
            rule.SetLoggingLevels(level, NLog.LogLevel.Fatal);
          }
        } else {
          var rule = NLog.LogManager.Configuration.FindRuleByName(RuleName);
          if (rule == null) {
            logger.Error($"RuleName '{RuleName}' not found.");
            return;
          }
          rule.SetLoggingLevels(level, NLog.LogLevel.Fatal);
        }

        NLog.LogManager.ReconfigExistingLoggers();

      }
      catch (Exception ex) {
        logger.Error(ex, $"Can't set LogLevel '{Level}' for rule '{RuleName}'");
      }

    }
    public static void ResetLogLevels() {
      NLog.LogManager.Configuration = NLog.LogManager.Configuration.Reload();
      NLog.LogManager.ReconfigExistingLoggers();
    }

  }

}
