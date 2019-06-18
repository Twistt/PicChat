using ArachnidCreations.SqliteDAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnRunQuery_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = DAL.Load(tbQuery.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = DAL.Load("select * from sqlite_master;");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DAL.DBFile = tbSQLite.Text;
        }
    }
}
