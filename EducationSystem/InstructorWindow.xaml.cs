using System.Diagnostics;
using System.Windows;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;

namespace EducationSystem;

public partial class InstructorWindow : Window
{
    private List<MaterialModel> materials = new();
    private int instructorID;
    public InstructorWindow(int instructorId)
    {
        InitializeComponent();
        instructorID = instructorId;
        CoursesGrid.ItemsSource = DbHelper.GetCourses().Where(course => course.InstructorId == instructorID);
    }
    
    private void UploadFilesButton_Click(object sender, RoutedEventArgs e)
    {
        // Создаем окно выбора файлов
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = true, // Позволяем выбрать несколько файлов
            Filter = "Documents|*.pdf;*.doc;*.docx;*.txt|All files|*.*" // Указываем фильтр для файлов
        };
        if (openFileDialog.ShowDialog() == true)
        {
            string projectFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Materials");
            // Проверяем существует ли папка materials, если нет - создаем ее
            if (!Directory.Exists(projectFolderPath))
            {
                Directory.CreateDirectory(projectFolderPath);
            }
            foreach (var file in openFileDialog.FileNames)
            {
                // Получаем имя файла
                string fileName = Path.GetFileName(file);
                string destFilePath = Path.Combine(projectFolderPath, fileName);
                // Копируем файл в папку materials
                try
                {
                    if (CoursesGrid.SelectedIndex != -1)
                    {
                        File.Copy(file, destFilePath, true); // true для перезаписи файла, если он уже существует
                        DbHelper.AddMaterialToDatabase(fileName, (CoursesGrid.SelectedItem as CourseModel).CourseId);
                        MessageBox.Show($"{fileName} загружен успешно!", "Успех", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки {fileName}: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }

    private void CreateCourse(object sender, RoutedEventArgs e)
    {
        ManageCourseWindow manageCourseWindow = new ManageCourseWindow(new CourseModel(),Roles.Instructor,instructorID);
        manageCourseWindow.Show();
        Close();
    }
    
    private void View_Materials(object sender, RoutedEventArgs e)
    {
        if (CoursesGrid.SelectedIndex != -1)
        {
            materials = DbHelper.GetMaterialsByCourse((CoursesGrid.SelectedItem as CourseModel).CourseId);
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