using System;
using System.Windows;
using System.Windows.Media;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

using FoundryCenter.Localization;

namespace FoundryCenter
{
  /// <summary>
  /// Interaction logic for DlgAbout.xaml
  /// </summary>
  public partial class DlgAbout : Window
  {
    public DlgAbout()
    {
      InitializeComponent();
  
      VisualBrush myVisualBrush = new VisualBrush();
			myVisualBrush.Visual=(Application.Current.MainWindow as MainWindow).ProductLogo;
			ProductLogo.Fill = myVisualBrush;

			CompanyText.Text = FindResource("CompanyAdress") as String;

#if (VER_LVM || VER_LVM_DEMO || VER_LVM_TIME)
	  Title = zText.DlgAboutTitle + "   " + zText.ProductNameLVM;
#elif VER_NFS_HPD_LIGHT
      Title = zText.DlgAboutTitle + "   " + zText.ProductNameNFS_HPD_LIGHT;
#elif VER_NFS_GRAVITY_LIGHT
      Title = zText.DlgAboutTitle + "   " + zText.ProductNameNFS_GRAVITY_LIGHT;
#else
	  Title = zText.DlgAboutTitle + "   " + zText.ProductNameNFS;
#endif
        }
    }
}
