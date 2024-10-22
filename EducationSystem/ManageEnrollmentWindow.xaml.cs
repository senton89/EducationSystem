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
            _enrollment.EnrollmentDate = DateTime.Now;
            DataContext = this;
            LoadUsers();
            LoadCourses();
            LoadParticipants();
            ParticipantsList.ItemsSource = ConvertListToUsersInfo(Participants);
            CoursesList.ItemsSource = Courses;
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
                MessageBox.Show("Enrollment saved successfully.");
                Close();
            }
            else
            {
                MessageBox.Show("Please fix the errors.");
            }
        }

        private bool ValidateEnrollment()
        {
            // Validate user ID
            if (ParticipantsList.SelectedIndex<0)
            {
                MessageBox.Show("Please select at least one participant.");
                return false;
            }

            // Validate course ID
            if (CoursesList.SelectedIndex<0)
            {
                MessageBox.Show("Please select at least one course.");
                return false;
            }

            // Validate enrollment date
            if (Enrollment.EnrollmentDate == DateTime.MinValue)
            {
                MessageBox.Show("Enrollment date is required.");
                return false;
            }

            // Validate completion date
            if (Enrollment.CompletionDate < Enrollment.EnrollmentDate)
            {
                MessageBox.Show("Completion date must be later than enrollment date.");
                return false;
            }

            if (!Enrollment.Grade.HasValue)
            {
                MessageBox.Show("Grade is required.");
                return false;
            }

            // Validate grade
            if (Enrollment.Grade < 2 || Enrollment.Grade > 5)
            {
                MessageBox.Show("Grade must be between 2 and 5.");
                return false;
            }

            return true;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}