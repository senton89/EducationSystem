using System.Windows;

namespace EducationSystem;

public partial class Registration : Window
{
    public Registration()
    {
        InitializeComponent();
    }
    
    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {

        string firstName = FirstNameTextBox.Text;

        string lastName = LastNameTextBox.Text;

        string email = EmailTextBox.Text;

        string password = PasswordBox.Password;

        string role = RoleComboBox.SelectionBoxItem.ToString();

        string phoneNumber = PhoneNumberTextBox.Text;

        string department = DepartmentTextBox.Text;

        if(DbHelper.AddNewUser(firstName, lastName, email, password, role, phoneNumber, department)) 
            Close();
    }
}