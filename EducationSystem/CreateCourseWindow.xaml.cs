﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace EducationSystem;

public partial class CreateCourseWindow : Window
{
    private readonly CourseModel _course;
    public ObservableCollection<UserModel> Instructors { get; set; }
    public CourseModel Course => _course;
    
    public CreateCourseWindow(CourseModel course)
    {
        InitializeComponent();
        _course = course;
        DataContext = this;
        // Инициализация других данных для привязки (например, список преподавателей)
        LoadInstructors();
        
        var instructorsInfo = Instructors.Select(instructor => new InstructorInfo
        {
            DisplayName = $"{instructor.FirstName} {instructor.LastName}",
            Email = instructor.Email,
            Department = instructor.Department
        }).ToList();
        
        InstructorsList.ItemsSource = instructorsInfo;
    }
    
private void LoadInstructors()
{
    // Здесь вы получите список инструкторов из базы данных
    Instructors = DbHelper.GetInstructors();
}


    private void SaveCourse(object sender, RoutedEventArgs e)
    {
        // Сохранение курса в базе данных
        string title = Course.Title;
        int duration = Course.Duration;
        Course.CreatedAt = DateTime.Now;
        Course.UpdatesAt = DateTime.Now;
        Course.InstructorId = Instructors[InstructorsList.SelectedIndex].UserID;
        
        if (ValidateCourse(title,duration))
        {
            DbHelper.SaveCourse(_course);
            MessageBox.Show("Курс успешно сохранён.");
            Close(); // Закрытие окна после успешного сохранения
        }
        else
        {
            MessageBox.Show("Пожалуйста, исправьте ошибки.");
        }
    }
    private bool ValidateCourse(string title,int duration)
    {
        // Проверка названия курса
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Title cannot be empty or whitespace.");
            return false;
        }
  
        // Проверка длины названия
        if (title.Length > 100)
        {
            MessageBox.Show("Title cannot exceed 100 characters.");
            return false;
        }

   
        // Проверка продолжительности курса
        if (duration <= 0)
        {
            MessageBox.Show("Duration must be a positive number.");
            return false;
        }
   
        // Если все проверки пройдены
        return true;

    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    public class InstructorInfo
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
    }
}