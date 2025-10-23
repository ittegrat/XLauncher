using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using NLog;

namespace XLauncher.UI
{
  public partial class MainView
  {

    void OnSelectUiLoglevel(object sender, SelectionChangedEventArgs e) {

      if (e.AddedItems.Count != 1)
        return;

      var minLevel = (LogLevel)e.AddedItems[0];

      foreach (
        var rule in LogManager.Configuration.LoggingRules.Where(r => r.RuleName != USAGE_LOGGER)
      ) {
        rule.SetLoggingLevels(minLevel, LogLevel.Fatal);
      }

      LogManager.ReconfigExistingLoggers();

    }

    ICommand cmdLogClearView;
    public ICommand CmdLogClearView { get { return cmdLogClearView ?? (cmdLogClearView = new Command(nameof(CmdLogClearView), this, ExecLogClearView)); } }
    void ExecLogClearView() {
      App.Logs.Clear();
    }

    ICommand cmdLogCopyAllLines;
    public ICommand CmdLogCopyAllLines { get { return cmdLogCopyAllLines ?? (cmdLogCopyAllLines = new Command(nameof(CmdLogCopyAllLines), this, ExecLogCopyAllLines, CanExecLogCopyAllLines)); } }
    bool CanExecLogCopyAllLines() {
      return App.Logs.Count > 0;
    }
    void ExecLogCopyAllLines() {
      var sb = new System.Text.StringBuilder();
      foreach (var line in App.Logs) {
        sb.AppendLine(line);
      }
      Clipboard.SetText(sb.ToString());
    }

    ICommand cmdLogCopyLine;
    public ICommand CmdLogCopyLine { get { return cmdLogCopyLine ?? (cmdLogCopyLine = new Command(nameof(CmdLogCopyLine), this, ExecLogCopyLine, CanExecLogCopyLine)); } }
    bool CanExecLogCopyLine() {
      return LogView.SelectedIndex >= 0;
    }
    void ExecLogCopyLine() {
      var line = (string)LogView.SelectedItem;
      Clipboard.SetText(line);
    }

    ICommand cmdLogOpenFolder;
    public ICommand CmdLogOpenFolder { get { return cmdLogOpenFolder ?? (cmdLogOpenFolder = new Command(nameof(CmdLogOpenFolder), this, ExecLogOpenFolder)); } }
    void ExecLogOpenFolder() {
      System.Diagnostics.Process.Start(Configuration.Instance.LocalTempFolder);
    }

    ICommand cmdLogResetLevels;
    public ICommand CmdLogResetLevels { get { return cmdLogResetLevels ?? (cmdLogResetLevels = new Command(nameof(CmdLogResetLevels), this, ExecLogResetLevels)); } }
    void ExecLogResetLevels() {
      UiLoglevel.SelectedIndex = -1;
      XaiLoglevel.SelectedItem = Configuration.Instance.XaiLoglevel;
      LogManager.Configuration = LogManager.Configuration.Reload();
      LogManager.ReconfigExistingLoggers();
    }

  }
}
