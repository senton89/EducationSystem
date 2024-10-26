using System.Windows;

namespace EducationSystem;

public partial class AdministratorWindow : Window
{
    public AdministratorWindow()
    {
        InitializeComponent();
    }

    private void RegistrationButtonClick(object sender, RoutedEventArgs e)
    {
        Registration registration = new Registration();
        registration.Show();
    }

    private void CourseManage(object sender, RoutedEventArgs e)
    {
        CourseMonitoring courseMonitoring = new(Roles.Administrator, 0);
        courseMonitoring.Show();
        Close();    
    }

    private void EnrollmentManage(object sender, RoutedEventArgs e)
    {
        EnrollmentMonitoring enrollmentMonitoring = new();
        enrollmentMonitoring.Show();
        Close();
    }

    private void CreateReport(object sender, RoutedEventArgs e)
    {
        ReportMonitoring reportMonitoring = new();
        reportMonitoring.Show();
        Close();
    }

    private void ViewMaterials(object sender, RoutedEventArgs e)
    {
        ViewMaterials viewMaterials = new();
        viewMaterials.Show();
        Close();
    }
}