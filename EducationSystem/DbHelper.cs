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

        public static bool AuthenticateUser(string username, string password, out string role)
        {
            role = string.Empty;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // Запрос для получения пользователя по email
                string query = "SELECT role, password_hash, salt FROM users WHERE email = @username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // Если пользователь найден
                        {
                            var storedPasswordHash = reader.GetString(1); // Получаем хэш пароля
                            role = reader.GetString(0); // Получаем роль
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
                            MessageBox.Show("Enrollment deleted successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Enrollment not found.");
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
                // Define the SQL command to insert or update the course
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
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
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
        public string Instructor { get; set; }
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
    
}