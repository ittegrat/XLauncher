using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class MainView
  {

    ObservableCollection<GlobalAddin> globalAddins = Configuration.Instance.LocalSettings.GlobalAddins;

    ICommand cmdGaiAdd;
    public ICommand CmdGaiAdd => cmdGaiAdd ?? (cmdGaiAdd = new Command(nameof(CmdGaiAdd), this, ExecGaiAdd));
    void ExecGaiAdd() {

      var sai = (GlobalAddin)GAList.SelectedItem;

      var ai = new GlobalAddin {
        Active = true,
        ReadOnly = true
      };

      var aibox = new AddinBox(Array.Empty<string>(), sai?.Path, true) {
        Title = "Add Global Addin",
        DataContext = ai
      };

      aibox.Closing += (sender, args) => {

        var aib = (AddinBox)sender;

        if (aib.DialogResult != true)
          return;

        string msg = null;

        var any = globalAddins.Where(a => a.FileName.Equals(ai.FileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (any != null) {
          msg = new StringBuilder("Global addin '")
            .Append(any.FileName)
            .Append("'")
            .Append(String.IsNullOrWhiteSpace(any.Key) ? String.Empty : $"\n(alias '{any.Key}')")
            .Append(" already exists.")
            .ToString()
          ;
        }

        if (msg == null) {
          any = globalAddins.Where(a => a.DisplayName.Equals(ai.DisplayName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
          if (any != null) {
            msg = new StringBuilder("Global addin alias '")
              .Append(any.DisplayName)
              .Append("'\nis already used by '")
              .Append(any.FileName)
              .Append("'.")
              .ToString()
            ;
          }
        }

        if (msg != null) {
          MessageBox.Show(
            msg,
            Strings.APP_NAME,
            MessageBoxButton.OK,
            MessageBoxImage.Error
          );
          args.Cancel = true;
        }

      };

      if (aibox.ShowDialog() != true)
        return;

      globalAddins.Add(ai);
      GAList.SelectedItem = ai;

    }

    ICommand cmdGaiCopyPath;
    public ICommand CmdGaiCopyPath => cmdGaiCopyPath ?? (cmdGaiCopyPath = new Command(nameof(CmdGaiCopyPath), this, ExecGaiCopyPath));
    void ExecGaiCopyPath() {
      if (!(GAList.SelectedItem is GlobalAddin ai))
        return;
      Clipboard.SetText(ai.Path);
    }

    ICommand cmdGaiDelete;
    public ICommand CmdGaiDelete => cmdGaiDelete ?? (cmdGaiDelete = new Command(nameof(CmdGaiDelete), this, ExecGaiDelete, () => GAList.SelectedIndex >= 0));
    void ExecGaiDelete() {

      if (!(GAList.SelectedItem is GlobalAddin ai))
        return;

      var msg = new StringBuilder("Are you sure you want to delete addin '")
        .Append(ai.DisplayName)
        .Append("'")
        .Append(String.IsNullOrWhiteSpace(ai.Key) ? String.Empty : $"\n(i.e. '{ai.FileName}')")
        .Append("?")
      ;

      if (Configuration.Instance.LocalSettings.ConfirmDelete) {
        if (MessageBoxResult.Yes != MessageBox.Show(
          msg.ToString(),
          Strings.APP_NAME,
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        )) return;
      }

      var idx = GAList.SelectedIndex;
      globalAddins.Remove(ai);
      GAList.SelectedIndex = Math.Min(idx, globalAddins.Count - 1);

    }

    ICommand cmdGaiDisableAll;
    public ICommand CmdGaiDisableAll => cmdGaiDisableAll ?? (cmdGaiDisableAll = new Command(nameof(CmdGaiDisableAll), this, ExecGaiDisableAll, () => globalAddins.Any(a => a.Active)));
    void ExecGaiDisableAll() {
      foreach (var ai in globalAddins)
        ai.Active = false;
      GAList.Items.Refresh();
    }

    ICommand cmdGaiEdit;
    public ICommand CmdGaiEdit => cmdGaiEdit ?? (cmdGaiEdit = new Command(nameof(CmdGaiEdit), this, ExecGaiEdit, () => GAList.SelectedIndex >= 0));
    void ExecGaiEdit() {

      if (!(GAList.SelectedItem is GlobalAddin sai))
        return;

      var ai = sai.Clone();

      var aibox = new AddinBox(Array.Empty<string>(), null, true) {
        Title = "Edit Global Addin",
        DataContext = ai
      };

      aibox.Closing += (sender, args) => {

        var aib = (AddinBox)sender;

        if (aib.DialogResult != true)
          return;

        string msg = null;

        var any = globalAddins.Where(a => !a.Equals(sai)).Where(a => a.FileName.Equals(ai.FileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (any != null) {
          msg = new StringBuilder("Global addin '")
            .Append(any.FileName)
            .Append("'")
            .Append(String.IsNullOrWhiteSpace(any.Key) ? String.Empty : $"\n(alias '{any.Key}')")
            .Append(" already exists.")
            .ToString()
          ;
        }

        if (msg == null) {
          any = globalAddins.Where(a => !a.Equals(sai)).Where(a => a.DisplayName.Equals(ai.DisplayName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
          if (any != null) {
            msg = new StringBuilder("Global addin alias '")
              .Append(any.DisplayName)
              .Append("'\nis already used by '")
              .Append(any.FileName)
              .Append("'.")
              .ToString()
            ;
          }
        }

        if (msg != null) {
          MessageBox.Show(
            msg,
            Strings.APP_NAME,
            MessageBoxButton.OK,
            MessageBoxImage.Error
          );
          args.Cancel = true;
        }

      };

      if (aibox.ShowDialog() != true)
        return;

      sai.Key = ai.Key;
      sai.Path = ai.Path;
      sai.ReadOnly = ai.ReadOnly;

      GAList.Items.Refresh();

    }

    ICommand cmdGaiEnableAll;
    public ICommand CmdGaiEnableAll => cmdGaiEnableAll ?? (cmdGaiEnableAll = new Command(nameof(CmdGaiEnableAll), this, ExecGaiEnableAll, () => !globalAddins.All(a => a.Active)));
    void ExecGaiEnableAll() {
      foreach (var ai in globalAddins)
        ai.Active = true;
      GAList.Items.Refresh();
    }

    ICommand cmdGaiMoveDown;
    public ICommand CmdGaiMoveDown => cmdGaiMoveDown ?? (cmdGaiMoveDown = new Command(nameof(CmdGaiMoveDown), this, ExecGaiMoveDown, CanExecGaiMoveDown));
    bool CanExecGaiMoveDown() {
      return (GAList.SelectedIndex >= 0) && (GAList.SelectedIndex < globalAddins.Count - 1);
    }
    void ExecGaiMoveDown() {

      if (!(GAList.SelectedItem is GlobalAddin ai))
        return;

      var idx = GAList.SelectedIndex;
      globalAddins.Move(idx, idx + 1);

    }

    ICommand cmdGaiMoveUp;
    public ICommand CmdGaiMoveUp => cmdGaiMoveUp ?? (cmdGaiMoveUp = new Command(nameof(CmdGaiMoveUp), this, ExecGaiMoveUp, CanExecGaiMoveUp));
    bool CanExecGaiMoveUp() {
      return (GAList.SelectedIndex > 0);
    }
    void ExecGaiMoveUp() {

      if (!(GAList.SelectedItem is GlobalAddin ai))
        return;

      var idx = GAList.SelectedIndex;
      globalAddins.Move(idx, idx - 1);

    }

  }
}
