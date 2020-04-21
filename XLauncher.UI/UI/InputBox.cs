using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace XLauncher.UI
{
  public class InputBox : Window
  {

    public string Text { get; set; }

    public InputBox(string label, string title, string hint = null) {

      Title = title;
      Text = hint;

      var tb = new TextBox {
        MinWidth = 300,
        Margin = new Thickness(8, 3, 8, 10)
      };
      tb.SetBinding(
        TextBox.TextProperty,
        new Binding(nameof(Text)) {
          Source = this,
          UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        }
      );

      Content = new StackPanel {
        Children = {
          new TextBlock {
            Text = label,
            Margin = new Thickness(8,0,8,0)
          },
          tb
        }
      };

      Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Rocket.png"));
      Owner = Application.Current.MainWindow;
      SizeToContent = SizeToContent.WidthAndHeight;

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
      else if (e.Key == Key.Return) {
        DialogResult = true;
      } else { return; }

      if (DialogResult != null)
        Close();

    }

  }
}
