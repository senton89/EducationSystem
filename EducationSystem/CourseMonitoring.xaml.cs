using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace EducationSystem;

public partial class CourseMonitoring : Window
{
    private List<CourseModel> courses;
    private List<CourseInfo> coursesInfo = new();
    public List<UserModel> Instructors { get; set; }
    
    public CourseMonitoring()
    {
        InitializeComponent();
        Instructors = DbHelper.GetInstructors();
        LoadCoursesInfo();
        CoursesGrid.ItemsSource = coursesInfo;
    }

    private void LoadCoursesInfo()
    {
        courses = DbHelper.GetCourses();
        coursesInfo.Clear();
        foreach (CourseModel course in courses)
        {
            var instructorInfo = GetInstructorInfo(course.InstructorId);
            var courseInfo = new CourseInfo
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Duration = course.Duration,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                Instructor = instructorInfo.DisplayName 
            };
            coursesInfo.Add(courseInfo);
        }
    }

    private void CreateCourse(object sender, RoutedEventArgs e)
    {
        ManageCourseWindow createCourseWindow = new ManageCourseWindow(new CourseModel());
        createCourseWindow.Show();
    }


    private void EditCourse(object sender, RoutedEventArgs e)
    {
        if (CoursesGrid.SelectedItem == null)
        {
            MessageBox.Show("Please choose course");
            return;
        }
        UpdateCourse(courses.FirstOrDefault(
            (course => course.CourseId == (CoursesGrid.SelectedItem as CourseInfo).CourseId),
            new CourseModel()));
    }

    private void DeleteCourse(object sender, RoutedEventArgs e)
    {
        if (CoursesGrid.SelectedIndex != null)
        {
            DbHelper.DeleteCourse(courses
                .First(course => course.CourseId == (CoursesGrid.SelectedItem as CourseInfo).CourseId)
                .CourseId);
        }
        else
            MessageBox.Show("Please choose course");
        RefreshGrid();
    }

    private void RefreshCourse(object sender, RoutedEventArgs e)
    {
        RefreshGrid();
    }
    public void RefreshGrid()
    {
        LoadCoursesInfo();
        CoursesGrid.ItemsSource = coursesInfo;
        CoursesGrid.Items.Refresh();
    }

    private void UpdateCourse(CourseModel course)
    {
        ManageCourseWindow manageCourseWindow = new ManageCourseWindow(course);
        manageCourseWindow.Show();
    }

    private void ReturnToMain(object sender, RoutedEventArgs e)
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }
    private UserInfo GetInstructorInfo(int instructorId)
    {
        var instructor = Instructors.First(instructor => instructor.UserID == instructorId);
        return new UserInfo
        {
            DisplayName = $"{instructor.FirstName} {instructor.LastName}",
            Email = instructor.Email,
            Department = instructor.Department
        };
    }
}