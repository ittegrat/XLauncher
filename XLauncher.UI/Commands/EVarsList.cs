using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    public ICommand CmdEvAdd => cmdEvAdd ?? (cmdEvAdd = new Command(this, ExecEvAdd, IsLocalEnv));
    void ExecEvAdd() {

      var sev = (EVar)EVarsList.SelectedItem;

      var ev = new EVar {
        Framework = sev?.Framework
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
      CurrentEnvironment.Render(EVars);
      UpdateColumnWidths(EVarsList);
      EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name);

    }

    ICommand cmdEvDelete;
    public ICommand CmdEvDelete => cmdEvDelete ?? (cmdEvDelete = new Command(this, ExecEvDelete, IsEVarSelected));
    void ExecEvDelete() {

      if (!(EVarsList.SelectedItem is EVar ev))
        return;

      if (Configuration.Instance.LocalSettings.ConfirmDelete) {
        if (MessageBoxResult.Yes != MessageBox.Show(
          $"Are you sure you want to delete variable '{ev.Name}'?.",
          Strings.APP_NAME,
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        )) return;
      }

      var idx = EVarsList.SelectedIndex;
      CurrentEnvironment.Remove(ev);
      CurrentEnvironment.Render(EVars);
      EVarsList.SelectedIndex = Math.Min(idx, EVars.Count - 1); ;

    }

    ICommand cmdEvEdit;
    public ICommand CmdEvEdit => cmdEvEdit ?? (cmdEvEdit = new Command(this, ExecEvEdit, IsEVarSelected));
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
    public ICommand CmdEvMoveDown => cmdEvMoveDown ?? (cmdEvMoveDown = new Command(this, ExecEvMoveDown, CanExecEvMoveDown));
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
      CurrentEnvironment.Render(EVars);
      EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name);

    }

    ICommand cmdEvMoveUp;
    public ICommand CmdEvMoveUp => cmdEvMoveUp ?? (cmdEvMoveUp = new Command(this, ExecEvMoveUp, CanExecEvMoveUp));
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
      CurrentEnvironment.Render(EVars);
      EVarsList.SelectedItem = EVars.First(e => e.Name == ev.Name);

    }

  }
}
