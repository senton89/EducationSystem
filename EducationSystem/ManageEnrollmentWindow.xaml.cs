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
            DataContext = this;
            LoadUsers();
            LoadCourses();
            LoadParticipants();
            ParticipantsList.ItemsSource = ConvertListToUsersInfo(Participants);
            CoursesList.ItemsSource = Courses;
        }
        
        public static List<UserInfo> ConvertListToUsersInfo(List<UserModel> Users)
        {
            var instructorsInfo = Users.Select(instructor => new UserInfo
            {
                DisplayName = $"{instructor.FirstName} {instructor.LastName}",
                Email = instructor.Email,
                Department = instructor.Department
            }).ToList();
            return instructorsInfo;
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
            if (ValidateEnrollment())
            {
                // DbHelper.SaveEnrollment(_enrollment);
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
            if (Enrollment.UserID <= 0)
            {
                MessageBox.Show("User ID must be a positive number.");
                return false;
            }

            // Validate course ID
            if (Enrollment.CourseID <= 0)
            {
                MessageBox.Show("Course ID must be a positive number.");
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

            // Validate grade
            if (Enrollment.Grade < 0 || Enrollment.Grade > 100)
            {
                MessageBox.Show("Grade must be between 0 and 100.");
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