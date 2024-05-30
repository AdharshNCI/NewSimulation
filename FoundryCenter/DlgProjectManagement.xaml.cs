using System;
using System.ComponentModel;
using System.IO;

using System.Windows;

namespace FoundryCenter
{
  /// <summary>
  /// Interaction logic for DlgProjectManagement.xaml
  /// </summary>
  public partial class DlgProjectManagement : Window, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (null != PropertyChanged) PropertyChanged(this, e);
    }

    private string m_DefaultProjectsPath;
    public string DefaultProjectsPath 
    { 
      get { return(m_DefaultProjectsPath); }
      set { m_DefaultProjectsPath = value; OnPropertyChanged(new PropertyChangedEventArgs("DefaultProjectsPath")); }
    }

		private string m_Default3DGeometryPath;
		public string Default3DGeometryPath
		{
			get { return (m_Default3DGeometryPath); }
			set { m_Default3DGeometryPath = value; OnPropertyChanged(new PropertyChangedEventArgs("Default3DGeometryPath")); }
		}

    private string m_CurrentProject;
    public string CurrentProject 
    {
      get { return (m_CurrentProject); }
      set { m_CurrentProject = value; OnPropertyChanged(new PropertyChangedEventArgs("CurrentProject")); }  
    }

    public DlgProjectManagement()
    {
      DefaultProjectsPath = (Application.Current as App).Property.DefaultProjectsPath;
			Default3DGeometryPath = (Application.Current as App).Property.Default3DGeometryPath;
      CurrentProject = (Application.Current as App).Property.CurrentProject;

      InitializeComponent();

      DataContext = this;
    }

    private void OnOK(object sender, RoutedEventArgs e)
    {
      (Application.Current as App).Property.DefaultProjectsPath = DefaultProjectsPath;
			(Application.Current as App).Property.Default3DGeometryPath = Default3DGeometryPath;
      (Application.Current as App).Property.CurrentProject = CurrentProject;

      DialogResult = true;
      Close();
    }

    private void OnBrowseLocation(object sender, RoutedEventArgs e)
    {
      var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

      dlg.Multiselect=false;
      dlg.ShowNewFolderButton = true;
      dlg.RootFolder = Environment.SpecialFolder.MyComputer; 
      try
      {
	      dlg.SelectedPath = Path.GetFullPath(DefaultProjectsPath);
      }
      catch { }

      if((bool)dlg.ShowDialog() == true)  DefaultProjectsPath = dlg.SelectedPath;
    }

		private void OnBrowse3DGeometry(object sender, RoutedEventArgs e)
		{
			var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

      dlg.Multiselect=false;
			dlg.ShowNewFolderButton = true;
			dlg.RootFolder = Environment.SpecialFolder.MyComputer;
			try
			{
				dlg.SelectedPath = Path.GetFullPath(Default3DGeometryPath);
			}
			catch { }

			if ((bool)dlg.ShowDialog() == true) Default3DGeometryPath = dlg.SelectedPath;
		}

    private void OnBrowseProject(object sender, RoutedEventArgs e)
    {
      var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

      dlg.Multiselect=false;
      dlg.ShowNewFolderButton = true;
      dlg.RootFolder = Environment.SpecialFolder.MyComputer;
      try
      {
        string path = App.GetRelativePath(DefaultProjectsPath,CurrentProject);
        if (String.IsNullOrEmpty(CurrentProject)) path += "\\"; //Sw // - для того, чтобы зайти в каталог DefaultProjectsPath
        dlg.SelectedPath = path;
      }
      catch {}

      if ((bool)dlg.ShowDialog() == true) CurrentProject = dlg.SelectedPath;
    }

  }
}
