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
            if (DbHelper.AuthenticateUser(username, password, out Roles role,out int userID))
            {
                switch (role)
                {
                    case Roles.Participant:
                        ParticipantWindow participantWindow = new ParticipantWindow(userID);
                        participantWindow.Show();
                        Close();
                        return;
                    case Roles.Instructor:
                        InstructorWindow instructorWindow = new InstructorWindow();
                        instructorWindow.Show();
                        Close();
                        return;
                    case Roles.Administrator:
                        AdministratorWindow adminWindow = new AdministratorWindow();
                        adminWindow.Show();
                        Close();
                        return;
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }
    }
}