namespace DBBrowser
{
    partial class Form1
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tbQuery = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnRunQuery = new System.Windows.Forms.Button();
            this.tbSQLite = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGridView1.Location = new System.Drawing.Point(0, 301);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(800, 522);
            this.dataGridView1.TabIndex = 0;
            // 
            // tbQuery
            // 
            this.tbQuery.Location = new System.Drawing.Point(12, 45);
            this.tbQuery.Multiline = true;
            this.tbQuery.Name = "tbQuery";
            this.tbQuery.Size = new System.Drawing.Size(661, 250);
            this.tbQuery.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(680, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Get Master";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnRunQuery
            // 
            this.btnRunQuery.Location = new System.Drawing.Point(679, 239);
            this.btnRunQuery.Name = "btnRunQuery";
            this.btnRunQuery.Size = new System.Drawing.Size(108, 23);
            this.btnRunQuery.TabIndex = 2;
            this.btnRunQuery.Text = "Run Query";
            this.btnRunQuery.UseVisualStyleBackColor = true;
            this.btnRunQuery.Click += new System.EventHandler(this.btnRunQuery_Click);
            // 
            // tbSQLite
            // 
            this.tbSQLite.Location = new System.Drawing.Point(13, 19);
            this.tbSQLite.Name = "tbSQLite";
            this.tbSQLite.Size = new System.Drawing.Size(660, 20);
            this.tbSQLite.TabIndex = 3;
            this.tbSQLite.Text = "E:\\Development\\Development\\Webchat.DNC\\ParaChat.DNC\\ParaChat.db";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(679, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Load DB";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 823);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tbSQLite);
            this.Controls.Add(this.btnRunQuery);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbQuery);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox tbQuery;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnRunQuery;
        private System.Windows.Forms.TextBox tbSQLite;
        private System.Windows.Forms.Button button2;
    }
}

