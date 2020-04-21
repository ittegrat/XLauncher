using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.IO;
using System.Threading;
using System.Diagnostics;

namespace XLauncher.Setup
{

  using Common;

  public partial class App : Application
  {

    enum ErrCode { ERR_OK = 0, ERR_SETUP, ERR_UPDATE, ERR_ARGS, ERR_ABORT }

    static NLog.Logger uLogger = NLog.LogManager.GetLogger("UsageLogger");
    static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static Mutex singleInstance;

    Configuration config = Configuration.Instance;
    bool IsUpdater = false;

    void OnStartup(object sender, StartupEventArgs e) {

      ErrCode ec;

      if (e.Args.Length > 0) {

        if (e.Args[0] == Strings.UPDATER_ARGS) {
          ec = Update();
        } else {
          logger.Info($"Unknown command line arguments: '{String.Join(" ", e.Args)}");
          ec = ErrCode.ERR_ARGS;
        }

      } else {
        ec = Setup(config.CleanInstall, config.QuietInstall);
      }

      logger.Debug($"Error code is '{ec}'");
      Shutdown((int)ec);

    }

    bool CreateShortcuts(bool quiet) {

      var target = Path.Combine(config.InstallFolder, config.AppFilename);
      if (!File.Exists(target))
        throw new ArgumentException($"The target file '{target}' does not exist.");

      void CreateShortcut(string linkPath) {
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
    ErrCode Setup(bool clean, bool quiet) {

      try {

        if (!IsUpdater) {
          singleInstance = new Mutex(true, Strings.MTX_APPLICATION, out bool granted);
          if (!granted)
            throw new WaitHandleCannotBeOpenedException("Single instance mutex cannot be acquired.");
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

        if (exist && clean) {
          logger.Debug("Cleaning existing installation.");
          Directory.Delete(dst, true);
        }

        Directory.CreateDirectory(dst);

        foreach (var f in Directory.GetFiles(src)) {
          var fn = Path.GetFileName(f);
          logger.Debug($"Copying file '{fn}'.");
          File.Copy(f, Path.Combine(dst, fn), true);
        }

        var dlink = CreateShortcuts(quiet);

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

      return ErrCode.ERR_OK;

    }
    ErrCode Update() {

      IsUpdater = true;

      if (Mutex.TryOpenExisting(Strings.MTX_APPLICATION, out singleInstance)) {
        logger.Debug("Single instance mutex opened.");
        if (EventWaitHandle.TryOpenExisting(Strings.MTX_UPDATER, out var ewh)) {
          logger.Debug("Updater EventWaitHandle opened.");

          try {

            if (ewh.Set()) {

              if (singleInstance.WaitOne(config.WaitTimeOut)) {

                logger.Debug($"Sleeping {config.WaitShutdown}.");
                Thread.Sleep(config.WaitShutdown);

                if (Setup(false, true) != ErrCode.ERR_OK)
                  throw new InvalidOperationException("Setup failure.");

                var startInfo = new ProcessStartInfo {
                  FileName = Path.Combine(config.InstallFolder, config.AppFilename)
                };

                ewh.Reset();

                Process.Start(startInfo);

                ewh.WaitOne();
                singleInstance.ReleaseMutex();

                return ErrCode.ERR_OK;

              }

              logger.Error($"Single instance mutex cannot be acquired.");

            }

            logger.Error($"EventWaitHandle cannot be signaled.");

          }
          catch (Exception ex) {
            logger.Error(ex, "Update failure");
            return ErrCode.ERR_UPDATE;
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
