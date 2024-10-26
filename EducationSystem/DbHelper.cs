using System.Collections.ObjectModel;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace EducationSystem
{
    public class DbHelper
    {
        private static string connectionString =
            "Host=localhost; Database=corporate_training; Username=postgres; Password=postgres;";

        public static bool AuthenticateUser(string username, string password, out Roles role, out int userID)
        {
            role = Roles.Participant;
            userID = 0;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // Запрос для получения пользователя по email
                string query = "SELECT user_id, role, password_hash, salt FROM users WHERE email = @username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // Если пользователь найден
                        {
                            userID = reader.GetInt32(0); // Получаем id
                            role = (Roles)Enum.Parse(typeof(Roles), reader.GetString(1)); // Получаем роль
                            var storedPasswordHash = reader.GetString(2); // Получаем хэш пароля
                            byte[] salt = (byte[])reader["salt"]; // Получаем хэш для шифрования
                            // Проверяем пароль
                            if (VerifyPassword(password, storedPasswordHash, salt))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;

        }

        private static bool VerifyPassword(string password, string storedHash, byte[] salt)
        {
            if (storedHash == string.Empty || storedHash.Length == 0)
                return false;
            // Сравните хеши, используя тот же алгоритм хеширования
            using (var hmac = new HMACSHA256(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return CompareByteArrays(computedHash, Convert.FromBase64String(storedHash));
            }
        }

        private static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        public static bool AddNewUser(string firstName, string lastName, string email, string password, string role,
            string phoneNumber, string department)
        {

            using (var connection = new NpgsqlConnection(connectionString))
            {

                connection.Open();

                string query =
                    "INSERT INTO users (first_name, last_name, email, password_hash, role, phone_number, department, created_at, updated_at, salt) " +

                    "VALUES (@firstName, @lastName, @email, @passwordHash, @role, @phoneNumber, @department, @createdAt, @updatedAt, @salt)";

                using (var command = new NpgsqlCommand(query, connection))

                {

                    command.Parameters.AddWithValue("firstName", firstName);

                    command.Parameters.AddWithValue("lastName", lastName);

                    command.Parameters.AddWithValue("email", email);

                    // Хеширование пароля

                    using (var hmac = new HMACSHA256())

                    {

                        var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

                        command.Parameters.AddWithValue("passwordHash", passwordHash);

                        var salt = hmac.Key;

                        command.Parameters.AddWithValue("salt", salt);
                    }



                    command.Parameters.AddWithValue("role", role);

                    command.Parameters.AddWithValue("phoneNumber", phoneNumber);

                    command.Parameters.AddWithValue("department", department);

                    command.Parameters.AddWithValue("createdAt", DateTime.Now);

                    command.Parameters.AddWithValue("updatedAt", DateTime.Now);

                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Пользователь успешно добавлен!");
                        return true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Неправильно введены поля или указанная почта уже существует!");
                    }

                    return false;
                }

            }
        }

        public static List<UserModel> GetUsers()
        {
            var users = new List<UserModel>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT user_id, first_name, last_name, email,role, phone_number, department FROM users";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Roles.TryParse(reader.GetString(4), out Roles role);
                        var user = new UserModel
                        {
                            UserID = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            Role = role,
                            PhoneNumber = reader.IsDBNull(5) ? null : reader.GetString(4),
                            Department = reader.IsDBNull(6) ? null : reader.GetString(5)
                        };
                        users.Add(user);
                    }
                }
            }

            return users;
        }

        public static List<UserModel> GetInstructors()
        {
            return GetUsers().Where(u => u.Role == Roles.Instructor).ToList();
        }

        public static List<UserModel> GetParticipant()
        {
            return GetUsers().Where(u => u.Role == Roles.Participant).ToList();
        }

        public static void SaveCourse(CourseModel course)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                // Define the SQL command to insert or update the course
                string query = course.CourseId == 0
                    ? "INSERT INTO courses (title, description, duration, instructor_id) VALUES (@Title, @Description, @Duration, @InstructorId)"
                    : // Insert case
                    "UPDATE courses SET title = @Title, description = @Description, duration = @Duration, instructor_id = @InstructorId, updated_at = CURRENT_TIMESTAMP WHERE course_id = @CourseId"; // Update case
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@Title", course.Title);
                    command.Parameters.AddWithValue("@Description", course.Description); // Handle null description
                    command.Parameters.AddWithValue("@Duration", course.Duration);
                    command.Parameters.AddWithValue("@InstructorId", course.InstructorId);

                    if (course.CourseId != 0)
                    {
                        command.Parameters.AddWithValue("@CourseId", course.CourseId);
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<CourseModel> GetCourses()
        {
            List<CourseModel> courses = new List<CourseModel>();

            using (var connection = new NpgsqlConnection(connectionString))

            {
                connection.Open();
                using (var command =
                       new NpgsqlCommand(
                           "SELECT course_id, title, description, duration, created_at, updated_at, instructor_id FROM courses",
                           connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new CourseModel
                            {
                                CourseId = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Duration = reader.GetInt32(3),
                                CreatedAt = reader.GetDateTime(4),
                                UpdatedAt = reader.GetDateTime(5),
                                InstructorId = reader.GetInt32(6)
                            });
                        }
                    }
                }
            }

            return courses;
        }

        public static void DeleteCourse(int courseId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM courses WHERE course_id = @courseId", connection))
                {
                    command.Parameters.AddWithValue("courseId", courseId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Курс успешно удален.");
                    }
                    else
                    {
                        MessageBox.Show($"Курс не найден.");
                    }
                }
            }
        }

        public static List<EnrollmentModel> GetEnrollments()
        {
            List<EnrollmentModel> enrollments = new List<EnrollmentModel>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command =
                       new NpgsqlCommand(
                           "SELECT enrollment_id, user_id, course_id, enrollment_date, completion_date, grade FROM Enrollments",
                           connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var enrollment = new EnrollmentModel
                        {
                            EnrollmentID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            CourseID = reader.GetInt32(2),
                            EnrollmentDate = reader.GetDateTime(3),
                            CompletionDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            Grade = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5)
                        };
                        enrollments.Add(enrollment);
                    }
                }
            }

            return enrollments;
        }

        public static List<EnrollmentModel> GetEnrollmentsByCourse(int courseId)
        {
            List<EnrollmentModel> enrollments = new List<EnrollmentModel>();
            enrollments = GetEnrollments().Where(e => e.CourseID == courseId).ToList();
            return enrollments;
        }

        public static void DeleteEnrollment(int enrollmentId)
        {
            string query = "DELETE FROM enrollments WHERE enrollment_id = @EnrollmentID";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EnrollmentID", enrollmentId);
                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запись удалена успешно");
                        }
                        else
                        {
                            MessageBox.Show("Запись не найдена");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public static void SaveEnrollment(EnrollmentModel enrollment)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                if (enrollment.EnrollmentID == 0)
                {
                    string checkQuery =
                        "SELECT COUNT(*) FROM enrollments WHERE user_id = @UserID AND course_id = @CourseID";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserID", enrollment.UserID);
                        checkCommand.Parameters.AddWithValue("@CourseID", enrollment.CourseID);
                        connection.Open();
                        Int32.TryParse(checkCommand.ExecuteScalar().ToString(), out int count);
                        if (count > 0)
                        {
                            MessageBox.Show("Запись с таким пользователем и курсом уже существует.");
                            return;
                        }
                    }
                    connection.Close();
                }

                // Define the SQL command to insert or update the course
                connection.Open();
                string query = enrollment.EnrollmentID == 0
                    ? "INSERT INTO enrollments (user_id, course_id, enrollment_date, completion_date, grade) VALUES (@UserID, @CourseID, @EnrollmentDate, @CompletionDate, @Grade)"
                    : "UPDATE enrollments SET user_id = @UserID, course_id = @CourseID, enrollment_date = @EnrollmentDate, completion_date = @CompletionDate, grade = @Grade WHERE enrollment_id = @EnrollmentID"; // Update case
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@UserID", enrollment.UserID);
                    command.Parameters.AddWithValue("@CourseID", enrollment.CourseID);
                    command.Parameters.AddWithValue("@EnrollmentDate", enrollment.EnrollmentDate);
                    command.Parameters.AddWithValue("@CompletionDate",
                        (object)enrollment.CompletionDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Grade", (object)enrollment.Grade ?? DBNull.Value);
                    if (enrollment.EnrollmentID != 0)
                    {
                        command.Parameters.AddWithValue("@EnrollmentID", enrollment.EnrollmentID);
                    }
                    
                    command.ExecuteNonQuery();
                    SaveRelationShip(enrollment.UserID, enrollment.CourseID, enrollment.CompletionDate <= DateTime.Now,
                        connection);
                }
            }
        }

        public static void SaveRelationShip(int UserID, int CourseID, bool isCompleted, NpgsqlConnection connection)
        {
            // Define the SQL command to insert or update the relationship
            string query = "INSERT INTO relationship (user_id, course_id, Type) VALUES (@UserID, @CourseID, @Type)";

            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@UserID", UserID);
                command.Parameters.AddWithValue("@CourseID", CourseID);
                command.Parameters.AddWithValue("@Type",
                    isCompleted ? TypeStatement.Taught.ToString() : TypeStatement.Enrolled.ToString());

                // Open connection if it's not already open
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                // Execute the command to save the relationship
                command.ExecuteNonQuery();
            }
        }
        
        public static List<ReportModel> GetReports()
        {
            List<ReportModel> reports = new List<ReportModel>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT report_id, course_id, total_hours, avg_grade, completion_rate, generated_at FROM reports";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReportModel report = new ReportModel
                            {
                                ReportID = reader.GetInt32(0), // report_id
                                CourseID = reader.GetInt32(1),  // course_id
                                TotalHours = reader.GetDecimal(2), // total_hours
                                AvgGrade = reader.GetDecimal(3),    // avg_grade
                                CompletionRate = reader.GetDecimal(4) // completion_rate
                            };
                            reports.Add(report);
                        }
                    }
                }
            }
            return reports;
        }

        public static void SaveReport(ReportModel report)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                // Проверка на существование отчета
                string checkQuery = "SELECT COUNT(*) FROM reports WHERE course_id = @CourseID";
                using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@CourseID", report.CourseID);
                   
                    connection.Open();
                    
                    Int32.TryParse(checkCommand.ExecuteScalar().ToString(), out int count);
                    if (count > 0)
                    {
                        MessageBox.Show("Отчет по этому курсу уже существует.");
                        return;
                    }
                }
                // Определяем SQL-команду для вставки или обновления отчета
                string query =
                    "INSERT INTO reports (course_id, total_hours, avg_grade, completion_rate) VALUES (@CourseID, @TotalHours, @AvgGrade, @CompletionRate)";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    // Добавляем параметры для предотвращения SQL-инъекций
                    command.Parameters.AddWithValue("@CourseID", report.CourseID);
                    command.Parameters.AddWithValue("@TotalHours", report.TotalHours);
                    command.Parameters.AddWithValue("@AvgGrade", report.AvgGrade);
                    command.Parameters.AddWithValue("@CompletionRate", report.CompletionRate);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Отчет создан");
                }
            }
        }

        public static void DeleteReport(int reportID)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM reports WHERE report_id = @ReportID";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReportID", reportID);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        MessageBox.Show("Отчет не найден или уже удален.");
                }
            }
            MessageBox.Show("Отчет удален");
        }

        public static void AddMaterialToDatabase(string fileName,int courseId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM materials WHERE course_id = @CourseID AND material_name = @MaterialName";
                using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@CourseID", courseId);
                    checkCommand.Parameters.AddWithValue("@MaterialName", fileName);
                    
                    Int32.TryParse(checkCommand.ExecuteScalar().ToString(), out int count);
                    if (count > 0)
                    {
                        MessageBox.Show("Такой материал уже существует.");
                        return;
                    }
                }
                using (var command = new NpgsqlCommand("INSERT INTO materials (material_name,course_id) VALUES (@material_name,@course_id)", connection))
                {
                    command.Parameters.AddWithValue("material_name", fileName);
                    command.Parameters.AddWithValue("course_id", courseId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<MaterialModel> GetMaterialsByCourse(int courseId)
        {
            var materials = new List<MaterialModel>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM materials WHERE course_id = @CourseID";
                using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@CourseID", courseId);
                    
                    Int32.TryParse(checkCommand.ExecuteScalar().ToString(), out int count);
                    if (count == 0)
                    {
                        MessageBox.Show("Материалов по этому курсу нет");
                        return materials;
                    }
                }
                using (var command =
                       new NpgsqlCommand(
                           "SELECT material_id, material_name FROM materials WHERE course_id = @course_id", connection))

                {
                    command.Parameters.AddWithValue("course_id", courseId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var material = new MaterialModel
                            {
                                MaterialID = reader.GetInt32(0),
                                MaterialName = reader.GetString(1)
                            };
                            materials.Add(material);
                        }
                    }
                }
            }
            return materials;
        }
    }

    public class UserModel
    {
        public int UserID { get; set; } // Уникальный идентификатор пользователя
        public string FirstName { get; set; } // Имя
        public string LastName { get; set; } // Фамилия
        public string Email { get; set; } // Электронная почта
        public string PasswordHash { get; set; } // Хэш пароля
        public Roles Role { get; set; } // Роль пользователя
        public DateTime CreatedAt { get; set; } // Дата создания
        public DateTime UpdatedAt { get; set; } // Дата обновления
        public string PhoneNumber { get; set; } // Номер телефона
        public string Department { get; set; } // Отдел
    }
    public class CourseModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int InstructorId { get; set; }
    }
    public class CourseInfo
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Instructor { get; set; }
    }
    public class UserInfo
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
    }

    public class EnrollmentModel
    {
        public int EnrollmentID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal? Grade { get; set; }
    }
    
    public class EnrollmentInfo
    {
        public int EnrollmentID { get; set; }
        public string Participant { get; set; }
        public int ParticipantID { get; set; }
        public string Course { get; set; }
        public int CourseID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal? Grade { get; set; }
    }
    
    public class ReportModel
    {
        public int ReportID { get; set; }
        public int CourseID { get; set; }
        public decimal TotalHours { get; set; }
        public decimal? AvgGrade { get; set; }
        public decimal CompletionRate { get; set; }
    }
    
    public class ReportInfo
    {
        public int ReportID { get; set; }
        public string Course { get; set; }
        public decimal TotalHours { get; set; }
        public decimal AvgGrade { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class MaterialModel
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public int CourseID { get; set; }
    }
}