using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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
        private User editUser;
        private DataTable roleTable;

        public Admin()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
            connection = new SqlConnection(connectionString); //Создали соединение с базой данных
            dgUser.ItemsSource = getUsersInfo().DefaultView;
            cbRole.ItemsSource = getRoles().DefaultView;
            cbRole.DisplayMemberPath = "role_name";
            cbRole.SelectedValuePath = "role_id";
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

        private DataTable getRoles()
        {
            String SQL = "select * FROM [Role]";
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable roleTable = new DataTable();
            this.roleTable = roleTable;
            adapter.Fill(roleTable);
            return roleTable;
        }

        private DataTable getUsersInfo() {

            String SQL = "select user_id,username,isactive, [User].[desc], [Role].role_name,[Role].role_id,lastlogin,pass,insertdate,updatedate FROM [User] JOIN [Role] on [Role].role_id =[User].role_id";
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            if (connection.State != ConnectionState.Open)
            { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
            DataTable usersTable = new DataTable();
            adapter.Fill(usersTable);
            return usersTable;
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
            String SQL = "Select * FROM [GetLogs] where 1 = case when @username3 is null then 1 when username = @username3 then 1 else 0 end and insertdate between coalesce(@begindate,'1900-01-01') and coalesce(@enddate,'2999-12-31')";
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlParameter pUserName = command.Parameters.Add("@username3", SqlDbType.NVarChar, -1); //Определяем, что это за параметры. Их тип.
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
            }
        }

        private void dgUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUser.SelectedItem != null) {
                DataRowView row = (DataRowView)dgUser.SelectedItem;
                tbUserName.Text = row["username"].ToString();
                tbDescription.Text = row["desc"].ToString();
                if ((int)row["isactive"] == 1) {
                    cbActive.IsChecked = true;
                }
                else {
                    cbActive.IsChecked = false;
                }
                cbRole.SelectedValue = row["role_id"].ToString();
                this.editUser = new User((int)row["user_id"],row["username"].ToString(),row["desc"].ToString(),
                    row["pass"].ToString(),row["role_name"].ToString(),(int)row["role_id"],cbActive.IsChecked??false);
            }
        }

        private void cbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void bCancelUser_Click(object sender, RoutedEventArgs e)
        {
            tbUserName.Text = this.editUser.username;
            tbDescription.Text = this.editUser.description;
            cbActive.IsChecked = this.editUser.isactive;
            cbRole.SelectedValue = this.editUser.roleid.ToString();
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        //Метод для шифровки строки по алгоритму MD5
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private void bSaveUser_Click(object sender, RoutedEventArgs e)
        {
            if (this.connection.State != ConnectionState.Open)
            { this.connection.Open(); }
            if (this.editUser == null)
            {
                insertUser();
            }
            else {
                updateUser();
            }
            
            this.editUser = null;
            dgUser.ItemsSource = getUsersInfo().DefaultView;
            tbDescription.Clear();
            tbUserName.Clear();
            pbPassword.Clear();
            cbActive.IsChecked = false;
            cbRole.SelectedItem = null;

        }

        private void insertUser() {
            
            string pUserName = "";
            string pPassword = "";
            string pDesc = "";
            string pRoleId = "";
            string pActive = "";
            if (tbUserName.Text.Equals(String.Empty))
            {
                MessageBox.Show("Не задано имя пользователя");
                return;
            }
            else pUserName = "'"+tbUserName.Text+"'";

            if (pbPassword.Password.Equals(String.Empty))
            {
                MessageBox.Show("Не задан пароль");
                return;
            }
            else pPassword = "'"+GetMd5Hash(MD5.Create(), pbPassword.Password)+"'";

            if (cbActive.IsChecked ?? false)
            {
                pActive = "1";
            }
            else pActive = "0";

            pDesc = tbDescription.Text;
            if (!pDesc.Equals(String.Empty)) { pDesc = "'" + pDesc + "'"; }
            else pDesc = "null";

            if (cbRole.SelectedItem == null)
            {
                MessageBox.Show("Не задана роль пользователя");
                return;
            }
            else pRoleId = cbRole.SelectedValue.ToString();

            string SQL = "INSERT INTO [User](username,isactive,[desc],role_id,pass) VALUES ("+pUserName
                +", "+pActive+", "+pDesc+", "+pRoleId+", "+pPassword+")";
            SqlCommand commandIns = new SqlCommand(SQL, this.connection);

            commandIns.ExecuteNonQuery();
        }

        private void updateUser() {
            
            string pUserId2 = ""; //Определяем, что это за параметры. Их тип.
            pUserId2 = this.editUser.userid.ToString();
            string pUserName2 = "";
            string pPassword2 = "";
            string pDesc2 = "";
            string pRoleId2 = "";
            string pActive2 = "";

            if (tbUserName.Text.Equals(String.Empty))
            {
                pUserName2 = this.editUser.username;
            }
            else pUserName2 = tbUserName.Text;

            if (pbPassword.Password.Equals(String.Empty))
            {
                pPassword2 = this.editUser.password;
            }
            else pPassword2 = GetMd5Hash(MD5.Create(), pbPassword.Password);

            if (cbActive.IsChecked ?? false)
            {
                pActive2 = "1";
            }
            else pActive2 = "0";

            if (tbDescription.Text.Equals(String.Empty))
            {
                pDesc2 = this.editUser.description;
            }
            else pDesc2 = tbDescription.Text;

            if (cbRole.SelectedItem == null)
            {
                pRoleId2 = this.editUser.roleid.ToString();
            }
            else pRoleId2 = cbRole.SelectedValue.ToString();

            if ( !pUserName2.Equals(String.Empty)) pUserName2 = "'" + pUserName2 + "'";
            else pUserName2 = "null";
            if (!pDesc2.Equals(String.Empty)) pDesc2 = "'" + pDesc2 + "'";
            else pDesc2 = "null";
            if (!pPassword2.Equals(String.Empty)) pPassword2 = "'" + pPassword2 + "'";
            else pPassword2 = "null";

            string SQL = "UPDATE [User] SET username = "+pUserName2 
                + ", isactive = "+pActive2 + ", [desc] = "+pDesc2+", role_id = "+pRoleId2 
                + ", pass = "+pPassword2 + " WHERE user_id = "+pUserId2;
            SqlCommand commandUpd = new SqlCommand(SQL, this.connection);


            commandUpd.ExecuteNonQuery();
        }
    }
    public class User {
        public int userid { get; set; }
        public string username {get; set;}
        public bool isactive { get; set; }
        public int roleid { get; set; }
        public string rolename { get; set; }
        public string description { get; set; }
        public string password { get; set; }

        public User(int userid, string username, string description, string password, string rolename, int roleid, bool isactive) {
            this.userid = userid;
            this.username = username;
            this.description = description;
            this.password = password;
            this.roleid = roleid;
            this.rolename = rolename;
            this.isactive = isactive;

        }

    }
}
