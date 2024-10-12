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

        string role = "Participant"; // Можно выбрать роль из выпадающего списка

        string phoneNumber = PhoneNumberTextBox.Text;

        string department = DepartmentTextBox.Text;

        DbContext.AddNewUser(firstName, lastName, email, password, role, phoneNumber, department);

        MessageBox.Show("Пользователь успешно добавлен!");

    }
}