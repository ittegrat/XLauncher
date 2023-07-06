using System;
using System.Collections;
using System.Configuration;
using System.IO;

namespace XLauncher.Setup
{

  using Common;

  internal class Configuration
  {

    readonly Hashtable config;

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Configuration Instance { get; } = new Configuration();

    public bool CleanInstall { get; }
    public bool CreateLinks { get; }
    public bool DesktopLink { get; }
    public bool QuietInstall { get; }
    public TimeSpan WaitTimeOut { get; }
    public TimeSpan WaitSleep { get; }

    public string AppFilename { get; }
    public string DistributionFolder { get; }
    public string LinkDescription { get; }
    public string LinkFilename { get; }

    public string InstallFolder { get; set; }

    string appPath = null;
    string AppPath => appPath ?? (appPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));

    Configuration() {

      if ((config = ConfigurationManager.GetSection(Strings.SETUP_CONFIG_SECTION) as Hashtable) == null)
        throw new ConfigurationErrorsException($"The configuration section '{Strings.SETUP_CONFIG_SECTION}' does not exist.");

      CleanInstall = GetValue("setup.clean", true);
      CreateLinks = GetValue("link.create", true);
      DesktopLink = GetValue("link.desktop", true);
      QuietInstall = GetValue("setup.quiet", false);
      WaitTimeOut = TimeSpan.FromSeconds(GetValue<double>("setup.wait.timeout", 15));
      WaitSleep = TimeSpan.FromSeconds(GetValue<double>("setup.wait.shutdown", 1));

      AppFilename = GetValue("app.filename", $"{Strings.APP_NAME}.exe");
      LinkDescription = GetValue("link.description", $"Start {Strings.APP_NAME}");
      LinkFilename = GetValue("link.filename", $"{Strings.APP_NAME}.lnk");

      DistributionFolder = GetDistributionFolder(GetValue("distribution.folder", Strings.APP_NAME));
      InstallFolder = GetInstallFolder(
        GetValue("install.root", "ApplicationData"),
        GetValue("install.folder", Strings.APP_NAME)
      );

    }

    string GetDistributionFolder(string path) {
      if (Path.IsPathRooted(path)) {
        return path;
      } else {
        return Path.GetFullPath(
          Path.Combine(
            Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
            path
          )
        );
      }
    }
    string GetInstallFolder(string root, string folder) {
      if (Path.IsPathRooted(folder)) {
        return folder;
      } else if (Path.IsPathRooted(root)) {
        return Path.GetFullPath(Path.Combine(root, folder));
      } else {
        root = Environment.GetFolderPath(
          (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), root, true)
        );
        return Path.GetFullPath(Path.Combine(root, folder));
      }
    }
    T GetValue<T>(string key, T @default) {
      return config.ContainsKey(key) ? (T)Convert.ChangeType(config[key], typeof(T)) : @default;
    }

  }
}
