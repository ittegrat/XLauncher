using System;
using System.Windows.Input;

namespace XLauncher.UI
{
  public class Command : ICommand
  {

    readonly MainView view;
    readonly Action execute;
    readonly Func<bool> canExecute;

    public event EventHandler CanExecuteChanged {
      add => view.CanExecuteChanged += value;
      remove => view.CanExecuteChanged -= value;
    }

    public Command(MainView view, Action execute)
      : this(view, execute, () => true) { }

    public Command(MainView view, Action execute, Func<bool> canExecute) {
      this.view = view;
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter) {
      return canExecute();
    }
    public void Execute(object parameter) {
      execute();
    }

  }
}
