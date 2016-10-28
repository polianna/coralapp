using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            // Конфиг.менеджер прочитал app.config.  Из массива забрали именноD Default connection. И теперь мы знаем к какой БД идти.
            connection = new SqlConnection(connectionString); //Соединение с базой данных
            Commodity commodity = new Commodity();
            commodity.Name = "код товара";
            DataContext = commodity;
        }   

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;

            switch (tabItem)
            {
                case "tabSearch":
                    dgSearch.ItemsSource = allLedgers().DefaultView;
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
            String SQL = "Select * FROM [LastLedger] where commodity_name like @name or coralclub_id like @code";
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

        private DataTable allLedgers() {
            String SQL = "Select * FROM [LastLedger]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State == ConnectionState.Closed)
            { connection.Open(); }
            DataTable commodityTable = new DataTable();
            adapter.Fill(commodityTable);
            return commodityTable;
        }

        private void bSearchAllProduct_Click(object sender, RoutedEventArgs e)
        {
            dgSearch.ItemsSource = allLedgers().DefaultView;
        }

        private void bNewAddInTable_Click(object sender, RoutedEventArgs e)
        {
            string commodityName = tbNewProductName.Text;
            string commodityCode = tbNewProductCode.Text;
            string quantity = tbNewProductQuantity.Text;
            dgNew.Rows.Add();
        }
    }

    public class Commodity : IDataErrorInfo
        
    {

        public string Name { get; set; }

        public string Error
        {
            get
            {
                return this[string.Empty];
            }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "Name")
                {
                    if (Name.Equals("код товара")) return null;
                    if (string.IsNullOrEmpty(Name))
                    {
                        result = "Код не может быть пустым";
                        return result;
                    }
                    string st = @"^[A-Z]{2}[0-9]{1,6}$";
                    if (!Regex.IsMatch(Name, st))
                    {
                        result = "Код не соответствует шаблону XX000000";
                        return result;
                    }
                }
                return null;

            }
        }
    }
    }
