using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace EducationSystem;

public partial class CourseMonitoring : Window
{
    private int userId;
    private Roles userRole;
    private List<CourseModel> courses;
    private List<CourseInfo> coursesInfo = new();
    public List<UserModel> Instructors { get; }
    
    public CourseMonitoring(Roles role, int userID)
    {
        InitializeComponent();
        Instructors = DbHelper.GetInstructors();
        LoadCoursesInfo();
        CoursesGrid.ItemsSource = coursesInfo;
        userRole = role;
        userId = userID;
    }

    private void LoadCoursesInfo()
    {
        courses = DbHelper.GetCourses();
        coursesInfo.Clear();
        coursesInfo = GetCourseInfo(courses);
    }

    private List<CourseInfo> GetCourseInfo(List<CourseModel> courses)
    {
        List<CourseInfo> coursesInfo = new();
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
        return coursesInfo;
    }

    private void CreateCourse(object sender, RoutedEventArgs e)
    {
        ManageCourseWindow createCourseWindow = new ManageCourseWindow(new CourseModel(), userRole);
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
        ManageCourseWindow manageCourseWindow = new ManageCourseWindow(course,userRole);
        manageCourseWindow.Show();
    }

    private void ReturnToMain(object sender, RoutedEventArgs e)
    {
        ReturnToMain();
    }
    
    private void ReturnToMain()
    {
        switch (userRole)
        {
            case Roles.Instructor:
                InstructorWindow instructorWindow = new InstructorWindow(userId);
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
    
    private void SearchCourses(object sender, RoutedEventArgs e)
    {
        string searchText = SearchTextBox.Text.ToLower();
        var filteredCourses = courses.Where(course =>
            course.Title.ToLower().Contains(searchText) ||
            course.Description.ToLower().Contains(searchText) ||
            (Instructors.First(instructor => instructor.UserID == course.InstructorId)
                .FirstName + " " + 
            Instructors.First(instructor => instructor.UserID == course.InstructorId)
                .LastName).Contains(searchText)).ToList();
        CoursesGrid.ItemsSource = GetCourseInfo(filteredCourses);
    }
}