using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class EVarBox : Window
  {

    public IEnumerable<string> Frameworks { get; }

    public EVarBox(IEnumerable<string> frameworks) {

      InitializeComponent();

      Frameworks = frameworks;
      Owner = Application.Current.MainWindow;

      Loaded += OnLoaded;
      KeyDown += OnKeyDown;

    }

    void OnLoaded(object sender, RoutedEventArgs e) {
      Top = Owner.Top + (Owner.Height - ActualHeight) / 2;
      Left = Owner.Left + (Owner.Width - ActualWidth) / 2;
    }
    void OnKeyDown(object sender, KeyEventArgs e) {

      if (e.Key == Key.Escape)
        DialogResult = false;
      else if ((e.Key == Key.Return) && Validate()) {
        DialogResult = true;
      }

      if (DialogResult != null)
        Close();

    }

    bool Validate() {

      var ev = (EVar)DataContext;

      ev.Name = ev.Name?.Trim();

      if (String.IsNullOrEmpty(ev.Framework)) {
        MessageBox.Show(
          $"Framework cannot be empty.",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      if (String.IsNullOrEmpty(ev.Name)) {
        MessageBox.Show(
          $"Variable name cannot be empty.",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      if (String.IsNullOrWhiteSpace(ev.Value)) {
        var ans = MessageBox.Show(
          $"The variable value is empty.\nProceed anyway?",
          Strings.APP_NAME,
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning,
          MessageBoxResult.No
        );
        if (ans != MessageBoxResult.Yes)
          return false;
      }

      return true;

    }

  }
}
