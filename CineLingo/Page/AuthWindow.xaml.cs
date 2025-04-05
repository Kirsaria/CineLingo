using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CineLingo.Page
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public const string ConnectionString = "Server=localhost;Database=cineLingoDictionary;User Id=root;Password=1111;";
        public static int CurrentUserId { get; private set; }
        public static string CurrentUsername { get; private set; }
        private readonly Regex _usernameRegex = new Regex(@"^[a-zA-Z0-9_]{4,20}$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private readonly Regex _passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$");
        public static string CurrentUserRole { get; private set; } = "";
        public AuthWindow()
        {
            InitializeComponent();
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsername.Text;
            string password = LoginPassword.Password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль");
                return;
            }
            try
            {
                using(var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT id, username, passwordHash FROM Users WHERE username = @username";
                    using(var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using(var reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                string storedHash = reader["passwordHash"].ToString();
                                string inputHash = HashPassword(password);
                                if(storedHash == inputHash)
                                {
                                    CurrentUserId = Convert.ToInt32(reader["id"]);
                                    CurrentUsername = reader["username"].ToString();
                                    CurrentUserRole = GetUserRole(username);
                                    MessageBox.Show(CurrentUserRole);
                                    DialogResult = true;
                                    Close();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный пароль");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
            }
        }

        private string GetUserRole(string username)
        {
            string role = "";
            string roleQuery = "SELECT roles FROM Users WHERE username = @username";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                using (var roleCommand = new MySqlCommand(roleQuery, connection))
                {
                    roleCommand.Parameters.AddWithValue("@username", username);
                    using (var roleReader = roleCommand.ExecuteReader())
                    {
                        if (roleReader.Read())
                        {
                            role = roleReader["roles"].ToString();
                        }
                    }
                }
            }
            return role;
        }

        private void Register_Click(object sender, EventArgs e)
        {
            string username = RegUsername.Text.Trim();
            string email = RegEmail.Text.Trim();
            string password = RegPassword.Password;

            if (!_usernameRegex.IsMatch(username))
            {
                MessageBox.Show("Имя пользователя должно содержать от 4 до 20 символов (латинские буквы, цифры и _)");
                return;
            }

            if (!_emailRegex.IsMatch(email))
            {
                MessageBox.Show("Пожалуйста, введите корректный email");
                return;
            }

            if (!_passwordRegex.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать:\n- минимум 8 символов\n- хотя бы одну заглавную букву\n- хотя бы одну строчную букву\n- хотя бы одну цифру");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE username = @username OR email = @email";
                    using (var checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@username", username);
                        checkCommand.Parameters.AddWithValue("@email", email);
                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким именем или email уже существует");
                            return;
                        }
                    }
                    string insertQuery = "INSERT INTO Users (username, passwordHash, email) VALUES (@username, @passwordHash, @email)";
                    using (var insertCommand = new MySqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@username", username);
                        insertCommand.Parameters.AddWithValue("@passwordHash", HashPassword(password));
                        insertCommand.Parameters.AddWithValue("@email", email);
                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Регистрация прошла успешно!");
                            DialogResult = true;
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }
    }
}
