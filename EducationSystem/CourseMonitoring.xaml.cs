using System.Windows;
using System.Windows.Input;

namespace EducationSystem;

public partial class CourseMonitoring : Window
{
    private List<CourseModel> courses;
    
    public CourseMonitoring()
    {
        InitializeComponent();
        CoursesGrid.ItemsSource = LoadCourses();
    }

    private List<CourseModel> LoadCourses()
    {
        courses = DbHelper.GetCourses();
        return courses;
    }

    private void CreateCourse(object sender, RoutedEventArgs e)
    {
        ManageCourseWindow createCourseWindow = new ManageCourseWindow(new CourseModel());
        createCourseWindow.Show();
        CoursesGrid.ItemsSource = LoadCourses();
    }


    private void EditCourse(object sender, RoutedEventArgs e)
    {
        if (CoursesGrid.SelectedItem == null)
        {
            MessageBox.Show("Please choose course");
            return;
        }
        UpdateCourse(courses.FirstOrDefault(
            (course => course.CourseId == (CoursesGrid.SelectedItem as CourseModel).CourseId),
            new CourseModel()));
    }

    private void DeleteCourse(object sender, RoutedEventArgs e)
    {
        if (CoursesGrid.SelectedIndex != null)
        {
            DbHelper.DeleteCourse(courses
                .FirstOrDefault(course => course.CourseId == (CoursesGrid.SelectedItem as CourseModel).CourseId)
                .CourseId);
        }
        else
            MessageBox.Show("Please choose course");
        CoursesGrid.ItemsSource = LoadCourses();
    }

    private void RefreshCourse(object sender, RoutedEventArgs e)
    {
        CoursesGrid.ItemsSource = LoadCourses();
    }

    private void UpdateCourse(CourseModel course)
    {
        ManageCourseWindow manageCourseWindow = new ManageCourseWindow(course);
        manageCourseWindow.Show();
    }
}