using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
namespace EducationSystem
{
    public partial class ReportMonitoring : Window
    {
        private List<ReportModel> reports { get; set; }
        private List<ReportInfo> ReportsInfos { get; set; }
        public List<CourseModel> Courses { get; set; }
        public ReportMonitoring()
        {
            InitializeComponent();
            Courses = DbHelper.GetCourses();
            LoadReports();
            DataContext = this;
            ReportsGrid.ItemsSource = ReportsInfos;
            CoursesList.ItemsSource = Courses;
        }

        private void LoadReports()
        {
            reports = DbHelper.GetReports(); // Предполагается, что есть метод для получения отчетов
            ReportsInfos = new List<ReportInfo>();
            foreach (ReportModel report in reports)
            {
                var courseInfo = Courses.FirstOrDefault(p => p.CourseId == report.CourseID);
                var reportInfo = new ReportInfo
                {
                    ReportID = report.ReportID,
                    Course = courseInfo.Title,
                    TotalHours = report.TotalHours,
                    AvgGrade = report.AvgGrade??0,
                    CompletionRate = report.CompletionRate
                };
                ReportsInfos.Add(reportInfo);
            }
        }

        private void CreateReport_Click(object sender, RoutedEventArgs e)
        {
            var selectedCourse = Courses.FirstOrDefault(course => course.CourseId == (CoursesList.SelectedItem as CourseModel).CourseId);
            var enrollments = DbHelper.GetEnrollmentsByCourse(selectedCourse.CourseId);
            var validGrades = enrollments.Where(enrollment => enrollment.Grade.HasValue).
                Select(enrollment => enrollment.Grade.Value);
            var report = new ReportModel
            {
                CourseID = selectedCourse.CourseId,
                TotalHours = selectedCourse.Duration,
                AvgGrade = validGrades.Any() ? validGrades.Average() : 0,
                CompletionRate = (enrollments.Count(enrollment => enrollment.CompletionDate.HasValue) /
                    (enrollments.Count>0 ? enrollments.Count : 1) * 100),
            };
            DbHelper.SaveReport(report);
        }
        
        private void DeleteReport_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsGrid.SelectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите отчет.");
                return;
            }
            DbHelper.DeleteReport((ReportsGrid.SelectedItem as ReportInfo).ReportID);
            ReportsGrid.SelectedIndex = -1;
        }

        private void RefreshReports()
        {
            LoadReports();
            ReportsGrid.ItemsSource = ReportsInfos;
            ReportsGrid.Items.Refresh();
        }

        private void ReturnToMain(object sender, RoutedEventArgs e)
        {
            AdministratorWindow adminWindow = new AdministratorWindow();
            adminWindow.Show();
            Close();
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            RefreshReports();
        }
    }
}