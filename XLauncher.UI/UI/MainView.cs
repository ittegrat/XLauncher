using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace XLauncher.UI
{

  using DataAdapters;

  public partial class MainView : Window
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    Environment CurrentEnvironment = null;

    public ObservableCollection<Environment> Environments { get; } = new ObservableCollection<Environment>();
    public ObservableCollection<Addin> Addins { get; } = new ObservableCollection<Addin>();
    public ObservableCollection<EVar> EVars { get; } = new ObservableCollection<EVar>();

    public event EventHandler CanExecuteChanged;

    public MainView() {
      InitializeComponent();
      Initialize();
    }

    void Initialize() {

      CollectionViewSource
        .GetDefaultView(Environments)
        .GroupDescriptions.Add(
          new PropertyGroupDescription(Environment.GroupPropertyName)
        )
      ;

      CollectionViewSource
        .GetDefaultView(Addins)
        .GroupDescriptions.Add(
          new PropertyGroupDescription(Addin.GroupPropertyName)
        )
      ;

      CollectionViewSource
        .GetDefaultView(EVars)
        .GroupDescriptions.Add(
          new PropertyGroupDescription(EVar.GroupPropertyName)
        )
      ;

      Addins.CollectionChanged += (s, e) => CurrentEnvironment.Dirty = true;
      EVars.CollectionChanged += (s, e) => CurrentEnvironment.Dirty = true;

      LoadEnvironments();

    }

    void OnSelectEnvironment(object sender, SelectionChangedEventArgs e) {

      foreach (var env in Environments)
        env.Save();

      CurrentEnvironment = (Environment)EnvList.SelectedItem;

      if (CurrentEnvironment == null)
        return;

      CurrentEnvironment.Render(ControlsList.Children, Addins, EVars);
      UpdateColumnWidths(AddinsList, EVarsList);

      CurrentEnvironment.Dirty = false;
      Configuration.Instance.LocalSettings.LastEnvironmentId = CurrentEnvironment.Id;

      RaiseCanExecuteChanged(this, null);

      if (Configuration.Instance.LocalSettings.RestoreTab)
        EnvView.SelectedIndex = 0;

    }
    void RaiseCanExecuteChanged(object sender, RoutedEventArgs e) {
      var handler = CanExecuteChanged;
      handler?.Invoke(this, EventArgs.Empty);
    }
    void SelectExcelPath(object sender, RoutedEventArgs e) {
      if (Configuration.Instance.LocalSettings.SelectExcelPath()) {
        ExcelPathBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        ExcelArchBox.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateTarget();
      }
    }

    void LoadEnvironments() {

      Environments.Clear();
      Environment.Fill(Environments);

      var env = Environments.Where(e => e.Id == Configuration.Instance.LocalSettings.LastEnvironmentId).FirstOrDefault();
      if (env != null)
        EnvList.SelectedItem = env;
      else
        EnvList.SelectedIndex = 0;

    }
    void UpdateColumnWidths(params ListView[] lviews) {
      // https://dlaa.me/blog/post/9425496
      foreach (var lv in lviews) {
        if (!(lv.View is GridView gv))
          continue;
        foreach (var c in gv.Columns) {
          if (double.IsNaN(c.Width)) {
            c.Width = 0;
            c.Width = double.NaN;
          }
        }
      }
    }

  }

}
