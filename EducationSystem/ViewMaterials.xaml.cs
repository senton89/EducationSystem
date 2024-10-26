using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace EducationSystem;

public partial class ViewMaterials : Window
{
    private string[] files;
    
    public ViewMaterials()
    {
        InitializeComponent();
        LoadFiles();
    }

    private void LoadFiles()
    {
        string projectFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Materials");
        if (Directory.Exists(projectFolderPath))
        {
            files = Directory.GetFiles(projectFolderPath);
            foreach (var file in files)
            {
                FilesListBox.Items.Add(Path.GetFileName(file));
            }
        }
        else
        {
            MessageBox.Show("Папка 'materials' не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private void ViewFilesButton_Click(object sender, RoutedEventArgs e)
    {
        if (FilesListBox.SelectedItem != null)
        {
            // Получаем индекс выбранного элемента
            int selectedIndex = FilesListBox.SelectedIndex;
            // Открываем файл с использованием полного пути
            Process.Start(new ProcessStartInfo
            {
                FileName = files[selectedIndex],
                UseShellExecute = true // Используем оболочку для открытия файла
            });
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите файл из списка.");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AdministratorWindow adminWindow = new AdministratorWindow();
        adminWindow.Show();
        Close();
    }
}