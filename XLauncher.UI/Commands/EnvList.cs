using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class MainView
  {

    public bool IsLocalEnv() {
      return (EnvList.SelectedItem is Environment env) && env.IsLocal;
    }

    ICommand cmdEnvClone;
    public ICommand CmdEnvClone => cmdEnvClone ?? (cmdEnvClone = new Command(nameof(CmdEnvClone), this, ExecEnvClone));
    void ExecEnvClone() {

      if (!(EnvList.SelectedItem is Environment senv))
        return;

      var name = senv.Name;
      if (Environments.Any(e => e.IsLocal && (e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))) {

        var ibox = new InputBox("New environment name:", "Clone Environment", name + " - Copy");

        ibox.Closing += (sender, args) => {

          var ib = (InputBox)sender;

          if (ib.DialogResult != true)
            return;

          name = ib.Text.Trim();
          if (Environments.Any(e => e.IsLocal && (e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))) {
            MessageBox.Show(
              $"Environment '{name}' already exists.\nPlease choose a different name.",
              Strings.APP_NAME,
              MessageBoxButton.OK,
              MessageBoxImage.Error
            );
            args.Cancel = true;
          };

        };

        if (ibox.ShowDialog() != true)
          return;

      }

      var nenv = senv.Clone(name);
      nenv.Dirty = true;
      nenv.Save();

      Environments.Add(nenv);
      EnvList.SelectedItem = Environments.First(e => e.Id == nenv.Id);

    }

    ICommand cmdEnvDelete;
    public ICommand CmdEnvDelete => cmdEnvDelete ?? (cmdEnvDelete = new Command(nameof(CmdEnvDelete), this, ExecEnvDelete, IsLocalEnv));
    void ExecEnvDelete() {

      if (!(EnvList.SelectedItem is Environment env))
        return;

      if (Configuration.Instance.LocalSettings.ConfirmDelete) {
        if (MessageBoxResult.Yes != MessageBox.Show(
          $"Are you sure you want to delete environment '{env.Name}'?",
          Strings.APP_NAME,
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        )) return;
      }

      var idx = EnvList.SelectedIndex;
      env.Delete();
      Environments.Remove(env);
      EnvList.SelectedIndex = Math.Min(idx, Environments.Count - 1); ;

    }

    ICommand cmdEnvExport;
    public ICommand CmdEnvExport => cmdEnvExport ?? (cmdEnvExport = new Command(nameof(CmdEnvExport), this, ExecEnvExport, IsLocalEnv));
    void ExecEnvExport() {
      NotYet();
    }

    ICommand cmdEnvImport;
    public ICommand CmdEnvImport => cmdEnvImport ?? (cmdEnvImport = new Command(nameof(CmdEnvImport), this, ExecEnvImport));
    void ExecEnvImport() { NotYet(); }

    ICommand cmdEnvReload;
    public ICommand CmdEnvReload => cmdEnvReload ?? (cmdEnvReload = new Command(nameof(CmdEnvReload), this, ExecEnvReload));
    void ExecEnvReload() {

      LoadEnvironments();

      if (!(EnvList.Background is SolidColorBrush scb))
        return;

      if (EnvList.Background.IsFrozen || EnvList.Background.IsSealed)
        EnvList.Background = new SolidColorBrush(scb.Color);

      var ca = new ColorAnimation(
        Configuration.Instance.FlashColor,
        scb.Color,
        Configuration.Instance.FlashSpan
      );

      EnvList.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

    }

    ICommand cmdEnvReset;
    public ICommand CmdEnvReset => cmdEnvReset ?? (cmdEnvReset = new Command(nameof(CmdEnvReset), this, ExecEnvReset, CanExecEnvReset));
    bool CanExecEnvReset() {
      if (!(EnvList.SelectedItem is Environment env))
        return false;
      return (!env.IsLocal) && env.HasPreferences;
    }
    void ExecEnvReset() {
      if (!(EnvList.SelectedItem is Environment env))
        return;
      env.Reset();
      LoadEnvironments();
    }

    ICommand cmdEnvRename;
    public ICommand CmdEnvRename => cmdEnvRename ?? (cmdEnvRename = new Command(nameof(CmdEnvRename), this, ExecEnvRename, IsLocalEnv));
    void ExecEnvRename() {

      if (!(EnvList.SelectedItem is Environment env))
        return;

      var name = env.Name;
      var ibox = new InputBox("New environment name:", "Rename Environment", name);

      ibox.Closing += (sender, args) => {

        var ib = sender as InputBox;

        if (ib.DialogResult != true)
          return;

        name = ib.Text.Trim();
        if (Environments.Any(ne => ne.IsLocal && (ne.Name == name))) {
          MessageBox.Show(
            $"Environment '{name}' already exists.",
            Strings.APP_NAME,
            MessageBoxButton.OK,
            MessageBoxImage.Error
          );
          args.Cancel = true;
        };

      };

      if (ibox.ShowDialog() != true)
        return;

      env.Rename(name);

    }

    ICommand cmdEnvSaveSession;
    public ICommand CmdEnvSaveSession => cmdEnvSaveSession ?? (cmdEnvSaveSession = new Command(nameof(CmdEnvSaveSession), this, ExecEnvSaveSession));
    void ExecEnvSaveSession() {
      ExecLaunch(true);
    }

    ICommand cmdEnvOpenFolder;
    public ICommand CmdEnvOpenFolder => cmdEnvOpenFolder ?? (cmdEnvOpenFolder = new Command(nameof(CmdEnvOpenFolder), this, ExecEnvOpenFolder));
    void ExecEnvOpenFolder() {
      if (!(EnvList.SelectedItem is Environment env))
        return;
      System.Diagnostics.Process.Start(env.Folder);
    }

    static void NotYet() {
      MessageBox.Show("Not yet implemented.", "XLauncher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

  }
}
