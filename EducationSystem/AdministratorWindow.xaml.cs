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
        Close();
    }

    private void CourseManage(object sender, RoutedEventArgs e)
    {
        CourseMonitoring courseMonitoring = new(Roles.Administrator);
        courseMonitoring.Show();
        Close();    
    }

    private void EnrollmentManage(object sender, RoutedEventArgs e)
    {
        EnrollmentMonitoring enrollmentMonitoring = new();
        enrollmentMonitoring.Show();
        Close();
    }
}