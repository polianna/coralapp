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
        //метод для выхода в форму входа в систему. срабатывает по нажатии на кнопку "Выйти"
        {
            Window1 loginWindow = new Window1();
            loginWindow.Show(); //открываем форму входа
            this.Close(); //И закрываем текущее
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //Метод, срабатывающий при нажатии на вкладку из панель вкладок
        {
            if (this.connection == null)
            {//Если нет соединения в бд, то
                this.connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                // Забрали строчку. Конфиг.менеджер прочитал app.config.  Из массива забрали именно Default connection. И теперь мы знаем к какой БД идти.
                this.connection = new SqlConnection(this.connectionString); //Создали соединение с базой данных
            }
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;
            //Взяли имя вкладки и записали в переменную
            switch (tabItem)
            {
                case "tabLogs":
                    //В случае нажатия на вкладку "Журнал учета событий"
                    dgLog.ItemsSource = getLogs().DefaultView;
                    //Помещаем в таблицу результат выполнения методе qetLogs. Реализация см.ниже
                    DataTable userList = getUsers(); //Забрали данные из БД. Реализацию см. ниже
                    //Указали источник для выпадающего списка в поле ввода имени пользователя
                    cbUserName.ItemsSource = userList.DefaultView;
                    break;
                case "tabDb":
                    cbTableName.ItemsSource = getTables().DefaultView;
                    //Помещаем в таблицу результат выполнения метода qetTables. Реализацию см.ниже
                    break;
            }
        }

        private DataTable getRoles()
        { //метод для получения всех значений из представления (view) getRoles
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
            //метод для получения значений информации о пользователях из бд
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
            //метод для получения всех значений из представления (view) getLogs
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
            //Метод для получения журнала учета событий по выбранному пользователю за выбранный период
            String SQL = "Select * FROM [GetLogs] where 1 = case when @username3 is null then 1 when username = @username3 then 1 else 0 end and insertdate between coalesce(@begindate,'1900-01-01') and coalesce(@enddate,'2999-12-31')";
            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlParameter pUserName = command.Parameters.Add("@username3", SqlDbType.NVarChar, -1); //Определяем, что это за параметры. Их тип.
            SqlParameter pBeginDate = command.Parameters.Add("@begindate", SqlDbType.Date);
            SqlParameter pEndDate = command.Parameters.Add("@enddate", SqlDbType.Date);
            string userName = cbUserName.Text; //сохраняем в стороковой переменной username значение, введенное пользователем в поле ввода имени пользователя
            if (userName.Equals(String.Empty)) //Если пользователь ничего не ввел имя пользователя
                pUserName.Value = DBNull.Value; //То параметру присваиваем значение null
            else pUserName.Value = userName; DateTime? fromDate = dpFromDate.SelectedDate;
            //Если админ ввел имя пользователя, то параметру pUserName присваиваем введенное значение, 
            //А новой переменной fromDate, отвечающую за начальную дату, присваиваем выбранную пользователем дату из календара или null, если он ничего не ввел
            if (fromDate == null) //если админ не ввел дату
            {
                pBeginDate.Value = new DateTime(1900, 1, 1);
                //То параметру, отвечающему за начальную дату присваиваем 1.01.1900
            }
            else
            {  //Если пользовател ввел дату, то параметру присваиваем введенное значение
                pBeginDate.Value = (DateTime)fromDate;
            }
            DateTime? toDate = dpToDate.SelectedDate;
            //Конечной дате присваиваем дату, выбранную пользователем или null
            if (toDate == null)
            {//если пользователь не ввел дату
                pEndDate.Value = new DateTime(2099, 12, 31);
                //То параметру, отвечающему за конечную дату присваиваем 21.12.2099
            }
            else
            { //Если пользовател ввел дату, то параметру присваиваем введенное значение
                pEndDate.Value = (DateTime)toDate;

            }
            command.Prepare(); //Подготовил и собрал наш СКЛ запрос. Например, определил по формату вид содердимой переменной. Для строки расставил кавычки.
            SqlDataAdapter adapter = new SqlDataAdapter(command); //подготовили пакет к отправке
            DataTable commodityTable = new DataTable(); //Определили элемент как таблицу 
            adapter.Fill(commodityTable); //Заполнили логическую структуру (таблицу)
            return commodityTable; //Ввернули данные, полученные SQL запросом в виде таблицы
        }

        private DataTable getUsers()
        //Метод для получения имен пользователей из представления User
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
        {//Вспомогательный метод
            e.Handled = true;
        }

        private void bFilterLog_Click(object sender, RoutedEventArgs e)
        { //метод, выводящий в таблицу отфильтрованный по пользователю и дате журнал учета событий
            dgLog.ItemsSource = getFilteredLogs().DefaultView;
            //Выводим в таблицу результат выполнения метода getFilteredLogs
        }

        private void bAllLog_Click(object sender, RoutedEventArgs e)
        {//метод, выводящий в таблицу журнал учета событий по всем данным
            dgLog.ItemsSource = getLogs().DefaultView;
            //Выводим в таблицу результат выполнения метода getLogs
        }

        private void bSQLExec_Click(object sender, RoutedEventArgs e)
        //Метод, выводящий в таблицу результат выполнения sql запроса
        //после нажатия на кнопку "Выполнить SQL" на вкладке "Изменение данных в БД" 
        {
            if (bSave.IsEnabled) { bSave.IsEnabled = false; }
            if (bCancel.IsEnabled) { bCancel.IsEnabled = false; }
            //Делаем неактивными кнопки "Сохранить" и "Отменить", чтобы админ не мог сохранить в бд результат выполнения sql запроса
            if (tbSQLText.Text.Equals(String.Empty)) {
                MessageBox.Show("Пустой SQL запрос");
            //Если пользователь не ввел sql запрос, то выдаем сообщение об ошибке
            }
            else
            { //Если пользователь ввел sql запрос
                try
                {
                    //То пытаемся вывести в таблицу результат его выполнения
                    //при помощи метода runSQL, которому в качестве пареметров передан
                    //текст введенный пользователем в поле ввода SQL запроса
                    dgTable.ItemsSource = runSQL(tbSQLText.Text).DefaultView;
                }
                catch (Exception ex) { return; }
                //если не вышло выполнить sql запрос, то ничего не делаем
            }
        }

        private DataTable runSQL(string SQL)
        //Метод для получения результата выполнения sql запроса
        {
            try
            {//Создаем sql комманду, содержащую нашу строку, полученную в качестве параметра метода
                SqlCommand command = new SqlCommand(SQL, this.connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                if (connection.State != ConnectionState.Open)
                { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
                DataTable table = new DataTable();
                adapter.Fill(table); //заполняем таблицу (логический элемент)
                return table; // //возвращаем полученную таблицу как результат
            }
            catch (Exception ex)
            { //Если не получилось получить результат выполнения sql запроса
                //Выдаем сообщение об ошибке
                MessageBox.Show("Неправильный SQL запрос");
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void bTableView_Click(object sender, RoutedEventArgs e)
        {// Метод, который выводит выбранную админом таблицу
            //срабатывает после нажатия на кнопку "Просмотр таблицы" во вкладке "Изменение данных в бд"
            if (cbTableName.Text.Equals(String.Empty)) {
                MessageBox.Show("Не выбрана таблица");
                //Если пользователь не выбрал имя таблицы, то выдаем сообщение об ошибке
                return;
            }
            try
            { //Если пользователь выбрал таблицу
                //То делаем активными кнопки "Сохранить" и "Отменить", чтобы админ мог менять данные в бд
                if (!bSave.IsEnabled) { bSave.IsEnabled = true; }
                if (!bCancel.IsEnabled) { bCancel.IsEnabled = true; }
                string SQL = "SELECT * FROM ["+cbTableName.Text+"]";
                //Пишем строку sql, в которой хотим получить все значения из выбранной таблицы
                SqlCommand command = new SqlCommand(SQL, this.connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                this.adapter = adapter;
                if (connection.State != ConnectionState.Open)
                { connection.Open(); } //Если соединение не открыто еще, то мы его открываем
                DataTable table = new DataTable();
                adapter.Fill(table); //Заполняем элемент
                this.table = table;
                dgTable.ItemsSource = table.DefaultView; //Выводим полученные данные в таблицу на форме
            
            }
            catch (Exception ex)
            { //Если не получилось вывести, то выдаем сообщение об ошибке
                MessageBox.Show(ex.Message);
            }
        }

        private DataTable getTables() {
            //Метод для получения списка таблиц
            //Метод используется во вкладке "Журнал учета событий"
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
        {//Вспомогательный метод
            e.Handled = true;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        //Метод для отменения изменений, внесенный администратором в таблицу 
        //При нажатии на кнопку "Отменить" во вкладке "Изменение данных в бд"
        {
            //используем метод созданный при помощи sql builder
            dgTable.ItemsSource = this.table.DefaultView;
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        //Метод для обновления данных в бд (сохраняем изменения внесенные администратором) 
        // Срабатывает при нажатии на кнопку "Сохранить" во вкладке "Изменение данных в бд"
        {
            try
            {
                this.adapter.Update(this.table);
                //используем метод созданный при помощи sql builder
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
            //метод для заполнения полей ввода, находящихся над таблицей во вкладке "Управление пользователями"
            //и сохранения значений из выбранной строки таблицы
        { //заполним поля ввода, которые находятся выше таблицы
            if (dgUser.SelectedItem != null) {//Если выбранный объект в таблице не пуст
                DataRowView row = (DataRowView)dgUser.SelectedItem; //то сохраняем объект
                tbUserName.Text = row["username"].ToString(); //в поле ввода имени пользователя вставляем имя, которое мы получили из объекта
                tbDescription.Text = row["desc"].ToString();  //в поле описания пользователя вставляем описание, которое мы получили из объекта
                if ((int)row["isactive"] == 1) { //если пользователь активен (актуален), то ставим галочку в компоненте CheckBox
                    cbActive.IsChecked = true; 
                }
                else {
                    cbActive.IsChecked = false; //если пользователь не активен, то убираем галочку
                }
                cbRole.SelectedValue = row["role_id"].ToString(); //в поле ввода (выбора) роли пользователя ставим идентификатор роли
                this.editUser = new User((int)row["user_id"],row["username"].ToString(),row["desc"].ToString(), //сохраняем в массив старые значения из выбранной строки таблицы
                    row["pass"].ToString(),row["role_name"].ToString(),(int)row["role_id"],cbActive.IsChecked??false);
  
            }
        }

        private void cbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//вспомогательная функция
            e.Handled = true;
        }

        private void bCancelUser_Click(object sender, RoutedEventArgs e)
        {//При нажатии на кнопку "отменить" во вкладке "Управление пользователями"
            //Мы присваиваем компонентам ввода старые значения
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
        //метод для сохранения изменений в бд
        //срабатывает при нажатии на кнопку "Сохранить" во вкладке "Управление пользователями"
        {
            if (this.connection.State != ConnectionState.Open)
            { this.connection.Open(); } //если соединение с бд закрыто, то открываем
            if (this.editUser == null) //если мы не выбирали пользователя
            {
                insertUser(); //то можно создать пользователя
            }
            else {
                updateUser(); //иначе его обновить
            }
            //Очищаем компоненты ввода и массив со старыми данными
            this.editUser = null;
            dgUser.ItemsSource = getUsersInfo().DefaultView;
            tbDescription.Clear();
            tbUserName.Clear();
            pbPassword.Clear();
            cbActive.IsChecked = false;
            cbRole.SelectedItem = null;

        }

        private void insertUser() {
            //метод для создания пользователя
            //создаем вспомогательные переменные
            string pUserName = "";
            string pPassword = "";
            string pDesc = "";
            string pRoleId = "";
            string pActive = "";
            if (tbUserName.Text.Equals(String.Empty))
            {//Если администратов не ввел имя пользователя, то выдаем сообщение об ошибке
                MessageBox.Show("Не задано имя пользователя");
                return;
            }
            else pUserName = "'"+tbUserName.Text+"'"; //если админ ввел имя пользователя, то мы присваиваем нашей
            //вспомогательной переменной введенное имя

            if (pbPassword.Password.Equals(String.Empty))
            {//если не введен пароль, то выдаем сообщение об ошибке
                MessageBox.Show("Не задан пароль");
                return;
            }
            else pPassword = "'"+GetMd5Hash(MD5.Create(), pbPassword.Password)+"'";
            //иначе кодируем пароль по алгоритму МД5 и присваиваем вспомогательной переменной полученное значение

            if (cbActive.IsChecked ?? false)
                //если пользователь активен
            {
                pActive = "1"; //то присваиваем переменной pActiv 1
            }
            else pActive = "0"; //иначе присваиваем ноль

            pDesc = tbDescription.Text; //присваиваем вспомогательной переменной строку, введенную администратором в текстовое поле для доп. информации
            if (!pDesc.Equals(String.Empty)) { pDesc = "'" + pDesc + "'"; } //и если не пуста, то обрабатываем строку
            else pDesc = "null"; //если доп. информацию не ввели, то присваиваем null

            if (cbRole.SelectedItem == null) //если администратор не ввел роль пользователя 
            {
                MessageBox.Show("Не задана роль пользователя"); //то выдаем сообщение об ошибке
                return;
            }
            else pRoleId = cbRole.SelectedValue.ToString(); //иначе забираем значенив во вспомогательную переменную
            //далее пишем SQL запрос, который добавит данные в бд
            string SQL = "INSERT INTO [User](username,isactive,[desc],role_id,pass) VALUES ("+pUserName
                +", "+pActive+", "+pDesc+", "+pRoleId+", "+pPassword+")";
            //создаем sql комманду
            SqlCommand commandIns = new SqlCommand(SQL, this.connection);
            //и выполняем её
            commandIns.ExecuteNonQuery();
        }

        private void updateUser() {
            //метод для обновления существующего пользователя в бд
            //создаем вспомогательные переменные
            string pUserId2 = ""; //Определяем, что это за параметры. Их тип.
            pUserId2 = this.editUser.userid.ToString();
            string pUserName2 = "";
            string pPassword2 = "";
            string pDesc2 = "";
            string pRoleId2 = "";
            string pActive2 = "";

            if (tbUserName.Text.Equals(String.Empty))
                //если имя пользователя не введено, 
            {//то присваиваем вспомогательной переменной старое значение
                pUserName2 = this.editUser.username;
            }
            else pUserName2 = tbUserName.Text;
            //иначе считываем имя из компонента ввода

            if (pbPassword.Password.Equals(String.Empty))
            //если пароль пользователя не введен, 
            {//то присваиваем вспомогательной переменной старое значение
                pPassword2 = this.editUser.password;
            }
            else pPassword2 = GetMd5Hash(MD5.Create(), pbPassword.Password);
            //иначе считываем пароль из компонента ввода, кодируем его при помощи МД5 и присваиваем значение вспомогательной переменной

            if (cbActive.IsChecked ?? false)
                //определяем активен ли пользователь или нет
            {
                pActive2 = "1";
            }
            else pActive2 = "0";

            if (tbDescription.Text.Equals(String.Empty))
            //если доп.информация не введена, 
            {//то присваиваем вспомогательной переменной старое значение
                pDesc2 = this.editUser.description;
            }
            else pDesc2 = tbDescription.Text;
            //иначе присваиваем вспом.переменной значение введенное в компонент ввода

            if (cbRole.SelectedItem == null)
                //если роль не выбрана
            {
                pRoleId2 = this.editUser.roleid.ToString(); //то созраняем старую роль
            }
            else pRoleId2 = cbRole.SelectedValue.ToString(); //иначе считываем из компонента ввода

            //если строки не пустые, то мы их обрабатываем, добавляя одинарные ковычки
            //иначе присваиваем null
            if ( !pUserName2.Equals(String.Empty)) pUserName2 = "'" + pUserName2 + "'";
            else pUserName2 = "null";
            if (!pDesc2.Equals(String.Empty)) pDesc2 = "'" + pDesc2 + "'";
            else pDesc2 = "null";
            if (!pPassword2.Equals(String.Empty)) pPassword2 = "'" + pPassword2 + "'";
            else pPassword2 = "null";
            //создаем sql запрос, который обновит данные о пользователях в бд
            string SQL = "UPDATE [User] SET username = "+pUserName2 
                + ", isactive = "+pActive2 + ", [desc] = "+pDesc2+", role_id = "+pRoleId2 
                + ", pass = "+pPassword2 + " WHERE user_id = "+pUserId2;
            //создаем sql комманду
            SqlCommand commandUpd = new SqlCommand(SQL, this.connection);

            //и выполняем ее без возврата в систему результата
            commandUpd.ExecuteNonQuery();
        }
    }
    public class User {
        //класс Пользователи
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
