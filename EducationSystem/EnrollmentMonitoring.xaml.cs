using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EducationSystem;

namespace EducationSystem
{
    public partial class EnrollmentMonitoring : Window
    {
        //TODO: курсы для пользователей(форма со списком), добавление и изменение только для админов
        private List<EnrollmentModel> enrollments;
        public EnrollmentMonitoring()
        {
            InitializeComponent();
            LoadEnrollments();
            EnrollmentsGrid.ItemsSource = enrollments;
        }
        private void LoadEnrollments()
        {
            // enrollments = DbHelper.GetEnrollments();
        }
        private void CreateEnrollment(object sender, RoutedEventArgs e)
        {
            ManageEnrollmentWindow createEnrollmentWindow = new ManageEnrollmentWindow(new EnrollmentModel());
            createEnrollmentWindow.Show();
        }
        private void EditEnrollment(object sender, RoutedEventArgs e)
        {
            if (EnrollmentsGrid.SelectedItem == null)
            MessageBox.Show("Please choose an enrollment");
                return;
            
            var selectedEnrollment = (EnrollmentModel)EnrollmentsGrid.SelectedItem;
            UpdateEnrollment(selectedEnrollment);
        }
        private void DeleteEnrollment(object sender, RoutedEventArgs e)
        {
            if (EnrollmentsGrid.SelectedItem != null)
            {
                // var selectedEnrollment = (Enrollment)EnrollmentsGrid.SelectedItem;
                // DbHelper.DeleteEnrollment(selectedEnrollment.EnrollmentID);
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
            EnrollmentsGrid.ItemsSource = enrollments;
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