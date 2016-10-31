using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private SqlConnection connection; //Свойство. Оно описано ниже.
        private ObservableCollection<Product> newProducts = new ObservableCollection<Product>();

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
            connection = new SqlConnection(connectionString); //Создали соединение с базой данных
            Commodity commodity = new Commodity();
            commodity.Name = "код товара";
            DataContext = commodity;
        }   

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//Метод для определения, на какую из вкладок щелкнул пользователь. 
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;
            //Взяли имя вкладки и записали в переменную
            switch (tabItem)
            {
                case "tabSearch":
                    dgSearch.ItemsSource = allLedgers().DefaultView;
                    //Если имя вкладки = "TabSearch", то мы записали внутрь таблицы все товары со склада. 
                    break;
                case "tabNew":
                    dgNew.ItemsSource = this.newProducts;
                    DataTable priceList = getPriceList();
                    cbNewProductName.ItemsSource = priceList.DefaultView;
                    cbNewProductCode.ItemsSource = priceList.DefaultView;
                    break;
                default:
                    return;
            }
        }
        

        private void tbSearchProductName_GotFocus(object sender, RoutedEventArgs e)
            //Метод для очистки text box при нажатии
        {
            if (tbSearchProductName.Text == "наименование")
            tbSearchProductName.Clear();
        }
       
        private void tbSearchProductCode_GotFocus(object sender, RoutedEventArgs e)
        //Метод для очистки text box при нажатии
        {
            if (tbSearchProductCode.Text == "код товара")
                tbSearchProductCode.Clear();
        }

        private void dgSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        
        }

        private void button_Click(object sender, RoutedEventArgs e) 
  //Метод для поиска товара по коду или по названию. 
        {
            String SQL = "Select * FROM [LastLedger] where commodity_name like @name or coralclub_id like @code";
//Создаем SQL комманду, т.к необходимо понять, что такое @name and @code, для этого
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlParameter parName = command.Parameters.Add("@name", SqlDbType.NVarChar, -1); //Определяем, что это за параметры. Их тип.
            SqlParameter parCode = command.Parameters.Add("@code", SqlDbType.NVarChar, -1);
            parName.Value = tbSearchProductName.Text; //Заполняем параметры
            parCode.Value = tbSearchProductCode.Text;
            command.Prepare(); //Подготовил и собрал наш СКЛ запрос. Например, определил по формату вид содердимой переменной. Для строки расставил кавычки.
            SqlDataAdapter adapter = new SqlDataAdapter(command); //подготовили пакет к отправке
            DataTable commodityTable = new DataTable(); //Определили элемент как таблицу 
            adapter.Fill(commodityTable); //Заполнили логическую структуру (таблицу)
            dgSearch.ItemsSource = commodityTable.DefaultView; // Вернули default (оригинальные) данные в компонент data grid

        }

        private DataTable allLedgers() {
            //Метод для возврата таблицы с данными об остатке товара на складе на текущий момент
            String SQL = "Select * FROM [LastLedger]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State == ConnectionState.Closed)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable commodityTable = new DataTable();
            adapter.Fill(commodityTable);
            return commodityTable;
        }

        private void bSearchAllProduct_Click(object sender, RoutedEventArgs e)
            //Метод для поиска остатков ВСЕХ товаров на складе по нажатии на кнопку
        {
            dgSearch.ItemsSource = allLedgers().DefaultView;
        }

        private void bNewAddInTable_Click(object sender, RoutedEventArgs e)
            //Метод для ???
        {
            string commodityName = cbNewProductName.Text;
            string commodityCode = cbNewProductCode.Text;
            int quantity = Int32.Parse(tbNewProductQuantity.Text);
            int priceid = -1;

            if ((cbNewProductCode.SelectedValue == null && commodityCode != String.Empty)
                || (cbNewProductName.SelectedValue == null && commodityName != String.Empty))
            {
                MessageBox.Show("Выбран несуществующий товар");
                return;
            }

            if (commodityCode != String.Empty && commodityName != String.Empty) {

                int priceidCode = Int32.Parse((cbNewProductCode.SelectedValue as DataRowView)["price_id"].ToString());
                int priceidName = Int32.Parse((cbNewProductName.SelectedValue as DataRowView)["price_id"].ToString());
                if (priceidCode != priceidName)
                {
                    MessageBox.Show("Код и наименование товара не соответствуют");
                    return;
                }
                else priceid = priceidCode;
            }
            else
            {
                if (commodityCode != String.Empty)
                    priceid = Int32.Parse((cbNewProductCode.SelectedValue as DataRowView)["price_id"].ToString());
                else
                {
                    if (commodityName != String.Empty)
                    {
                        priceid = Int32.Parse((cbNewProductName.SelectedValue as DataRowView)["price_id"].ToString());
                    }
                    else {
                        MessageBox.Show("Не выбран ни один товар");
                        return;
                    }
                }
            }
            this.newProducts.Add(new Product(commodityName, commodityCode, quantity, priceid));
        }

        private void bNewAddInDB_Click(object sender, RoutedEventArgs e)
        {
            foreach (Product p in this.newProducts) {
                
            }

            this.newProducts.Clear();
        }

        private DataTable getPriceList() {
            String SQL = "Select * FROM [ActualPriceList]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State == ConnectionState.Closed)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable priceTable = new DataTable();
            adapter.Fill(priceTable);
            return priceTable;
        }

        private void cbNewProductName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

        }

        private void cbNewProductCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        /*
private void addNewProductDB() {
   try
   {
       SqlCommand command = new SqlCommand("select pass from [User] where username = @username", this.connection);
       command.Parameters.Add("username", SqlDbType.NVarChar).Value = tbLogin.Text;
       connection.Open();
       SqlDataReader reader = command.ExecuteReader(); //записали в нашу структуру данных полученный список

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
}
*/
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
                    if (!Regex.IsMatch(Name, st)) //Если вводимая строка не соотв.регулярному выражению, то 
                    {
                        result = "Код не соответствует шаблону XX000000";
                        return result;
                    }
                }
                return null;

            }
        }
    }

    public class Product {
        public string name { get; set; }
        public int quantity { get; set; }
        public string coralid { get; set; }
        public int priceid { get; set; }

        public Product(string name, string coralid, int quantity, int priceid) {
            this.name = name;
            this.coralid = coralid;
            this.quantity = quantity;
            this.priceid = priceid;
        }

    }
    }
