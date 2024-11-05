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
    public partial class Password : Form
    {
        public delegate void PasswordEventHandler(string password);

        // 定义一个事件，基于上面的委托
        public event PasswordEventHandler PasswordChanged;

        public delegate void PasswordFlagEventHandler(bool flag);

        // 定义一个事件，基于上面的委托
        public event PasswordFlagEventHandler FlagChecked;
        public Password()
        {
            InitializeComponent();
        }

        public string OldPassword = "";
        public bool SettingNewPassword;
        private void button_Click(object sender, EventArgs e)
        {
            Button clickedbutton = (Button)sender;
            if(textBox1.Text.Length < 6)
                textBox1.Text += clickedbutton.Name.Substring(6);
        }

        private void Backspace_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);
            }
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        protected virtual void OnPasswordChanged(string newPassword)
        {
            PasswordChanged?.Invoke(newPassword);
        }
        protected virtual void OnFlagChecked(bool flag)
        {
            FlagChecked?.Invoke(flag);
        }
        private void OKbutton_Click(object sender, EventArgs e)
        {
            string newPassword = textBox1.Text;
            if (this.Text == "设置保护口令")
            {
                OnPasswordChanged(newPassword);
                this.Close();
            }
            else if (this.Text == "验证保护口令" && newPassword == OldPassword)
            {
                bool newFlag = true;
                if(SettingNewPassword)
                    OnPasswordChanged("");
                else OnFlagChecked(newFlag);
                this.Close();
                
            }
            else
            {
                MessageBox.Show("口令错误");
                textBox1.Clear();
            }

        }
    }
}
