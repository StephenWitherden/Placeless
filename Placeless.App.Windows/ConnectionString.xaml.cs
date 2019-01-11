using Placeless.MetadataStore.Sql;
using System;
using System.Collections.Generic;
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

namespace Placeless.App.Windows
{
    /// <summary>
    /// Interaction logic for ConnectionString.xaml
    /// </summary>
    public partial class ConnectionString : Window
    {
        private readonly IPlacelessconfig _config;

        public ConnectionString(IPlacelessconfig config)
        {
            _config = config;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string connectionString = _config.GetValue(SqlMetadataStore.CONNECTION_STRING_SETTING);

            var builder = new SqlConnectionStringBuilder(connectionString);
            cboDatabase.Text = builder.InitialCatalog;
            txtUserName.Text = builder.UserID;
            txtServerName.Text = builder.DataSource;
            cboAuthenticationMethod.Text = builder.IntegratedSecurity ?
                 "Windows Authentication" : "SQL Server Authentication";

            EnableDisable();
        }

        private void cboAuthenticationMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableDisable();
        }

        private void EnableDisable()
        {
            var comboboxItem = cboAuthenticationMethod.SelectedItem as ComboBoxItem;
            if (comboboxItem.Content.ToString() == "SQL Server Authentication")
            {
                pnlUserNamePassword.Visibility = Visibility.Visible;
                txtUserName.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }
            else
            {
                pnlUserNamePassword.Visibility = Visibility.Collapsed;
                txtUserName.IsEnabled = false;
                txtPassword.IsEnabled = false;
            }
        }

        private string getConnectionString()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.InitialCatalog = cboDatabase.Text;
            builder.DataSource = txtServerName.Text;

            if (cboAuthenticationMethod.Text == "SQL Server Authentication")
            {
                builder.IntegratedSecurity = false;
                builder.UserID = txtUserName.Text;
                builder.Password = txtPassword.Password;
            }
            else
            {
                builder.IntegratedSecurity = true;
            }

            return builder.ConnectionString;
        }

        private List<string> getDatabaseList(string connectionString)
        {
            List<string> list = new List<string>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases where name not in ('master', 'model', 'msdb', 'tempdb')", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return list;

        }

        private void cboDatabase_DropDownOpened(object sender, EventArgs e)
        {
            cboDatabase.Items.Clear();
            var databases = getDatabaseList(getConnectionString());

            foreach (var database in databases)
            {
                cboDatabase.Items.Add(database);
            }
        }

        private void btnNewDatabase_Click(object sender, RoutedEventArgs e)
        {
            var createDatabaseForm = new CreateDatabase();
            if (createDatabaseForm.ShowDialog().GetValueOrDefault())
            {
                string connectionString = getConnectionString();
                SqlMetadataStore.CreateDatabase(connectionString,
                    createDatabaseForm.DatabaseName,
                    createDatabaseForm.DatabaseFile,
                    createDatabaseForm.LogFile,
                    createDatabaseForm.FileFolder
                    );
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            _config.SetValue(SqlMetadataStore.CONNECTION_STRING_SETTING, getConnectionString());

            Close();
        }
    }
}
