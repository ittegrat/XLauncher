using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class MainView
  {

    const string USAGE_LOGGER = "UsageLogger";
    static NLog.Logger uLogger = NLog.LogManager.GetLogger(USAGE_LOGGER);

    ICommand cmdLaunch;
    public ICommand CmdLaunch { get { return cmdLaunch ?? (cmdLaunch = new Command(this, ExecLaunch, CanLaunch)); } }
    bool CanLaunch() {

      if (!File.Exists(Configuration.Instance.LocalSettings.ExcelPath)) {
        logger.Error($"The Excel path '{Configuration.Instance.LocalSettings.ExcelPath}' is invalid.");
        return false;
      }

      if (!File.Exists(Configuration.Instance.XaiPath)) {
        logger.Error($"The Addin path '{Configuration.Instance.XaiPath}' is invalid.");
        return false;
      }

      return true;

    }
    void ExecLaunch() {

      if (!(EnvList.SelectedItem is Environment env))
        return;

      env.Save();

      if (!Directory.Exists(Configuration.Instance.LocalTempFolder))
        Directory.CreateDirectory(Configuration.Instance.LocalTempFolder);

      var sf = Path.Combine(
        Configuration.Instance.LocalTempFolder,
        $"{Strings.XLSESSION_BASENAME}_{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xml"
      );

      env.SaveSession(sf);

      var startInfo = new ProcessStartInfo {
        FileName = Configuration.Instance.LocalSettings.ExcelPath,
        Arguments = $"{Configuration.Instance.XArgs} {Configuration.Instance.XaiPath}",
        UseShellExecute = false
      };

      startInfo.EnvironmentVariables.Add(Strings.XLSESSION_EVAR, sf);

      foreach (var ev in env.EVars)
        startInfo.EnvironmentVariables[ev.Name] = System.Environment.ExpandEnvironmentVariables(ev.Value);

      var proc = Process.Start(startInfo);

      uLogger.Info("{local}{group}{name}{arch}",
        env.IsLocal,
        env.Group,
        env.Name,
        Configuration.Instance.LocalSettings.ExcelArch
      );

    }

  }

}
