using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace XLauncher.UI
{

  using Common;

  public partial class App : Application
  {

    const int SW_RESTORE = 9;

    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr handle, int nCmdShow);
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsIconic(IntPtr handle);

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static readonly Configuration config = Configuration.Instance;
    static readonly DispatcherTimer autoClose = new DispatcherTimer();
    static readonly DispatcherTimer autoReload = new DispatcherTimer();

    static Mutex singleInstance;
    static string appPath = null;

    public static ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

    public static string Domain { get; } = Environment.UserDomainName?.Trim().ToUpperInvariant();
    public static string Machine { get; } = Environment.MachineName?.Trim().ToUpperInvariant();
    public static string User { get; } = Environment.UserName?.Trim().ToUpperInvariant();
    public static Version Version { get; } = typeof(App).Assembly.GetName().Version;

    public static string GetRootedPath(string path) {
      appPath = appPath ??
        (appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
      ;
      return Path.IsPathRooted(path)
        ? path
        : Path.GetFullPath(Path.Combine(appPath, path))
      ;
    }
    public static void LogMessage(string message) {
      Logs.Add(message);
    }

    void OnStartup(object sender, StartupEventArgs e) {
      try {

        var check = true;
        singleInstance = new Mutex(true, Strings.MTX_APPLICATION, out bool granted);
        if (!granted) {

          if (EventWaitHandle.TryOpenExisting(Strings.MTX_UPDATER, out var ewh)) {
            ewh.Set();
            if (!singleInstance.WaitOne(config.WaitTimeOut))
              throw new WaitHandleCannotBeOpenedException("Single instance mutex cannot be acquired.");
            check = false;
          } else {
            GiveUp();
            return;
          }

        }

        logger.Info("Single instance mutex acquired.");

        if (check && NeedsUpdate(true, out string updaterPath)) {

          logger.Info("Starting update process.");
          logger.Debug($"Updater process is '{updaterPath}'.");

          try {

            var ewh = new EventWaitHandle(false, EventResetMode.ManualReset, Strings.MTX_UPDATER);

            var startInfo = new ProcessStartInfo {
              FileName = updaterPath,
              Arguments = $"{Strings.UPDATER_ARGS} {Version}",
              WindowStyle = ProcessWindowStyle.Normal
            };
            using (var proc = Process.Start(startInfo)) {

              // Wait for setup process to initialize.
              if (!ewh.WaitOne(config.WaitTimeOut))
                throw new WaitHandleCannotBeOpenedException("Updater process not responding.");

              singleInstance.ReleaseMutex();

              Shutdown();

            }
          }
          catch (Exception ex) {
            logger.Error(ex, "Updater process failed");
          }

        }

        logger.Info($"Cleaning temp folder '{config.LocalTempFolder}'.");
        DeleteTempFiles();

        if (config.AutoReloadTime >= TimeSpan.Zero) {
          autoReload.Tick += SetAutoReload;
          SetAutoReload(autoReload, null);
        }

        logger.Info("Starting the UI.");

      }
      catch (Exception ex) {
        logger.Fatal(ex, "Startup error");
        MessageBox.Show(
          $"Startup error:\n{ex.Message}",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        Shutdown();
      }
    }
    void OnExit(object sender, ExitEventArgs e) {
      config.SaveSettings();
      NLog.LogManager.Shutdown();
    }

    void DeleteTempFiles() {
      try {

        var cutoff = DateTime.Today - config.LocalTempKeepDays;
        var pfxes = config.LocalTempPrefixes.ToArray();

        foreach (var fn in Directory.EnumerateFiles(config.LocalTempFolder)) {

          foreach (var pfx in pfxes) {

            if (fn.StartsWith(pfx) && File.GetLastWriteTime(fn) < cutoff) {
              try {
                File.Delete(fn);
              }
              catch (Exception ex) {
                logger.Error(ex, $"Can't delete file '{fn}'");
              }
            }

          }

        }

      }
      catch (Exception ex) {
        logger.Error(ex, "Can't delete temporary files");
      }

    }
    void GiveUp() {

      logger.Info("Another instance detected. Begin shutting down.");

      var proc = Process.GetCurrentProcess();
      var other = Process.GetProcessesByName(proc.ProcessName).Where(p => p.Id != proc.Id).First();

      var handle = other.MainWindowHandle;
      if (IsIconic(handle)) {
        ShowWindow(handle, SW_RESTORE);
      }
      SetForegroundWindow(handle);

      Shutdown();

    }
    bool NeedsUpdate(bool interactive, out string updaterPath) {

      logger.Trace("Checking updates.");
      logger.Trace($"Check is{(interactive ? String.Empty : " not")} interactive.");

      updaterPath = null;

      try {

        foreach (var root in config.UpdateRoots) {
          var vfile = Path.Combine(root, config.VersionFile);
          if (!File.Exists(vfile)) continue;
          var other = Version.Parse(
            File.ReadAllText(vfile)
          );
          logger.Debug($"This version: {Version}");
          logger.Debug($"Other version: {other}");
          if (other > Version) {
            if (
              interactive &&
              Process.GetProcessesByName("EXCEL").Count() > 0 &&
              MessageBoxResult.OK != MessageBox.Show(
                $"A new version of the {Strings.APP_NAME} is available.\n" +
                "Please close all your EXCEL instances and press the OK button.",
                Strings.APP_NAME,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning
            )) {
              logger.Info($"Updating cancelled by the user.");
              return false;
            }
            logger.Info($"Updating to version '{other}'.");
            updaterPath = Path.Combine(root, config.SetupFilename);
            return true;
          }
        }

      }
      catch (Exception ex) {
        logger.Warn(ex, "Version cannot be checked");
      }

      logger.Info("No updates available.");
      return false;

    }
    void SetAutoReload(object sender, EventArgs ea) {
      autoReload.Stop();
      try {
        if (autoReload.Tag != null) {
          logger.Info("Starting daily maintenance.");
          if (NeedsUpdate(false, out string _)) {
            autoClose.Interval = NextInterval(config.AutoCloseTime);
            logger.Info($"Setting AutoClose timer at '{DateTime.Now + autoClose.Interval}'.");
            autoClose.Tick += (s, e) => {
              logger.Info($"AutoClosing {Strings.APP_NAME}.");
              MainWindow.Close();
            };
            autoClose.Start();
            return;
          }
          var cmd = ((MainView)MainWindow).CmdEnvReload;
          if (cmd.CanExecute(null)) {
            cmd.Execute(new Object());
            logger.Info("Environments reloaded.");
          }
        } else {
          autoReload.Tag = new Object();
        }
      }
      catch (Exception ex) {
        logger.Error(ex, "AutoReload EventHandler error");
      }
      autoReload.Interval = NextInterval(config.AutoReloadTime);
      logger.Info($"Setting AutoReload timer at '{DateTime.Now + autoReload.Interval}'.");
      autoReload.Start();
    }

    TimeSpan NextInterval(TimeSpan time) {

      var now = DateTime.Now;

      var dt = now.Date;
      if (now.TimeOfDay >= time)
        dt = dt.AddDays(1);
      dt += time;

      return dt - now;

    }

  }
}
