using System;
using System.ComponentModel;
using System.IO;

using System.Windows;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Win32;

using FoundryCenter.Localization;

namespace FoundryCenter
{
  /// <summary>
  /// Interaction logic for DlgOptionsPath.xaml
  /// </summary>
  public partial class DlgOptionsPath : Window, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (null != PropertyChanged) PropertyChanged(this, e);
    }

    private string m_SettingsPath;
    public string SettingsPath 
    { 
      get { return(m_SettingsPath); }
      set { m_SettingsPath = value; OnPropertyChanged(new PropertyChangedEventArgs("SettingsPath")); }
    }

    public DlgOptionsPath()
    {
      SettingsPath = (Application.Current as App).Property.GetSettingsPath();

      InitializeComponent();

      DataContext = this;
    }

    private void OnOK(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
    }

    private void OnBrowseLocation(object sender, RoutedEventArgs e)
    {
      var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

			try
			{
      dlg.FileName =Path.GetFullPath(SettingsPath); 
      dlg.Filter = zText.OptionsImportFilter;
      dlg.DefaultExt = "xml";
      dlg.CheckFileExists = false;
			}
			catch (System.Exception)
			{
				
			}
      if ((bool)dlg.ShowDialog() == true) SettingsPath = dlg.FileName;
    }
  }
}
