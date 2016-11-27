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
    /// Логика взаимодействия для manager.xaml
    /// </summary>
    public partial class manager : Window
    {
        public manager()
        {
            InitializeComponent();
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Window1 loginWindow = new Window1();
            loginWindow.Show(); //то открываем главное окно
            this.Close(); //И закрываем текущее
        }
    }
}
