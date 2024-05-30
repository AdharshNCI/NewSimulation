using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Win32;

using FoundryCenter.Localization;

namespace FoundryCenter
{
  public class LVMProperty : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (null != PropertyChanged) PropertyChanged(this, e);
    }

    public string Language { get; set; }
    public string DatabaseExtension { get; set; }
    public string MaterialPath { get; set; }
    public int MeasureUnit { get; set; }
    public int DefaultModelApp { get; set; }

    public string DefaultProjectsPath { get; set; }
	public string Default3DGeometryPath { get; set; }
    public string CurrentProject { get; set; }
    public string StyleButton { get; set; }

    public void ClearRegistry(int iModule)
    {
      String[] name = new String[8] {"Import3D", "Initial settings", "Simulate", "Browser", "Material Database", "Default settings", "Calibrate","Stress analysis"};

      String path = String.Format("Software\\{0}\\{1}\\{2}", App.ProductTitle,name[iModule],App.AppVersionShort);
      Registry.CurrentUser.DeleteSubKeyTree(path);
    }

    public LVMProperty()
    {
      CurrentProject = "Untitled";
      DefaultProjectsPath = string.Format("{0}\\{1}\\Projects", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), App.ProductTitle);
			Default3DGeometryPath = string.Format("{0}\\{1}\\3D geometry", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), App.ProductTitle);

      #if (VER_LVM || VER_LVM_DEMO)
        Language="ru";
      #else 
        Language="en";
      #endif

      MeasureUnit = 0;
      DatabaseExtension = (IntPtr.Size == 8) ? ".sdf" : ".mdb"; //Sw в зав-ти от х86/x64 в Net4.0 - Environment.Is64BitOperatingSystem 
      MaterialPath=string.Format(".\\System\\Material{0}",DatabaseExtension);

      StyleButton = "LeftSideImageButton";  // "ImageButton", "LeftSideImageButton", "TextButton" 
    }

    /// <summary>
    /// Считывает settings из AppSettings.xml
    /// </summary>
    /// <returns></returns>
    public void Load()
    {
      try
      {
        LoadSettings(GetFullSettingsPath());
      }
      catch(SystemException e)
      {
        string message = e.Message+"\n"+zMessage.SetupFileError;
        if(MessageBox.Show(message, "", MessageBoxButton.YesNo, MessageBoxImage.Information)==MessageBoxResult.Yes)
        { 
          // Запрос файла дубликата
          var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

          dlg.CheckFileExists=true;
          dlg.Filter=zText.FileOpenFilterXml;

          if((bool)dlg.ShowDialog())
          {
            try
            {
              LoadSettings(dlg.FileName);
            }
            catch(SystemException e1)
            {
              message=e1.Message+"\n"+zMessage.SetupFileDublicateError;
              MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
          }
        }
      }
    }

    public void LoadSettings(string settingspath)
    {
      try
      {
        //string settingspath = GetFullSettingsPath();

        XPathDocument  doc=new XPathDocument(settingspath);
        XPathNavigator navigator=doc.CreateNavigator();
        XPathNavigator node;

        if (null != (node = navigator.SelectSingleNode("//CommonSettings//LanguageCulture"))) Language = node.Value;
        if (null != (node = navigator.SelectSingleNode("//CommonSettings//DatabaseExtension"))) DatabaseExtension = node.Value;
        if (null != (node = navigator.SelectSingleNode("//CommonSettings//DatabasePath"))) MaterialPath = node.Value;
        if (null != (node = navigator.SelectSingleNode("//CommonSettings//MeasureUnit"))) MeasureUnit = node.ValueAsInt;
        if (null != (node = navigator.SelectSingleNode("//SimulateSettings//DefaultApplicationModel"))) DefaultModelApp = node.ValueAsInt;
        if (null != (node = navigator.SelectSingleNode("//CenterSettings//CommandButtonStyle"))) StyleButton = node.Value;

        if (null != (node = navigator.SelectSingleNode("//CommonSettings//Project")))
        {
					String txt;

					txt=node.GetAttribute("DefaultLocationPath", String.Empty);
					if(!String.IsNullOrEmpty(txt)) DefaultProjectsPath=txt;

					txt=node.GetAttribute("Default3DGeometryPath", String.Empty);
					if(!String.IsNullOrEmpty(txt)) Default3DGeometryPath=txt;

					txt=node.GetAttribute("CurrentPath", String.Empty);
					if(!String.IsNullOrEmpty(txt)) CurrentProject=txt;
        }

        if(null != (node=navigator.SelectSingleNode("//CenterSettings//ShowButtonList"))) 
        {
          String txt;
          Array listM = Application.Current.FindResource("ModulusList") as Array;
          foreach(ButtonInfo button in listM)
          {
            txt=node.GetAttribute(button.Title,String.Empty); 
            button.IsShow=(String.IsNullOrEmpty(txt)) ? true : Convert.ToBoolean(txt);
          }
        }

        {
          Array listM = Application.Current.FindResource("ModulusList") as Array;
          foreach(ButtonInfo info in listM)
            if(info.Title.Equals("ModulusNovaMethod"))
            {
              if(null != (node=navigator.SelectSingleNode("//CenterSettings//NovaMethod"))) 
              {
                info.Argument = node.GetAttribute("ToolsArgument", String.Empty);

                String path=node.GetAttribute("ToolsCommand", String.Empty);
                info.CommandName = Path.GetFullPath(path);
              }
              break;
            }
        }

        XPathNodeIterator iter = navigator.Select("//CenterSettings//ExternalToolsList//Tool");
        if (null != iter)
        {
          while(iter.MoveNext())
          {
            node=iter.Current;
            String name = node.GetAttribute("ToolsTitle", String.Empty);
            String procedure = node.GetAttribute("ToolsCommand", String.Empty);
            String parameter = node.GetAttribute("ToolsArgument", String.Empty);

            if (String.IsNullOrEmpty(name) == false && String.IsNullOrEmpty(procedure) == false)
            {
              App.UserModulusList.Add(new ButtonInfo(name, procedure, parameter, true));
            }
          }
        }

      }
      catch(SystemException e) { Debug.WriteLine(e.ToString()); throw; }
      catch { }

      // Проверка всех параметров
      if(IntPtr.Size == 8)
        switch(DatabaseExtension.ToLowerInvariant())
        {
          case ".mdb":  DatabaseExtension= ".sdf"; break;
        }

      if (MeasureUnit < 0) MeasureUnit = 0; else if (MeasureUnit > 2) MeasureUnit = 2;

      switch (StyleButton)
      {
        case "ImageButton":
        case "LeftSideImageButton":
        case "TextButton": break;
        default: StyleButton = "LeftSideImageButton"; break;
      }

#if (VER_NFS_HPD_LIGHT)
        if(DefaultModelApp==0 ||DefaultModelApp==1 || DefaultModelApp==3) DefaultModelApp=2;
#elif(VER_NFS_GRAVITY_LIGHT)
      if(DefaultModelApp==1||DefaultModelApp==3) DefaultModelApp=2;
#elif (VER_NFS_LIGHT || VER_LVM_TIME)
      if(DefaultModelApp==1 || DefaultModelApp==3) DefaultModelApp=2;
#endif
    }

    public void SaveSettings()
    {
      string settingspath = GetFullSettingsPath();
      try
      {
        XmlDocument doc=new XmlDocument();
        XPathNavigator navigator=doc.CreateNavigator();
        XPathNavigator Node;
        XPathNavigator node;
        XmlElement elem;

        try
        {
          doc.Load(settingspath);
        }
        catch { }

        if(null == (node=navigator.SelectSingleNode("//Settings")))
        {
          // похоже файла-то небыло!
          XmlDeclaration xmldecl=doc.CreateXmlDeclaration("1.0", null, null); doc.AppendChild(xmldecl);
          elem=doc.CreateElement("Settings"); doc.AppendChild(elem);
        } 

        if(null == (Node=navigator.SelectSingleNode("//CommonSettings")))
        {
          // Пропала секция  <CommonSettings>
          node=navigator.SelectSingleNode("Settings");
          node.AppendChild("<CommonSettings/>"); 
          Node=navigator.SelectSingleNode("//CommonSettings");      
        }

        if (null != (node = navigator.SelectSingleNode("//CommonSettings//LanguageCulture"))) node.SetTypedValue(Language);
        else Node.AppendChild(string.Format("<LanguageCulture>{0}</LanguageCulture>", Language));

        if(null != (node=navigator.SelectSingleNode("//CommonSettings//MeasureUnit"))) node.SetTypedValue(MeasureUnit);
        else  Node.AppendChild(string.Format("<MeasureUnit>{0}</MeasureUnit>",MeasureUnit));

        if (null != (node = navigator.SelectSingleNode("//CommonSettings//Project")))
        {
          if (node.MoveToAttribute("DefaultLocationPath", String.Empty)) { node.SetTypedValue(DefaultProjectsPath); node.MoveToParent(); }
          else node.CreateAttribute(String.Empty, "DefaultLocationPath", String.Empty, DefaultProjectsPath);

					if (node.MoveToAttribute("Default3DGeometryPath", String.Empty)) { node.SetTypedValue(Default3DGeometryPath); node.MoveToParent(); }
					else node.CreateAttribute(String.Empty, "Default3DGeometryPath", String.Empty, Default3DGeometryPath);

          if (node.MoveToAttribute("CurrentPath", String.Empty)) { node.SetTypedValue(CurrentProject); node.MoveToParent(); }
          else node.CreateAttribute(String.Empty, "CurrentPath", String.Empty, CurrentProject);
        }
        else
        {
          Node.AppendChild(String.Format("<Project DefaultLocationPath=\"{0}\" Default3DGeometryPath=\"{1}\" CurrentPath=\"{2}\" />", DefaultProjectsPath, Default3DGeometryPath, CurrentProject));
        }

        if(null == (Node=navigator.SelectSingleNode("//CenterSettings")))
        {
          // Пропала секция  <CenterSettings>
          node=navigator.SelectSingleNode("Settings");
          node.AppendChild("<CenterSettings/>"); 
          Node=navigator.SelectSingleNode("//CenterSettings");      
        }

        if (null != (node = navigator.SelectSingleNode("//CenterSettings//CommandButtonStyle"))) node.SetTypedValue(StyleButton);
        else Node.AppendChild(string.Format("<CommandButtonStyle>{0}</CommandButtonStyle>", StyleButton));

        Array list = Application.Current.TryFindResource("ModulusList") as Array;
        if (null != (node = navigator.SelectSingleNode("//CenterSettings//ShowButtonList")))
        {
          foreach (ButtonInfo button in list)
          {
            if (node.MoveToAttribute(button.Title, String.Empty)) { node.SetTypedValue(button.IsShow); node.MoveToParent(); }
            else node.CreateAttribute(String.Empty, button.Title, String.Empty, button.IsShow.ToString());
          }
        }
        else
        {
          StringBuilder nodeText=new StringBuilder("<ShowButtonList ");
          foreach (ButtonInfo button in list)
          {
            nodeText.AppendFormat(" {0}=\"{1}\"",button.Title,button.IsShow);
          }
          nodeText.Append("></ShowButtonList>");

          Node.AppendChild(nodeText.ToString());
        }

        if (null != (node = navigator.SelectSingleNode("//CenterSettings//ExternalToolsList")))
        {
          node.DeleteSelf(); // удаляем секцию вместе со списком
        }

        // восстанавливаем секция  <CenterSettings//ExternalToolsList>
        node=navigator.SelectSingleNode("Settings//CenterSettings");
        node.AppendChild("<ExternalToolsList/>");
        Node = navigator.SelectSingleNode("//CenterSettings//ExternalToolsList");

        foreach (ButtonInfo info in App.UserModulusList)
        {
          String nodeText=String.Format("<Tool ToolsTitle=\"{0}\" ToolsCommand=\"{1}\" ToolsArgument=\"{2}\" /> ", info.Title, info.CommandName, info.Argument);
          Node.AppendChild(nodeText);
        }

        /**********   Simulate   **************/
        if(null == (Node=navigator.SelectSingleNode("//SimulateSettings")))
        {
          // Пропала секция  <SimulateSettings>
          node=navigator.SelectSingleNode("Settings");
          node.AppendChild("<SimulateSettings/>"); 
          Node=navigator.SelectSingleNode("//SimulateSettings");      
        }

        try
        {
          if (null != (node = navigator.SelectSingleNode("//SimulateSettings//DefaultApplicationModel"))) node.SetTypedValue(DefaultModelApp);
          else Node.AppendChild(string.Format("<DefaultApplicationModel>{0}</DefaultApplicationModel>", DefaultModelApp));
        }
        catch { }

        XmlTextWriter writer=new XmlTextWriter(settingspath, Encoding.UTF8);
        writer.Formatting=Formatting.Indented;
        doc.WriteContentTo(writer);
        writer.Close();
      }
      catch(SystemException e) { Debug.WriteLine(e.Message); }
      catch { Debug.WriteLine("### Save System settings is crashed"); }
    }

    /// <summary>
    /// Возвращает полный маршрут к каталогу текущего проекта. 
    /// Если не удается произвольную строку преобразовать в маршрут, возвращается пустая строка 
    /// </summary>
    /// <returns></returns>
    public string GetCurrentProject()
    {
      try
      {
        return App.GetRelativePath(DefaultProjectsPath, CurrentProject);
      }
      catch
      {
        return string.Empty; 
      }
    }

    /// <summary>
    /// Возвращает строку с маршрутом к AppSettings.xml, заданную в ".\\System\\AppSettingsPath.xml"
    /// в неблагоприятных случаях ".\\System\\AppSettings.xml"
    /// </summary>
    /// <returns></returns>
    public string GetSettingsPath()
    {
      string path = Path.GetFullPath(".\\System\\AppSettingsPath.xml");
      string settingsPath = ".\\System\\AppSettings.xml";

      try
      {
        XPathDocument  doc=new XPathDocument(path);
        XPathNavigator navigator=doc.CreateNavigator();
        XPathNavigator node;

        if (null != (node = navigator.SelectSingleNode("//AppSettingsPath"))) settingsPath = node.Value;
      }
      catch (SystemException e) { Debug.WriteLine(e.ToString()); settingsPath=".\\System\\AppSettings.xml"; }
      catch { settingsPath=".\\System\\AppSettings.xml"; }

      return settingsPath;
    }

    public string GetFullSettingsPath()
    {
      string settingsPath = GetSettingsPath();

      return Path.GetFullPath(settingsPath);
    }

    /// <summary>
    /// Запись в ".\\System\\AppSettingsPath.xml"
    /// </summary>
    /// <returns></returns>
    public void WriteAppSettingsPath(string settingsPath)
    {
      string path = Path.GetFullPath(".\\System\\AppSettingsPath.xml");

      try
      {
        XmlDocument doc = new XmlDocument();
        XPathNavigator navigator = doc.CreateNavigator();
        XPathNavigator Node;
        XPathNavigator node;
        XmlElement elem;

        try
        {
          doc.Load(path);
        }
        catch { }

        if (null == (node = navigator.SelectSingleNode("//Settings")))
        {
          // похоже файла-то небыло!
          XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", null, null); doc.AppendChild(xmldecl);
          elem = doc.CreateElement("Settings"); doc.AppendChild(elem);
        }

        Node = navigator.SelectSingleNode("//Settings");

        if (null != (node = navigator.SelectSingleNode("//AppSettingsPath"))) node.SetTypedValue(settingsPath);
        else Node.AppendChild(string.Format("<AppSettingsPath>{0}</AppSettingsPath>", settingsPath));

        XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        doc.WriteContentTo(writer);
        writer.Close();
      }
      catch (SystemException ex) { Debug.WriteLine(ex.ToString()); }
      catch { Debug.WriteLine("### Save AppSettingsPath.xml is crashed"); }
    }

    /// <summary>
    /// Возвращает строку с полным маршрутом к AppSettings.xml, заданную в ".\\System\\AppSettingsPath.xml"
    /// </summary>
    /// <returns></returns>
    public string GetMaterialPath()
    {
      string result=Path.GetFullPath(MaterialPath);

      if(IntPtr.Size == 8) 
      {
        // в x64 режиме не должно быть расширений "mdb" -> заменяем на "sdf"
        if(string.Compare(Path.GetExtension(result),".mdb",true) == 0) 
        { 
          result=Path.ChangeExtension(result,DatabaseExtension);
        } 
      }

      return (result);
    }

  }
}

