using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EducationSystem
{
    public partial class ManageEnrollmentWindow : Window
    {
        private readonly EnrollmentModel _enrollment;
        public List<UserModel> Users { get; set; }
        public List<CourseModel> Courses { get; set; }
        private List<UserModel> Participants;
        public EnrollmentModel Enrollment => _enrollment;

        public ManageEnrollmentWindow(EnrollmentModel enrollment)
        {
            InitializeComponent();
            _enrollment = enrollment;
            _enrollment.EnrollmentDate = enrollment.EnrollmentDate==DateTime.MinValue ? DateTime.Now : enrollment.EnrollmentDate;
            _enrollment.CompletionDate = DateTime.Now;
            DataContext = this;
            LoadUsers();
            LoadCourses();
            LoadParticipants();
            if (enrollment.EnrollmentID == null)
            {
                ParticipantsList.ItemsSource = ConvertListToUsersInfo(Participants);
                CoursesList.ItemsSource = Courses;
            }
            else
            {
                ParticipantsList.ItemsSource = ConvertListToUsersInfo(Participants).Where(participant => participant.UserId == enrollment.UserID);
                CoursesList.ItemsSource = Courses.Where(course => course.CourseId == enrollment.CourseID);
            }
        }
        
        public static List<UserInfo> ConvertListToUsersInfo(List<UserModel> Users)
        {
            var usersInfo = Users.Select(user => new UserInfo
            {
                UserId = user.UserID,
                DisplayName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Department = user.Department
            }).ToList();
            return usersInfo;
        }
        
        private void LoadUsers()
        {
            Users = DbHelper.GetUsers();
        }

        private void LoadCourses()
        {
            Courses = DbHelper.GetCourses();
        }
        
        private void LoadParticipants()
        {
            Participants = DbHelper.GetParticipant();
        }

        private void SaveEnrollment(object sender, RoutedEventArgs e)
        {
            _enrollment.UserID = (ParticipantsList.SelectedItem as UserInfo).UserId;
            _enrollment.CourseID = (CoursesList.SelectedItem as CourseModel).CourseId;
            if (ValidateEnrollment())
            {
                DbHelper.SaveEnrollment(_enrollment);
                MessageBox.Show("Запись успешно сохранена");
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста исправьте ошибки");
            }
        }

        private bool ValidateEnrollment()
        {
            // Validate user ID
            if (ParticipantsList.SelectedIndex<0)
            {
                MessageBox.Show("Пожалуйста выберите обучающегося");
                return false;
            }

            // Validate course ID
            if (CoursesList.SelectedIndex<0)
            {
                MessageBox.Show("Пожалуйста выберите курс");
                return false;
            }

            // Validate enrollment date
            if (Enrollment.EnrollmentDate == DateTime.MinValue)
            {
                MessageBox.Show("Выберите дату записи");
                return false;
            }

            // Validate completion date
            if (Enrollment.CompletionDate < Enrollment.EnrollmentDate)
            {
                MessageBox.Show("Дата завершения должна быть не ранее даты зачисления");
                return false;
            }

            if (Enrollment.Grade.HasValue)
            {
                if (Enrollment.Grade < 2 || Enrollment.Grade > 5)
                {
                    MessageBox.Show("Оценка должна быть в диапазоне от 2 до 5");
                    return false;
                }
            }
            
            return true;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}