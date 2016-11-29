using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
    /// Логика взаимодействия для checkin.xaml
    /// </summary>
    public partial class checkin : Window
    {
        public checkin()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(sendit("coralappmail@gmail.com", "Заявка на регистрацию","Необходимо зарегистрировать пользователя "+tbEmail.Text));
        }

        public string sendit(string ReciverMail, string subject, string message)
            //Метод для отправки сообщений имейл
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("coralappamail@gmail.com"); //адрес отправителя сообщения
            msg.To.Add(ReciverMail); //адрес получателей
            msg.Subject = subject;
            msg.Body = message;
            SmtpClient client = new SmtpClient();
            //Smtp - протокол, по которому отправляется почта.  
            // Создаем клиента, который является промежуточным звеном между нами - coralappmail и сервером адрасатов (админ, руководитель и тд)
            client.UseDefaultCredentials = true; //Настраиваем, чтобы гуглу все понравилось.
            client.Host = "smtp.gmail.com"; //
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("coralappmail@gmail.com", "privetpolina123");
            client.Timeout = 20000;
            try
            {
                client.Send(msg);
                return "Письмо отправлено";
            }
            catch (Exception ex) //ловим ошибку
            {
                return ex.Message;
            }
            finally
            {
                msg.Dispose(); //удаляем объект нашего письма
            }
        }

        private void tbEmail_GotFocus(object sender, RoutedEventArgs e)
        //Метод для очистки окна при нажатии на text box
        {
            tbEmail.Clear();
        }
    }
}
