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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace coralapp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String connectionString;
        SqlConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            connection = new SqlConnection(connectionString);
        }   

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;

            switch (tabItem)
            {
                case "tabSearch":
                    String SQL = "Select * FROM [CurrentLedger]";
                    
                    SqlCommand command = new SqlCommand(SQL, this.connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    connection.Open();
                    DataTable commodityTable = new DataTable();
                    adapter.Fill(commodityTable);
                    dgSearch.ItemsSource = commodityTable.DefaultView;
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
        {
            String SQL = "Select * FROM [CurrentLedger] where commodity_name like @name or coralclub_id like @code";

            SqlCommand command = new SqlCommand(SQL, this.connection);
            SqlParameter parName = command.Parameters.Add("@name", SqlDbType.NVarChar, -1);
            SqlParameter parCode = command.Parameters.Add("@code", SqlDbType.NVarChar, -1);
            parName.Value = tbSearchProductName.Text;
            parCode.Value = tbSearchProductCode.Text;
            command.Prepare();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable commodityTable = new DataTable();
            adapter.Fill(commodityTable);
            dgSearch.ItemsSource = commodityTable.DefaultView;

        }
    }
}
