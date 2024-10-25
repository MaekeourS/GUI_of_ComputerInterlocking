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
    public partial class ImportingData : Form
    {
        public delegate void importingDataEventHandler(string newData);

        // 定义一个事件，基于上面的委托
        public event importingDataEventHandler importingData;
        public ImportingData()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string newData = textBox1.Text;
            OnimportingData(newData);
            this.Close();
        }

        protected virtual void OnimportingData(string newData)
        {
            importingData?.Invoke(newData);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
