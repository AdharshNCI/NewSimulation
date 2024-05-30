
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FoundryCenter.HaspCode;
using FoundryCenter.Localization;

namespace FoundryCenter
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>

  public partial class App : System.Windows.Application 
  {
    private static readonly double   appversion = 6.65;
    private static readonly int      apprelease = 1;
    private static readonly DateTime applinkdate = new DateTime(2023,11,23);

    //////////////////////////////////////////////////////////////////////

    // Public параметры
    public static string ProductTitle { get; private set; }
    public static string AppVersion { get { return string.Format("{0} {1} {2} ", appversion.ToString("F2", CultureInfo.InvariantCulture), zText.AppRelease, apprelease); } }
    public static string AppVersionShort { get { return string.Format("{0}", appversion.ToString("F2", CultureInfo.InvariantCulture)); } }
    public static string AppLinkDate { get { return applinkdate.ToShortDateString(); } }
    public static ArrayList UserModulusList { get; private set; } //static - нехочется обращаться через (Application.Current as).UserModulusList

    // Номера модулей считываемые из HASP
#if VER_LVM_TIME
    public enum ModulusFeature { Solid = 6, FlowSolid = 7, Stress=8 };
#else
    public enum ModulusFeature { Solid = 10, QuickFlow = 11, FlowSolid = 12, Flow = 13, Stress = 14 };
#endif

    public LVMProperty Property { get; private set; }
    public VendorCode VendorCode { get; private set; }

    static App()
    {
      UserModulusList = new ArrayList();

#if VER_LVM  
      ProductTitle = "LVMFlow";
#elif VER_LVM_DEMO  
      ProductTitle = "LVMFlow";
#elif VER_LVM_TIME  
      ProductTitle = "LVMFlow";
#elif VER_NFS
      ProductTitle = "NovaFlow";
#elif VER_NFS_DEMO
      ProductTitle = "NovaFlow";
#elif VER_NFS_LIGHT
      ProductTitle = "NovaFlowLight";
#elif VER_NFS_HPD_LIGHT
      ProductTitle = "NovaFlow";
#elif VER_NFS_GRAVITY_LIGHT
      ProductTitle = "NovaFlow";
#elif VER_ALTAIR
      ProductTitle = "NovaFlow";
#else
      ProductTitle = "FoundryCenter";
#endif

    }

    void OnStartupApp(object sender, StartupEventArgs e)
    {

#if VER_ALTAIR
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/ALTAIR.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource);
#elif VER_LVM  
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/LVM.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource);
#elif VER_LVM_DEMO
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/LVMDemo.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource); 
#elif VER_LVM_TIME  
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/LVMTime.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource);
#elif VER_NFS
	  ResourceDictionary resource = new ResourceDictionary();
	  resource.Source = new Uri("ConfigurationResources/NFS.xaml", UriKind.Relative);
	  Resources.MergedDictionaries.Add(resource);
#elif VER_NFS_DEMO
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/NFSDemo.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource); 
#elif VER_NFS_LIGHT
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/NFSLight.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource);
#elif VER_NFS_HPD_LIGHT
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/NFSHPDLight.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource);
#elif VER_NFS_GRAVITY_LIGHT
      ResourceDictionary resource = new ResourceDictionary();
      resource.Source = new Uri("ConfigurationResources/NFSGravityLight.xaml", UriKind.Relative);
      Resources.MergedDictionaries.Add(resource);
#endif

      Property = new LVMProperty(); Property.Load();
      VendorCode = new VendorCode(); // if DEMO не вызывать

      if (e.Args.Length > 0)
      {
        try
        {
          Thread.CurrentThread.CurrentUICulture = new CultureInfo(e.Args[0]);
        }
        catch (ArgumentException ex)
        {
          Debug.WriteLine(ex.Message, "Bad command-line argument");
        }
      }
      else
      {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Property.Language);
      }

      MainWindow mainWindow = new MainWindow(); //~ StartupUri(XAML)
      Rect rect = FoundryCenter.Settings.Default.AppWindowPlacement;
      mainWindow.Top = rect.Top;
      mainWindow.Left = rect.Left;
      mainWindow.Width = rect.Width;
      mainWindow.Height = rect.Height;
      mainWindow.Show();
    }

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      e.Handled = true;
      MessageBox.Show(e.Exception.Message); // zMessage.UnhandledException);
    }

    private void OnAppExit(object sender, ExitEventArgs e)
    {
      //VendorCode.Logout();
    }

    /// <summary>
    /// При задании path c символа "\" - возвращает маршрут относительно диска приложения
    /// При задании path c символа ".\" - возвращает маршрут относительно каталога запуска приложения,
    /// При задании path c символа "..\" - возвращает маршрут на одном уровне с каталогом приложения
    /// При задании path просто имени - возвращает полный маршрут относительно directory
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static public string GetRelativePath(string directory,string path)
    {
      string result = path;

      if (result.StartsWith("\\") || result.StartsWith(".\\") || result.StartsWith("..\\")) // относительно application directory
      {
        result = Path.GetFullPath(path);
      }
      else if (Path.IsPathRooted(path) == false)
      {
        result = Path.Combine(Path.GetFullPath(directory), path);
      }
      return (result);
    }


  }

  public class ButtonInfo : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (null != PropertyChanged) PropertyChanged(this, e);
    }

    public App.ModulusFeature Feature { get; set; } // Номер ф-ции в ключе
    private String _Title;
    public String Title
    {
      get { return (_Title); }
      set { _Title=value; OnPropertyChanged(new PropertyChangedEventArgs("Title")); }
    }

    private String _IcoPath;
    public String IcoPath
    {
      get { return (_IcoPath); }
      set { _IcoPath=value; OnPropertyChanged(new PropertyChangedEventArgs("IcoPath")); }
    }

    // разрешен или нет в HASP-ключе запуск модуля 
    private bool _IsEnable; 
    public bool IsEnable 
    {
      get { return (_IsEnable); }
      set { _IsEnable = value; OnPropertyChanged(new PropertyChangedEventArgs("IsEnable")); } 
    }

    private String _CommandName;
    public String CommandName 
    {
      get { return (_CommandName); }
      set { _CommandName = value; OnPropertyChanged(new PropertyChangedEventArgs("CommandName")); }
    }
    public String Argument { get; set; }
    public bool IsShow { get; set; }
    public bool IsShowInMenu { get; set; }



    public ButtonInfo(String _Text, String _IcoPath, String _ExeName, String _ExeParameters, bool _IsEnable, App.ModulusFeature _Feature, bool _IsShow, bool _IsShowInMenu)
    {
      Title = _Text;
      IcoPath = _IcoPath;

      CommandName = _ExeName;
      Argument = _ExeParameters;

      IsShow = _IsShow;
      IsShowInMenu = _IsShowInMenu;
      IsEnable = _IsEnable;
      Feature = _Feature;
    }

    public ButtonInfo() : this(String.Empty, String.Empty, String.Empty, String.Empty, true, 0, true, false) { }
    public ButtonInfo(String _Text) : this(_Text, String.Empty, String.Empty, String.Empty, true, 0, true, false) { }
    public ButtonInfo(String _Text, String _ExeName, String _ExeParameters, bool _IsEnable) : this(_Text, String.Empty, _ExeName, _ExeParameters, _IsEnable, 0, true, false) { }
    public ButtonInfo(ButtonInfo info) : this(info.Title, info.IcoPath, info.CommandName, info.Argument, info.IsEnable, info.Feature, info.IsShow, info.IsShowInMenu) { }
  }

  public class LVMCommands
  {
    // File
    public static RoutedUICommand FileOpen { get; private set; }
    public static RoutedUICommand FileArchiving { get; private set; }

    // Options
    public static RoutedUICommand ProjectManagement { get; private set; }
    public static RoutedUICommand Language { get; private set; }
    public static RoutedUICommand MeasureUnits { get; private set; }
    public static RoutedUICommand ModelApp { get; private set; }
    public static RoutedUICommand OptionsPath { get; private set; }

    // Tools
    public static RoutedUICommand RunTools { get; private set; }
    public static RoutedUICommand CommandList { get; private set; }
    public static RoutedUICommand UserTools { get; private set; }

    // Remote assistance
    public static RoutedUICommand OnlineSupport { get; private set; }
    public static RoutedUICommand Support { get; private set; }
    public static RoutedUICommand HomePage { get; private set; }
    public static RoutedUICommand WriteC2V { get; private set; }
    public static RoutedUICommand ApplyV2C { get; private set; }

    // Help
    public static RoutedUICommand AboutProgram { get; private set; }
    public static RoutedUICommand Tutorial { get; private set; }

    static LVMCommands()
    {
      // File
      FileOpen = new RoutedUICommand(zMenu.FileOpen, "FileOpen", typeof(LVMCommands));
      FileArchiving = new RoutedUICommand(zMenu.FileArchiving, "FileArchiving", typeof(LVMCommands));

      // Options
      ProjectManagement = new RoutedUICommand(zMenu.OptionsProject, "ProjectManagement", typeof(LVMCommands));
      Language = new RoutedUICommand(zMenu.OptionsLanguage, "Language", typeof(LVMCommands));
      MeasureUnits = new RoutedUICommand(zMenu.OptionsMeasureUnit, "MeasureUnits", typeof(LVMCommands));
      ModelApp = new RoutedUICommand(zMenu.OptionsModelApp, "ModelApp", typeof(LVMCommands));
      OptionsPath = new RoutedUICommand(zMenu.OptionsPath, "OptionsPath", typeof(LVMCommands));

      // Tools
      RunTools = new RoutedUICommand(zMenu.ToolsRun, "Run", typeof(LVMCommands));
      CommandList = new RoutedUICommand(zMenu.ToolsCommandList, "CommandList", typeof(LVMCommands));
      UserTools = new RoutedUICommand(zMenu.ToolsUserTools, "UserTools", typeof(LVMCommands));

      // Remote assistance
      OnlineSupport = new RoutedUICommand(zMenu.RemoteAssistanceOnLineSupport, "On-line support", typeof(LVMCommands));
      Support = new RoutedUICommand(zMenu.RemoteAssistanceSupport, "Support", typeof(LVMCommands));
      HomePage = new RoutedUICommand(zMenu.RemoteAssistanceHomePage, "HomePage", typeof(LVMCommands));
      WriteC2V = new RoutedUICommand(zMenu.RemoteAssistanceWriteC2V, "WriteC2V", typeof(LVMCommands));
      ApplyV2C = new RoutedUICommand(zMenu.RemoteAssistanceApplyV2C, "ApplyV2C", typeof(LVMCommands));

      // Help
      AboutProgram = new RoutedUICommand(zMenu.HelpAboutProgram, "AboutProgram", typeof(LVMCommands));
      Tutorial = new RoutedUICommand(zMenu.HelpTutorial, "Tutorial", typeof(LVMCommands));
    }
  }

  public static class LocalizeResource
  {
    public static zText GetResourceTextInstance() { return new zText(); }
    public static zMenu GetResourceMenuInstance() { return new zMenu(); }
  }

  [ValueConversion(typeof(DateTime), typeof(String))]
  public class TimeToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      DateTime date=(DateTime)value;

      if (date <= DateTime.MinValue) return zText.LicenceNoSet;
      else if (date >= new DateTime(9999,12,31)) return zText.LicenceUnlimited;

      return date.ToLongDateString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if(null == value) return DateTime.MinValue;
      return DateTime.MinValue;
    }
  }

  

  [ValueConversion(typeof(String), typeof(String))]
  public class LicenceToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      String licence=(String)value;

      if (licence == null) return zText.LicenceNoSet;

      return licence;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value;
    }
  }

  [ValueConversion(typeof(String), typeof(BitmapImage))]
  public class PathToImageConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      String path=value.ToString();

      if (path.Length == 0) return (null); //path="Images/NullIco.png";

      BitmapImage bmp = new BitmapImage();
      bmp.BeginInit();
      bmp.UriSource = new Uri(path, UriKind.Relative);
      bmp.EndInit();
      return bmp;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }
  }

  [ValueConversion(typeof(Int32), typeof(bool))]
  public class IsSelectedIndex : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((Int32)value < 0) ? false : true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return -1;
    }
  }

}


