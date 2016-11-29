using Accord.Statistics.Models.Regression.Linear;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Data.OleDb;
using Microsoft.Win32;
using System.Windows.Data;

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
        //Определяем список (коллекцию) - содержимое таблицы поставок товара
        private ObservableCollection<Product> saleProducts = new ObservableCollection<Product>();
        //Определяем список (коллекцию) - содержимое таблицы продажи товаров
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
                //Если соединение с бд отсутствует, то создаем его.
                this.connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
                this.connection = new SqlConnection(this.connectionString); //Создали соединение с базой данных
            }
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;
            //Взяли имя вкладки и записали в переменную
            switch (tabItem)
            {
                case "tabSearch":
                    //если пользователь нажал на вкладку "Что на складе?"
                    dgSearch.ItemsSource = allLedgers().DefaultView;
                    //Вернули default(оригинальные) данные в таблицу. 
                    dgSearch.Columns.Clear(); //Очистили заголовки столбцов
                    //Чтобы задать новые имена столбцов и чтобы поля в таблице были привязаны к источнику
                    //Используем структуру Dictionary, в которой первое поле - заголовок, второе ссылка на бд
                    Dictionary<string, string> columns = new Dictionary<string, string>();
                    columns.Add("Наименование товара", "commodity_name");
                    columns.Add("Код товара", "coralclub_id");
                    columns.Add("Описание", "desc");
                    columns.Add("Количество", "quantity");
                    columns.Add("Срок годности", "expiration_date");
                    columns.Add("Стоимость (в у.е.)", "price_value");
                    columns.Add("Стоимость (в баллах)", "point_value");
                    foreach (KeyValuePair<string, string> column in columns)
                        //Для каждого элемента из структыры Dictionary проведём одну и ту же операцию
                    {
                        DataGridTextColumn c = new DataGridTextColumn();
                        c.Header = column.Key; //Заголовку присваиваем ключ, то есть именуем столбец
                        c.Binding = new Binding(string.Format("[{0}]", column.Value)); 
                        // Связке присваиваем ссылку на значение из БД
                        dgSearch.Columns.Add(c); //Каждой колонке из таблицы присваиваем заголовок и связку.
                    }
                    //Таким образом, если имя вкладки = "TabSearch", то мы записали внутрь таблицы все товары со склада. 
                    break;
                case "tabNew":
                   //Если пользователь нажал на вкладку "Новые поступления"
                    dgNew.ItemsSource = this.newProducts;
                    //Источником для таблицы поставок товара является коллекция newProducts. Она создана выше.
                    DataTable priceList = getPriceList(); //Забрали данные из БД. Реализацию см. ниже
                    //Для того, чтобы можно было выбрать имя или код товара из выпадающего списка, 
                    //Указываем откуда будут браться данные (источник). Выпадающий список содержит данные из бд.
                    cbNewProductName.ItemsSource = priceList.DefaultView; 
                    cbNewProductCode.ItemsSource = priceList.DefaultView;
                    break;
                case "tabSale":
                    //Если пользователь нажал на вкладку "Продажа товара"
                    dgSale.ItemsSource = this.saleProducts;
                    //Указываем источник для таблицы продаж - коллекция saleProducts
                    DataTable saleList = getAvailableCommodityList(); //Забрали данные из БД. Реализацию см. ниже
                    //Для того, чтобы можно было выбрать имя или код товара из выпадающего списка, 
                    //Указываем откуда будут браться данные (источник). Выпадающий список содержит данные из бд.
                    cbSaleProductName.ItemsSource = saleList.DefaultView;
                    cbSaleProductCode.ItemsSource = saleList.DefaultView;
                    break;
                default:
                    return;
            }
        }
        

        private void tbSearchProductName_GotFocus(object sender, RoutedEventArgs e)
            //Метод для очистки текстового поля (text box) при нажатии на комонент
        {
            if (tbSearchProductName.Text == "наименование")
            tbSearchProductName.Clear();
        }
       
        private void tbSearchProductCode_GotFocus(object sender, RoutedEventArgs e)
        //Метод для очистки текствого поля (text box) при нажатии на компонент
        {
            if (tbSearchProductCode.Text == "код товара")
                tbSearchProductCode.Clear();
        }
        

        private void button_Click(object sender, RoutedEventArgs e) 
        //Метод для поиска товара по коду или по названию. 
        {
            String SQL = "Select * FROM [LastLedger] where commodity_name like @name or coralclub_id like @code";
            //Создаем SQL команду: выбираем всё из представления (view) последних остатков, где имя и код введенные пользователем совпадают с именем и кодом из бд
            //Необходимо понять, что такое @name and @code, для этого:
            SqlCommand command = new SqlCommand(SQL, this.connection);
            //Определяем, что это за параметры. Их тип.
            SqlParameter parName = command.Parameters.Add("@name", SqlDbType.NVarChar, -1);
            SqlParameter parCode = command.Parameters.Add("@code", SqlDbType.NVarChar, -1);
            parName.Value = tbSearchProductName.Text; //Заполняем параметры, забирая значения введённые пользователем из текстового поля
            parCode.Value = tbSearchProductCode.Text; 
            command.Prepare(); //Подготовил и собрал наш SQL запрос. Например, определил по формату вид содержимой переменной. Для строки расставил кавычки.
            SqlDataAdapter adapter = new SqlDataAdapter(command); //подготовили пакет к отправке
            DataTable commodityTable = new DataTable(); //Определили элемент как таблицу 
            adapter.Fill(commodityTable); //Заполнили логическую структуру (таблицу)
            dgSearch.ItemsSource = commodityTable.DefaultView; // Вернули default (оригинальные) данные в таблицу
            dgSearch.Columns.Clear(); //Очищаем заголовки столбцов
            Dictionary<string, string> columns = new Dictionary<string, string>();
            //Для того, чтобы поля в таблице были привязаны к источнику
            //Указываем название столбца и ссылку на значение в бд
            columns.Add("Наименование товара", "commodity_name");
            columns.Add("Код товара", "coralclub_id");
            columns.Add("Описание", "desc");
            columns.Add("Количество", "quantity");
            columns.Add("Срок годности", "expiration_date");
            columns.Add("Стоимость (в у.е.)", "price_value");
            columns.Add("Стоимость (в баллах)", "point_value");
            //Для каждого элемента из структыры Dictionary проведём одну и ту же операцию
            foreach (KeyValuePair<string, string> column in columns)
            {
                DataGridTextColumn c = new DataGridTextColumn();
                c.Header = column.Key; //Заголовку присваиваем ключ
                c.Binding = new Binding(string.Format("[{0}]", column.Value)); 
                //Связке присваиваем ссылку на значение в БД
                dgSearch.Columns.Add(c);
            }
        }

        private DataTable allLedgers() {
            //Метод для возврата таблицы с данными об остатке товара на складе на текущий момент
            String SQL = "Select * FROM [LastLedger]";
            //Создаем SQL комманду: выбираем всё из представления (view) из бД, которая содержит информацию об остатках на складе
            SqlCommand command = new SqlCommand(SQL, this.connection); //Создаем SQL комманду
            SqlDataAdapter adapter = new SqlDataAdapter(command); //Подготовили пакет к отправке
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable commodityTable = new DataTable(); //Определили элемент как таблицу
            adapter.Fill(commodityTable); //Заполнили логическую структуру (таблицу)
            return commodityTable; //Возвращаем результат выполнения данной функции
        }

        private void bSearchAllProduct_Click(object sender, RoutedEventArgs e)
            //Метод для поиска остатков товаров на складе при нажатии на кнопку 
            // "Весь товар на складе"
        {
            dgSearch.ItemsSource = allLedgers().DefaultView; 
            //Источником для таблицы является результат выполнения функции allLedgers
            dgSearch.Columns.Clear(); //Очищаем заголовки столбцов
            //Используем структуру, в которой первое поле - заголовок, второе - ссылка на значение в бд
            Dictionary<string, string> columns = new Dictionary<string, string>();
            columns.Add("Наименование товара", "commodity_name");
            columns.Add("Код товара", "coralclub_id");
            columns.Add("Описание", "desc");
            columns.Add("Количество", "quantity");
            columns.Add("Срок годности", "expiration_date");
            columns.Add("Стоимость (в у.е.)", "price_value");
            columns.Add("Стоимость (в баллах)", "point_value");
            //Для каждого элемента из структыры Dictionary проведём одну и ту же операцию
            foreach (KeyValuePair<string, string> column in columns)
            {
                DataGridTextColumn c = new DataGridTextColumn();
                c.Header = column.Key; //заголовку присваиваем ключ
                c.Binding = new Binding(string.Format("[{0}]", column.Value)); //Связке присваиваем ссылку на значение в бд
                dgSearch.Columns.Add(c); //Присваиваем заголовок и связку каждому столбцу
            }
        }

        private void bNewAddInTable_Click(object sender, RoutedEventArgs e)
            //Метод для добавления товара в таблицу по нажатии кнопки во вкладке "Новые поступления"
        {

            string commodityName = cbNewProductName.Text; //Переменная для хранения имени товара
            string commodityCode = cbNewProductCode.Text; //Переменная для хранения кода товара
            string commodityCodeSelected = null;
            string commodityNameSelected = null;
            int quantity = 0; //Переменная для хранения количества товара
            try
            {
                //Присваиваем переменной, отвечающей за количества товара, число введенное пользователем в текстовое поле
                quantity = Int32.Parse(tbNewProductQuantity.Text); 
            }
            catch (Exception exc)
            //Если пользователь ввёл не число, то ловим ошибку и присваиваем переменной quantity ноль
            {
                quantity = 0;
                tbNewProductQuantity.Text = "0";
            }

            string expirationdate = "";
            if (dpExpirationDate.Text.Equals(String.Empty)) {
                expirationdate = DateTime.Today.ToShortDateString();
            }
            else expirationdate = dpExpirationDate.Text; //Присваеваем переменной отвечающей за срок годность дату, выбранную пользователем в календаре 
            DateTime expDate = DateTime.ParseExact(expirationdate, "dd.MM.yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture);
            if (expDate<DateTime.Today.Date) {
                MessageBox.Show("Срок годности не может быть меньше текущей даты");
                expirationdate = DateTime.Today.ToShortDateString();
            }

            int priceid = -1; //Вспомогательная переменная для добавления товара в БД


            if ((cbNewProductCode.SelectedValue == null && commodityCode != String.Empty)
                || (cbNewProductName.SelectedValue == null && commodityName != String.Empty))
                //если пользователь ввёл имя или код товара, но при этом нет совпадений введенного значения ни с одной записью из выпадающего списка, то
            {
                MessageBox.Show("Выбран несуществующий товар");
                //То выдаем сообщение об ошибке.
                return;
            }

            if (commodityCode != String.Empty && commodityName != String.Empty) {
                //Если код товара и имя товара, введенные пользователем, соответствуют разным товарам
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
                //если имя и код товара соответствуют одному товару, то
                {
                    priceid = priceidCode; //иначе присваиваем атрибуту priceid значение
                    //и забираем соответствующее имя и код товара из базы данных
                    commodityNameSelected = (cbNewProductCode.SelectedValue as DataRowView)["commodity_name"].ToString();
                    commodityCodeSelected = (cbNewProductCode.SelectedValue as DataRowView)["coralclub_id"].ToString();
                } 

            }
            else
            { //если пользователь ввел код или имя товара и оно совпало со значением из выпадающего списка
                if (commodityCode != String.Empty)
                    //Если пользователь ввел код товара
                {
                    //То присваиваем переменным цены, кода и имени товара соответствующие значения из бд
                    priceid = Int32.Parse((cbNewProductCode.SelectedValue as DataRowView)["price_id"].ToString());
                    commodityNameSelected = (cbNewProductCode.SelectedValue as DataRowView)["commodity_name"].ToString();
                    commodityCodeSelected = (cbNewProductCode.SelectedValue as DataRowView)["coralclub_id"].ToString();
                }

                //то забираем строчечку и присваиваем нашей переменной значение из БД

                else
                {
                    if (commodityName != String.Empty)
                        //Если пользователь не ввел код товара, но ввел имя товара
                    {
                        //То присваиваем переменным цены, кода и имени товара соответствующие значения из бд
                        priceid = Int32.Parse((cbNewProductName.SelectedValue as DataRowView)["price_id"].ToString());
                        commodityNameSelected = (cbNewProductName.SelectedValue as DataRowView)["commodity_name"].ToString();
                        commodityCodeSelected = (cbNewProductName.SelectedValue as DataRowView)["coralclub_id"].ToString();
                    }

                    else { //если пользователь не ввел ни имя, ни код товара
                        //То выдаем сообщение об ошибке
                        MessageBox.Show("Не выбран ни один товар");
                        return;
                    }
                }
            }
            this.newProducts.Add(new Product(commodityNameSelected,
                commodityCodeSelected, quantity, priceid, expirationdate));
            //Дабавляем в коллекцию товар (те данные, что мы забрали из бд и из полей ввода)
        }

        private void bNewAddInDB_Click(object sender, RoutedEventArgs e)
            //метод для добавления данных из таблицы (а точнее коллекции) в бд 
            //для вкладки "Новые поступления" при нажатии на кнопку "Добавить в базу данных"
        {
            foreach (Product p in this.newProducts) {
                if (p.quantity > 0)
                    //Для каждого элемента из коллекции мы вызываем метод, 
                    //который в свою очередь добавляет данные в бд
                {
                    addNewSupplyDB(p.priceid, p.quantity, p.expirationdate);
                }
            }

            this.newProducts.Clear(); //Очищаем коллекцию
            //Чистим поля ввода для пользователя
            cbNewProductCode.SelectedIndex = -1;
            cbNewProductName.SelectedIndex = -1;
            tbNewProductQuantity.Text = "0";
            DataTable priceList = getPriceList(); //Забрали данные из БД. Реализацию см. ниже
            //Снова указываем от куда брать данные в выпадающий список. 
            cbNewProductName.ItemsSource = priceList.DefaultView; 
            cbNewProductCode.ItemsSource = priceList.DefaultView;
        }

        private DataTable getPriceList() {
            //метод для получения из представления (view) ActualPriceList всех значений
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
        //метод для получения всех значений из представления (view) AvailableCommodityList
        {
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
        { //Вспомогательный метод. Нужен, чтобы не обновлялось ничего больше кроме компонентов combobox. Защита от создателей MVS
            e.Handled = true; 

        }

        private void cbNewProductCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { //Вспомогательный метод. Нужен, чтобы не обновлялось ничего больше кроме компонентов combobox. Защита от создателей MVS
            e.Handled = true;
        }

        
private void addNewSupplyDB(int priceId, int quantity, string expiration) {
            //Метод для добавления данных в бд
   try
   {
                if (this.connection.State != ConnectionState.Open)
                { this.connection.Open(); } //Если соединение не открыто, открываем его
                using (SqlCommand cmd = new SqlCommand("newSupply", this.connection))
                    //используя новую SQL комманду
                {
                    cmd.CommandType = CommandType.StoredProcedure; //определили тип sql комманды как процедуру

                    SqlParameter pPriceId = new SqlParameter("@price_id", SqlDbType.Int);
                    //Определили параметр и его тип
                    //После чего присвоили этому параметру значение параметра priceID
                    pPriceId.Value = priceId; 

                    SqlParameter pQuantity = new SqlParameter("@quantity", SqlDbType.Int);
                    //Определили параметр и его тип
                    //После чего присвоили этому параметру значение параметpa quantity
                    pQuantity.Value = quantity;

                    SqlParameter pExpiration = new SqlParameter("@expiration", SqlDbType.Date);
                    //Определили параметр и его тип
                    //После чего присвоили этому параметру значение параметра expiration
                    pExpiration.Value = expiration;
                    //Занесли полученные значения в базу данных
                    cmd.Parameters.Add(pPriceId);
                    cmd.Parameters.Add(pQuantity);
                    cmd.Parameters.Add(pExpiration);
                    cmd.ExecuteNonQuery(); //указали, что нам не нужны результаты.
                }

            }
   catch (Exception ex)
   {
       // Если не получилось занести данные в БД, выдаем сообщение об ошибке
       MessageBox.Show(ex.Message);
   }
}
        private void saleProductDB(int ledgerId, int quantity, int priceId) {
            //метод для добавление данных в бд
            //аналогичен предыдущему методу (addNewSupplyDB)
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
        {//Вспомогательный метод.
            e.Handled = true;
        }

        private void cbSaleProductCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//Вспомогательный метод.
            e.Handled = true;
        }

        private void bSaleAddInTable_Click(object sender, RoutedEventArgs e)
            //Метод для добавления данных в таблицу при нажатии на кнопку "Добавить в корзину" во вкладке "Продажа товара"
        {
            string commodityName = cbSaleProductName.Text; //Переменная для хранения имени товара
            string commodityCode = cbSaleProductCode.Text; //Переменная для хранения кода товара
            string commodityCodeSelected = null;
            string commodityNameSelected = null;
            int quantity = 0; //переменная для хранения количества товара
            try
                //Присваиваем переменной quantity число введенное пользователем в текстовое поле
            {
                quantity = Int32.Parse(tbSaleProductQuantity.Text); 
            }
            catch (Exception exc) {
                //Если было введено что-то помимо цифр, присваиваем переменной quantity ноль
                quantity = 0;
                tbSaleProductQuantity.Text = "0";
            }
            int priceid = -1; //Вспомогательная переменная для добавления товара в БД
            int ledgerid = -1;
            int currentLedger = 0; // количество остатка товара (все типы)
            int onSale1 = 0; // количество остатков по акции 3+1
            int onSale2 = 0;//количество остатков по акции 4+1
            int onSale3 = 0;//количество остатков по акции 5+1
            int notOnSale = 0; // количество остатков без акции          

            if ((cbSaleProductCode.SelectedValue == null && commodityCode != String.Empty)
                || (cbSaleProductName.SelectedValue == null && commodityName != String.Empty))
            //если пользователь ввел имя или код товара, но при этом нет совпадений введенного значения ни с одной записью из выпадающего списка,
            {
                MessageBox.Show("Выбран несуществующий товар");
                //То выдаем сообщение об ошибке
                return;
            }

            if (commodityCode != String.Empty && commodityName != String.Empty)
            {
                //Если имя и код товара относятся к разным товарам 
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
                //если пользователь ввел имя и код товара для одного и того же товара
                {
                    //то заносим в наши вспомогательные переменные дополнительную информацию из бд
                    priceid = priceidCode; 
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
            { //если пользователь ввел только имя или только код товара. И данное значение совпало со значением из выпадающего списка
                if (commodityCode != String.Empty)
                    //Если пользователь ввел только код товара
                {//то заносим в наши вспомогательные переменные дополнительную информацию из бд
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

                else
                {
                    if (commodityName != String.Empty)
                        //Если пользователь ввел только имя товара
                    {//то заносим в наши вспомогательные переменные дополнительную информацию из бд
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
                    { //если пользователь не ввел ни имя ни код товара, 
                        //То выдаем сообщение об ошибке
                        MessageBox.Show("Не выбран ни один товар");
                        return;
                    }
                }
            }
            foreach (Product prod in this.saleProducts) {
                //Проходимся по каждому экземпляру коллекции, чтобы проверить если добавленный товар в коллекции 
                if (priceid == prod.priceid) { //если идентификатор цены у выбранного товара совпадает с id из коллекции, значит этот товар есть в коллекции
                    int total = prod.quantity + quantity; //сколько мы хотим продать (total) = количество товаров уже лежащее в корзине для одного наименования + сколько продавец хочет ещё продать
                    if (currentLedger >= total) //если остатки (по акциям и без акции) товара на складе больше total
                    {
                        if (cbSalePromo.IsChecked ?? false) { //Если продавец поставел флажок в CheckBox "По акции?", то есть хочет продать товар по акции
                            if (onSale1 + onSale2 + onSale3 == 0)
                                //если товаров по акции нет
                            {
                                MessageBoxResult dialogResult = MessageBox.Show("Акционный товар отсутствует.\r\n Выполнить продажу без акции?",
                                                                                "Предупреждение", MessageBoxButton.YesNo);
                                //То выдаем сообщение об ошибке с возможностью выбора: продать товар без акции или отменить продажу данного товара
                                if (dialogResult == MessageBoxResult.Yes)
                                    //Если продавец нажал "Да", то есть захотел продать товар без ации, то 
                                {
                                    prod.quantity += quantity; //увеличиваем количество товара "Итого" в корзине на quantity
                                    prod.withoutSale += quantity; // увеличиваем количество товара "Без акции" в корзине на quantity
                                    dgSale.Items.Refresh(); //обновляем таблицу
                                    return;
                                }
                                //Если продавец нажал "нет", то ничего не проиходит.
                            }
                            else { //если есть хотя бы один товар по любой акции
                                //вызываем специальный метод для проверки возможности продажи товара по акции и без акции 
                                int[] results = calculateVolumes(quantity, currentLedger, onSale1, onSale2, onSale3);
                                //возвращаем результат выполнения функции в массив
                                //обновляем количество проданных по акции, без акции и общее количество
                                prod.onSale1 += results[0];
                                prod.onSale2 += results[1];
                                prod.onSale3 += results[2];
                                prod.withoutSale += results[3];
                                prod.quantity += results[0] + results[1] + results[2] + results[3];
                                dgSale.Items.Refresh(); //обновляем таблицу
                                return;
                            }
                        }
                        else
                        //если продавец не поставил флажок в CheckBox "По акции?", то есть продает товар без акции
                        { //то мы обновляем количество товаров в корзине (в таблице) продаваемых без акции и общее количество
                            prod.quantity += quantity;
                            prod.withoutSale += quantity;
                            dgSale.Items.Refresh();
                            return;
                        }
                    }
                    else MessageBox.Show("На складе недостаточно товара. Доступное количество - " + currentLedger);
                    //Если total превышает остатки, то выдаем сообщение об ошибке
                    return;
                }
            }
            if (currentLedger >= quantity)
                //Если продавец еще не добавлял данный товар в корзину. то мы сравниваем 
                //остаток товара (по акции и без акции) и quantity (количество введенное пользователем в текстовое поле)
            {   //осуществляем проверки аналогичные проверкам выше
                if (cbSalePromo.IsChecked ?? false)
                {//если хотим продать товар по акции
                    if (onSale1 + onSale2 + onSale3 == 0)
                    {//если товара по акции нет, то выдаем сообщение об ошибке с возможность выбора, продать товар без акции или нет
                        MessageBoxResult dialogResult = MessageBox.Show("Акционный товар отсутствует.\r\nВыполнить продажу без акции?",
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
                    //если есть хотя бы один товар по акции, то вызываем метод calculateVolumes
                    //и возвращаем в таблицу результаты 
                    {
                        int[] results = calculateVolumes(quantity,currentLedger,onSale1,onSale2,onSale3);
                        this.saleProducts.Add(new Product(commodityNameSelected,
                    commodityCodeSelected, quantity, priceid, ledgerid, results[0], results[1], results[2], results[3],
                    onSale1, onSale2, onSale3, notOnSale, currentLedger));
                        return;
                    }
                }
                else
                {//если мы хотим продать товар без акции, то обновляем и информацию в таблице
                    this.saleProducts.Add(new Product(commodityNameSelected,
                    commodityCodeSelected, quantity, priceid, ledgerid, 0, 0, 0, quantity,
                    onSale1, onSale2, onSale3, notOnSale, currentLedger));
                    return;
                }
                
            }
            else MessageBox.Show("На складе недостаточно товара. Доступное количество - " + currentLedger);
            //Если товара недостаточно, то выдаем сообщение об ошибке
        }

        private int[] calculateVolumes(int quantity, int currentLedger, int onSale1, int onSale2, int onSale3 ) {
            //метод для подсчета количества товара, которое можно продать по 1, 2 и 3 акции и без акции
            //учитывая спрос покупателя и остатки. Используется в случае ввода информации продавцом в текстовые поля.
            int demand = quantity; //переменная, хранящая спрос покупателя (сколько он хочет купить)
            int saledOnSale1 = 0; //переменная, хранящая количество товара, которое можно продать по акции 1
            int saledOnSale2 = 0; //переменная хранящая количество товара, которое можно продать по акции 2
            int saledOnSale3 = 0; //переменная, хранящая количество товара, которое можно продать по акции 3
            int saledWithoutSale = 0; //переменная, хранящая количество товара, которое продадут без акции
            while (demand > 0) 
                //если спрос не удовлетворен
            {
                if (onSale1 > SALE1VALUE-1)
                    //если остатков по 1 акции больше количества товара необходимого для проведения 1 акции
                    //то есть если соблюдаются условия проведения 1 акции
                {
                    while (onSale1 > SALE1VALUE-1 && demand > SALE1VALUE-2)
                    {//пока соблюдается условие проведения 1 акции
                        //и пока спрос больше или равен минимальному количествую товара, которое нужно купить, чтобы получить подарок
                        currentLedger -= SALE1VALUE; //вычитаем из остатков товары, которые продали (+1 в подарок) по акции 1(в данном случае 4)
                        demand -= SALE1VALUE; //вычитаем из спроса товары, проданные по акции 1
                        if (demand > currentLedger)
                            //если спрос оказался больше остатков
                        {//то делаем откат данного действия и прерываем цикл
                            currentLedger += SALE1VALUE;
                            demand += SALE1VALUE;
                            break;
                        }
                        else
                        //если остатки >=спроса
                        {
                            onSale1 -= SALE1VALUE; //вычитаем из товаров, проданных по акции 1, число товаров, которое отдали по этой акции
                            saledOnSale1 += SALE1VALUE; //вычитаем из товаров, проданных по 1 акции, сколько мы продали
                        }
                    }
                }
                if (onSale2 > SALE2VALUE-1)
                    //данный алгоритм аналогичен алгоритму, соответствующему продаже по 1 акции 
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
                //данный алгоритм аналогичен алгоритму, соответствующему продаже по 1 акции 
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
                    //Если спрос не удовлетворен после продажи по акциям, мы прожаем товар без акции
                {
                    saledWithoutSale = demand;
                    demand = 0;
                }

            }
            return new int[4] { saledOnSale1,saledOnSale2,saledOnSale3,saledWithoutSale};
            //Возвращаем в качестве результата количество товара, проданное по 1,2,3 акции соответственно и без акции
        }

        private int[] calculateBySaleVolumes(int saleType, int demand, int saleQuantity, int totalQuantity) {
            //метод для подсчета количества товара, которое можно продать по 1, 2 и 3 акции и без акции
            //учитывая спрос покупателя и остатки. Используется в случае изменения данных в ячейках таблицы
            int saleValue = 0; //(минимальное количество товара, которое необходимо купить по акции, чтобы получить 1 в подарок) +1.
            int saledOnSale = 0; //количество товара проданное по акции
            int saledWithoutSale = 0; //количество товара проданное без акции
            switch (saleType) {
                case 1: saleValue = SALE1VALUE; break; //если мы изменяем значение в столбце 1 акции, то присваиваем saleValue значение SALE1VALUE
                case 2: saleValue = SALE2VALUE; break; //если мы изменяем значение в столбце 2 акции, то присваиваем saleValue значение SALE2VALUE
                case 3: saleValue = SALE3VALUE; break; //если мы изменяем значение в столбце 3 акции, то присваиваем saleValue значение SALE3VALUE
                default: saleValue = Int32.MaxValue; break; //иначе присваиваем максимальное значение, возможное в системе
            }
            while (demand > 0)
                //пока спрос не удовлетворен
            {
                if (saleQuantity > saleValue - 1)
                    //остатков на складе на которые действует акция больше, чем кол-во которое нужно купить, чтобы выполнить условия акции
                {
                    while (saleQuantity > saleValue - 1 && demand > saleValue - 2)
                        //пока выполняется условие проведения акции 
                        //и спрос больше или равен минимальному количеству товара, которое нужно купить для проведения акции
                    {
                        totalQuantity -= saleValue; //из остатков вычитаем проданное кол-во
                        demand -= saleValue; //вычитаем из спроса проданное кол-во
                        if (demand > totalQuantity)
                        { //Если спрос больше остатков, то отменяем изменения и прерываем цикл
                            totalQuantity += saleValue;
                            demand += saleValue;
                            break;
                        }
                        else
                        {
                            //если спрос меньше остатков, то 
                            //вычитаем из остатков на складе проданное кол-во
                            saleQuantity -= saleValue;
                            saledOnSale += saleValue; //вычитаем из кол-во товара, проданного по акции, сколько мы продали
                        }
                    }
                }
                if (demand > 0) {
                    //если спрос не удовлетворен, то продаем без акции
                    saledWithoutSale += demand;
                    demand = 0;
                }
            }
            return new int[2]{ saledOnSale, saledWithoutSale};
            //возвращаем кол-во товара, проданного по данной акции и без акции
        }

        private void bSaleAddInDB_Click(object sender, RoutedEventArgs e)
            //метод, который добавляет данные о продаже товара в бд и очищает компоненты
            //для вкладки "Продажа товара" при нажатии на кнопку "Продать" 
        {
            foreach (Product p in this.saleProducts)
                //для каждого экземпляра из коллекции
            {
                if (p.quantity > 0)
                    //если количество товара больше 0
                {
                    //вызываем метод, который добавляет данные из коллекции в бд
                    saleProductDB(p.ledgerid, p.quantity, p.priceid);
                }
            }
            //после чего очищаем коллекци, компоненты на форме
            this.saleProducts.Clear();
            cbSaleProductCode.SelectedIndex = -1;
            cbSaleProductName.SelectedIndex = -1;
            tbSaleProductQuantity.Text = "0";
            //обновляем информацию об доступных товарар при помощи метода getAvailableCommodityList, который описан выше
            DataTable saleList = getAvailableCommodityList(); 
            //обновили информацию в выпадающем списке для формы ввода имени и кода товара
            cbSaleProductName.ItemsSource = saleList.DefaultView;
            cbSaleProductCode.ItemsSource = saleList.DefaultView;
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
            //метод, который срабатывает при завершении редактирования ячейки таблицы (нажатие на enter)
            //он позволяет корректно изменить значения количества проданных товаров по акциям, без акции и всего.
        {
            if (e.EditAction == DataGridEditAction.Commit)
            //мы передаем в EditAction (то, чем вызвано событие) состояние Commit (изменение), а не состояние - ошибку (Cancel)
            {
                int count_new = 0;
                Product prod_prev = (Product)e.Row.DataContext;
                //чтобы работать не с объектом, а независимо от него, забираем строчку
                try
                {
                    //пытаемся присвоить переменной введенное число
                    count_new = Int32.Parse((e.EditingElement as TextBox).Text);
                }
                catch (Exception exc) {
                    count_new = 0;
                    //если пользователь ввел не число, то присваиваем переменной ноль
                }
                string column = e.Column.Header.ToString();
                //получаем заголовок столбца, чтобы понимать с какой колонкой работаем
                int[] results = new int[2] { 0, 0 };
                //создаем пустой массив для результата
                int prev_value = 0;

                switch (column)
                {
                    case "По акции 1":
                        //если пользователь меняет ячейку в колонке "по акции 1"
                        if (count_new > prod_prev.origOnSale1)
                        //сравниваем сколько пользователь запросил товара с тем, сколько до добавления в корзину на складе было товара реализуемого по 1 акции
                        {
                            //если продавец запросил кол-во большее, чем доступно на складе по акции, то мы выдаем ошибку и откатываем изменения
                            MessageBox.Show("Недостаточно товара на складе. Доступно товара по акции 1 - " + prod_prev.origOnSale1);
                            e.Cancel = true;
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        results = calculateBySaleVolumes(1, count_new, prod_prev.origOnSale1, prod_prev.ledger);
                        //вызываем метод calculateBySaleVolumes, указываем в качестве передаваемых параметров
                        // тип акции 1, кол-во товара, которое ввел пользователь, остатки на складе по 1 акции до внесения данных в козину, и остатки товара всего на складе до внесения товара в корзину
                        prev_value = prod_prev.quantity; //забрали старое значение из коллекции всего проданного товара "Итого"
                        prod_prev.quantity -= prod_prev.onSale1; //вычли из старого общего количества кол-во проданное по 1 акции (старые данные)
                        prod_prev.quantity += results[0] + results[1]; //и прибавили кол-во товара проданное по акции и без акции
                        if (prod_prev.quantity > prod_prev.ledger)
                            //если количество "Итого", которое мы хотим продать больше остатков на складе
                        {
                            //то выдаем пользователю ошибку и откатываем все изменения
                            prod_prev.quantity = prev_value; 
                            MessageBox.Show("Недостаточно товара на складе. Всего доступно товара - " + prod_prev.ledger);
                            e.Cancel = true; //откат изменений
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        prod_prev.onSale1 = results[0]; //присваиваем кол-ву проданных товаров по акции 1 новое значение
                        prod_prev.withoutSale += results[1]; //прибавляем к кол-ву проданные товаров без акции новое значение

                        break; //прекращаем выполнение switch
                    case "По акции 2":
                        //в случае 2 акции производим действия, аналогичные действиям в случае 1 акции
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
                        //в случае 2 акции производим действия, аналогичные действиям в случае 1 акции
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
                        //в случае измения значения в ячейке, относящейся к столбцу "без акции"
                        prev_value = prod_prev.quantity; //присваиваем вспомогательной переменной значение, которое было в ячейке "Итого" до изменений
                        prod_prev.quantity -= prod_prev.withoutSale; //вычитам из ячейки "Итого" количество товара, которое мы раньше хотели продать без акции
                        prod_prev.quantity += count_new; //вместо него прибавляем то количество, которое мы сейчас хотим продать без акции
                        if (prod_prev.quantity > prod_prev.ledger)
                            //если мы хотим продать товара больше, чем есть на складе
                        { //то возвращаем в "итого" в таблицу предыдущее значение
                            prod_prev.quantity = prev_value;
                            //выдаем сообщение об ошибке
                            MessageBox.Show("Недостаточно товара на складе. Всего доступно товара - " + prod_prev.ledger);
                            e.Cancel = true; //и откатываем все изменения
                            (sender as DataGrid).CancelEdit();
                            return;
                        }
                        prod_prev.withoutSale = count_new; //присваиваем ячейке таблицы новое значения товара, продаваемого без акции
                        break;
                }
                e.Cancel = true; //мы все рассчитали для пользователя, выбрали новые значения 
                //и теперь осталось отменить то значение, которое ввел пользователь
                //оставив те значения, которые мы рассчитали на основе того,
                //что он ввёл
                //например, пользователь ввел в 1 акцию число 10. значит, рассчитаться должно так: по 1 акции мы реализуем 8 шт, без акции 2, 
                //с помощью данного действия, мы отменяем введенное им 10, оставляя рассчитанные 8 и 2
                (sender as DataGrid).CancelEdit();
            }

        }

        private string GetConnectionString(string filePath)
        {
            //создаем структуру Dictionary с двумя параметрами: ключ и значение
            Dictionary<string, string> props = new Dictionary<string, string>();
            //Определяем экзампляры коллекции
            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
            props["Extended Properties"] = "Excel 12.0 XML";
            props["Data Source"] = filePath;

            // XLS - Excel 2003 and Older
            //props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
            //props["Extended Properties"] = "Excel 8.0";
            //props["Data Source"] = "C:\\MyExcel.xls";

            StringBuilder sb = new StringBuilder();
            //объявляем "построитель строк".
            //Для каждого экземпляра коллекции мы создаем строку, в которой
            //берем ключ и присоединяем к нему значение 
            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }
            //после чего объединяем все строки, которые мы собрали при помощи "построителя строк"
            return sb.ToString();
        }

        private DataTable ReadExcelFile(string filePath)
            //метод для получения данных в виде таблицы из excel файла
        {
            
            string connectionString = GetConnectionString(filePath);

            using (OleDbConnection conn = new OleDbConnection(connectionString))
                //Ado.NET позволяет нам работать с офисом (в частности с excel файлами)
                //как с бд. Здесь мы создали соединение.
            {
                conn.Open(); //открываем соединение с "бд" - excel файлом
                OleDbCommand cmd = new OleDbCommand(); //Создаем новый Ole запрос
                cmd.Connection = conn; //для комманды установили соединение

                // Get all Sheets in Excel File
                //Получаем все листы из excel файл 
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                // Loop through all Sheets to get data
                //просмотриваем все листы для получения данных. Берем первую строку.
                DataRow dr = dtSheet.Rows[0];

                string sheetName = dr["TABLE_NAME"].ToString();
                // присваиваем нашей стороковой переменной sheetName - имя листа

                // Get all rows from the Sheet
                //получаем все строки из листа при помощи комманды
                cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

                DataTable dt = new DataTable();
                //определяем элемент как таблицу
                dt.TableName = sheetName; //присваваем нашей имени таблицы имя листа

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                //подготовили пакет к отправке
                da.Fill(dt); //заполнили логическую структуру (таблицу)


                cmd = null; //очистили переменную cmd
                conn.Close(); //закрыли соединение
                return dt; //вернули в качестве результата полученную таблицу
            }
            
        }

        private int getPriceId(string commodityName, double priceValue, double pointValue) {
            //метод для получения идентификатора цены товара 
            try
            {
                if (this.connection.State != ConnectionState.Open)
                { this.connection.Open(); } //открываем соеденение с бд, если оно закрыто
                using (SqlCommand cmd = new SqlCommand("SELECT dbo.getPriceId(@commodity_name,@price_value,@point_value)", this.connection))
                { //создаем SQL комманду
                    //определяем SQL параметр. Их значение и тип.
                    SqlParameter pCommodityName = new SqlParameter("@commodity_name", SqlDbType.NVarChar);
                    pCommodityName.Value = commodityName;
                    //определяем SQL параметр. Их значение и тип.
                    SqlParameter pPriceValue = new SqlParameter("@price_value", SqlDbType.Real);
                    pPriceValue.Value = priceValue;
                    //определяем SQL параметр. Их значение и тип.
                    SqlParameter pPointValue = new SqlParameter("@point_value", SqlDbType.Real);
                    pPointValue.Value = pointValue;
                    //дабавляем вместо неизвестных параметров конкретные значения
                    cmd.Parameters.Add(pCommodityName);
                    cmd.Parameters.Add(pPriceValue);
                    cmd.Parameters.Add(pPointValue);
                    //Для извлечения данных с помощью DataReader необходимо создать экземпляр объекта Command,
                    //а затем создать объект DataReader, вызвав метод Command.ExecuteReader для получения строк из источника данных. 
                    SqlDataReader reader = cmd.ExecuteReader();
                    int price_id = -1;
                    while (reader.Read()) {
                        //считали идентификатор цены 
                        price_id = Int32.Parse(reader[0].ToString());
                    }
                    reader.Close(); //закрываем DataRead, чтобы открыть соединение для дальнейшей работы
                    return price_id;
                }

            }
            catch (Exception ex)
            //если возникла ошибка, выдаем сообщение об ошибке
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }

        private void bNewUploadFromExcel_Click(object sender, RoutedEventArgs e)
            //метод для загрузки в таблицу данных, полученных из excel файла
        {
            // Create an instance of the open file dialog box.
            // СОздать экземпляр диалогового окна открытия файла
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

               //Устанавливаем опции и индекс для фильтра
            openFileDialog1.Filter = "Excel Files (.xlsx)|*.xlsx";
            openFileDialog1.FilterIndex = 1;

            //Чтобы показать диалоговорое окно вызываем метод ShowDialog
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Процесс вставки, если пользователь выбрал "Ок"
            string fileName = "";
            if (userClickedOK == true)
            {
                // Открываем выбранный файл для чтения
                fileName = openFileDialog1.FileName;
            }
            else { 
                //если пользователь не выбрал файл, выводится сообщение об ошибке
                MessageBox.Show("Не выбран файл с исходными данными");
                return;
            }
            DataTable excel = ReadExcelFile(fileName);
            //в табличную структуру возвращаем результат работы метода ReadExcelFile
            this.newProducts = new ObservableCollection<Product>();
            //Заполняем коллекцию данными, полученными из файла
            //для каждой строки
            foreach (DataRow row in excel.Rows)
            {
                int ledger = (int)((double)row["Остаток"]);
                if (ledger > 0) { //если остаток больше нуля
                    string commodity_name = (string)row["Наименование"];
                    string coralclub_code = ((double)row["Код"]).ToString();
                    double price_value = (double)row["Цена"];
                    double point_value = (double)row["Очки"];
                    string expiration_date = "27.11.2016";
                    int price_id = getPriceId(commodity_name,price_value,point_value);
                    //возвращаем идентификатор цены при помощи метода getPriceId
                    if (price_id == -1) {
                        //если идентификатор цены из файла не совпал с идентификатором цены в БД, то выводим сообщение об ошибке
                        MessageBox.Show(String.Format("Товар {0} с ценой {1} руб и {2} очков не найден в БД.\r\nНеобходимо сначала добавить товар.", commodity_name, price_value, point_value));
                    }
                    else {

                        //если идентификаторы совпали добавляем в коллекцию новый товар
                        this.newProducts.Add(new Product(commodity_name, coralclub_code, ledger, price_id, expiration_date));
                    }
                }
            }
            dgNew.ItemsSource = this.newProducts; //заполняем таблицу данными из коллекции
    }

        private void bSearchPromoProduct_Click(object sender, RoutedEventArgs e)
            //метод вывода в таблицу товаров, продающихся по акции
            //при нажатии на кнопку "Акционный товар на складе" во вкладке "Что на складе?"
        {
            dgSearch.ItemsSource = productsOnSale().DefaultView;
            //заполняем таблицу остатками акционных товаров
            dgSearch.Columns.Clear(); //стираем заголовки
            //создаем структуру Dictionary
            Dictionary<string, string> columns = new Dictionary<string, string>();
            //добавляем элементы в структуру
            columns.Add("Наименование товара", "commodity_name");
            columns.Add("Код товара", "coralclub_id");
            columns.Add("Описание", "desc");
            columns.Add("Количество", "quantity");
            columns.Add("Срок годности", "expiration_date");
            columns.Add("Стоимость (в у.е.)", "price_value");
            columns.Add("Стоимость (в баллах)", "point_value");
            columns.Add("Тип акции", "sale_type");
            foreach (KeyValuePair<string, string> column in columns)
            { //для каждого элемента структуры мы
                DataGridTextColumn c = new DataGridTextColumn();
                c.Header = column.Key; //определяем заголовок как ключ колонки
                c.Binding = new Binding(string.Format("[{0}]", column.Value));
                //присваиваем связке ссылку на значение в бд
                dgSearch.Columns.Add(c); //добавляем заголовки столбцов вместе со ссылками на бд в таблицу
            }
        }

        private DataTable productsOnSale() {
            //метод для получания всех данных из представления (view) о товарах, которsе продаются по акции
            String SQL = "Select * FROM [ProductsOnSale]";
            //создаем новуб sql комманду
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command); //готовим пакет к отправке
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable salesTable = new DataTable(); //определяем логическую структуру как таблицу
            adapter.Fill(salesTable); //заполняем таблицу
            return salesTable; //возвращаем полученные данные в виде таблицы
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
            //метод для перехода к форме входа в систему. срабатывает при нажатии на кнопку "Выход"
        {
            Window1 loginWindow = new Window1();
            loginWindow.Show(); //открываем главное окно
            this.Close(); //И закрываем текущее
        }

        private void dgNew_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        { //метод для проверки измененных пользователем данных в ячейку таблицы во вкладке "Новые поступления"
            if (e.EditAction == DataGridEditAction.Commit)
                //при изменении ячейки и нажатии на enter
            {
                int count_new = 0;
                Product prod_prev = (Product)e.Row.DataContext;
                //считываем из таблицы строку
                try
                { //пытаемся присвоить переменной введенное пользователем количество товара
                    count_new = Int32.Parse((e.EditingElement as TextBox).Text);
                }
                catch (Exception exc)
                {
                    //если пользователь ввел не число, то присваиваем переменной значение ноль
                    count_new = 0;
                }

                prod_prev.quantity = count_new; //записываем в ячейку новое значение (из переменной count_new)

                e.Cancel = true; // отменяем число введенное пользователем, оставляя наше присвоенное значение
                (sender as DataGrid).CancelEdit();
            }
        }
        
    }




    public class Commodity : IDataErrorInfo
    //создаем класс Commodity
        
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
                    string st = @"^[0-9]*$";
                    if (!Regex.IsMatch(Name, st)) //Если вводимая строка не соотв.регулярному выражению, то 
                    {
                        result = "Код может содержать только цифры";
                        return result;
                    }
                }
                return null;

            }
        }
    }

    public class Product: INotifyPropertyChanged
        //создаем класс Product
    {

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
        public string expirationdate { get; set; }
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

        public Product(string name, string coralid, int quantity, int priceid, string expirationdate) {
            //конструктор первый
            this.name = name;
            this.coralid = coralid;
            this.quantity = quantity;
            this.priceid = priceid;
            this.onSale1 = 0;
            this.onSale2 = 0;
            this.onSale3 = 0;
            this.withoutSale = quantity;
            this.expirationdate = expirationdate;
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
        {//вызывается, если свойство поменялось
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
    }
