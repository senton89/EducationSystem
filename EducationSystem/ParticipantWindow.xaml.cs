using System.Windows;

namespace EducationSystem;

public partial class ParticipantWindow : Window
{
    private int UserId;
    private List<CourseModel> courses;
    private List<CourseInfo> coursesInfo = new();
    public List<UserModel> Instructors { get; set; }
    public List<EnrollmentModel> Enrollments { get; set; }
    
    public ParticipantWindow(int UserID)
    {
        InitializeComponent();
        UserId = UserID;
        RefreshGrids();
    }

    private void RefreshGrids()
    {
        Instructors = DbHelper.GetInstructors();
        Enrollments = DbHelper.GetEnrollments();
        LoadCoursesInfo();
        CoursesGrid.ItemsSource = coursesInfo;
        CoursesGrid.Items.Refresh();
        UserCoursesGrid.ItemsSource = LoadUserCourses();
        UserCoursesGrid.Items.Refresh();
    }

    private List<CourseInfo> LoadUserCourses()
    {
        List<CourseInfo> courses = new();
        var userEnrollments = Enrollments.Where(enrollment => enrollment.UserID == UserId);
        foreach (var enrollment in userEnrollments)
        {
            courses.Add(coursesInfo.First(course => course.CourseId == enrollment.CourseID));
        }
        return courses;
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

    private void CreateEnrollment(object sender, RoutedEventArgs e)
    {
        if (CoursesGrid.SelectedIndex == -1)
        {
            MessageBox.Show("Select a course to add enrollment");
            return;
        }
        EnrollmentModel enrollment = new EnrollmentModel
        {
            UserID = UserId,
            CourseID = (CoursesGrid.SelectedItem as CourseInfo).CourseId,
            EnrollmentDate = DateTime.Now
        };
        DbHelper.SaveEnrollment(enrollment);
        MessageBox.Show("Enrollment added");
        CoursesGrid.SelectedIndex = -1;
    }

    private void RefreshButtonClick(object sender, RoutedEventArgs e)
    {
        RefreshGrids();
    }

    private void DeleteEnrollment(object sender, RoutedEventArgs e)
    {
        DbHelper.DeleteEnrollment((Enrollments.First(
            enrollment => enrollment.UserID == UserId && 
                          enrollment.CourseID == 
                          (UserCoursesGrid.SelectedItem as CourseInfo).CourseId))
            .EnrollmentID);
    }
}