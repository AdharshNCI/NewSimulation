using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

using FoundryCenter.Localization;
using Ionic.Zip;

namespace FoundryCenter
{
  /// <summary>
  /// Логика взаимодействия для DlgFileArchiving.xaml
  /// </summary>
  public partial class DlgFileArchiving : Window, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (null != PropertyChanged) PropertyChanged(this, e);
    }

    private static bool FileNotAdded=new bool();

    private string m_CVGPath;
    public string CVGPath
    {
      get { return (m_CVGPath); }
      set { m_CVGPath = value; OnPropertyChanged(new PropertyChangedEventArgs("CVGPath")); }
    }

    private string m_DescriptorPath;
    public string DescriptorPath
    {
      get { return (m_DescriptorPath); }
      set { m_DescriptorPath = value; OnPropertyChanged(new PropertyChangedEventArgs("DescriptorPath")); }
    }

    private string m_DatabasePath;
    public string DatabasePath
    {
      get { return (m_DatabasePath); }
      set { m_DatabasePath = value; OnPropertyChanged(new PropertyChangedEventArgs("DatabasePath")); }
    }

    private string m_DiagramPath;
    public string DiagramPath
    {
      get { return (m_DiagramPath); }
      set { m_DiagramPath = value; OnPropertyChanged(new PropertyChangedEventArgs("DiagramPath")); }
    }

    private string m_ZipPath;
    public string ZipPath
    {
      get { return (m_ZipPath); }
      set { m_ZipPath = value; OnPropertyChanged(new PropertyChangedEventArgs("ZipPath")); }
    }

    public DlgFileArchiving()
    {
      App app = Application.Current as App;

      CVGPath = app.Property.GetCurrentProject(); CVGPath += "\\Geometry\\" + Path.GetFileName(CVGPath) + ".cvg";
      DescriptorPath = app.Property.GetCurrentProject(); 
      DatabasePath = app.Property.MaterialPath;
      DiagramPath = ".\\System\\Diagram"+app.Property.DatabaseExtension;
      ZipPath = ".\\" + Path.GetFileNameWithoutExtension(CVGPath) + ".zip";

      FileNotAdded = false;

      InitializeComponent();

      DataContext = this;
    }

    private void OnCVGLocation(object sender, RoutedEventArgs e)
    {
      var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

      dlg.FileName = CVGPath;
      dlg.AddExtension = true;
      dlg.CheckFileExists=true;
      dlg.Filter = zText.FileOpenFilterCVgeo;

      if ((bool)dlg.ShowDialog(this) == true) CVGPath = dlg.FileName;
    }

    private void OnDescriptorLocation(object sender, RoutedEventArgs e)
    {
      var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

      dlg.Multiselect=false;
      dlg.ShowNewFolderButton = false;
      dlg.RootFolder = Environment.SpecialFolder.MyComputer;
      dlg.SelectedPath = DescriptorPath; 
      if (Ookii.Dialogs.Wpf.VistaOpenFileDialog.IsVistaFileDialogSupported) dlg.SelectedPath += "\\";

      if ((bool)dlg.ShowDialog() == true) DescriptorPath = dlg.SelectedPath;
    }

    private void OnDatabaseLocation(object sender, RoutedEventArgs e)
    {
      Ookii.Dialogs.Wpf.VistaOpenFileDialog dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

      dlg.FileName = Path.GetFullPath(DatabasePath);
      dlg.CheckFileExists=true;
      dlg.Filter = zText.FileOpenFilterDatabase;

      if ((bool)dlg.ShowDialog(this) == true) DatabasePath = dlg.FileName;
    }

    private void OnZipLocation(object sender, RoutedEventArgs e)
    {
      Ookii.Dialogs.Wpf.VistaOpenFileDialog dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

      dlg.FileName = Path.GetFullPath(ZipPath); 
      dlg.AddExtension = true;
      dlg.CheckFileExists = false;
      dlg.Filter = zText.FileOpenFilterZip;

      if ((bool)dlg.ShowDialog(this) == true) ZipPath = dlg.FileName;
    }


    private void OnOK(object sender, RoutedEventArgs e)
    {
      if (!ZipPath.EndsWith(".zip")) ZipPath += ".zip";

      try
      {
        using (ZipFile zip = new ZipFile())
        {
          FileNotAdded = false;
          zip.ZipError += MyZipError;

          String project = Path.GetFileName(DescriptorPath);

          zip.AddFile(CVGPath,project+"\\Geometry");
          zip.AddFile(DescriptorPath + "\\" + project + ".psp", project+"\\"+project);
          zip.AddFile(DescriptorPath + "\\" + project + ".add", project+"\\"+project);
          zip.AddFile(DatabasePath, project);
          zip.AddFile(DiagramPath, project);

          zip.Save(Path.GetFullPath(ZipPath));
        }
      }
      catch (System.Exception)
      {
        return;
      }

      if (FileNotAdded) return; //если не все файлы добавлены, остаемся в диалоге 
      DialogResult = true;
      Close();
    }

    public static void MyZipError(object sender, ZipErrorEventArgs e)
    {
      // файлы *.add надо пропускать без предупреждения, т.к. они могут и отсутствовать
      // выдается предупреждение для всех файлов, кроме *.add
      if (String.Compare(Path.GetExtension(e.FileName), ".add", true) != 0)
      {
        MessageBox.Show(zMessage.NotAddFile + " : " + e.FileName);
        FileNotAdded = true;
      }

      e.CurrentEntry.ZipErrorAction = ZipErrorAction.Skip;
      e.Cancel = false;
    }
  }
}
