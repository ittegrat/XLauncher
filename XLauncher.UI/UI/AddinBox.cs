using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;

  public partial class AddinBox : Window
  {

    int index = 1;
    string initialFolder;
    bool isFullBox;

    public IEnumerable<string> Frameworks { get; }

    public AddinBox(IEnumerable<string> frameworks, string hint = null, bool isLimited = false) {

      InitializeComponent();

      isFullBox = !isLimited;

      Resources["IsFullBox"] = isFullBox;
      Resources["TextOpacity"] = isFullBox ? 1 : 0.5;
      Resources["KeyText"] = (isFullBox ? "Key" : "Alias") + ":";

      Frameworks = frameworks;
      initialFolder = (hint == null) ? String.Empty : Path.GetDirectoryName(hint);
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
      else if ((e.Key == Key.Return) && (isFullBox ? ValidateA() : ValidateGA())) {
        DialogResult = true;
      }

      if (DialogResult != null)
        Close();

    }
    void SelectPath(object sender, RoutedEventArgs e) {

      dynamic ai;
      if (isFullBox)
        ai = (Addin)DataContext;
      else
        ai = (GlobalAddin)DataContext;

      if (!String.IsNullOrWhiteSpace(ai.Path))
        initialFolder = Path.GetDirectoryName(ai.Path);

      var ofd = new OpenFileDialog {
        DereferenceLinks = true,
        Filter = "Add-ins (*.xlam;*.xla;*.xll)|*.xlam;*.xla;*.xll|Excel Files (*.xls*)|*.xls*|All files (*.*)|*.*",
        FilterIndex = index,
        InitialDirectory = initialFolder,
        Title = "Select file"
      };

      if (ofd.ShowDialog(this) == false)
        return;

      index = ofd.FilterIndex;
      ai.Path = ofd.FileName;

      if (isFullBox) {

        if (Path.GetExtension(ai.Path).Equals(".XLL", StringComparison.OrdinalIgnoreCase))
          ai.Arch = Configuration.Instance.LocalSettings.ExcelArch.ToString();
        else
          ai.Arch = Addin.ANY;

        ArchBox.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateTarget();

      }

      PathBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
      PathBox.Focus();

    }

    bool ValidateA() {

      var ai = (Addin)DataContext;

      if (String.IsNullOrEmpty(ai.Framework)) {
        MessageBox.Show(
          "Framework cannot be empty.",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      ai.Path = ai.Path?.Trim();

      if (String.IsNullOrEmpty(ai.Path)) {
        MessageBox.Show(
          "Path cannot be empty.",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      if (!File.Exists(ai.Path)) {
        MessageBox.Show(
          $"Invalid path:\n'{ai.Path}'",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      var ext = Path.GetExtension(ai.Path).ToUpperInvariant();
      var xll = ext == ".XLL";
      if ((xll && (ai.Arch == Addin.ANY)) || (!xll && (ai.Arch != Addin.ANY))) {
        MessageBox.Show(
          $"Invalid arch type '{ai.Arch}' for filetype '{ext}'.",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      return true;

    }
    bool ValidateGA() {

      var ai = (GlobalAddin)DataContext;

      ai.Path = ai.Path?.Trim();

      if (String.IsNullOrEmpty(ai.Path)) {
        MessageBox.Show(
          "Path cannot be empty.",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      if (!File.Exists(ai.Path)) {
        MessageBox.Show(
          $"Invalid path:\n'{ai.Path}'",
          Strings.APP_NAME,
          MessageBoxButton.OK,
          MessageBoxImage.Error
        );
        return false;
      }

      return true;

    }

  }
}
