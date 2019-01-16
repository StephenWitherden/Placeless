namespace Placeless.App.Windows
{
    partial class FirstTimeWizard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstTimeWizard));
            this.wizardControl1 = new AeroWizard.WizardControl();
            this.wizStart = new AeroWizard.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.wizDatabaseValidate = new AeroWizard.WizardPage();
            this.lblError = new System.Windows.Forms.Label();
            this.btnSelect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDatabasePath = new System.Windows.Forms.TextBox();
            this.wizDatabaseCreate = new AeroWizard.WizardPage();
            this.wizSelectFolders = new AeroWizard.WizardPage();
            this.txtFolders = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCreateDatabaseOutput = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).BeginInit();
            this.wizStart.SuspendLayout();
            this.wizDatabaseValidate.SuspendLayout();
            this.wizDatabaseCreate.SuspendLayout();
            this.wizSelectFolders.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.BackColor = System.Drawing.Color.White;
            this.wizardControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizardControl1.Location = new System.Drawing.Point(0, 0);
            this.wizardControl1.Margin = new System.Windows.Forms.Padding(4);
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.Pages.Add(this.wizStart);
            this.wizardControl1.Pages.Add(this.wizDatabaseValidate);
            this.wizardControl1.Pages.Add(this.wizDatabaseCreate);
            this.wizardControl1.Pages.Add(this.wizSelectFolders);
            this.wizardControl1.Size = new System.Drawing.Size(765, 511);
            this.wizardControl1.TabIndex = 0;
            this.wizardControl1.Title = "First time wizard";
            // 
            // wizStart
            // 
            this.wizStart.Controls.Add(this.label1);
            this.wizStart.Name = "wizStart";
            this.wizStart.Size = new System.Drawing.Size(718, 337);
            this.wizStart.TabIndex = 0;
            this.wizStart.Text = "First time execution";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.MaximumSize = new System.Drawing.Size(640, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(637, 60);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // wizDatabaseValidate
            // 
            this.wizDatabaseValidate.Controls.Add(this.lblError);
            this.wizDatabaseValidate.Controls.Add(this.btnSelect);
            this.wizDatabaseValidate.Controls.Add(this.label2);
            this.wizDatabaseValidate.Controls.Add(this.txtDatabasePath);
            this.wizDatabaseValidate.Name = "wizDatabaseValidate";
            this.wizDatabaseValidate.Size = new System.Drawing.Size(718, 337);
            this.wizDatabaseValidate.TabIndex = 1;
            this.wizDatabaseValidate.Text = "Create Database";
            this.wizDatabaseValidate.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.wizDatabaseValidate_Initialize);
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(24, 101);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(41, 20);
            this.lblError.TabIndex = 3;
            this.lblError.Text = "Error";
            this.lblError.Visible = false;
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(667, 35);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(31, 23);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "...";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(537, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please enter the path for the database here. Your photos will be stored here too:" +
    "";
            // 
            // txtDatabasePath
            // 
            this.txtDatabasePath.Location = new System.Drawing.Point(4, 33);
            this.txtDatabasePath.Name = "txtDatabasePath";
            this.txtDatabasePath.Size = new System.Drawing.Size(657, 27);
            this.txtDatabasePath.TabIndex = 0;
            // 
            // wizDatabaseCreate
            // 
            this.wizDatabaseCreate.Controls.Add(this.txtCreateDatabaseOutput);
            this.wizDatabaseCreate.Name = "wizDatabaseCreate";
            this.wizDatabaseCreate.Size = new System.Drawing.Size(718, 337);
            this.wizDatabaseCreate.TabIndex = 3;
            this.wizDatabaseCreate.Text = "Creating Database";
            this.wizDatabaseCreate.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.wizDatabaseCreate_Initialize);
            // 
            // wizSelectFolders
            // 
            this.wizSelectFolders.Controls.Add(this.txtFolders);
            this.wizSelectFolders.Controls.Add(this.label3);
            this.wizSelectFolders.Name = "wizSelectFolders";
            this.wizSelectFolders.Size = new System.Drawing.Size(718, 337);
            this.wizSelectFolders.TabIndex = 2;
            this.wizSelectFolders.Text = "Select Image Folders";
            this.wizSelectFolders.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.wizSelectFolders_Commit);
            // 
            // txtFolders
            // 
            this.txtFolders.Location = new System.Drawing.Point(8, 57);
            this.txtFolders.Multiline = true;
            this.txtFolders.Name = "txtFolders";
            this.txtFolders.Size = new System.Drawing.Size(689, 244);
            this.txtFolders.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 4);
            this.label3.MaximumSize = new System.Drawing.Size(640, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(631, 40);
            this.label3.TabIndex = 0;
            this.label3.Text = "If you want to read photos off your local computer or network share, please enter" +
    " the paths in the text box below. If not, you can just leave this blank.";
            // 
            // txtCreateDatabaseOutput
            // 
            this.txtCreateDatabaseOutput.Location = new System.Drawing.Point(4, 4);
            this.txtCreateDatabaseOutput.Multiline = true;
            this.txtCreateDatabaseOutput.Name = "txtCreateDatabaseOutput";
            this.txtCreateDatabaseOutput.Size = new System.Drawing.Size(711, 330);
            this.txtCreateDatabaseOutput.TabIndex = 0;
            // 
            // FirstTimeWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 511);
            this.Controls.Add(this.wizardControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FirstTimeWizard";
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).EndInit();
            this.wizStart.ResumeLayout(false);
            this.wizStart.PerformLayout();
            this.wizDatabaseValidate.ResumeLayout(false);
            this.wizDatabaseValidate.PerformLayout();
            this.wizDatabaseCreate.ResumeLayout(false);
            this.wizDatabaseCreate.PerformLayout();
            this.wizSelectFolders.ResumeLayout(false);
            this.wizSelectFolders.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizardControl1;
        private AeroWizard.WizardPage wizStart;
        private AeroWizard.WizardPage wizDatabaseValidate;
        private AeroWizard.WizardPage wizSelectFolders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDatabasePath;
        private AeroWizard.WizardPage wizDatabaseCreate;
        private System.Windows.Forms.TextBox txtFolders;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.TextBox txtCreateDatabaseOutput;
    }
}