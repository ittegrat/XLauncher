using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace XLauncher.UI
{

  using Common;
  using Entities.Authorization;
  using Entities.Environments;

  public class Configuration
  {

    readonly Hashtable config;

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Configuration Instance { get; } = new Configuration();

    // *****   APP CONFIG   *****
    public string SetupFilename { get; }
    public string VersionFile { get; }
    public TimeSpan WaitTimeOut { get; }
    public IEnumerable<string> UpdateRoots { get; }

    public TimeSpan AutoCloseTime { get; }
    public TimeSpan AutoReloadTime { get; }

    public string LocalSettingsFolder { get; }
    public string LocalSettingsFilename { get; }

    public string LocalTempFolder { get; }
    public IEnumerable<string> LocalTempPrefixes { get; }
    public TimeSpan LocalTempKeepDays { get; }

    public string XaiPath => GetAddinFilename();
    public string XArgs { get; }

    public IEnumerable<string> PublicRoots => sharedRoots.Union(LocalSettings.RootedLocalRoots);
    public string UserRoot { get; }

    public AuthType DefaultAuth { get; }
    public string GroupNameLocal { get; }
    public string GroupNamePublic { get; }
    public TimeSpan FlashSpan { get; }
    public Color FlashColor { get; }
    public Brush RWColor { get; }
    public Brush X86Color { get; }
    public Brush X64Color { get; }

    // *****   LOCAL CONFIG   *****
    public LocalSettings LocalSettings { get; }

    IEnumerable<string> sharedRoots;
    string xai = null;

    public void SaveSettings() {
      LocalSettings.Save(LocalSettingsFilename);
    }

    Configuration() {

      if ((config = ConfigurationManager.GetSection(Strings.CONFIG_SECTION) as Hashtable) == null)
        throw new ConfigurationErrorsException($"The configuration section '{Strings.CONFIG_SECTION}' does not exist.");

      Environment.UseEntityCache = GetValue("entity.usecache", false);

      SetupFilename = GetValue("setup.filename", "XLauncher.Setup.exe");
      VersionFile = GetValue("setup.versionfile", "version.txt");
      WaitTimeOut = TimeSpan.FromSeconds(GetValue("setup.waittimeout", 15));
      UpdateRoots = config.Keys
        .Cast<string>()
        .Where(s => s.StartsWith("setup.root."))
        .Select(s => (Str: s.Trim(), Idx: int.Parse(s.Substring(1 + s.LastIndexOf('.')))))
        .OrderBy(p => p.Idx)
        .Select(p => {
          var s = (string)config[p.Str];
          return String.IsNullOrWhiteSpace(s) ? null : App.GetRootedPath(s);
        })
        .Where(s => s != null)
        .Distinct()
      ;

      AutoReloadTime = GetTimerTime(GetValue("timer.autoreload", "-01:00"));
      AutoCloseTime = GetTimerTime(GetValue("timer.autoclose", "00:00"));

      LocalSettingsFolder = App.GetRootedPath(GetValue("local.settings.folder", "Settings"));
      LocalSettingsFilename = Path.Combine(LocalSettingsFolder, GetValue("local.settings.filename", "Settings.xml"));
      TryCreateDirectory(LocalSettingsFolder);

      LocalTempFolder = GetLocalTempFolder(GetValue("local.temp.folder", Strings.APP_NAME));
      LocalTempKeepDays = TimeSpan.FromDays(GetValue("local.tempfile.keepdays", 7));
      LocalTempPrefixes = config.Keys
        .Cast<string>()
        .Where(s => s.StartsWith("local.tempfile.pfx."))
        .Select(s => s.Trim())
        .Distinct()
      ;

      XArgs = GetValue("excel.args", "/x /r");

      sharedRoots = config.Keys
        .Cast<string>()
        .Where(s => s.StartsWith("public.root."))
        .Select(s => (Str: s.Trim(), Idx: int.Parse(s.Substring(1 + s.LastIndexOf('.')))))
        .OrderBy(p => p.Idx)
        .Select(p => {
          var s = (string)config[p.Str];
          return String.IsNullOrWhiteSpace(s) ? null : App.GetRootedPath(s);
        })
        .Where(s => s != null)
        .Distinct()
      ;
      UserRoot = App.GetRootedPath(GetValue("user.root", "Environments"));
      TryCreateDirectory(UserRoot);

      DefaultAuth = (AuthType)Enum.Parse(typeof(AuthType), GetValue("ui.default.auth", "deny"), true);
      GroupNameLocal = GetValue("ui.groupname.local", "User");
      GroupNamePublic = GetValue("ui.groupname.public", "Public");
      FlashSpan = TimeSpan.FromMilliseconds(GetValue("ui.flash.span", 750));
      FlashColor = (Color)ColorConverter.ConvertFromString(GetValue("ui.color.flash", "#104080"));
      RWColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetValue("ui.color.rw", "#800000")));
      X86Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetValue("ui.color.x86", "#0000BB")));
      X64Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetValue("ui.color.x64", "#006000")));

      LocalSettings = LocalSettings.Load(LocalSettingsFilename);

      LocalSettings.RestoreTab = GetValue("ui.restore.tab", true);
      LocalSettings.ShowToolTips = GetValue("ui.showtooltips", false);

    }

    string GetAddinFilename() {

      var path = xai ?? (xai = App.GetRootedPath(GetValue("excel.addin", Strings.APP_NAME)));

      if (!Path.GetExtension(path).Equals(".XLL", StringComparison.OrdinalIgnoreCase)) {
        if (LocalSettings.ExcelArch == ArchType.x86)
          path += "32.xll";
        else
          path += "64.xll";
      }

      return Path.GetFullPath(path);

    }
    string GetLocalTempFolder(string folder) {

      var path = Path.IsPathRooted(folder)
        ? folder
        : Path.Combine(Path.GetTempPath(), folder)
      ;

      return Path.GetFullPath(path);

    }
    TimeSpan GetTimerTime(string texpr) {
      try {
        var ss = texpr.Split(':');
        return new TimeSpan(int.Parse(ss[0]), int.Parse(ss[1]), 0);
      }
      catch (Exception ex) {
        logger.Error(ex, $"Can't parse time '{texpr}'");
        return new TimeSpan(-1);
      }
    }
    T GetValue<T>(string key, T @default) {
      return config.ContainsKey(key) ? (T)Convert.ChangeType(config[key], typeof(T)) : @default;
    }
    void TryCreateDirectory(string path) {
      try {
        Directory.CreateDirectory(path);
      }
      catch (Exception ex) {
        logger.Error(ex, $"Can't create directory '{path}'");
      }
    }

  }
}
