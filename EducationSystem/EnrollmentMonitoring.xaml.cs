using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EducationSystem;

namespace EducationSystem
{
    public partial class EnrollmentMonitoring : Window
    {
        //TODO: курсы для пользователей(форма со списком), добавление и изменение только для админов, поиск по таблицам
        private List<EnrollmentModel> enrollments { get; set; }
        private List<EnrollmentInfo> EnrollmentsInfos { get; set; }
        public List<UserModel> Participants { get; set; }
        public List<CourseModel> Courses { get; set; }
        
        public EnrollmentMonitoring()
        {
            InitializeComponent();
            Participants = DbHelper.GetParticipant();
            Courses = DbHelper.GetCourses();
            LoadEnrollments();
            DataContext = this;
            
            EnrollmentsGrid.ItemsSource = EnrollmentsInfos;
        }
        private void LoadEnrollments()
        {
            enrollments = DbHelper.GetEnrollments();
            EnrollmentsInfos = new List<EnrollmentInfo>();
            foreach (EnrollmentModel enrollment in enrollments)
            {
                var userInfo = Participants.FirstOrDefault(p => p.UserID == enrollment.UserID);
                var courseInfo = Courses.FirstOrDefault(p => p.CourseId == enrollment.CourseID);
                var enrollmentInfo = new EnrollmentInfo
                {
                    EnrollmentID = enrollment.EnrollmentID,
                    Participant = userInfo.FirstName + " " + userInfo.LastName,
                    ParticipantID = userInfo.UserID,
                    Course = courseInfo.Title,
                    CourseID = courseInfo.CourseId,
                    CompletionDate = Convert.ToDateTime(enrollment.CompletionDate),
                    EnrollmentDate = Convert.ToDateTime(enrollment.EnrollmentDate),
                    Grade = enrollment.Grade,
                };
                EnrollmentsInfos.Add(enrollmentInfo);
            }
        }
        
        private void CreateEnrollment(object sender, RoutedEventArgs e)
        {
            ManageEnrollmentWindow createEnrollmentWindow = new ManageEnrollmentWindow(new EnrollmentModel());
            createEnrollmentWindow.Show();
        }
        private void EditEnrollment(object sender, RoutedEventArgs e)
        {
            if (EnrollmentsGrid.SelectedItem == null)
            {
                MessageBox.Show("Please choose an enrollment");
                return;
            }

            var listElement = (EnrollmentInfo)EnrollmentsGrid.SelectedItem;
            var selectedEnrollment = new EnrollmentModel
            {
                EnrollmentID = listElement.EnrollmentID,
                UserID = Participants.FirstOrDefault(
                    user => user.UserID == listElement.ParticipantID).UserID,
                CourseID = Courses.FirstOrDefault(
                    course => course.CourseId == listElement.CourseID).CourseId,
                CompletionDate = listElement.CompletionDate,
                EnrollmentDate = listElement.EnrollmentDate,
                Grade = listElement.Grade,
            };
            UpdateEnrollment(selectedEnrollment);
        }
        private void DeleteEnrollment(object sender, RoutedEventArgs e)
        {
            if (EnrollmentsGrid.SelectedItem != null)
            {
                var selectedEnrollment = (EnrollmentModel)EnrollmentsGrid.SelectedItem;
                DbHelper.DeleteEnrollment(selectedEnrollment.EnrollmentID);
            }
            else
            {
                MessageBox.Show("Please choose an enrollment");
            }
            RefreshEnrollments();
        }
        private void RefreshEnrollments(object sender, RoutedEventArgs e)
        {
            RefreshEnrollments();
        }
        private void RefreshEnrollments()
        {
            LoadEnrollments();
            EnrollmentsGrid.ItemsSource = EnrollmentsInfos;
            EnrollmentsGrid.Items.Refresh();
        }
        private void UpdateEnrollment(EnrollmentModel enrollment)
        {
            ManageEnrollmentWindow manageEnrollmentWindow = new ManageEnrollmentWindow(enrollment);
            manageEnrollmentWindow.Show();
        }
        private void ReturnToMain(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}