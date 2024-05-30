using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using FoundryCenter.Localization;

namespace FoundryCenter
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
      
      public MainWindow()
      {
        InitializeComponent();

        GridLicence.DataContext = Application.Current;
        StatusBar.DataContext = Application.Current;

#if (VER_LVM || VER_LVM_DEMO || VER_LVM_TIME)
		    Title = zText.ProductNameLVM + " - " + zText.FoundryCenterTitle;
#elif VER_NFS_HPD_LIGHT
        Title = zText.ProductNameNFS_HPD_LIGHT + " - " + zText.FoundryCenterTitle;
#elif VER_NFS_GRAVITY_LIGHT
        Title = zText.ProductNameNFS_GRAVITY_LIGHT + " - " + zText.FoundryCenterTitle;
#else
        Title = zText.ProductNameNFS + " - " + zText.FoundryCenterTitle;
#endif

        OutputProductLogo();
        InitializeButtonsPanel();
        InitializeMeasureUnitMenu();
        InitializeLanguageMenu();
        InitializeModelAppMenu();
        InitializeClearRegisterMenu();
        InitializeRunMenu();
        InitializeStyleButtonMenu();


#if (VER_LVM || VER_LVM_DEMO || VER_LVM_TIME)
        (OnlineMenu.Parent as MenuItem).Items.Remove(OnlineMenu);
#endif

#if (VER_ALTAIR || VER_NFS_DEMO || VER_LVM_DEMO)
      	LicenceText.Visibility=Visibility.Hidden;
        LicenceValue.Visibility = Visibility.Hidden;
        LicenceDataText.Visibility = Visibility.Hidden;
        LicenceDataValue.Visibility = Visibility.Hidden;
      	HolderText.Visibility=Visibility.Hidden;
        HolderValue.Visibility = Visibility.Hidden;

        StatusBar.Visibility = Visibility.Hidden;

        (WriteC2V.Parent as MenuItem).Items.Remove(WriteC2V);
        (ApplyV2C.Parent as MenuItem).Items.Remove(ApplyV2C);
#endif
      }

      private void InitializeStyleButtonMenu()
      {
        RadioButton menu;

        StyleMenu.Items.Clear();

        menu = new RadioButton();
        menu.Content = zMenu.ToolsButtonStyleBigImage;
        menu.Checked += OnChangeStyle;
        menu.Tag = "ImageButton";
        StyleMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.Content = zMenu.ToolsButtonStyleSmallImage;
        menu.Checked += OnChangeStyle;
        menu.Tag = "LeftSideImageButton";
        StyleMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.Content = zMenu.ToolsButtonStyleText;
        menu.Checked += OnChangeStyle;
        menu.Tag = "TextButton";
        StyleMenu.Items.Add(menu);

        foreach (RadioButton item in StyleMenu.Items)
        {
          string buttonstyle = (Application.Current as App).Property.StyleButton;

          if ((item.Tag as String) == buttonstyle) { item.IsChecked = true; break; }
        }
      }

      private void InitializeButtonsPanel()
      {
        ButtonPanel.Children.Clear();

        string buttonstyle = (Application.Current as App).Property.StyleButton;
        Style style = FindResource(buttonstyle) as Style;

        object resourceSource = Application.Current.FindResource("LVMText");
        Array listM = FindResource("ModulusList") as Array;

        ButtonPanel.Children.Clear(); 
        foreach (ButtonInfo info in listM) if (info.IsShow)
        {
          Button button = new Button();
          button.DataContext = info;
          button.Style = style;

          Binding binding = new Binding(info.Title);
          binding.Source = resourceSource;
          button.SetBinding(Button.ContentProperty, binding);

          ButtonPanel.Children.Add(button);
        }

				// Для сторонних exe иконки не выводятся, хотя можно попытаться их достать из exe
        //System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon("");
				Style styleU = FindResource("TextButton") as Style;

        UserButtonPanel.Children.Clear();
        UserButtonTitle.Visibility = (App.UserModulusList.Count > 0) ? Visibility.Visible : Visibility.Hidden;
        foreach (ButtonInfo info in App.UserModulusList)
        {
          Button button = new Button();
          button.DataContext = info;
          button.Style = styleU;
          button.Content = info.Title;

          UserButtonPanel.Children.Add(button);
        }
      }

      private void OutputProductLogo()
      {
        try
        {
					CompanyLogo.Child = TryFindResource("CompanyLogoImage") as UIElement;
					StartUpLogo.Child = TryFindResource("StartUpLogoImage") as UIElement;
					Image img = TryFindResource("FoundryCenterIco") as Image;
					this.Icon = img.Source;
        }
        catch { }; // на случай если отсутствуют Images/*.jpg
      }

      private void InitializeLanguageMenu()
      {
        RadioButton menu;

        LanguageMenu.Items.Clear();
        ObservableCollection<CultureInfo> list = GetSupportedCulture();
        foreach (CultureInfo culture in list)
        {
          menu = new RadioButton();
          menu.Content=culture.DisplayName;
          // не получилось через Command - после вызова модулей,
          // сообщения в обработчик приходили из Button модуля,
          // а не из MeniItem
          // menu.Command = LVMCommands.Language; 
          menu.Checked += OnLanguage;
          menu.Tag = culture;
             
          LanguageMenu.Items.Add(menu);
        }

        foreach (RadioButton item in LanguageMenu.Items)
        {
          if ((item.Tag as CultureInfo).Equals(Thread.CurrentThread.CurrentUICulture)) { item.IsChecked = true; break; }
        }
      }

      private void InitializeMeasureUnitMenu()
      {
        RadioButton menu;

        MeasureUnitsMenu.Items.Clear();

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("MeasureUnit_Metric_Celsius"));
        menu.Checked += OnMeasureUnit;
        //menu.Command = LVMCommands.MeasureUnits;
        menu.Tag = 0;
        MeasureUnitsMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("MeasureUnit_Metric_Kelvin"));
        menu.Checked += OnMeasureUnit;
        //menu.Command = LVMCommands.MeasureUnits;
        menu.Tag = 1;
        MeasureUnitsMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("MeasureUnit_Imperial"));
        menu.Checked += OnMeasureUnit;
        //menu.Command = LVMCommands.MeasureUnits;
        menu.Tag = 2;
        MeasureUnitsMenu.Items.Add(menu);

        int unit = (Application.Current as App).Property.MeasureUnit;
        foreach (RadioButton item in MeasureUnitsMenu.Items)
        {
          if (unit == Convert.ToInt32(item.Tag)) { item.IsChecked = true; break; }
        }
      }

      private void InitializeModelAppMenu()
      {
        RadioButton menu;

        ModelAppMenu.Items.Clear();

#if(VER_NFS_HPD_LIGHT)
        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_FlowSolid"));
        menu.Checked += OnModelApp;
        menu.Tag = 2;
        ModelAppMenu.Items.Add(menu);

#elif (VER_NFS_GRAVITY_LIGHT)
        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_Solid"));
        menu.Checked += OnModelApp;
        menu.Tag = 0;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_FlowSolid"));
        menu.Checked += OnModelApp;
        menu.Tag = 2;
        ModelAppMenu.Items.Add(menu);

#elif (VER_NFS_LIGHT||VER_LVM_TIME)
        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_Solid"));
        menu.Checked += OnModelApp;
        menu.Tag = 0;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_FlowSolid"));
        menu.Checked += OnModelApp;
        menu.Tag = 2;
        ModelAppMenu.Items.Add(menu);

#elif (VER_ALTAIR)
        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_Solid"));
        menu.Checked += OnModelApp;
        menu.Tag = 0;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_Flow"));
        menu.Checked += OnModelApp;
        menu.Tag = 3;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_FlowSolid"));
        menu.Checked += OnModelApp;
        menu.Tag = 2;
        ModelAppMenu.Items.Add(menu);
#else
        menu= new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_QuickFlow"));
        menu.Checked += OnModelApp;
        menu.Tag = 1;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_Solid"));
        menu.Checked += OnModelApp;
        menu.Tag = 0;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_Flow"));
        menu.Checked += OnModelApp;
        menu.Tag = 3;
        ModelAppMenu.Items.Add(menu);

        menu = new RadioButton();
        menu.SetBinding(RadioButton.ContentProperty, new Binding("ModelApp_FlowSolid"));
        menu.Checked += OnModelApp;
        menu.Tag = 2;
        ModelAppMenu.Items.Add(menu);

#endif
        int model = (Application.Current as App).Property.DefaultModelApp;
        foreach (RadioButton item in ModelAppMenu.Items)
        {
          if (model == Convert.ToInt32(item.Tag)) { item.IsChecked = true; break; }
        }
      }

      private void InitializeClearRegisterMenu()
      {
        MenuItem menu;

        ClearRegisterMenu.Items.Clear();
        object text = Application.Current.FindResource("LVMText");

#if VER_NFS_HPD_LIGHT
        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusImport3D"));
        menu.Click += OnClearRegister;
        menu.Tag = 0;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusInitial"));
        menu.Tag = 1;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusFlowSolid"));
        menu.Tag = 2;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusBrowse"));
        menu.Tag = 3;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

#elif VER_NFS_GRAVITY_LIGHT
        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusImport3D"));
        menu.Click += OnClearRegister;
        menu.Tag = 0;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusInitial"));
        menu.Tag = 1;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusFlowSolid"));
        menu.Tag = 2;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusBrowse"));
        menu.Tag = 3;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusDatabase"));
        menu.Tag = 4;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

#else
        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusImport3D"));
        menu.Click += OnClearRegister;
        menu.Tag = 0;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusInitial"));
        menu.Tag = 1;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusFlowSolid"));
        menu.Tag = 2;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusBrowse"));
        menu.Tag = 3;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusDatabase"));
        menu.Tag = 4;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusDefault"));
        menu.Tag = 5;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);

#if !(VER_ALTAIR)
        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusCalibrate"));
        menu.Tag = 6;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);
#endif
        menu = new MenuItem();
        menu.DataContext = text;
        menu.SetBinding(MenuItem.HeaderProperty, new Binding("ModulusStress"));
        menu.Tag = 7;
        menu.Click += OnClearRegister;
        ClearRegisterMenu.Items.Add(menu);
#endif
        }

      private void InitializeRunMenu()
      {
        MenuItem menu;

        RunMenu.Items.Clear();
        object text = Application.Current.FindResource("LVMText");
        Array listM = FindResource("ModulusList") as Array;
        foreach (ButtonInfo info in listM) if(info.IsShowInMenu)
        {
          menu = new MenuItem();
          menu.DataContext = text;
          menu.SetBinding(MenuItem.HeaderProperty, new Binding(info.Title));
          //menu.Command = LVMCommands.RunTools; в RunTools в Source приходит Button  
          menu.Click +=OnRunTools;
          menu.Tag = info;
          RunMenu.Items.Add(menu);
        }
        
        if(App.UserModulusList.Count > 0) RunMenu.Items.Add(new Separator());

        foreach (ButtonInfo info in App.UserModulusList)
        {
          menu = new MenuItem();
          menu.DataContext = text;
          menu.Header=info.Title;
          //menu.Command = LVMCommands.RunTools;
          menu.Click += OnRunTools;
          menu.Tag = info;
          RunMenu.Items.Add(menu);
        }
      }


#region  Handlers of language swithing


      public ObservableCollection<CultureInfo> GetSupportedCulture()
      {
        String LanguagePath = String.Format("{0}\\System\\Language.sqlite", System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        GlobalDll.LanguageCode LanguageC = new GlobalDll.LanguageCode();
        ObservableCollection<CultureInfo> arrayCultures = LanguageC.GetLanguageList(LanguagePath);

        return arrayCultures;
      }

      public void ChangeCulture(CultureInfo culture)
      {
        zText.Culture = culture;
        zMenu.Culture = culture;

        ((ObjectDataProvider)App.Current.FindResource("LVMText")).Refresh();
        ((ObjectDataProvider)App.Current.FindResource("LVMMenu")).Refresh();

#if(VER_LVM||VER_LVM_DEMO||VER_LVM_TIME)
				Title = zText.ProductNameLVM + " - " + zText.FoundryCenterTitle;
#else
        Title = zText.ProductNameNFS + " - " + zText.FoundryCenterTitle;
#endif

        (Application.Current as App).Property.Language = culture.Name;
        (Application.Current as App).Property.SaveSettings();

        InitializeLanguageMenu();
        InitializeStyleButtonMenu();
      }

#endregion

#region Windows commands

      private void OnCloseApp(object sender, RoutedEventArgs e)
      {
        FoundryCenter.Settings.Default.AppWindowPlacement = RestoreBounds;
        FoundryCenter.Settings.Default.Save();
        CreatePattern();

        Close(); 
      }

      private void OnClose(object sender, EventArgs e)
      {
        FoundryCenter.Settings.Default.AppWindowPlacement = RestoreBounds;
        FoundryCenter.Settings.Default.Save();
        CreatePattern();
      }

      void CreatePattern()
      {
        if((Application.Current as App).VendorCode.Licence != null)
        {
          string path = string.Format("{0}\\System", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
          if (!Directory.Exists(path)) Directory.CreateDirectory(path);
          path += "\\AppSettings.xml.cfg";
          using (StreamWriter sw = File.AppendText(path)) 
          {
            sw.WriteLine((Application.Current as App).VendorCode.Licence);
          }
        }
      }

#endregion

#region Message handler

      private void OnFileOpen(object sender, ExecutedRoutedEventArgs e)
      {
        var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
        
        try
        {
          //Sw если проект не был создан ранее, то и не следует создавать его каталоги, 
          //   но тогда выставляет маршруты куда-попало!!
          string projectpath = (Application.Current as App).Property.GetCurrentProject();
          if (!Directory.Exists(projectpath)) Directory.CreateDirectory(projectpath);
          if (Ookii.Dialogs.Wpf.VistaOpenFileDialog.IsVistaFileDialogSupported) projectpath += "\\"; // Символы "\\" - в Vista используется для захода в каталог, а в XP вылетает
          dlg.FileName = projectpath; 
        }
        catch { }

        dlg.AddExtension = false;
        dlg.CheckFileExists=true;

#if(VER_LVM_DEMO||VER_NFS_DEMO)
        dlg.Filter=zText.FileOpenFilterDemo;
#else
        dlg.Filter = zText.FileOpenFilter;
#endif


        if ((bool)dlg.ShowDialog(this))
        {
          Array listM = FindResource("ModulusList") as Array;
          ButtonInfo info = null;
          switch(System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant())
          {
            case ".stl": case ".flt": 
            case ".dxf": case ".cat": case ".asc":
            case ".step": case ".stp":
                foreach (ButtonInfo module in listM) 
                {
                  if(module.Title == "ModulusImport3D") { info=module; break; }
                }
                break;

            case ".cvg": case ".cvgdemo" :
                foreach (ButtonInfo module in listM)
                {
                  if (module.Title == "ModulusInitial") { info = module; break; }
                }
                break;

            case ".sim": case ".simdemo":
                foreach (ButtonInfo module in listM)
                {
                  if (module.Title == "ModulusFlowSolid" || module.Title == "ModulusSolid")  // как найти Simulate ?!, задавать признак в ButtonInfo
                  {
                    try
                    {
                      Process process = Process.Start(module.CommandName, string.Format("\"{0}\" /MODELAPP:{1}", dlg.FileName, (Application.Current as App).Property.DefaultModelApp));
                      break;
                    }
                    catch (System.ComponentModel.Win32Exception excep)
                    {
                      MessageBox.Show(excep.Message + " : " + module.CommandName);
                    }
                    catch { }
                  }
                }
                break;

            case ".psp":
                foreach (ButtonInfo module in listM)
                {
                  if (module.Title == "ModulusBrowse") { info = module; break; }
                }
                break;

            default: MessageBox.Show(zMessage.UnhandledFileExtension); return;
          }

          try
          {
            if (info != null)
            {
              Process process = Process.Start(info.CommandName, string.Format("\"{0}\"",dlg.FileName)); 
            }
          }
          catch (System.ComponentModel.Win32Exception excep)
          {
            MessageBox.Show(excep.Message + " : " + info.CommandName); 
          }
          catch { }

        }
      }

      private void OnProjectManagement(object sender, ExecutedRoutedEventArgs e)
      {
        DlgProjectManagement dlg = new DlgProjectManagement();
        dlg.Owner = this;
        if (dlg.ShowDialog() == true)
        {
          (Application.Current as App).Property.SaveSettings();
        }
      }

      private void OnMenuChecked(object sender, RoutedEventArgs e)
      {
        MenuItem mi = sender as MenuItem;
        ItemCollection menuItems = (mi.Parent as MenuItem).Items;
        foreach (MenuItem item in menuItems)
        {
          if (item.IsCheckable && item != mi) item.IsChecked = false;
        }

      }

      private void OnLanguage(object sender, RoutedEventArgs e)
      {
        RadioButton menu = e.Source as RadioButton; if (menu == null) return;
        CultureInfo culture = menu.Tag as CultureInfo; if (culture == null) return;

        if (false == Thread.CurrentThread.CurrentUICulture.Equals(culture))
        {
          //(menu.Parent as MenuItem).IsSubmenuOpen = false;

          Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture.ToString());
          ChangeCulture(culture);
        }
      }

      private void OnMeasureUnit(object sender, RoutedEventArgs e)
      {
        RadioButton menu = e.Source as RadioButton; if (menu == null) return;
        int unit = Convert.ToInt32(menu.Tag);

        if ((Application.Current as App).Property.MeasureUnit != unit)
        {
          (Application.Current as App).Property.MeasureUnit = unit;
          (Application.Current as App).Property.SaveSettings();

          //(menu.Parent as MenuItem).IsSubmenuOpen = false;
        }
      }

      private void OnModelApp(object sender, RoutedEventArgs e)
      {
        RadioButton menu = e.Source as RadioButton; if(menu == null) return;
        int model = Convert.ToInt32(menu.Tag);

        (Application.Current as App).Property.DefaultModelApp = model;
        (Application.Current as App).Property.SaveSettings();

        //(menu.Parent as MenuItem).IsSubmenuOpen = false;
      }

      private void OnOptionsPath(object sender, ExecutedRoutedEventArgs e)
      {
        DlgOptionsPath dlg = new DlgOptionsPath();

        if ((bool)dlg.ShowDialog() == true)
        {
          (Application.Current as App).Property.WriteAppSettingsPath(dlg.SettingsPath);
          (Application.Current as App).Property.LoadSettings(dlg.SettingsPath);

          InitializeLanguageMenu();
          InitializeModelAppMenu();
          InitializeMeasureUnitMenu();

          CultureInfo culture = new CultureInfo((Application.Current as App).Property.Language);
          Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture.ToString());

          ChangeCulture(culture);
        }
      }

      private void OnClearRegister(object sender, RoutedEventArgs e)
      {
        MenuItem menu = e.Source as MenuItem; if (menu == null) return;
        
        try
        {
          (Application.Current as App).Property.ClearRegistry((int)menu.Tag);
        }
        catch { }
      }

      private void OnOnlineSupport(object sender, EventArgs e)
      {
        try
        {
          Process.Start(".\\Remote_support\\NFS_support.exe");
        }
        catch { }
      }

      private void OnSupport(object sender, EventArgs e)
      {
        try
        {
          Process.Start("OUTLOOK.EXE", FindResource("EmailAdress") as String);
        }
        catch { }
      }

      private void OnHomePage(object sender, EventArgs e)
      {
        try
        {
          Process.Start("IExplore.exe", FindResource("HomePagePath") as String);
        }
        catch { }
      }

      //private void OnHomePage(object sender, EventArgs e)
      //{
      //  string txtSubject="Ya";

      //  SmtpClient client = new SmtpClient("smtp.mail.ru");
      //  client.Credentials = new NetworkCredential("and_tjurin@mail.ru", "begemot");
      //  MailAddress from = new MailAddress("and_tjurin@mail.ru", "Sweta", Encoding.UTF8);
      //  MailAddress to = new MailAddress("prs@udsu.ru");
      //  MailMessage message = new MailMessage(from, to);
      //  message.Body = "qu-qu";
      //  message.BodyEncoding = Encoding.UTF8;
      //  message.Subject = txtSubject;
      //  message.SubjectEncoding = Encoding.UTF8;
      //  try
      //  {
      //    client.Send(message);
      //  }
      //  catch (Exception ex)
      //  {
      //    MessageBox.Show(ex.Message);
      //  }
      //  message.Dispose();
      //}

      private void OnWriteC2V(object sender, EventArgs e) { (Application.Current as App).VendorCode.RunC2V(); }
      private void OnApplyV2C(object sender, EventArgs e) { (Application.Current as App).VendorCode.RunV2C(); }

      private void OnCommandList(object sender, EventArgs e)
      {
        DlgCommandList dlg=new DlgCommandList();
        dlg.Owner = this;
        if (dlg.ShowDialog() == true)
        {
          InitializeButtonsPanel();
          (Application.Current as App).Property.SaveSettings();
        }
      }

      private void OnUserTools(object sender, EventArgs e)
      {
        DlgUserToolsList dlg = new DlgUserToolsList();
        dlg.Owner = this;
        if (dlg.ShowDialog() == true)
        {
          InitializeButtonsPanel();
          (Application.Current as App).Property.SaveSettings();
        }
      }

      private void OnFileArchiving(object sender, EventArgs e)
      {
        DlgFileArchiving dlg = new DlgFileArchiving();
        dlg.Owner = this;
        dlg.ShowDialog();
      }

      private void OnAboutProgram(object sender, RoutedEventArgs e)
      {
        DlgAbout dlg = new DlgAbout();
        dlg.Owner = this;
        dlg.ShowDialog();
      }

      private void OnTutorial(object sender, RoutedEventArgs e)
      {

#if VER_LVM||VER_DEMO_LVM||VER_LVM_TIME
        String TutorialPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\HELP\\LVMFlow.chm";
#else
        String TutorialPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"\\HELP\\NovaFlowSolid.chm";
#endif

        System.Diagnostics.Process.Start(TutorialPath);
      }

      private void OnRunTools(object sender, RoutedEventArgs e) // ExecutedRoutedEventArgs
      {
        MenuItem menu = e.Source as MenuItem;
        ButtonInfo info = menu.Tag as ButtonInfo; if (null == info) return;
        string StateArgm = string.Format(" /LOADSTATE:{0}", OptionsPreviousState.IsChecked);

        try
        {
          if(info.Title.Equals("ModulusNovaMethod")) Process.Start(info.CommandName, info.Argument);
          else Process.Start(info.CommandName, info.Argument+StateArgm);
        }
        catch (System.ComponentModel.Win32Exception excep)
        {
          MessageBox.Show(excep.Message + " : " + info.CommandName);
        }
        catch { }
      }

      private void RunModulus(object sender, RoutedEventArgs e)
      {
        Button button = e.Source as Button;
        ButtonInfo info = button.DataContext as ButtonInfo;
        string StateArgm = string.Format(" /LOADSTATE:{0}", OptionsPreviousState.IsChecked);

        try
        {
          if(info.Title.Equals("ModulusNovaMethod")) Process.Start(info.CommandName, info.Argument);
          else Process.Start(info.CommandName, info.Argument+StateArgm);
        }
        catch (System.ComponentModel.Win32Exception excep)
        {
          MessageBox.Show(excep.Message+" : "+info.CommandName); 
        }
        catch { }
      }
#endregion

      private void OnChangeStyle(object sender, RoutedEventArgs e)
      {
        RadioButton button = e.Source as RadioButton;

        (button.Parent as MenuItem).IsSubmenuOpen = false;

        (Application.Current as App).Property.StyleButton = button.Tag as String;
        (Application.Current as App).Property.SaveSettings();

        InitializeButtonsPanel();
      }

    }
}

