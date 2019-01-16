using MartinCostello.SqlLocalDb;
using Microsoft.WindowsAPICodePack.Dialogs;
using Placeless.BlobStore.FileSystem;
using Placeless.MetadataStore.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Placeless.App.Windows
{
    public partial class FirstTimeWizard : Form
    {
        private readonly IPlacelessconfig _config;

        public FirstTimeWizard(IPlacelessconfig config)
        {
            InitializeComponent();
            _config = config;
        }

        private void wizSelectFolders_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtDatabasePath.Text = dialog.FileName;
            }
        }

        private void wizDatabaseValidate_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e)
        {
            lblError.Visible = false;

            using (var localDB = new SqlLocalDbApi())
            {
                if (!localDB.IsLocalDBInstalled())
                {
                    lblError.Text = "LocalDb is not installed, please install LocalDb 2012 or higher for Placeless to work.";
                    lblError.Visible = true;

                }
                else if (string.IsNullOrWhiteSpace(localDB.GetDefaultInstance().Name))
                {
                    lblError.Text = "Could not determine default instance name.";
                    lblError.Visible = true;
                }

            }
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


        private void wizDatabaseCreate_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e)
        {
            try
            {
                using (var localDB = new SqlLocalDbApi())
                {
                    string databaseName = "Placeless";

                    string instance = localDB.GetDefaultInstance().Name;
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = $"(LocalDb)\\{instance}";
                    builder.InitialCatalog = "Master";
                    builder.IntegratedSecurity = true;

                    int i = 0;
                    var databaseNames = getDatabaseList(builder.ConnectionString);

                    string mdf = System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}.mdf");
                    string ldf = System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}.ldf");

                    while (
                        System.IO.File.Exists(mdf) || 
                        System.IO.File.Exists(ldf) || 
                        databaseNames.Contains(databaseName)
                        )
                    {
                        i++;
                        databaseName = $"Placeless{i:D2}";
                        mdf = System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}.mdf");
                        ldf = System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}.ldf");
                    }


                    SqlMetadataStore.CreateDatabase(builder.ConnectionString, databaseName, mdf, ldf);

                    builder.InitialCatalog = databaseName;
                    _config.SetValue(SqlMetadataStore.CONNECTION_STRING_SETTING, builder.ConnectionString);
                    _config.SetValue(FileSystemBlobStore.BLOB_ROOT_PATH, System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}_Files"));
                    wizardControl1.NextPage();
                }
            }
            catch(Exception ex)
            {
                txtCreateDatabaseOutput.Text += ex.Message;
                txtCreateDatabaseOutput.Text += ex.StackTrace;
            }
        }
    }
}
