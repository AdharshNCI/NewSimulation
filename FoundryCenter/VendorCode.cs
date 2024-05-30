////////////////////////////////////////////////////////////////////
//                Aladdin Knowledge Systems (Deutschland) GmbH
//                    (c) AKS Ltd., All rights reserved
//
//
// $Id: haspvendorcode.cs,v 1.1 2004/03/09 16:16:06 dieter Exp $
////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////// 


//#define OUTPUTTRACE

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using Aladdin.HASP;
using FoundryCenter.Localization;

namespace FoundryCenter.HaspCode
{
    /// <summary>
    /// Class for holding the vendor code.
    /// This vendor code shall be used within the sample.
    /// </summary>
    public class VendorCode : INotifyPropertyChanged
    {
// в DEMO режиме хорошо-бы этот класс вырезать

#if  VER_CAMITO
      private readonly int DatePosition=0x1B0;
#else
      private readonly int DatePosition = 0x4B0;
#endif
      private readonly int HolderPosition = 0x020;

      private GlobalDll.HaspCode HaspInfo=new GlobalDll.HaspCode();

      private string CurrentVendorCode {get; set; }
      private delegate void InitializeDataDelegate();  // для того, чтобы опрос ключа выполнить в отд.потоке 

      /// <summary>
      /// The Base64 encoded vendor code for a key.
      /// </summary>
      private string[] VendorCodes = {

                //"gyKCgz5bXH9QSMzl0mcykLoOtBDrAfOMld / aWMDJM + 6SuV0cAq4c7kcEFL8NAMsbOBa5n5g1l5nZUtY / "+
                //"PRyDW2V3zg8y/n2ZmsVyu5q52AefzemmJbqeZ05hnyvjzluuIUZm7i6dJL1wfU3/PhPmQOJJ9BWed+SQ"+
                //"yQZDeSybtOYMd9elAzDquAICoOoqRNS2pFiSEUkWyvTOoSiwLyrd9htxBQnzo0UyjP/RkppMRvn0sUnS"+
                //"eUfwVnQrY6/97e60RtLM0WNg85mN5+nQWN7nogyJJa0bR9EeIUgYsWsmsQjKewV5o2fiGeETqMNpvWCm"+
                //"CFsSspMrNtDHg95DI1OtC10P2lWfHrMz0tZsNk6TH/h3jWPbwRxI3nWuVfxnvl1wtcK3p1Wmr0jMMni9"+
                //"pMn2obuWKmnYwh38rLXyZuzwcv+rf1Cj89bJ27pncVizonLEt4GwFH3Ta9mCIdixgC6+eUf49TmAIGc/"+
                //"fPh42AkTT1yvWaZVabLb4qE4pnkmThDVH/UrLzpF9L6yTPHxaWqp2mv1jyWIUMpUON+muxVnO0Iufg17"+
                //"fw610UFA0bI3Np4J7BVTRBJQRmVMIHHcqvMULsRamyrO1ALV9s2uv7ueFGgc4Vax101UcmpMGmzyrfpB"+
                //"3xXxYUQZ7pSsNla/2c42n/vEqGxdjyWb1i+lpTq8A2hc16PiM6xZXlUj+RB5aAUrvQHXFFTahwBAsiye"+
                //"SNxMmTjQGShRklcm/nq7HAFBXaLYqXrqgslmIHvsxo7tWRoIeUZVpnsCGOkKYb4IM65dg5fxAL6MeWcY"+
                //"0kp6jl9HRJwtOy0EmG22N1ZV/9SYydBYoo729OuGVigxghKMVquu9WCX37riJ5NhIscBj0hFYdDgmr5N"+
                //"D1WZHjro8GZqmTQgJy7LF0V+xrHx2elv0sKTxcur+dd3B8o+cjt07Wv+P5SmsxFNly5zgC2D6hjeY8gu"+
                //"fNP+uP3GRY+5ptEwWE20kw==",

              "qChfHzUxDuL5YKf2ZuN+6zBBSlV5HTwbY6w+1YmKsjlyS6vR6lNBPcOIjyxtXtxRd5EapQ0OwJaU/LT8" +
              "0RGiw5AgUe5/aK9sxGwftQWjwAfHgAIIQJtLDVz9svndz+DTvOUeOmPDYTB/HIyl/eGWmDKRGn1Pze0c" +
              "aVgMXrtUp16XH9Gvf7V+xVeTS/GiWSkJRqQP/33ltmwxFpLgVJN4p8X25y235Sjn0qrIPJOFuzZ9U+dm" +
              "/I0oqsiScdNQtWeg1xqhXaPv0ghqdW7cs86eU2/Xr2hHwSNa14LWE/eqmT8NDlPE2KnP013HaWfjG+sm" +
              "CFPBHu67w0NHv3Iy3zMvxjr4kUfDZVF0cSfxHYgNw1tGKzmUgRixqMn5Bn8AJKgUMYmPwmJ5n/VYcGs0" +
              "Ph537rtA+tCreh5EP0UZmkI5yZiAXSttKdxcQwshgBiaho/reiVVLpFPTEE16d7TeQAzohQQh/gULuAY" +
              "KnPx/YkRqVSfexLQRwobd4rmdeM++UCzCOx/nGwDRt4vc7juo96qQxGTlAAilrpwx0OVgs0LxN4X4VMC" +
              "RHG7LtBPt6Xv5KGol1NhWpN4qmXqJNyIm2wzlNwQXKnlz7Kko3a8za3CKn/VwTGAeeY5m7r2QSyMzrJS" +
              "CWaWoFwj3gkBirlc4dzMz5Smot//wYTyQJaskR0URo4Ef9uLHRUszkcSDYrMqg+0hBNP9fOeYmwgzf2l" +
              "ICXNDqniHEujuQ==",
              "GS/2tFOLFMkZGrCt3/Zik7qnV1BjKoKkMOXzPcFVUf2iKck+XBd7vqX0Ga7Fep0dR20EcOzBJo7E8vuJ"+
              "ppARJ7M4Mo6lKBwS/BeL/13yd85EEjMhoCliRi0qbovfnpDua1Ou5clwekUWD4u2yXRYC+tv1sfd3brl"+
              "enUhhonvD+FieIpuROu9MdPi4v1QZurOCTcr3AMMFbm/hjeroCbMUPd45kCs0NsNHWLburQ6ND/EKiDp"+
              "alFuXRy3qoRqDZxgmTCzF38djjJAQFnj5GkBVGRMPHCRv7ubrpBKq6hd5YbB1WiP7ZIjfuaLPPIl/xtG"+
              "pNQkzSrt7ZdyaCeniZkE5CKpCuQb+OQqKTNZg7JRtel3/aegKInP692DoPc05I7RO1FaVX1Gn/2t9oAh"+
              "C+SyLImYsgpRlynl/v0z4yu147z8kD+Ri3fqbmzzhaLpmfFG33Qi0qiLsZ4Ui78LUDMP6GHGGUH60jEV"+
              "FPiw+4zTAVgR5Pt2hxOBZa9+cdvNPJt6OGvsWKxuUFP7c+0m/B3/psNBg59pWyMA8ZBbEgmoSkd1REo+"+
              "IB8lcLoGzras7tR2giTt/zrQ5A7GOBy3tg+3tP0nfGcJo46IBFOJ3umDLnEgf8xzHOl0Qm0ymEBKYhKE"+
              "KnHxhPZb7G3jMfowSfS8VKggYKWPMY8fOQyHu/T5SbYMAcmqFHZ1Q8unmr+/Nz+etPUaaqad7iGUyeoi"+
              "oPeahzK9jFmb7A=="};


      public event PropertyChangedEventHandler PropertyChanged;
      public void OnPropertyChanged(PropertyChangedEventArgs e)
      {
        if (null != PropertyChanged) PropertyChanged(this, e);
      }

      private string statuserror = null;
      public string StatusError
      {
        get { return statuserror; }
        set
        {
          statuserror = value; OnPropertyChanged(new PropertyChangedEventArgs("StatusError"));
        }
      }

      private string licence = null;
      public string Licence
      {
        get { return licence; }
        set 
        { 
          licence = value; OnPropertyChanged(new PropertyChangedEventArgs("Licence")); 
        }
      }

      private DateTime time = DateTime.MinValue;
      public DateTime Time
      {
        get { return (time);  }
        set { time = value; OnPropertyChanged(new PropertyChangedEventArgs("Time")); }
      }

      private string holder = null;
      public string Holder
      {
        get { return holder; }
        set
        {
          holder = value;
          OnPropertyChanged(new PropertyChangedEventArgs("Holder"));
        }
      }


      private void InitializeDataDelegateMetod()  // для того, чтобы опрос ключа выполнить в отд.потоке 
      {
        try
        {
          Licence = GetLicence(); // что делать if не нашли ?
          Time = GetLicenceDate(); 
          Holder = GetHolder();

          Array list = Application.Current.FindResource("ModulusList") as Array;
          foreach (ButtonInfo info in list) if (false == info.IsEnable)
          {
            Hasp hasp = LoginToProgramm(info.Feature);
            if (hasp != null && hasp.IsLoggedIn())
            {
              info.IsEnable = true;
              hasp.Logout();
            }
          }
        }
        catch (Exception)
        {
        }

        // Контроль срока лицензии
        int days = Time.Subtract(DateTime.Today).Days;

        if (days < 0)
        {
            ReportStatus(zMessage.TimePeriodExpired);
        }
        else if (days <= 7)
        {
            ReportStatus(string.Format(zMessage.TimePeriodExpired, days));
        }
      }

      /// <summary>
      /// Constructor - does nothing by default
      /// </summary>
      public VendorCode()
      {
        CurrentVendorCode = null;

        InitializeDataDelegate dg = InitializeDataDelegateMetod; dg.BeginInvoke(null, null);
      }

      private Hasp Login(HaspFeature feature)
      {
        feature.SetOptions(FeatureOptions.IgnoreTS, FeatureOptions.Default);
      //feature.SetOptions(FeatureOptions.NotRemote, FeatureOptions.Default);

      Hasp hasp = new Hasp(feature);
        HaspStatus status = HaspStatus.StatusOk;

        if (null != CurrentVendorCode) status = hasp.Login(CurrentVendorCode);
        else
          foreach (string svc in VendorCodes)
          {
            status = hasp.Login(svc);
            if (HaspStatus.StatusOk == status) { CurrentVendorCode = svc; break; }
          }

#if !(VER_NFS_DEMO || VER_LVM_DEMO)
        if (HaspStatus.StatusOk != status)
          if (HaspFeature.ProgNumDefault == feature) ReportStatus(status);
#endif
        return hasp.IsLoggedIn() ? hasp : null;
      }

      /// <summary>
      /// Performs a default login
      /// </summary>
      private Hasp LoginDefault()
      {
        return Login(HaspFeature.ProgNumDefault);
      }

      /// <summary>
      /// Performs a login using the Programm number
      /// </summary>
      private Hasp LoginToProgramm(App.ModulusFeature feature)
      {
        return Login(HaspFeature.FromProgNum((int)feature));
      }

      private void Logout(Hasp hasp)
      {
        if (null != hasp)
        {
          if (hasp.IsLoggedIn()) hasp.Logout();
          //hasp.Dispose();
        }
      }

      private string GetLicence()
      {
        string result = null;
        Hasp hasp = LoginDefault(); if (null == hasp || !hasp.IsLoggedIn()) return result;

#if OUTPUTTRACE
        MessageBox.Show("Login  - OK");
#endif

        string info = null;
        long ID = 0;

        HaspStatus status = hasp.GetSessionInfo(Hasp.KeyInfo, ref info);
        if (HaspStatus.StatusOk == status)
        {
#if OUTPUTTRACE
          MessageBox.Show("GetSessionInfo  - OK");
#endif

          XmlReader rdr = XmlReader.Create(new StringReader(info));
          while (rdr.Read())
            if (rdr.Name == "haspid")
            {
              ID = rdr.ReadElementContentAsLong();

#if OUTPUTTRACE
              MessageBox.Show(String.Format("ID = {0}",ID));
#endif
              if(true==HaspInfo.MKMKey.ContainsKey((int)ID)) result=HaspInfo.MKMKey[(int)ID].ToString();
              else if(true==HaspInfo.NFKey.ContainsKey((int)ID)) result=HaspInfo.NFKey[(int)ID].ToString();
              break;
            }
        }
        hasp.Logout();

#if OUTPUTTRACE
        MessageBox.Show(String.Format("Licence - {0}",result));
#endif

        return result;
      }

      private DateTime GetLicenceDate()
      {
        DateTime result = DateTime.MinValue;
        Hasp hasp = LoginDefault(); if (null == hasp || !hasp.IsLoggedIn()) return result;

        try
        {
          HaspFile file = hasp.GetFile(HaspFileId.Main);
          if (file.IsLoggedIn())
          {
            byte[] buffer = new byte[10];

            file.FilePos=DatePosition;
            HaspStatus status = file.Read(buffer, 0, buffer.Length);
            if (HaspStatus.StatusOk == status)
            {
              string y = Encoding.ASCII.GetString(buffer, 0, 4);
              string m = Encoding.ASCII.GetString(buffer, 5, 2);
              string d = Encoding.ASCII.GetString(buffer, 8, 2);
              result = new DateTime(int.Parse(Encoding.ASCII.GetString(buffer, 0, 4)),
                                   int.Parse(Encoding.ASCII.GetString(buffer, 5, 2)),
                                   int.Parse(Encoding.ASCII.GetString(buffer, 8, 2)), 23, 59, 59);
            }
          }
        }
        catch { };

        hasp.Logout();
        return result;
      }

      private string GetHolder()
      {
        string result = null;
        Hasp hasp = LoginDefault(); if(null == hasp || !hasp.IsLoggedIn())return result;

#if OUTPUTTRACE
        MessageBox.Show("GetHolder: Login  - OK");
#endif

        try
        {
          HaspFile file = hasp.GetFile(HaspFileId.Main);
          if(file.IsLoggedIn())
          {
            byte[] buffer = new byte[40];

            file.FilePos = HolderPosition;
            HaspStatus status = file.Read(buffer, 0, buffer.Length);
            if(HaspStatus.StatusOk == status)
            {
              string text=Encoding.ASCII.GetString(buffer, 0, 32);
              Match match=Regex.Match(text,"\\#(?<Name>.+)\\#");
              result = match.Groups["Name"].Value;
            }
          }
        }
        catch { };

        hasp.Logout();
        return result;
      }

      public string GetFullData()
      {
        Hasp hasp = LoginDefault(); if (null == hasp || !hasp.IsLoggedIn()) return null;
        return (null);
      }

      /// <summary>
      /// Performs a login using the default feature
      /// and retrieves the update information using
      /// the Hasp's GetSessionInfo method.
      /// </summary>
      public  void RunC2V()    
      {
        Hasp hasp = LoginDefault(); if (null == hasp || !hasp.IsLoggedIn()) return;

        string info = null;

        // create an "Save As" dialog instance.
        var saveDialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
        saveDialog.Title = zText.DlgCreateC2VTitle;
        saveDialog.Filter = zText.DlgCreateC2VFilter;
        saveDialog.FilterIndex = 1;
        saveDialog.DefaultExt = "c2v";
        saveDialog.FileName = Licence + ".c2v";

        try
        {
          if (true != saveDialog.ShowDialog()) return;

          HaspStatus status = hasp.GetSessionInfo(Hasp.UpdateInfo, ref info);
          if (HaspStatus.StatusOk != status) { ReportStatus(status); return; }

          // save the info in a file
          Stream stream = saveDialog.OpenFile();
          if (null == stream)
          {
            MessageBox.Show("Failed to save file \"" + saveDialog.FileName + "\".", zText.DlgCreateC2VTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          }
          else
          {
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(info);
            writer.Close();
            stream.Close();
          }
        }
        catch (Exception e)
        {
          MessageBox.Show(e.ToString(), zText.DlgCreateC2VTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        hasp.Logout();
      }
      /// <summary>
      /// Updates the key using the passed update string
      /// and writes the returned acknowledge (if available)
      /// into the referenced TextBox.
      /// </summary>
      public  void RunV2C()
      {
         Hasp hasp = LoginDefault(); if (null == hasp || !hasp.IsLoggedIn()) return;

         // create an instance of the "File Open" dialog
         var openDialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
         openDialog.Title = zText.DlgOpenV2CTitle;
         openDialog.Filter = zText.DlgOpenV2CFilter;
         openDialog.DefaultExt = "v2c";

         if (true != openDialog.ShowDialog()) return;

         string info = null;

         try
         {
           // open the file and read its contents
           Stream stream = openDialog.OpenFile();
           if (null == stream)
           {
             MessageBox.Show("Failed to open file \"" + openDialog.FileName + "\".","HASP Update",MessageBoxButton.OK,MessageBoxImage.Error);
           }
           else
           {
             StreamReader reader = new StreamReader(stream);
	
	           Int64 size = stream.Length;
	           char[] chars = new char[(int)size];
	
	           reader.Read(chars, 0, chars.Length);
	           info = new string(chars);
	
	           reader.Close();
	           stream.Close();
           }
        }
        catch (Exception e)
        {
          MessageBox.Show(e.ToString(),"HASP Update",MessageBoxButton.OK,MessageBoxImage.Error);
          return;
        }

        string ack = null;

        // perform the update
        // please note that the Hasp's Update method is static.
        HaspStatus status = Hasp.Update(info, ref ack);
        if (HaspStatus.StatusOk != status) ReportStatus(status);
        hasp.Logout();

        // обновить инф-цию из ключа
        InitializeDataDelegate dg = InitializeDataDelegateMetod; dg.BeginInvoke(null, null);
      }
      public  void ReportStatus(HaspStatus status)
      {
        StatusError=string.Format("HaspKey error status: {0} ({1})", HaspInfo.ErrorCollection[(int)status], (int)status);
      }
      public void ReportStatus(string text)
      {
        StatusError = text; 
      }
    }
}
