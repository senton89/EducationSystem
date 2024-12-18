﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace EducationSystem;

public partial class ManageCourseWindow : Window
{
    private Roles userRole;
    private int instructorID;
    private readonly CourseModel _course;
    public List<UserModel> Instructors { get; set; }
    public CourseModel Course => _course;
    
    public ManageCourseWindow(CourseModel course,Roles role,int instructorId=0)
    {
        InitializeComponent();
        _course = course;
        DataContext = this;
        LoadInstructors();
        userRole = role;
        instructorID = instructorId;
        
        var instructorsInfo = ConvertListToUsersInfo(Instructors ?? new List<UserModel>());
        if (role == Roles.Instructor)
        {
            InstructorsList.ItemsSource = instructorsInfo.Where(instructor => instructor.UserId == instructorId);
            InstructorsList.SelectedIndex = 0;
            InstructorsList.IsReadOnly = true;
        }
        else
            InstructorsList.ItemsSource = instructorsInfo;
    }
    
    public static List<UserInfo> ConvertListToUsersInfo(List<UserModel> Instructors)
    {
        var instructorsInfo = Instructors.Select(instructor => new UserInfo
        {
            UserId = instructor.UserID,
            DisplayName = $"{instructor.FirstName} {instructor.LastName}",
            Email = instructor.Email,
            Department = instructor.Department
        }).ToList();
        return instructorsInfo;
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
        Course.CreatedAt = Course.CreatedAt == DateTime.MinValue?
            DateTime.Now : Course.CreatedAt;
        Course.UpdatedAt = DateTime.Now;
        Course.InstructorId = Instructors[InstructorsList.SelectedIndex>=0?InstructorsList.SelectedIndex:0].UserID;
        
        if (ValidateCourse(title,duration))
        {
            DbHelper.SaveCourse(_course);
            MessageBox.Show("Курс успешно сохранён.");
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

    private void ReturnToMain()
    {
        Close();
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        ReturnToMain();
    }
    
}