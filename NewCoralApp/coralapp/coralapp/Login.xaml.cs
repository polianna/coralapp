using System;
using System.Collections.Generic;
using System.Linq;
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
        public Window1()
        {
            InitializeComponent();
        }

        checkin childForm; //Запоминаем ссылку на дочернюю форму
 
        private void hellosorry_Click(object sender, EventArgs e)
        {
            if (childForm != null)
                childForm.Close(); //если форма открыта - закрываем
            OpenChildForm();
        }
 
        private void OpenChildForm() {
                childForm = new checkin();
                childForm.Closed += (s, e) => childForm = null; //При закрытии формы обнуляем ссылку
                childForm.Show();
        }

        private void tbLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            tbLogin.Clear();
        }

        private void tbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            tbPassword.Clear();
        }
    }
}
