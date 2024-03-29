using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace XLauncher.Setup
{

  using Common;

  public partial class App : Application
  {

    enum ErrCode { ERR_OK = 0, ERR_SETUP, ERR_UPDATE, ERR_ABORT, ERR_ARGS, ERR_LOCK, ERR_TIMEOUT }

    static readonly NLog.Logger uLogger = NLog.LogManager.GetLogger("UsageLogger");
    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static Mutex singleInstance;

    readonly Configuration config = Configuration.Instance;

    bool IsUpdater = false;

    void OnStartup(object sender, StartupEventArgs e) {

      logger.Info($"Current version: {typeof(App).Assembly.GetName().Version}");

      ErrCode ec;

      if (e.Args.Length > 0) {

        logger.Debug($"Command line arguments: '{String.Join(" ", e.Args)}'");

        if (e.Args[0] == Strings.UPDATER_ARGS) {
          ec = Update();
        } else {
          logger.Info($"Unknown command line arguments: '{String.Join(" ", e.Args)}'");
          ec = ErrCode.ERR_ARGS;
        }

      } else {
        ec = Setup(config.CleanInstall, config.QuietInstall);
      }

      if (ec != ErrCode.ERR_OK) {

        var xrunning = Process.GetProcessesByName("EXCEL").Count() > 0;

        logger.Error($"Error code is '{ec}'");
        if (xrunning)
          logger.Warn("Excel is running.");

        if (IsUpdater || (!config.QuietInstall)) {

          var msg = new StringBuilder(IsUpdater ? "Update" : "Setup");
          msg.Append($" failed with error '{ec}'.");
          if (ec == ErrCode.ERR_LOCK && xrunning)
            msg.Append("\nPlease close all your EXCEL instances and try again.");
          msg.Append($"\nFor additional details, please check the logfile");

          var target = NLog.LogManager.Configuration.ConfiguredNamedTargets
            .Where(t => t.Name == "applog")
            .FirstOrDefault()
            as NLog.Targets.FileTarget
          ;
          if (target?.FileName is NLog.Layouts.SimpleLayout layout) {
            msg.Append($":\n{layout.FixedText}");
          } else {
            msg.Append(".");
          }

          if (IsUpdater && ec == ErrCode.ERR_LOCK)
            msg.Append($"\n\n{Strings.APP_NAME} will be available in a few seconds.");

          MessageBox.Show(
            msg.ToString(),
            Strings.APP_NAME + " Setup",
            MessageBoxButton.OK,
            MessageBoxImage.Error
          );

        }

      }

      logger.Info("Exiting XLauncher.Setup");
      Shutdown((int)ec);

    }

    bool CreateShortcuts(bool quiet) {

      var target = Path.Combine(config.InstallFolder, config.AppFilename);
      if (!File.Exists(target))
        throw new ArgumentException($"The target file '{target}' does not exist.");

      void CreateShortcut(string linkPath) {
        logger.Debug($"Creating shortcut '{linkPath}'.");
        using (var sl = new ShellLinkObject(linkPath)) {
          sl.TargetPath = target;
          sl.Description = config.LinkDescription;
          sl.Save();
        }
      }

      var link = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
        config.LinkFilename
      );
      CreateShortcut(link);

      var dlink = false;
      link = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        config.LinkFilename
      );
      if (IsUpdater) {
        if (File.Exists(link)) {
          CreateShortcut(link);
          dlink = true;
        }
      } else {

        var ans = config.DesktopLink ? MessageBoxResult.Yes : MessageBoxResult.No;
        if (!quiet) {
          ans = MessageBox.Show(
            $"A shortcut to the {Strings.APP_NAME} application has been created inside the Start Menu.\n\n" +
            "You can pin it to the Taskbar or the Start Menu.\n\n" +
            "Create an additional shortcut on the Desktop ?",
            Strings.APP_NAME + " Setup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            ans
          );

          if (ans == MessageBoxResult.Yes) {
            CreateShortcut(link);
            dlink = true;
          }

        }
      }

      return dlink;

    }
    bool IsLocked(string fpath) {

      try {

        var finfo = new FileInfo(fpath);

        if (finfo.Exists) {
          try {
            using (var fs = finfo.Open(FileMode.Open, FileAccess.Read, FileShare.None)) { fs.Close(); }
          }
          catch (IOException iex) {
            logger.Debug(iex, "File Locked");
            return true;
          }
        }

      }
      catch (Exception ex) {
        logger.Debug(ex, "FileInfo error");
      }

      return false;

    }

    ErrCode Setup(bool clean, bool quiet) {

      logger.Info($"Operation mode is: '{(IsUpdater ? "Update" : "Setup")}'.");

      try {

        if (!IsUpdater) {
          singleInstance = new Mutex(true, Strings.MTX_APPLICATION, out bool granted);
          if (!granted)
            throw new WaitHandleCannotBeOpenedException("Single instance mutex cannot be acquired.");
          logger.Info("Single instance mutex acquired.");
        }

        var src = config.DistributionFolder;
        var dst = config.InstallFolder;

        logger.Debug($"Distribution folder: '{src}'");
        logger.Debug($"Install folder: '{dst}'");
        logger.Debug($"Quiet install: '{quiet}'");
        logger.Debug($"Clean install: '{clean}'");

        var exist = Directory.Exists(dst);
        logger.Debug($"Install folder exist: '{exist}'");

        if (!quiet) {

          var ans = MessageBox.Show(
            $"Do you want to install '{Strings.APP_NAME}' in\nfolder '{dst}'?",
            Strings.APP_NAME + " Setup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.Yes
          );

          if (ans != MessageBoxResult.Yes)
            return ErrCode.ERR_ABORT;

        }

        if (exist && (!quiet)) {

          var ans = MessageBox.Show(
            $"Install folder '{dst}' already exists.\nDelete it before continuing?",
            Strings.APP_NAME + " Setup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            clean ? MessageBoxResult.Yes : MessageBoxResult.No
          );

          if (ans == MessageBoxResult.Yes)
            clean = true;

        }

        var files = Directory.GetFiles(src)
          .ToDictionary(f => Path.Combine(dst, Path.GetFileName(f)))
        ;

        if (exist) {

          if (files.Keys.Any(f => IsLocked(f)))
            return ErrCode.ERR_LOCK;

          if (clean) {
            logger.Info("Cleaning existing installation.");
            Directory.Delete(dst, true);
          }

        }

        Directory.CreateDirectory(dst);

        foreach (var f in files) {
          logger.Debug($"Copying file '{Path.GetFileName(f.Value)}'.");
          File.Copy(f.Value, f.Key, true);
        }
        logger.Info("Files copied.");

        var dlink = false;
        if (config.CreateLinks)
          dlink = CreateShortcuts(quiet);
        logger.Info($"Shortcuts{(config.CreateLinks ? String.Empty : " not")} created.");

        uLogger.Info("{type}{version}{clean}{quiet}{dlink}{dst}",
          IsUpdater ? "Update" : "Setup",
          typeof(App).Assembly.GetName().Version.ToString(),
          clean,
          quiet,
          dlink,
          dst
        );

      }
      catch (Exception ex) {
        logger.Fatal(ex, "Setup error");
        return ErrCode.ERR_SETUP;
      }

      logger.Info($"{(IsUpdater ? "Update" : "Setup")} successful.");
      return ErrCode.ERR_OK;

    }
    ErrCode Update() {

      IsUpdater = true;

      if (Process.GetProcessesByName("EXCEL").Count() > 0)
        logger.Warn("Excel is running.");

      if (Mutex.TryOpenExisting(Strings.MTX_APPLICATION, out singleInstance)) {
        logger.Debug("Single instance mutex opened.");
        if (EventWaitHandle.TryOpenExisting(Strings.MTX_UPDATER, out var ewh)) {
          logger.Debug("Updater EventWaitHandle opened.");

          try {

            var procs = Process.GetProcessesByName("XLAUNCHER");
            if (procs.Length > 0) {
              logger.Debug("Resolve installation folder.");
              var fn = procs[0].MainModule.FileName;
              config.InstallFolder = Path.GetDirectoryName(fn);
            }

            if (ewh.Set()) {

              logger.Debug("Updater EventWaitHandle signaled.");

              if (singleInstance.WaitOne(config.WaitTimeOut)) {

                logger.Info("Single instance mutex acquired.");

                var start = DateTime.Now;
                while (true) {

                  if (Process.GetProcessesByName("XLAUNCHER").Count() > 0) {
                    Thread.Sleep(config.WaitSleep);
                  } else {
                    logger.Debug($"XLauncher shutdown wait time: {(DateTime.Now - start).TotalSeconds} s.");
                    break;
                  }

                  if (DateTime.Now - start > config.WaitTimeOut) {
                    logger.Debug($"Wait timeout after {(DateTime.Now - start).TotalSeconds} s.");
                    // throw new InvalidOperationException("Wait timeout.");
                    return ErrCode.ERR_TIMEOUT;
                  }

                }

                var ec = Setup(false, true);
                logger.Debug($"Setup error code is '{ec}'");

                if (ec == ErrCode.ERR_OK || ec == ErrCode.ERR_LOCK) {

                  var startInfo = new ProcessStartInfo {
                    FileName = Path.Combine(config.InstallFolder, config.AppFilename)
                  };

                  ewh.Reset();

                  logger.Info($"Starting process '{startInfo.FileName}'.");
                  Process.Start(startInfo);

                  ewh.WaitOne();
                  singleInstance.ReleaseMutex();
                  logger.Debug("Single instance mutex released.");

                  return ec;

                }

                throw new InvalidOperationException("Setup failure.");

              }

              logger.Error($"Single instance mutex cannot be acquired.");

            }

            logger.Error($"EventWaitHandle cannot be signaled.");

          }
          catch (Exception ex) {
            logger.Error(ex, "Update failure");
          }

        } else {
          logger.Error($"EventWaitHandle cannot be opened.");
        }
      } else {
        logger.Error("Single instance mutex cannot be opened.");
      }

      return ErrCode.ERR_UPDATE;

    }

  }

}
