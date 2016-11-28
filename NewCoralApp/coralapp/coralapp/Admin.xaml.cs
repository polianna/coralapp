using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace coralapp
{
    /// <summary>
    /// Логика взаимодействия для Admin.xaml
    /// </summary>
    public partial class Admin : Window
    {
        private DataTable table;
        private SqlDataAdapter adapter;
        private String connectionString;
        private SqlConnection connection; //Свойство. Оно описано ниже.

        public Admin()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
            connection = new SqlConnection(connectionString); //Создали соединение с базой данных
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Window1 loginWindow = new Window1();
            loginWindow.Show(); //то открываем главное окно
            this.Close(); //И закрываем текущее
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.connection == null)
            {
                this.connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
                this.connection = new SqlConnection(this.connectionString); //Создали соединение с базой данных
            }
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;
            //Взяли имя вкладки и записали в переменную
            switch (tabItem)
            {
                case "tabLogs":
                    dgLog.ItemsSource = getLogs().DefaultView;
                    DataTable userList = getUsers(); //Забрали данные из БД. Реализацию см. ниже
                    cbUserName.ItemsSource = userList.DefaultView;
                    break;
                case "tabDb":
                    cbTableName.ItemsSource = getTables().DefaultView;
                    break;
            }
        }

        private DataTable getLogs() {
            String SQL = "Select * FROM [GetLogs]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable logsTable = new DataTable();
            adapter.Fill(logsTable);
            return logsTable;
        }

        private DataTable getFilteredLogs() {
            String SQL = "Select * FROM [GetLogs] where 1 = case when @username is null then 1 when username = @username then 1 else 0 end and insertdate between coalesce(@begindate,'1900-01-01') and coalesce(@enddate,'2999-12-31')";
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlParameter pUserName = command.Parameters.Add("@username", SqlDbType.NVarChar, -1); //Определяем, что это за параметры. Их тип.
            SqlParameter pBeginDate = command.Parameters.Add("@begindate", SqlDbType.Date);
            SqlParameter pEndDate = command.Parameters.Add("@enddate", SqlDbType.Date);
            string userName = cbUserName.Text;
            if (userName.Equals(String.Empty))
                pUserName.Value = DBNull.Value;
            else pUserName.Value = userName; DateTime? fromDate = dpFromDate.SelectedDate;
            if (fromDate == null)
            {
                pBeginDate.Value = new DateTime(1900, 1, 1);
            }
            else {
                pBeginDate.Value = (DateTime)fromDate;
            }
            DateTime? toDate = dpToDate.SelectedDate;
            if (toDate == null)
            {
                pEndDate.Value = new DateTime(2099, 12, 31);
            }
            else
            {
                pEndDate.Value = (DateTime)toDate;
            }
            command.Prepare(); //Подготовил и собрал наш СКЛ запрос. Например, определил по формату вид содердимой переменной. Для строки расставил кавычки.
            SqlDataAdapter adapter = new SqlDataAdapter(command); //подготовили пакет к отправке
            DataTable commodityTable = new DataTable(); //Определили элемент как таблицу 
            adapter.Fill(commodityTable); //Заполнили логическую структуру (таблицу)
            return commodityTable;
        }

        private DataTable getUsers()
        {
            String SQL = "Select username FROM [User]";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable userTable = new DataTable();
            adapter.Fill(userTable);
            return userTable;
        }

        private void cbUserName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void bFilterLog_Click(object sender, RoutedEventArgs e)
        {
            dgLog.ItemsSource = getFilteredLogs().DefaultView;
        }

        private void bAllLog_Click(object sender, RoutedEventArgs e)
        {
            dgLog.ItemsSource = getLogs().DefaultView;
        }

        private void bSQLExec_Click(object sender, RoutedEventArgs e)
        {
            if (bSave.IsEnabled) { bSave.IsEnabled = false; }
            if (bCancel.IsEnabled) { bCancel.IsEnabled = false; }
            if (tbSQLText.Text.Equals(String.Empty)) {
                MessageBox.Show("Пустой SQL запрос");
            }
            else
            {
                try
                {
                    dgTable.ItemsSource = runSQL(tbSQLText.Text).DefaultView;
                }
                catch (Exception ex) { return; }
            }
        }

        private DataTable runSQL(string SQL)
        {
            try
            {
                SqlCommand command = new SqlCommand(SQL, this.connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                if (connection.State != ConnectionState.Open)
                { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неправильный SQL запрос");
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void bTableView_Click(object sender, RoutedEventArgs e)
        {
            if (cbTableName.Text.Equals(String.Empty)) {
                MessageBox.Show("Не выбрана таблица");
                return;
            }
            try
            {
                if (!bSave.IsEnabled) { bSave.IsEnabled = true; }
                if (!bCancel.IsEnabled) { bCancel.IsEnabled = true; }
                string SQL = "SELECT * FROM ["+cbTableName.Text+"]";
                SqlCommand command = new SqlCommand(SQL, this.connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                this.adapter = adapter;
                if (connection.State != ConnectionState.Open)
                { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
                DataTable table = new DataTable();
                adapter.Fill(table);
                this.table = table;
                dgTable.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private DataTable getTables() {
            try
            {
                string SQL = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='coraldb'";
                SqlCommand command = new SqlCommand(SQL, this.connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                if (connection.State != ConnectionState.Open)
                { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void cbTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            dgTable.ItemsSource = this.table.DefaultView;
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.adapter.Update(this.table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // Error during Update, add code to locate error, reconcile 
                // and try to update again.
            }
        }
    }
}
