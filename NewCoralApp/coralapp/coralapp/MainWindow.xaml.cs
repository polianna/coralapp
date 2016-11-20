using Accord.Statistics.Models.Regression.Linear;
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
        private ObservableCollection<Product> saleProducts = new ObservableCollection<Product>();

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
            if (this.connection == null) {
                this.connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
                this.connection = new SqlConnection(this.connectionString); //Создали соединение с базой данных
            }
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
                    DataTable priceList = getPriceList(); //Забрали данные из БД. Реализацию см. ниже
                    cbNewProductName.ItemsSource = priceList.DefaultView;
                    cbNewProductCode.ItemsSource = priceList.DefaultView;
                    break;
                case "tabSale":
                    dgSale.ItemsSource = this.saleProducts;
                    DataTable saleList = getAvailableCommodityList(); //Забрали данные из БД. Реализацию см. ниже
                    cbSaleProductName.ItemsSource = saleList.DefaultView;
                    cbSaleProductCode.ItemsSource = saleList.DefaultView;
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
//Создаем SQL команду, т.к необходимо понять, что такое @name and @code, для этого
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
            if (connection.State != ConnectionState.Open)
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
            //Метод для добавления товара в табличку по нажатии кнопки
        {

            string commodityName = cbNewProductName.Text; //Переменная для хранения имени товара
            string commodityCode = cbNewProductCode.Text; //Переменная для хранения кода товара
            string commodityCodeSelected = null;
            string commodityNameSelected = null;
            int quantity = Int32.Parse(tbNewProductQuantity.Text); //Переменная, хранящая численное значение (количество товара)
            int priceid = -1; //Вспомогательная переменная для добавления товара в БД


            if ((cbNewProductCode.SelectedValue == null && commodityCode != String.Empty)
                || (cbNewProductName.SelectedValue == null && commodityName != String.Empty))
                //если хотя бы один комбобокс заполнено, но при этом нет совпадений введенного значения ни с одной записью из списка, то
            {
                MessageBox.Show("Выбран несуществующий товар");
                //То выдаем сообщение об ошибке
                return;
            }

            if (commodityCode != String.Empty && commodityName != String.Empty) {
                //Если информация из двух комбобоксов относятся к разным товарам 
                // (проверка по связанному атрибуту - priceid. оно должно совпадать)
                int priceidCode = Int32.Parse((cbNewProductCode.SelectedValue as DataRowView)["price_id"].ToString());
                int priceidName = Int32.Parse((cbNewProductName.SelectedValue as DataRowView)["price_id"].ToString());
                if (priceidCode != priceidName)
                {
                    MessageBox.Show("Код и наименование товара не соответствуют");
                    //То выдаем сообщение об ошибке
                    return;
                }

                else
                {
                    priceid = priceidCode; //иначе присваиваем priceid любой айдишник цены
                    commodityNameSelected = (cbNewProductCode.SelectedValue as DataRowView)["commodity_name"].ToString();
                    commodityCodeSelected = (cbNewProductCode.SelectedValue as DataRowView)["coralclub_id"].ToString();
                } 

            }
            else
            { //если хотя бы один комбобокс пуст
                if (commodityCode != String.Empty)
                {
                    priceid = Int32.Parse((cbNewProductCode.SelectedValue as DataRowView)["price_id"].ToString());

                    commodityNameSelected = (cbNewProductCode.SelectedValue as DataRowView)["commodity_name"].ToString();
                    commodityCodeSelected = (cbNewProductCode.SelectedValue as DataRowView)["coralclub_id"].ToString();
                }

                //то забираем строчечку и присваиваем нашей переменной значение из БД

                else
                {
                    if (commodityName != String.Empty)
                    {
                        priceid = Int32.Parse((cbNewProductName.SelectedValue as DataRowView)["price_id"].ToString());
                        commodityNameSelected = (cbNewProductName.SelectedValue as DataRowView)["commodity_name"].ToString();
                        commodityCodeSelected = (cbNewProductName.SelectedValue as DataRowView)["coralclub_id"].ToString();
                    }

                    else { //если все пусто, то сообщение об ошибке выводим
                        MessageBox.Show("Не выбран ни один товар");
                        return;
                    }
                }
            }
            this.newProducts.Add(new Product(commodityNameSelected,
                commodityCodeSelected, quantity, priceid));
            //В майн виндоу создаем коллекцию и добавляем в нее товар (данные мы уже забрали из бд и при вводе) 
        }

        private void bNewAddInDB_Click(object sender, RoutedEventArgs e)
        {
            foreach (Product p in this.newProducts) {
                if (p.quantity > 0)
                {
                    addNewSupplyDB(p.priceid, p.quantity);
                }
            }

            this.newProducts.Clear();
            cbNewProductCode.SelectedIndex = -1;
            cbNewProductName.SelectedIndex = -1;
            tbNewProductQuantity.Text = "0";
        }

        private DataTable getPriceList() {
            //
            String SQL = "Select * FROM [ActualPriceList]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable priceTable = new DataTable();
            adapter.Fill(priceTable);
            return priceTable;
        }

        private DataTable getAvailableCommodityList()
        {
            //
            String SQL = "Select * FROM [AvailableCommodityList]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable commodityTable = new DataTable();
            adapter.Fill(commodityTable);
            return commodityTable;
        }

        private void cbNewProductName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true; //Чтобы не обновлялось ничего больше кроме комбобоксов. Защита от создателей MVS

        }

        private void cbNewProductCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        
private void addNewSupplyDB(int priceId, int quantity) {
   try
   {
                using (SqlCommand cmd = new SqlCommand("newSupply", this.connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter pPriceId = new SqlParameter("@price_id", SqlDbType.Int);
                    pPriceId.Value = priceId;

                    SqlParameter pQuantity = new SqlParameter("@quantity", SqlDbType.Int);
                    pQuantity.Value = quantity;

                    cmd.Parameters.Add(pPriceId);
                    cmd.Parameters.Add(pQuantity);
                    cmd.ExecuteNonQuery();
                }

            }
   catch (Exception ex)
   {
       MessageBox.Show(ex.Message);
   }
}

        private void cbSaleProductName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void cbSaleProductCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void bSaleAddInTable_Click(object sender, RoutedEventArgs e)
        {
            string commodityName = cbSaleProductName.Text; //Переменная для хранения имени товара
            string commodityCode = cbSaleProductCode.Text; //Переменная для хранения кода товара
            string commodityCodeSelected = null;
            string commodityNameSelected = null;
            int quantity = Int32.Parse(tbSaleProductQuantity.Text); //Переменная, хранящая численное значение (количество товара)
            int priceid = -1; //Вспомогательная переменная для добавления товара в БД
            int currentLedger = 0;


            if ((cbSaleProductCode.SelectedValue == null && commodityCode != String.Empty)
                || (cbSaleProductName.SelectedValue == null && commodityName != String.Empty))
            //если хотя бы один комбобокс заполнено, но при этом нет совпадений введенного значения ни с одной записью из списка, то
            {
                MessageBox.Show("Выбран несуществующий товар");
                //То выдаем сообщение об ошибке
                return;
            }

            if (commodityCode != String.Empty && commodityName != String.Empty)
            {
                //Если информация из двух комбобоксов относятся к разным товарам 
                // (проверка по связанному атрибуту - priceid. оно должно совпадать)
                int priceidCode = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["price_id"].ToString());
                int priceidName = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["price_id"].ToString());
                if (priceidCode != priceidName)
                {
                    MessageBox.Show("Код и наименование товара не соответствуют");
                    //То выдаем сообщение об ошибке
                    return;
                }

                else
                {
                    priceid = priceidCode; //иначе присваиваем priceid любой айдишник цены
                    commodityNameSelected = (cbSaleProductName.SelectedValue as DataRowView)["commodity_name"].ToString();
                    commodityCodeSelected = (cbSaleProductCode.SelectedValue as DataRowView)["coralclub_id"].ToString();
                    currentLedger = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["quantity"].ToString());
                }

            }
            else
            { //если хотя бы один комбобокс пуст
                if (commodityCode != String.Empty)
                {
                    priceid = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["price_id"].ToString());

                    commodityNameSelected = (cbSaleProductCode.SelectedValue as DataRowView)["commodity_name"].ToString();
                    commodityCodeSelected = (cbSaleProductCode.SelectedValue as DataRowView)["coralclub_id"].ToString();
                    currentLedger = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["quantity"].ToString());
                }

                //то забираем строчечку и присваиваем нашей переменной значение из БД

                else
                {
                    if (commodityName != String.Empty)
                    {
                        priceid = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["price_id"].ToString());
                        commodityNameSelected = (cbSaleProductName.SelectedValue as DataRowView)["commodity_name"].ToString();
                        commodityCodeSelected = (cbSaleProductName.SelectedValue as DataRowView)["coralclub_id"].ToString();
                        currentLedger = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["quantity"].ToString());
                    }

                    else
                    { //если все пусто, то сообщение об ошибке выводим
                        MessageBox.Show("Не выбран ни один товар");
                        return;
                    }
                }
            }
            foreach (Product prod in this.saleProducts) {
                if (priceid == prod.priceid) {
                    quantity += prod.quantity;
                    if (currentLedger >= quantity)
                    {
                        prod.quantity = quantity;
                        dgSale.Items.Refresh();
                    }
                    else MessageBox.Show("На складе недостаточно товара. Доступное количество - " + currentLedger);
                    return;
                }
            }
            if (currentLedger >= quantity)
            {
                this.saleProducts.Add(new Product(commodityNameSelected,
                    commodityCodeSelected, quantity, priceid));
            }
            else MessageBox.Show("На складе недостаточно товара. Доступное количество - " + currentLedger);
            //В майн виндоу создаем коллекцию и добавляем в нее товар (данные мы уже забрали из бд и при вводе)
        }

        private void bSaleAddInDB_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(prediction().ToString());
        }

        private double prediction()
        {
            // Declare some sample test data.
            double[] inputs = { 80, 60, 10, 20, 30 };
            double[] outputs = { 20, 40, 30, 50, 60 };

            // Use Ordinary Least Squares to learn the regression
            OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

            // Use OLS to learn the simple linear regression
            SimpleLinearRegression regression = ols.Learn(inputs, outputs);

            // Compute the output for a given input:
            double y = regression.Transform(85); // The answer will be 28.088

            return y;
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
        //Создали класс с полями, ура, методы. счастье. Спасбо за внимание

        //ПОЛИ-НА!!! :))
        public string name { get; set; }
        public int quantity { get; set; }
        public string coralid { get; set; }
        public int priceid { get; set; }
        public int curLedger { get; set; }

        public Product(string name, string coralid, int quantity, int priceid) {
            //конструктор первый
            this.name = name;
            this.coralid = coralid;
            this.quantity = quantity;
            this.priceid = priceid;
            this.curLedger = 0;
        }

        public Product(string name, string coralid, int quantity, int priceid, int curLedger)
        {
            //конструктор второй (расширенный)
            this.name = name;
            this.coralid = coralid;
            this.quantity = quantity;
            this.priceid = priceid;
            this.curLedger = curLedger;
        }

    }
    }
