using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using FoundryCenter.Localization;

namespace FoundryCenter
{
  /// <summary>
  /// Interaction logic for DlgUserToolsList.xaml
  /// </summary>
  public partial class DlgUserToolsList : Window
  {
    private ObservableCollection<ButtonInfo> dublicate = new ObservableCollection<ButtonInfo>();

    private class LocalButton : CheckBox
    {
      public ButtonInfo info { get; set; } // ссылка на исходные данные, правятся только по OK
      public LocalButton(ButtonInfo _info) { info = _info; }
    }

    public DlgUserToolsList()
    {
      foreach (ButtonInfo info in App.UserModulusList) dublicate.Add(new ButtonInfo(info));

      InitializeComponent();
      mList.ItemsSource = dublicate;
    }

    private void OnOK(object sender, RoutedEventArgs e)
    {
      App.UserModulusList.Clear();
      foreach (ButtonInfo info in dublicate)
      {
        App.UserModulusList.Add(info);
      }

      DialogResult = true;
      Close();
    }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
      dublicate.Add(new ButtonInfo(zText.NewExternalTool));
      mList.SelectedIndex = dublicate.Count-1;
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
      if (mList.SelectedItem == null) return;
      dublicate.RemoveAt(mList.SelectedIndex);
    }

    private void OnBrowse(object sender, RoutedEventArgs e)
    {
      if (mList.SelectedItem == null) return;

      var dlg=new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

      dlg.Title = zText.DlgExternalToolsTitle;
      dlg.DefaultExt = "exe";
      dlg.Filter = zText.DlgExternalToolsFilter;
      dlg.FileName = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      if (Ookii.Dialogs.Wpf.VistaOpenFileDialog.IsVistaFileDialogSupported) dlg.FileName += "\\";

      if (dlg.ShowDialog(this) == true)
      {
        (mList.SelectedItem as ButtonInfo).CommandName = dlg.FileName;
      }
    }
  }
}
