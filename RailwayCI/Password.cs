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
        public Password()
        {
            InitializeComponent();
        }

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

        private void OKbutton_Click(object sender, EventArgs e)
        {

        }
    }
}
