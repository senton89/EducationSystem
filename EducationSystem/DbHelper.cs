using System.Collections.ObjectModel;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace EducationSystem
{
    public class DbHelper
    {
        private static string connectionString = "Host=localhost; Database=corporate_training; Username=postgres; Password=postgres;";
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

        public static ObservableCollection<UserModel> GetInstructors()
        {
            var instructors = new ObservableCollection<UserModel>();
            

            using (var connection = new NpgsqlConnection(connectionString))

            {

                connection.Open();

                string query = "SELECT user_id, first_name, last_name, email, phone_number, department FROM users WHERE role = 'Instructor'";

                using (var command = new NpgsqlCommand(query, connection))

                using (var reader = command.ExecuteReader())

                {

                    while (reader.Read())

                    {

                        var instructor = new UserModel

                        {

                            UserID = reader.GetInt32(0),

                            FirstName = reader.GetString(1),

                            LastName = reader.GetString(2),

                            Email = reader.GetString(3),

                            PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),

                            Department = reader.IsDBNull(5) ? null : reader.GetString(5)

                        };

                        instructors.Add(instructor);
                    }
                }
            }
            return instructors;
        }

        public static void SaveCourse(CourseModel course)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))

            {
                // Define the SQL command to insert or update the course
                string query = "INSERT INTO courses (title, description, duration, instructor_id) VALUES (@Title, @Description, @Duration, @InstructorId)"; // Insert case
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@Title", course.Title);
                    command.Parameters.AddWithValue("@Description", course.Description); // Handle null description
                    command.Parameters.AddWithValue("@Duration", course.Duration);
                    command.Parameters.AddWithValue("@InstructorId", course.InstructorId);
                    
                    connection.Open();
                    command.ExecuteNonQuery();

                }

            }

        }
        public static List<CourseModel> GetCourses()
        {
            return new List<CourseModel>();
        }
        
    }

public class UserModel
    {

        public int UserID { get; set; } // Уникальный идентификатор пользователя

        public string FirstName { get; set; } // Имя

        public string LastName { get; set; } // Фамилия

        public string Email { get; set; } // Электронная почта

        public string PasswordHash { get; set; } // Хэш пароля

        public string Role { get; set; } // Роль пользователя

        public DateTime CreatedAt { get; set; } // Дата создания

        public DateTime UpdatedAt { get; set; } // Дата обновления

        public string PhoneNumber { get; set; } // Номер телефона

        public string Department { get; set; } // Отдел

    }
    public class CourseModel
    {
        public int course_id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatesAt { get; set; }
        public int InstructorId { get; set; }
    }
}