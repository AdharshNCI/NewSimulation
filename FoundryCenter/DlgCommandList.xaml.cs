using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;


namespace FoundryCenter
{
	public partial class DlgCommandList
	{
		public DlgCommandList()
		{
			this.InitializeComponent();

      ArrayList list = new ArrayList(14);
      object resourceSource = Application.Current.FindResource("LVMText");
      Array mlist = Application.Current.FindResource("ModulusList") as Array;

      foreach (ButtonInfo info in mlist)
      {
        CheckBox button = new CheckBox();
        button.DataContext = info;

        Binding binContent = new Binding(info.Title);
        binContent.Source = resourceSource;
        button.SetBinding(CheckBox.ContentProperty, binContent);

        button.IsChecked = info.IsShow;
        button.Margin=new Thickness(0, 2, 0, 0);
        list.Add(button);
      }

      mList.ItemsSource = list;
		}

    private void OnOK(object sender, RoutedEventArgs e)
    {
      bool value, bModified = false;

      foreach (CheckBox button in mList.ItemsSource)
      {
        value = (button.IsChecked == true) ? true : false;
        ButtonInfo info = (ButtonInfo)button.DataContext;
        if (info.IsShow != value) { info.IsShow = value; bModified = true; }
      }

      DialogResult = (bModified) ? true : false;
      Close();
    }
	}
}