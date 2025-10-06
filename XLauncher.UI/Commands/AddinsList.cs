using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class MainView
  {

    public bool IsAddinSelected() {
      return (AddinsList.SelectedIndex >= 0);
    }
    public bool IsLocalAndAddinSelected() {
      return (IsLocalEnv() && (AddinsList.SelectedIndex >= 0));
    }

    ICommand cmdAiAdd;
    public ICommand CmdAiAdd => cmdAiAdd ?? (cmdAiAdd = new Command(nameof(CmdAiAdd), this, ExecAiAdd, IsLocalEnv));
    void ExecAiAdd() {

      var sai = (Addin)AddinsList.SelectedItem;

      var ai = new Addin {
        Framework = sai?.Framework,
        ReadOnly = true
      };

      var aibox = new AddinBox(CurrentEnvironment.FNames, sai?.Path) {
        Title = "Add Addin",
        DataContext = ai
      };

      aibox.Closing += (sender, args) => {

        var aib = (AddinBox)sender;

        if (aib.DialogResult != true)
          return;

        string msg = null;

        var any = Addins.Where(a => a.QFileName.Equals(ai.QFileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (any != null) {
          msg = new StringBuilder("Addin '")
            .Append(any.QFileName)
            .Append("'")
            .Append(String.IsNullOrWhiteSpace(any.Key) ? String.Empty : $"\n(alias '{any.Key}')")
            .Append(" already exists.")
            .ToString()
          ;
        }

        if (msg == null) {
          any = Addins.Where(a => a.Id.Equals(ai.Id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
          if (any != null) {
            msg = new StringBuilder("Addin key '")
              .Append(any.Id)
              .Append("'\nis already used by '")
              .Append(any.QFileName)
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

      CurrentEnvironment.Add(ai, sai);
      CurrentEnvironment.Render(Addins);
      UpdateColumnWidths(AddinsList);
      AddinsList.SelectedItem = Addins.First(a => a.Path == ai.Path);

    }

    ICommand cmdAiCopyPath;
    public ICommand CmdAiCopyPath => cmdAiCopyPath ?? (cmdAiCopyPath = new Command(nameof(CmdAiCopyPath), this, ExecAiCopyPath, IsAddinSelected));
    void ExecAiCopyPath() {
      if (!(AddinsList.SelectedItem is Addin ai))
        return;
      Clipboard.SetText(ai.Path);
    }

    ICommand cmdAiDelete;
    public ICommand CmdAiDelete => cmdAiDelete ?? (cmdAiDelete = new Command(nameof(CmdAiDelete), this, ExecAiDelete, IsLocalAndAddinSelected));
    void ExecAiDelete() {

      if (!(AddinsList.SelectedItem is Addin ai))
        return;

      var last = CurrentEnvironment.ItemsCount(ai.Framework) < 2;

      if (Configuration.Instance.LocalSettings.ConfirmDelete) {
        string msg;
        if (last) {
          msg = $"As the addin '{Path.GetFileName(ai.Path)}' (Arch: {ai.Arch}) is the last element of" +
                $" the framework '{ai.Framework}', the entire framework will be deleted. Continue?";
        } else {
          msg = $"Are you sure you want to delete addin '{Path.GetFileName(ai.Path)}' (Arch: {ai.Arch})?";
        }
        if (MessageBoxResult.Yes != MessageBox.Show(msg, Strings.APP_NAME, MessageBoxButton.YesNo, MessageBoxImage.Warning))
          return;
      }

      var idx = AddinsList.SelectedIndex;
      if (last)
        CurrentEnvironment.Remove(ai.Framework);
      else
        CurrentEnvironment.Remove(ai);
      CurrentEnvironment.Render(Addins);
      AddinsList.SelectedIndex = Math.Min(idx, Addins.Count - 1);

    }

    ICommand cmdAiEdit;
    public ICommand CmdAiEdit => cmdAiEdit ?? (cmdAiEdit = new Command(nameof(CmdAiEdit), this, ExecAiEdit, IsLocalAndAddinSelected));
    void ExecAiEdit() {

      if (!(AddinsList.SelectedItem is Addin sai))
        return;

      var ai = sai.Clone();

      var aibox = new AddinBox(CurrentEnvironment.FNames) {
        Title = "Edit Addin",
        DataContext = ai
      };

      aibox.Closing += (sender, args) => {

        var aib = (AddinBox)sender;

        if (aib.DialogResult != true)
          return;

        string msg = null;

        var any = Addins.Where(a => !a.Equals(sai)).Where(a => a.QFileName.Equals(ai.QFileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (any != null) {
          msg = new StringBuilder("Addin '")
            .Append(any.QFileName)
            .Append("'")
            .Append(String.IsNullOrWhiteSpace(any.Key) ? String.Empty : $"\n(alias '{any.Key}')")
            .Append(" already exists.")
            .ToString()
          ;
        }

        if (msg == null) {
          any = Addins.Where(a => !a.Equals(sai)).Where(a => a.Id.Equals(ai.Id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
          if (any != null) {
            msg = new StringBuilder("Addin key '")
              .Append(any.Id)
              .Append("'\nis already used by '")
              .Append(any.QFileName)
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

      if ((ai.Framework == sai.Framework) && (ai.Arch == sai.Arch)) {
        sai.SetValue(ai);
        AddinsList.Items.Refresh();
      } else {
        CurrentEnvironment.Remove(sai);
        CurrentEnvironment.Add(ai, null);
        CurrentEnvironment.Render(Addins);
        AddinsList.SelectedItem = Addins.First(a => a.Path == ai.Path);
      }
      CurrentEnvironment.Dirty = true;

      UpdateColumnWidths(AddinsList);

    }

    ICommand cmdAiMoveDown;
    public ICommand CmdAiMoveDown => cmdAiMoveDown ?? (cmdAiMoveDown = new Command(nameof(CmdAiMoveDown), this, ExecAiMoveDown, CanExecAiMoveDown));
    bool CanExecAiMoveDown() {

      if (!(AddinsList.SelectedItem is Addin ai))
        return false;

      if (AddinsList.SelectedIndex == AddinsList.Items.Count - 1)
        return false;

      var nai = (Addin)AddinsList.Items[AddinsList.SelectedIndex + 1];

      return IsLocalEnv() && (ai.Framework == nai.Framework);

    }
    void ExecAiMoveDown() {

      if (!(AddinsList.SelectedItem is Addin ai))
        return;

      CurrentEnvironment.MoveDown(ai);
      RenderAiMove(ai);

    }

    ICommand cmdAiMoveUp;
    public ICommand CmdAiMoveUp => cmdAiMoveUp ?? (cmdAiMoveUp = new Command(nameof(CmdAiMoveUp), this, ExecAiMoveUp, CanExecAiMoveUp));
    bool CanExecAiMoveUp() {

      if (!(AddinsList.SelectedItem is Addin ai))
        return false;

      if (AddinsList.SelectedIndex == 0)
        return false;

      var pai = (Addin)AddinsList.Items[AddinsList.SelectedIndex - 1];

      return IsLocalEnv() && (ai.Framework == pai.Framework);

    }
    void ExecAiMoveUp() {

      if (!(AddinsList.SelectedItem is Addin ai))
        return;

      CurrentEnvironment.MoveUp(ai);
      RenderAiMove(ai);

    }

    ICommand cmdAiOpenFolder;
    public ICommand CmdAiOpenFolder => cmdAiOpenFolder ?? (cmdAiOpenFolder = new Command(nameof(cmdAiOpenFolder), this, ExecAiOpenFolder));
    void ExecAiOpenFolder() {
      if (!(AddinsList.SelectedItem is Addin sai))
        return;
      System.Diagnostics.Process.Start(Path.GetDirectoryName(sai.Path));
    }

    void RenderAiMove(Addin ai) {
      CurrentEnvironment.Render(Addins);
      AddinsList.SelectedItem = Addins.First(a => a.Path == ai.Path);
      AddinsList.UpdateLayout();
      ((ListViewItem)AddinsList.ItemContainerGenerator.ContainerFromIndex(AddinsList.SelectedIndex)).Focus();
    }

  }
}
