using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace coralapp
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        string connectionString;
        SqlDataAdapter adapter;

        public Window1()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
            //Метод для шифровки строки по алгоритму MD5
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
            //Верификация. Т.е сравниваем полученную строку, сперва ее переведя
            //в МД5, с зашифрованной строкой. 
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        checkin childForm; //Запоминаем ссылку на дочернюю форму
 
        private void hellosorry_Click(object sender, EventArgs e)
            //Метод для открытия вспомогательно окна с проверкой открыто ли оно ранее.
        {
            if (childForm != null)
            { childForm.Close();
                childForm = null;
            }//если форма открыта - закрываем
            OpenChildForm();
        }
 
        private void OpenChildForm() {
                childForm = new checkin();
                 //При закрытии формы обнуляем ссылку
                childForm.Show();
        }

        private void tbLogin_GotFocus(object sender, RoutedEventArgs e)
            //Метод для очистки окна при нажатии на text box
        {
            tbLogin.Clear();
        }

        private void bLogin_Click(object sender, RoutedEventArgs e)
            //Метод для аутентификации в системе (соединение с БД, проверка пароля, если все хорошо, то переход в новое окно) 
        {
            SqlConnection connection = null;
            SqlDataReader reader = null; //Структура, тут это список 
            string dbPass = "";
            try
            {
                connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand("select pass from [User] where username = @username", connection);
                command.Parameters.Add("username", SqlDbType.NVarChar).Value = tbLogin.Text;
                connection.Open();
                reader = command.ExecuteReader(); //записали в нашу структуру данных полученный список

                // write each record
                while (reader.Read())
                {
                    dbPass = reader.GetString(0); //считываем данные из списка

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally //Тут закрываем наше соединение и очищаем структуру reader (закрываем его и он больше недоступен)
            {
                if (reader != null)
                    reader.Close();

                if (connection != null)
                    connection.Close();
            }

            using (MD5 md5Hash = MD5.Create())
            {
                bool isLogin = VerifyMd5Hash(md5Hash, pbPassword.Password, dbPass);
                if (isLogin) //если пароль совпадает с паролем из БД
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show(); //то открываем главное окно
                    this.Close(); //И закрываем текущее
                }
                else
                {
                    MessageBox.Show("Auth wrong"); //Если пароль неверен, то сообщ.об ошибке
                }
            }
        }

        private void pbPassword_GotFocus(object sender, RoutedEventArgs e)
            //Метод для очистки text box при нажатии на данный компонент
        {
            pbPassword.Clear();
        }
    }
}
