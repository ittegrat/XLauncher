using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class MainView
  {

    public bool IsEVarSelected() {
      return (IsLocalEnv() && (EVarsList.SelectedIndex >= 0));
    }

    ICommand cmdEvAdd;
    public ICommand CmdEvAdd => cmdEvAdd ?? (cmdEvAdd = new Command(nameof(CmdEvAdd), this, ExecEvAdd, IsLocalEnv));
    void ExecEvAdd() {

      var sev = (EVar)EVarsList.SelectedItem;

      var ev = new EVar {
        Framework = sev?.Framework ?? CurrentEnvironment.FNames.Last()
      };

      var evbox = new EVarBox(CurrentEnvironment.FNames) {
        Title = "Add Environment Variable",
        DataContext = ev
      };

      evbox.Closing += (sender, args) => {

        var evb = (EVarBox)sender;

        if (evb.DialogResult != true)
          return;

        if (EVars.Any(e => e.Name.Equals(ev.Name, StringComparison.OrdinalIgnoreCase))) {
          MessageBox.Show(
            $"Variable '{ev.Name}' is already defined.\nPlease choose a different name.",
            Strings.APP_NAME,
            MessageBoxButton.OK,
            MessageBoxImage.Error
          );
          args.Cancel = true;
        }

      };

      if (evbox.ShowDialog() != true)
        return;

      CurrentEnvironment.Add(ev, sev);
      RenderCommandEv(() => EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name));

    }

    ICommand cmdEvDelete;
    public ICommand CmdEvDelete => cmdEvDelete ?? (cmdEvDelete = new Command(nameof(CmdEvDelete), this, ExecEvDelete, IsEVarSelected));
    void ExecEvDelete() {

      if (!(EVarsList.SelectedItem is EVar ev))
        return;

      var last = CurrentEnvironment.ItemsCount(ev.Framework) < 2;

      if (CheckLast(last))
        return;

      if (Configuration.Instance.LocalSettings.ConfirmDelete) {
        string msg;
        if (last) {
          msg = $"As the variable '{ev.Name}' is the last element of the framework '{ev.Framework}'," +
                $" the entire framework will be deleted. Continue?";
        } else {
          msg = $"Are you sure you want to delete variable '{ev.Name}'?";
        }
        if (MessageBoxResult.Yes != MessageBox.Show(msg, Strings.APP_NAME, MessageBoxButton.YesNo, MessageBoxImage.Warning))
          return;
      }

      var idx = EVarsList.SelectedIndex;
      if (last)
        CurrentEnvironment.Remove(ev.Framework);
      else
        CurrentEnvironment.Remove(ev);
      RenderCommandEv(() => EVarsList.SelectedIndex = Math.Min(idx, EVars.Count - 1));

    }

    ICommand cmdEvEdit;
    public ICommand CmdEvEdit => cmdEvEdit ?? (cmdEvEdit = new Command(nameof(CmdEvEdit), this, ExecEvEdit, IsEVarSelected));
    void ExecEvEdit() {

      if (!(EVarsList.SelectedItem is EVar sev))
        return;

      var ev = sev.Clone();

      var evbox = new EVarBox(CurrentEnvironment.FNames) {
        Title = "Edit Environment Variable",
        DataContext = ev
      };

      evbox.Closing += (sender, args) => {

        var evb = (EVarBox)sender;

        if (evb.DialogResult != true)
          return;

        if (EVars.Where(e => !e.Equals(sev)).Any(e => e.Name.Equals(ev.Name, StringComparison.OrdinalIgnoreCase))) {
          MessageBox.Show(
            $"Variable '{ev.Name}' is already defined.\nPlease choose a different name.",
            Strings.APP_NAME,
            MessageBoxButton.OK,
            MessageBoxImage.Error
          );
          args.Cancel = true;
        }

      };

      if (evbox.ShowDialog() != true)
        return;

      if (ev.Framework == sev.Framework) {
        sev.SetValue(ev);
        EVarsList.Items.Refresh();
      } else {
        CurrentEnvironment.Remove(sev);
        CurrentEnvironment.Add(ev, null);
        CurrentEnvironment.Render(EVars);
        EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name);
      }
      CurrentEnvironment.Dirty = true;

      UpdateColumnWidths(EVarsList);

    }

    ICommand cmdEvMoveDown;
    public ICommand CmdEvMoveDown => cmdEvMoveDown ?? (cmdEvMoveDown = new Command(nameof(CmdEvMoveDown), this, ExecEvMoveDown, CanExecEvMoveDown));
    bool CanExecEvMoveDown() {

      if (!(EVarsList.SelectedItem is EVar ev))
        return false;

      if (EVarsList.SelectedIndex == EVarsList.Items.Count - 1)
        return false;

      var nev = (EVar)EVarsList.Items[EVarsList.SelectedIndex + 1];

      return IsLocalEnv() && (ev.Framework == nev.Framework);

    }
    void ExecEvMoveDown() {

      if (!(EVarsList.SelectedItem is EVar ev))
        return;

      CurrentEnvironment.MoveDown(ev);
      RenderCommandEv(() => EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name));

    }

    ICommand cmdEvMoveUp;
    public ICommand CmdEvMoveUp => cmdEvMoveUp ?? (cmdEvMoveUp = new Command(nameof(CmdEvMoveUp), this, ExecEvMoveUp, CanExecEvMoveUp));
    bool CanExecEvMoveUp() {

      if (!(EVarsList.SelectedItem is EVar ev))
        return false;

      if (EVarsList.SelectedIndex == 0)
        return false;

      var pev = (EVar)EVarsList.Items[EVarsList.SelectedIndex - 1];

      return IsLocalEnv() && (ev.Framework == pev.Framework);

    }
    void ExecEvMoveUp() {

      if (!(EVarsList.SelectedItem is EVar ev))
        return;

      CurrentEnvironment.MoveUp(ev);
      RenderCommandEv(() => EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name));

    }

    void RenderCommandEv(Action UpdateSelected) {
      CurrentEnvironment.Render(EVars);
      UpdateSelected();
      UpdateColumnWidths(EVarsList);
      EVarsList.UpdateLayout();
      if (EVarsList.SelectedIndex >= 0)
        ((ListViewItem)EVarsList.ItemContainerGenerator.ContainerFromIndex(EVarsList.SelectedIndex)).Focus();
    }

  }
}
