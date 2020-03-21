using MartinCostello.SqlLocalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAPICodePack.Dialogs;
using Placeless.BlobStore.FileSystem;
using Placeless.MetadataStore.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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

        private void ReportStatus(string status, int progress)
        {
            this.Invoke((MethodInvoker) delegate
            {
                txtCreateDatabaseOutput.Text += status + "\r\n";
                pbCreateDatabase.Value = progress;
            });
        }


        private void wizDatabaseCreate_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e)
        {
           var createDatabaseTask = new Task(() =>
           {
               try
               {
                   using (var localDB = new SqlLocalDbApi())
                   {
                       ReportStatus("Searching for Localdb instance", 1);
                       ReportStatus("Checking default Instance", 2);

                       var connected = false;
                       string instance = localDB.DefaultInstanceName;
                       List<string> instanceNames = new List<string>(localDB.GetInstanceNames());
                       instanceNames = instanceNames.Where(n => n != instance).ToList(); // only non-default instances

                       SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                       int iInstance = 0;
                       while (!connected)
                       {
                           builder.DataSource = $"(LocalDb)\\{instance}";
                           builder.InitialCatalog = "Master";
                           builder.IntegratedSecurity = true;
                           ReportStatus($"Connecting to {instance}", 2);

                           var con = new SqlConnection(builder.ConnectionString);
                           try
                           {
                               con.Open();
                               connected = true;
                           }
                           catch
                           {
                               connected = false;
                               if (iInstance >= instanceNames.Count)
                               {
                                   ReportStatus("Error: No LocalDb database instances could be connected to.", 0);
                                   return;
                               }
                               instance = instanceNames[iInstance];
                               iInstance++;
                           }
                       }

                       ReportStatus($"Successfully connected to {instance}", 20);

                       string databaseName = "Placeless";
                       int i = 0;
                       var databaseNames = getDatabaseList(builder.ConnectionString);

                       string mdf = System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}.mdf");
                       string ldf = System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}.ldf");

                       ReportStatus($"Selecting a unique database name", 30);

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
                       ReportStatus($"Creating database {databaseName}.", 35);
                       ReportStatus($"Data File: {mdf}", 35);
                       ReportStatus($"Log File: {ldf}", 35);

                       SqlMetadataStore.CreateDatabase(builder.ConnectionString, databaseName, mdf, ldf);

                       builder.InitialCatalog = databaseName;

                       ReportStatus($"Database Created, migrating", 80);

                       var _dbContextOptions = SqlServerDbContextOptionsExtensions.UseSqlServer(
                           new DbContextOptionsBuilder(),
                           builder.ConnectionString,
                           options => options.CommandTimeout(120)
                           ).Options;

                       using (var dbContext = new ApplicationDbContext(_dbContextOptions))
                       {
                           dbContext.Database.Migrate();
                       }

                       ReportStatus($"Migrations executed, updating config", 90);

                       _config.SetValue(SqlMetadataStore.CONNECTION_STRING_SETTING, builder.ConnectionString);
                       _config.SetValue(FileSystemBlobStore.BLOB_ROOT_PATH, System.IO.Path.Combine(txtDatabasePath.Text, $"{databaseName}_Files"));

                       ReportStatus($"Configuration Updated", 100);

                       this.Invoke((MethodInvoker)delegate
                       {
                           wizardControl1.NextPage();
                       });
                   }
               }
               catch (Exception ex)
               {
                   this.Invoke((MethodInvoker)delegate
                   {
                       txtCreateDatabaseOutput.Text += ex.Message + "\r\n";
                       txtCreateDatabaseOutput.Text += ex.StackTrace + "\r\n";
                   });
               }
           }, TaskCreationOptions.LongRunning);
           createDatabaseTask.Start();
        }
    }
}
