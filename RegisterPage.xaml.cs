using Microsoft.Maui.Controls;
using System;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Proyecto_de_prueba
{
    public partial class RegisterPage : ContentPage
    {
        private string verificationCode;
        private string pendingEmail;
        private string pendingUsername;
        private string pendingPassword;

        // Cadena de conexión correcta
        private readonly string connectionString = "Data Source=172.18.205.117,1433;Initial Catalog=Usuario;User ID=deiler;Password=deiler;Encrypt=True;TrustServerCertificate=True;Pooling=False;MultiSubnetFailover=True;Trusted_Connection=False;";

        public RegisterPage()
        {
            InitializeComponent();
        }

        // Enviar código de verificación al correo
        private async void OnSendVerificationCodeButtonClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;

            // Validar el correo electrónico
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                await DisplayAlert("Error", "Por favor, ingresa un correo electrónico válido.", "OK");
                return;
            }

            // Generar y enviar el código de verificación
            verificationCode = GenerateVerificationCode();
            await SendVerificationCodeEmail(email, verificationCode);

            pendingEmail = email; // Guardar el correo mientras se espera la verificación

            VerificationCodeEntry.IsVisible = true;
            VerifyCodeButton.IsVisible = true;
        }

        // Verificar el código ingresado
        private async void OnVerifyCodeButtonClicked(object sender, EventArgs e)
        {
            string enteredCode = VerificationCodeEntry.Text;

            if (enteredCode == verificationCode)
            {
                await DisplayAlert("Éxito", "Código de verificación correcto. Ahora puede registrarse.", "OK");
                RegisterButton.IsVisible = true;
                VerifyCodeButton.IsVisible = false;
            }
            else
            {
                await DisplayAlert("Error", "El código de verificación es incorrecto.", "OK");
            }
        }




        // Método para verificar si el correo ya está en uso
        private bool IsEmailInUse(string email)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Usuarios WHERE correo = @correo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@correo", email);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0; // Devuelve verdadero si el correo ya está en uso
                }
            }
        }




        // Registrar usuario en la base de datos
        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Validar los campos
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
                return;
            }

            if (!IsValidPassword(password))
            {
                await DisplayAlert("Error", "La contraseña no cumple con los requisitos de seguridad.\r\nMínimo 7 caracteres\r\n• Debe utilizar mínimo una mayúscula\r\n• Debe utilizar mínimo un símbolo especial\r\n• Debe utilizar mínimo un numero\r\n• Debe utilizar mínimo una minúscula", "OK");
                return;
            }

            // Verificar si el correo ya está en uso
            if (IsEmailInUse(pendingEmail))
            {
                await DisplayAlert("Error", "Correo ya en uso", "OK");
                return;
            }

            // Guardar los datos de usuario en la base de datos
            if (RegisterUser(pendingEmail, username, password))
            {
                await DisplayAlert("Éxito", "Usuario registrado con éxito.", "OK");
                await Navigation.PopAsync(); // Navegar de vuelta
            }
            else
            {
                await DisplayAlert("Error", "Hubo un problema al registrar el usuario.", "OK");
            }
        }


        // Método para guardar el usuario en la base de datos
        private bool RegisterUser(string email, string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Usuarios (correo, id_usuario, contraceña) VALUES (@correo, @username, @contraseña)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@correo", email);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@contraseña", password); // Deberías encriptar la contraseña

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        return result > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                DisplayAlert("SQL Error", $"SQL Error: {ex.Message}", "OK");
                return false;
            }
        }

        // Método para validar el correo electrónico
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        // Validar que la contraseña cumpla con los requisitos
        private bool IsValidPassword(string password)
        {
            if (password.Length < 7)
                return false;

            bool hasUpperCase = false;
            bool hasLowerCase = false;
            bool hasDigits = false;
            bool hasSpecialChar = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                if (char.IsLower(c)) hasLowerCase = true;
                if (char.IsDigit(c)) hasDigits = true;
                if (char.IsSymbol(c) || char.IsPunctuation(c)) hasSpecialChar = true;
            }

            return hasUpperCase && hasLowerCase && hasDigits && hasSpecialChar;
        }

        // Generar el código de verificación
        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(10000, 99999).ToString();
        }

        // Enviar el código de verificación por correo usando SendGrid
        private async Task SendVerificationCodeEmail(string email, string verificationCode)
        {
            var client = new SendGridClient("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");
            var from = new EmailAddress("moreravalverde@gmail.com", "deiler");
            var to = new EmailAddress(email);
            var subject = "Código de Verificación";
            var plainTextContent = $"Tu código de verificación es: {verificationCode}";
            var htmlContent = $"<strong>Tu código de verificación es: {verificationCode}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg);
                Console.WriteLine($"Correo enviado a: {email}, Estado: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
            }
        }
    }
}

