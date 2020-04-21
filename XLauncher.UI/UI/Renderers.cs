using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace XLauncher.UI
{

  using Common;
  using DataAdapters;
  using EE = Entities.Environments;

  public static class Renderers
  {

    public static UIElement NoControls() {

      return new Border() {
        //-- BorderBrush = new SolidColorBrush(Colors.Black),
        BorderBrush = new SolidColorBrush(Color.FromRgb(0x10, 0x40, 0x80)),
        BorderThickness = new Thickness(3),
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 12, 0, 0),
        Child = new TextBlock {
          //-- Background = new SolidColorBrush(Colors.AliceBlue),
          FontSize = 20,
          FontWeight = FontWeights.Bold,
          HorizontalAlignment = HorizontalAlignment.Center,
          Padding = new Thickness(12, 6, 12, 10),
          Text = "Please press the 'Launch' button to start Excel."
        }
      };

    }
    public static UIElement Render(EE.Box bx, Environment parent) {
      var gbox = new GroupBox {
        Header = bx.Text,
        Margin = new Thickness(3, 3, 3, 0),
        Content = RenderControls(bx.Controls, parent)
      };
      return gbox;
    }

    static UIElement RenderControls(EE.Control[] controls, Environment parent) {

      var sp = new StackPanel();

      foreach (var c in controls) {

        switch (c) {

          case EE.CheckBox ck:
            sp.Children.Add(Renderers.Render(ck, parent));
            break;

          case EE.ComboBoxEx cx:
            sp.Children.Add(Renderers.Render(cx, parent));
            break;

          case EE.ComboBox cb:
            sp.Children.Add(Renderers.Render(cb, parent));
            break;

          case EE.DatePicker dp:
            sp.Children.Add(Renderers.Render(dp, parent));
            break;

          case EE.TextBox tb:
            sp.Children.Add(Renderers.Render(tb, parent));
            break;

        }

      }

      return sp;

    }

    static UIElement Render(EE.CheckBox cb, Environment parent) {

      var cbox = new CheckBox {
        Content = cb.Text.IfNull(cb.Name),
        Margin = new Thickness(0, 4, 0, 0),
        ToolTip = Configuration.Instance.LocalSettings.ShowToolTips ? $"{cb.Parent.Name}.{cb.Name}" : null
      };
      cbox.SetBinding(
        CheckBox.IsCheckedProperty,
        new Binding(nameof(EE.CheckBox.Value)) {
          Source = cb
        }
      );
      OnValueChanged(cbox, CheckBox.IsCheckedProperty, parent);

      return cbox;

    }
    static UIElement Render(EE.ComboBox cb, Environment parent) {

      var tblock = new TextBlock {
        Margin = new Thickness(0, 0, 4, 0),
        Text = cb.Text.IfNull(cb.Name) + ":",
        ToolTip = Configuration.Instance.LocalSettings.ShowToolTips ? $"{cb.Parent.Name}.{cb.Name}" : null,
        VerticalAlignment = VerticalAlignment.Center
      };

      var cbox = new ComboBox {
        ItemsSource = cb.Items
      };
      cbox.SetBinding(
        ComboBox.SelectedItemProperty,
        new Binding(nameof(EE.ComboBox.Value)) {
          Source = cb
        }
      );
      OnValueChanged(cbox, ComboBox.SelectedItemProperty, parent);

      var sp = new StackPanel {
        Margin = new Thickness(0, 4, 0, 0),
        Orientation = Orientation.Horizontal
      };

      sp.Children.Add(tblock);
      sp.Children.Add(cbox);

      return sp;

    }
    static UIElement Render(EE.ComboBoxEx cb, Environment parent) {

      var ckbox = new CheckBox {
        Content = cb.Text.IfNull(cb.Name) + ":",
        Margin = new Thickness(0, 0, 4, 0),
        ToolTip = Configuration.Instance.LocalSettings.ShowToolTips ? $"{cb.Parent.Name}.{cb.Name}" : null,
        VerticalContentAlignment = VerticalAlignment.Center
      };
      ckbox.SetBinding(
        CheckBox.IsCheckedProperty,
        new Binding(nameof(EE.ComboBoxEx.Active)) {
          Source = cb
        }
      );
      OnValueChanged(ckbox, CheckBox.IsCheckedProperty, parent);

      var cbox = new ComboBox {
        ItemsSource = cb.Items
      };
      cbox.SetBinding(
        ComboBox.IsEnabledProperty,
        new Binding(nameof(CheckBox.IsChecked)) {
          Source = ckbox
        }
      );
      cbox.SetBinding(
        ComboBox.SelectedItemProperty,
        new Binding(nameof(EE.ComboBoxEx.Value)) {
          Source = cb
        }
      );
      OnValueChanged(cbox, ComboBox.SelectedItemProperty, parent);

      var sp = new StackPanel {
        Margin = new Thickness(0, 4, 0, 0),
        Orientation = Orientation.Horizontal
      };

      sp.Children.Add(ckbox);
      sp.Children.Add(cbox);

      return sp;

    }
    static UIElement Render(EE.DatePicker dp, Environment parent) {

      var cbox = new CheckBox {
        Content = dp.Text.IfNull(dp.Name) + ":",
        Margin = new Thickness(0, 0, 4, 0),
        MinHeight = 24,
        ToolTip = Configuration.Instance.LocalSettings.ShowToolTips ? $"{dp.Parent.Name}.{dp.Name}" : null,
        VerticalContentAlignment = VerticalAlignment.Center
      };
      cbox.SetBinding(
        CheckBox.IsCheckedProperty,
        new Binding(nameof(EE.DatePicker.Active)) {
          Source = dp
        }
      );
      OnValueChanged(cbox, CheckBox.IsCheckedProperty, parent);

      var dpic = new DatePicker {
        IsTodayHighlighted = true
      };
      dpic.SetBinding(
        DatePicker.IsEnabledProperty,
        new Binding(nameof(CheckBox.IsChecked)) {
          Source = cbox
        }
      );
      dpic.SetBinding(
        DatePicker.SelectedDateProperty,
        new Binding(nameof(EE.DatePicker.Value)) {
          Source = dp
        }
      );
      OnValueChanged(dpic, DatePicker.SelectedDateProperty, parent);
      dpic.MouseRightButtonDown += (s, e) => {
        dpic.DisplayDate = DateTime.Today;
      };

      var sp = new StackPanel {
        Margin = new Thickness(0, 4, 0, 0),
        Orientation = Orientation.Horizontal
      };

      sp.Children.Add(cbox);
      sp.Children.Add(dpic);

      return sp;

    }
    static UIElement Render(EE.TextBox tb, Environment parent) {

      var tblock = new TextBlock {
        Margin = new Thickness(0, 0, 4, 0),
        Text = tb.Text.IfNull(tb.Name) + ":",
        ToolTip = Configuration.Instance.LocalSettings.ShowToolTips ? $"{tb.Parent.Name}.{tb.Name}" : null
      };

      var tbox = new TextBox();
      tbox.SetBinding(
        TextBox.TextProperty,
        new Binding(nameof(EE.TextBox.Value)) {
          Source = tb,
          UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        }
      );
      OnValueChanged(tbox, TextBox.TextProperty, parent);

      var sp = new StackPanel {
        Margin = new Thickness(0, 4, 0, 0),
        Orientation = Orientation.Horizontal
      };

      sp.Children.Add(tblock);
      sp.Children.Add(tbox);

      return sp;

    }

    static void OnValueChanged<T>(T target, DependencyProperty dp, Environment parent) {
      DependencyPropertyDescriptor
        .FromProperty(dp, typeof(T))
        ?.AddValueChanged(
          target,
          (s, e) => parent.Dirty = true
        )
      ;
    }

  }

}
