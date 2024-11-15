using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RailwayCI
{
    public partial class NameChange : Form
    {
        public delegate void NameChangedEventHandler(string newName);

        // 定义一个事件，基于上面的委托
        public event NameChangedEventHandler NameChanged;

        public NameChange()
        {
            InitializeComponent();
        }
        public string newName = "";
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                newName = textBox1.Text;
                OnNameChanged(newName);
                this.Close();
            }
            else MessageBox.Show("站场名不能为空！");
        }

        protected virtual void OnNameChanged(string newName)
        {
            NameChanged?.Invoke(newName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (newName == "")
                Application.Exit();
            this.Close();
        }

        private void NameChange_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (newName == "")
                Application.Exit();
            this.Close();
        }
    }
}
