using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace coralapp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String connectionString;
        private SqlConnection connection;
        private ExampleViewModel m_ViewModel;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            // Конфиг.менеджер прочитал app.config.  Из массива забрали именноD Default connection. И теперь мы знаем к какой БД идти.
            connection = new SqlConnection(connectionString); //Соединение с базой данных
            m_ViewModel = new ExampleViewModel();
            DataContext = m_ViewModel;
        }   

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;

            switch (tabItem)
            {
                case "tabSearch":
                    allLedgers();
                    break;
                default:
                    return;
            }
        }
        

        private void tbSearchProductName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearchProductName.Text == "наименование")
            tbSearchProductName.Clear();
        }
       
            private void tbSearchProductCode_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearchProductCode.Text == "код товара")
                tbSearchProductCode.Clear();
        }

        private void dgSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        
        }

        private void button_Click(object sender, RoutedEventArgs e) 
  //Процедура для поиска товара по коду или по названию. 
        {
            String SQL = "Select * FROM [CurrentLedger] where commodity_name like @name or coralclub_id like @code";
//Создаем SQL комманду
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlParameter parName = command.Parameters.Add("@name", SqlDbType.NVarChar, -1); //Определяем, что это за параметры. Из тип.
            SqlParameter parCode = command.Parameters.Add("@code", SqlDbType.NVarChar, -1);
            parName.Value = tbSearchProductName.Text; //Заполняем параметры
            parCode.Value = tbSearchProductCode.Text;
            command.Prepare(); //Подготовил и собрал наш СКЛ запрос. Например, определил по формату вид содердимой переменной. Для строки расставил кавычки.
            SqlDataAdapter adapter = new SqlDataAdapter(command); //подготовили пакет к отправке
            DataTable commodityTable = new DataTable(); 
            adapter.Fill(commodityTable); //Определяем вид таблицы, как ту что получим в результате выполнения СКЛ запроса
            dgSearch.ItemsSource = commodityTable.DefaultView; // Вернули данные в компонент data grid

        }

        private void allLedgers() {
            String SQL = "Select * FROM [CurrentLedger]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State == ConnectionState.Closed)
            { connection.Open(); }
            DataTable commodityTable = new DataTable();
            adapter.Fill(commodityTable);
            dgSearch.ItemsSource = commodityTable.DefaultView;
        }

        private void bSearchAllProduct_Click(object sender, RoutedEventArgs e)
        {
            allLedgers();
        }
    }

    public class ExampleViewModel : INotifyPropertyChanged
        
    {
        private string m_Name = "код товара";
        public ExampleViewModel()
        {

        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new Exception("Name can not be empty.");
                }
                if (m_Name != value)
                {
                    m_Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }
    }
}
