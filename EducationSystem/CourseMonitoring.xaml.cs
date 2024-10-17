using System.Windows;

namespace EducationSystem;

public partial class CourseMonitoring : Window
{
    public CourseMonitoring()
    {
        InitializeComponent();
        LoadCourses();
    }

    private void LoadCourses()
    {
        CoursesGrid.ItemsSource = DbHelper.GetCourses();
    }

    private void CreateCourse(object sender, RoutedEventArgs e)
    {
        CreateCourseWindow createCourseWindow = new CreateCourseWindow(new CourseModel());
        createCourseWindow.Show();
    }


    private void EditCourse(object sender, RoutedEventArgs e)
    {
        
    }

    private void DeleteCourse(object sender, RoutedEventArgs e)
    {
      
    }
}