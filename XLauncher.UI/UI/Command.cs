using System;
using System.Windows;
using System.Windows.Input;

namespace XLauncher.UI
{

  using Common;

  public class Command : ICommand
  {

    static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    readonly string name;
    readonly MainView view;
    readonly Action execute;
    readonly Func<bool> canExecute;

    public event EventHandler CanExecuteChanged {
      add => view.CanExecuteChanged += value;
      remove => view.CanExecuteChanged -= value;
    }

    public Command(string name, MainView view, Action execute)
      : this(name, view, execute, () => true) { }
    public Command(string name, MainView view, Action execute, Func<bool> canExecute) {
      this.name = name.IfNull("Unnamed");
      this.view = view;
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter) {
      try {
        return canExecute();
      }
      catch (Exception ex) {
        logger.Error(ex, $"{name} command");
      }
      return false;
    }
    public void Execute(object parameter) {
      try {
        execute();
      }
      catch (Exception ex) {
        logger.Error(ex, $"{name} command");
        if (parameter == null)
          MessageBox.Show(ex.Message, Strings.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

  }
}
