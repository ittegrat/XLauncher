using System.Linq;
using System.Windows.Input;

using NLog;

namespace XLauncher.UI
{
  public partial class MainView
  {

    ICommand cmdClearLogs;
    public ICommand CmdClearLogs { get { return cmdClearLogs ?? (cmdClearLogs = new Command(this, ExecClearLogs)); } }
    void ExecClearLogs() {
      App.Logs.Clear();
    }

    ICommand cmdOpenLogFolder;
    public ICommand CmdOpenLogFolder { get { return cmdOpenLogFolder ?? (cmdOpenLogFolder = new Command(this, ExecOpenLogFolder)); } }
    void ExecOpenLogFolder() {
      System.Diagnostics.Process.Start(Configuration.Instance.LocalTempFolder);
    }

    ICommand cmdResetLogLevel;
    public ICommand CmdResetLogLevel { get { return cmdResetLogLevel ?? (cmdResetLogLevel = new Command(this, ExecResetLogLevel)); } }
    void ExecResetLogLevel() {
      LogManager.Configuration = LogManager.Configuration.Reload();
      LogManager.ReconfigExistingLoggers();
    }

    ICommand cmdSetDebugLogLevel;
    public ICommand CmdSetDebugLogLevel { get { return cmdSetDebugLogLevel ?? (cmdSetDebugLogLevel = new Command(this, ExecSetDebugLogLevel)); } }
    void ExecSetDebugLogLevel() {
      SetLogLevel(LogLevel.Debug);
    }

    ICommand cmdSetTraceLogLevel;
    public ICommand CmdSetTraceLogLevel { get { return cmdSetTraceLogLevel ?? (cmdSetTraceLogLevel = new Command(this, ExecSetTraceLogLevel)); } }
    void ExecSetTraceLogLevel() {
      SetLogLevel(LogLevel.Trace);
    }

    void SetLogLevel(LogLevel minLevel) {

      foreach (
        var rule in LogManager.Configuration.LoggingRules.Where(r => r.RuleName != USAGE_LOGGER)
      ) {
        rule.SetLoggingLevels(minLevel, LogLevel.Fatal);
      }

      LogManager.ReconfigExistingLoggers();

    }

  }
}
