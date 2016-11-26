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
using System.Data.OleDb;

namespace coralapp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int SALE1VALUE = 4;
        private const int SALE2VALUE = 5;
        private const int SALE3VALUE = 6;

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
                if (this.connection.State != ConnectionState.Open)
                { this.connection.Open(); }
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
        private void saleProductDB(int ledgerId, int quantity, int priceId) {
            try
            {
                if (this.connection.State != ConnectionState.Open)
                { this.connection.Open(); }
                using (SqlCommand cmd = new SqlCommand("saleProduct", this.connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter pLedgerId = new SqlParameter("@ledger_id", SqlDbType.Int);
                    pLedgerId.Value = ledgerId;

                    SqlParameter pQuantity = new SqlParameter("@quantity", SqlDbType.Int);
                    pQuantity.Value = quantity;

                    SqlParameter pPriceId = new SqlParameter("@price_id", SqlDbType.Int);
                    pPriceId.Value = priceId;

                    cmd.Parameters.Add(pLedgerId);
                    cmd.Parameters.Add(pQuantity);
                    cmd.Parameters.Add(pPriceId);
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
            int quantity = 0;
            try
            {
                quantity = Int32.Parse(tbSaleProductQuantity.Text); //Переменная, хранящая численное значение (количество товара)
            }
            catch (Exception exc) {
                quantity = 0;
                tbSaleProductQuantity.Text = "0";
            }
            int priceid = -1; //Вспомогательная переменная для добавления товара в БД
            int ledgerid = -1;
            int currentLedger = 0;
            int onSale1 = 0;
            int onSale2 = 0;
            int onSale3 = 0;
            int notOnSale = 0;
            

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
                    onSale1 = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["on_sale_1"].ToString());
                    onSale2 = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["on_sale_2"].ToString());
                    onSale3 = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["on_sale_3"].ToString());
                    notOnSale = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["not_on_sale"].ToString());
                    ledgerid = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["ledger_id"].ToString());
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
                    onSale1 = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["on_sale_1"].ToString());
                    onSale2 = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["on_sale_2"].ToString());
                    onSale3 = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["on_sale_3"].ToString());
                    notOnSale = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["not_on_sale"].ToString());
                    ledgerid = Int32.Parse((cbSaleProductCode.SelectedValue as DataRowView)["ledger_id"].ToString());
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
                        onSale1 = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["on_sale_1"].ToString());
                        onSale2 = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["on_sale_2"].ToString());
                        onSale3 = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["on_sale_3"].ToString());
                        notOnSale = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["not_on_sale"].ToString());
                        ledgerid = Int32.Parse((cbSaleProductName.SelectedValue as DataRowView)["ledger_id"].ToString());
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
                    int total = prod.quantity + quantity;
                    if (currentLedger >= total)
                    {
                        if (cbSalePromo.IsChecked ?? false) {
                            if (onSale1 + onSale2 + onSale3 == 0)
                            {
                                MessageBoxResult dialogResult = MessageBox.Show("Акционный товар отсутствует.\r\n Выполнить продажу без акции?",
                                                                                "Предупреждение", MessageBoxButton.YesNo);
                                if (dialogResult == MessageBoxResult.Yes)
                                {
                                    prod.quantity += quantity;
                                    prod.withoutSale += quantity;
                                    dgSale.Items.Refresh();
                                    return;
                                }
                            }
                            else {
                                int[] results = calculateVolumes(quantity, currentLedger, onSale1, onSale2, onSale3);
                                prod.onSale1 += results[0];
                                prod.onSale2 += results[1];
                                prod.onSale3 += results[2];
                                prod.withoutSale += results[3];
                                prod.quantity += results[0] + results[1] + results[2] + results[3];
                                dgSale.Items.Refresh();
                                return;
                            }
                        }
                        else
                        {
                            prod.quantity += quantity;
                            prod.withoutSale += quantity;
                            dgSale.Items.Refresh();
                            return;
                        }
                    }
                    else MessageBox.Show("На складе недостаточно товара. Доступное количество - " + currentLedger);
                    return;
                }
            }
            if (currentLedger >= quantity)
            {
                if (cbSalePromo.IsChecked ?? false)
                {
                    if (onSale1 + onSale2 + onSale3 == 0)
                    {
                        MessageBoxResult dialogResult = MessageBox.Show("Акционный товар отсутствует.\r\n Выполнить продажу без акции?",
                                                                        "Предупреждение", MessageBoxButton.YesNo);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            this.saleProducts.Add(new Product(commodityNameSelected,
                    commodityCodeSelected, quantity, priceid, ledgerid, 0, 0, 0, quantity,
                    onSale1, onSale2, onSale3, notOnSale, currentLedger));
                            return;
                        }
                    }
                    else
                    {
                        int[] results = calculateVolumes(quantity,currentLedger,onSale1,onSale2,onSale3);
                        this.saleProducts.Add(new Product(commodityNameSelected,
                    commodityCodeSelected, quantity, priceid, ledgerid, results[0], results[1], results[2], results[3],
                    onSale1, onSale2, onSale3, notOnSale, currentLedger));
                        return;
                    }
                }
                else
                {
                    this.saleProducts.Add(new Product(commodityNameSelected,
                    commodityCodeSelected, quantity, priceid, ledgerid, 0, 0, 0, quantity,
                    onSale1, onSale2, onSale3, notOnSale, currentLedger));
                    return;
                }
                
            }
            else MessageBox.Show("На складе недостаточно товара. Доступное количество - " + currentLedger);
            //В майн виндоу создаем коллекцию и добавляем в нее товар (данные мы уже забрали из бд и при вводе)
        }

        private int[] calculateVolumes(int quantity, int currentLedger, int onSale1, int onSale2, int onSale3 ) {
            int demand = quantity;
            int saledOnSale1 = 0;
            int saledOnSale2 = 0;
            int saledOnSale3 = 0;
            int saledWithoutSale = 0;
            while (demand > 0)
            {
                if (onSale1 > SALE1VALUE-1)
                {
                    while (onSale1 > SALE1VALUE-1 && demand > SALE1VALUE-2)
                    {
                        currentLedger -= SALE1VALUE;
                        demand -= SALE1VALUE;
                        if (demand > currentLedger)
                        {
                            currentLedger += SALE1VALUE;
                            demand += SALE1VALUE;
                            break;
                        }
                        else
                        {
                            onSale1 -= SALE1VALUE;
                            saledOnSale1 += SALE1VALUE;
                        }
                    }
                }
                if (onSale2 > SALE2VALUE-1)
                {
                    while (onSale2 > SALE2VALUE-1 && demand > SALE2VALUE-2)
                    {
                        currentLedger -= SALE2VALUE;
                        demand -= SALE2VALUE;
                        if (demand > currentLedger)
                        {
                            currentLedger += SALE2VALUE;
                            demand += SALE2VALUE;
                            break;
                        }
                        else
                        {
                            onSale2 -= SALE2VALUE;
                            saledOnSale2 += SALE2VALUE;
                        }
                    }
                }
                if (onSale3 > SALE3VALUE-1)
                {
                    while (onSale3 > SALE3VALUE-1 && demand > SALE3VALUE-2)
                    {
                        currentLedger -= SALE3VALUE;
                        demand -= SALE3VALUE;
                        if (demand > currentLedger)
                        {
                            currentLedger += SALE3VALUE;
                            demand += SALE3VALUE;
                            break;
                        }
                        else
                        {
                            onSale3 -= SALE3VALUE;
                            saledOnSale3 += SALE3VALUE;
                        }
                    }
                }
                if (demand > 0)
                {
                    saledWithoutSale = demand;
                    demand = 0;
                }

            }
            return new int[4] { saledOnSale1,saledOnSale2,saledOnSale3,saledWithoutSale};
        }

        private int[] calculateBySaleVolumes(int saleType, int demand, int saleQuantity, int totalQuantity) {
            int saleValue = 0;
            int saledOnSale = 0;
            int saledWithoutSale = 0;
            switch (saleType) {
                case 1: saleValue = SALE1VALUE; break;
                case 2: saleValue = SALE2VALUE; break;
                case 3: saleValue = SALE3VALUE; break;
                default: saleValue = Int32.MaxValue; break;
            }
            while (demand > 0)
            {
                if (saleQuantity > saleValue - 1)
                {
                    while (saleQuantity > saleValue - 1 && demand > saleValue - 2)
                    {
                        totalQuantity -= saleValue;
                        demand -= saleValue;
                        if (demand > totalQuantity)
                        {
                            totalQuantity += saleValue;
                            demand += saleValue;
                            break;
                        }
                        else
                        {
                            saleQuantity -= saleValue;
                            saledOnSale += saleValue;
                        }
                    }
                }
                if (demand > 0) {
                    saledWithoutSale += demand;
                    demand = 0;
                }
            }
            return new int[2]{ saledOnSale, saledWithoutSale};
        }

        private void bSaleAddInDB_Click(object sender, RoutedEventArgs e)
        {
            foreach (Product p in this.saleProducts)
            {
                if (p.quantity > 0)
                {
                    saleProductDB(p.ledgerid, p.quantity, p.priceid);
                }
            }

            this.saleProducts.Clear();
            cbSaleProductCode.SelectedIndex = -1;
            cbSaleProductName.SelectedIndex = -1;
            tbSaleProductQuantity.Text = "0";
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
        

        private void dgSale_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                int count_new = 0;
                Product prod_prev = (Product)e.Row.DataContext;
                try
                {
                    count_new = Int32.Parse((e.EditingElement as TextBox).Text);
                }
                catch (Exception exc) {
                    count_new = 0;
                }
                string column = e.Column.Header.ToString();
                int[] results = new int[2] { 0, 0 };
                int prev_value = 0;

                switch (column)
                {
                    case "По акции 1":
                        if (count_new > prod_prev.origOnSale1)
                        {
                            MessageBox.Show("Недостаточно товара на складе. Доступно товара по акции 1 - " + prod_prev.origOnSale1);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        results = calculateBySaleVolumes(1, count_new, prod_prev.origOnSale1, prod_prev.ledger);
                        prev_value = prod_prev.quantity;
                        prod_prev.quantity -= prod_prev.onSale1;
                        prod_prev.quantity += results[0] + results[1];
                        if (prod_prev.quantity > prod_prev.ledger)
                        {
                            prod_prev.quantity = prev_value;
                            MessageBox.Show("Недостаточно товара на складе. Всего доступно товара - " + prod_prev.ledger);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        prod_prev.onSale1 = results[0];
                        prod_prev.withoutSale += results[1];

                        break;
                    case "По акции 2":
                        if (count_new > prod_prev.origOnSale2)
                        {
                            MessageBox.Show("Недостаточно товара на складе. Доступно товара по акции 2 - " + prod_prev.origOnSale2);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        results = calculateBySaleVolumes(2, count_new, prod_prev.origOnSale2, prod_prev.ledger);
                        prev_value = prod_prev.quantity;
                        prod_prev.quantity -= prod_prev.onSale2;
                        prod_prev.quantity += results[0] + results[1];
                        if (prod_prev.quantity > prod_prev.ledger)
                        {
                            prod_prev.quantity = prev_value;
                            MessageBox.Show("Недостаточно товара на складе. Всего доступно товара - " + prod_prev.ledger);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        prod_prev.onSale2 += results[0];
                        prod_prev.withoutSale += results[1];
                        break;
                    case "По акции 3":
                        if (count_new > prod_prev.origOnSale3)
                        {
                            MessageBox.Show("Недостаточно товара на складе. Доступно товара по акции 3 - " + prod_prev.origOnSale3);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        results = calculateBySaleVolumes(3, count_new, prod_prev.origOnSale3, prod_prev.ledger);
                        prev_value = prod_prev.quantity;
                        prod_prev.quantity -= prod_prev.onSale3;
                        prod_prev.quantity += results[0] + results[1];
                        if (prod_prev.quantity > prod_prev.ledger)
                        {
                            prod_prev.quantity = prev_value;
                            MessageBox.Show("Недостаточно товара на складе. Всего доступно товара - " + prod_prev.ledger);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        prod_prev.onSale3 = results[0];
                        prod_prev.withoutSale += results[1];
                        break;
                    case "Без акции":
                        prev_value = prod_prev.quantity;
                        prod_prev.quantity -= prod_prev.withoutSale;
                        prod_prev.quantity += count_new;
                        if (prod_prev.quantity > prod_prev.ledger)
                        {
                            prod_prev.quantity = prev_value;
                            MessageBox.Show("Недостаточно товара на складе. Всего доступно товара - " + prod_prev.ledger);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        prod_prev.withoutSale = count_new;
                        break;
                }
                e.Cancel = true;
                (sender as DataGrid).CancelEdit();
            }

        }

        private string GetConnectionString()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
            props["Extended Properties"] = "Excel 12.0 XML";
            props["Data Source"] = "C:\\Users\\xkirax\\Google Диск\\Полина\\БД товаров.xlsx";

            // XLS - Excel 2003 and Older
            //props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
            //props["Extended Properties"] = "Excel 8.0";
            //props["Data Source"] = "C:\\MyExcel.xls";

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }

            return sb.ToString();
        }

        private DataTable ReadExcelFile()
        {
            string connectionString = GetConnectionString();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                // Loop through all Sheets to get data
                DataRow dr = dtSheet.Rows[0];
                
                    string sheetName = dr["TABLE_NAME"].ToString();

                    // Get all rows from the Sheet
                    cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

                    DataTable dt = new DataTable();
                    dt.TableName = sheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                

                cmd = null;
                conn.Close();
                return dt;
            }
            
        }

        private void bNewUploadFromExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable excel = ReadExcelFile();
            this.newProducts = new ObservableCollection<Product>();
            foreach (DataRow row in excel.Rows)
            {
                int ledger = (int)((double)row["Остаток"]);
                if (ledger > 0) {
                    this.newProducts.Add(new Product((string)row["Наименование"], ((double)row["Код"]).ToString(), ledger, 0));
                }
            }
            dgNew.ItemsSource = this.newProducts;
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

    public class Product: INotifyPropertyChanged
    {
        //Создали класс с полями, ура, методы. счастье. Спасбо за внимание

        //ПОЛИ-НА!!! :))
        public string name { get; set; }
        private int sale1;
        private int sale2;
        private int sale3;
        private int withoutsale;
        private int total;

        public int quantity
        {
            get { return this.total; }
            set
            {
                if (this.total != value)
                {
                    this.total = value;
                    this.NotifyPropertyChanged("quantity");
                }
            }
        }
        public string coralid { get; set; }
        public int priceid { get; set; }
        public int ledgerid { get; set; }
        public int onSale1 {
            get { return this.sale1; }
            set {
                if (this.sale1 != value)
                {
                    this.sale1 = value;
                    this.NotifyPropertyChanged("onSale1");
                }
            }
        }
        public int onSale2
        {
            get { return this.sale2; }
            set
            {
                if (this.sale2 != value)
                {
                    this.sale2 = value;
                    this.NotifyPropertyChanged("onSale2");
                }
            }
        }
        public int onSale3
        {
            get { return this.sale3; }
            set
            {
                if (this.sale3 != value)
                {
                    this.sale3 = value;
                    this.NotifyPropertyChanged("onSale3");
                }
            }
        }
        public int withoutSale
        {
            get { return this.withoutsale; }
            set
            {
                if (this.withoutsale != value)
                {
                    this.withoutsale = value;
                    this.NotifyPropertyChanged("withoutSale");
                }
            }
        }
        public int origOnSale1 { get; set; }
        public int origOnSale2 { get; set; }
        public int origOnSale3 { get; set; }
        public int origWithoutSale { get; set; }
        public int ledger { get; set; }

        public Product(string name, string coralid, int quantity, int priceid) {
            //конструктор первый
            this.name = name;
            this.coralid = coralid;
            this.quantity = quantity;
            this.priceid = priceid;
            this.onSale1 = 0;
            this.onSale2 = 0;
            this.onSale3 = 0;
            this.withoutSale = quantity;
        }

        public Product(string name, string coralid, int quantity, int priceid, int ledgerid, int onSale1, int onSale2, int onSale3, int withoutSale,
            int origOnSale1, int origOnSale2, int origOnSale3, int origWithoutSale, int ledger)
        {
            //конструктор второй (расширенный)
            this.name = name;
            this.coralid = coralid;
            this.priceid = priceid;
            this.ledgerid = ledgerid;
            this.onSale1 = onSale1;
            this.onSale2 = onSale2;
            this.onSale3 = onSale3;
            this.withoutSale = withoutSale;
            this.quantity = onSale1 + onSale2 + onSale3 + withoutSale;
            this.origOnSale1 = origOnSale1;
            this.origOnSale2 = origOnSale2;
            this.origOnSale3 = origOnSale3;
            this.origWithoutSale = origWithoutSale;
            this.ledger = ledger;
            Console.WriteLine(ledger);
        }

        public Product(Product prod) {
            this.name = prod.name;
            this.coralid = prod.coralid;
            this.priceid = prod.priceid;
            this.onSale1 = prod.onSale1;
            this.onSale2 = prod.onSale2;
            this.onSale3 = prod.onSale3;
            this.withoutSale = prod.withoutSale;
            this.quantity = prod.quantity;
            this.origOnSale1 = prod.origOnSale1;
            this.origOnSale2 = prod.origOnSale2;
            this.origOnSale3 = prod.origOnSale3;
            this.origWithoutSale = prod.origWithoutSale;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
    }
