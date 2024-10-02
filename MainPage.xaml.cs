using Microsoft.Maui.Controls;
using System;
using Microsoft.Data.SqlClient;

namespace Proyecto_de_prueba
{
    public partial class MainPage : ContentPage
    {
        // Cadena de conexión correcta
        private readonly string connectionString = "Data Source=172.18.205.117,1433;Initial Catalog=Usuario;User ID=deiler;Password=deiler;Encrypt=True;TrustServerCertificate=True;Pooling=False;MultiSubnetFailover=True;Trusted_Connection=False;";

        public MainPage()
        {
            InitializeComponent();
        }

        // Método que se ejecuta cuando se hace clic en el botón de login
        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            if (IsValidUser(username, password))
            {
                DisplayAlert("Login Successful", "Welcome " + username, "OK");
                // Navegar a otra página o realizar alguna acción
            }
            else
            {
                DisplayAlert("Login Failed", "Invalid username or password", "OK");
            }
        }

        // Método para validar el usuario
        private bool IsValidUser(string username, string password)
        {
            bool isValid = false;

            try
            {
                // Usar la cadena de conexión para abrir la conexión a la base de datos
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Usuarios WHERE id_usuario = @username AND contraceña = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        connection.Open();
                        int userCount = (int)command.ExecuteScalar();
                        isValid = userCount > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejo de excepciones específico de SQL
                DisplayAlert("SQL Error", $"SQL Error: {ex.Message}\n{ex.StackTrace}", "OK");
            }
            catch (Exception ex)
            {
                // Manejo de excepciones general
                DisplayAlert("Error", $"An error occurred: {ex.Message}\n{ex.StackTrace}", "OK");
            }

            return isValid;
        }

        // Método que se ejecuta cuando se hace clic en el botón de crear cuenta
        private void OnCreateAccountButtonClicked(object sender, EventArgs e)
        {
            // Navegar a la nueva página de registro
            Navigation.PushAsync(new RegisterPage());
        }

        // Método que se ejecuta cuando se hace clic en el botón de recuperar contraseña
        private void OnRecoverPasswordButtonClicked(object sender, EventArgs e)
        {
            // Navegar a la página de restablecimiento de contraseña
            Navigation.PushAsync(new ResetPasswordPage());
        }
    }
}






