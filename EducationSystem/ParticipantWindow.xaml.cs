using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace EducationSystem;

public partial class ParticipantWindow : Window
{
    private int UserId;
    private List<MaterialModel> materials;
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
            MessageBox.Show("Выберите курс для записи");
            return;
        }
        EnrollmentModel enrollment = new EnrollmentModel
        {
            UserID = UserId,
            CourseID = (CoursesGrid.SelectedItem as CourseInfo).CourseId,
            EnrollmentDate = DateTime.Now
        };
        DbHelper.SaveEnrollment(enrollment);
        MessageBox.Show("Вы успешно записаны");
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

    private void View_Materials(object sender, RoutedEventArgs e)
    {
        if (UserCoursesGrid.SelectedIndex != -1)
        {
            materials = DbHelper.GetMaterialsByCourse((UserCoursesGrid.SelectedItem as CourseInfo).CourseId);
            MaterialsGrid.ItemsSource = new List<MaterialModel>();
            if (materials.Count > 0)
                MaterialsGrid.ItemsSource = materials;
        }
        else
            MessageBox.Show("Выберите курс для отображения материала");
    }

    private void MaterialDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (MaterialsGrid.SelectedItem != null)
        {
            // Открываем файл с использованием полного пути
            var materialName = (MaterialsGrid.SelectedItem as MaterialModel).MaterialName;
            var materialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Materials/") + materialName;
            Process.Start(new ProcessStartInfo
            {
                FileName = materialPath,
                UseShellExecute = true // Используем оболочку для открытия файла
            });
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите файл из списка.");
        }
    }
}