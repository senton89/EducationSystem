using System.Windows;


namespace EducationSystem
{
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginTextBox.Text;
            string password = PasswordBox.Password;
            if (DbHelper.AuthenticateUser(username, password, out string role))
            {
                MessageBox.Show($"Авторизация успешна! Роль: {role}");
                // Можно продолжать процесс аутентификации, перенаправление пользователя и т.п.
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }
    }
}