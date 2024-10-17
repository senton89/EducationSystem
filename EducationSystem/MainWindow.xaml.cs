using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EducationSystem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
    }


    private void ButtonAuth_OnClick(object sender, RoutedEventArgs e)
    {
        Authorization authorization = new Authorization();
        authorization.Show();
    }

    private void ButtonReg_OnClick(object sender, RoutedEventArgs e)
    {
        Registration registration = new Registration();
        registration.Show();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        CourseMonitoring courseMonitoring = new CourseMonitoring();
        courseMonitoring.Show();
        Close();
    }
}