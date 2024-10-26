using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RailwayCI
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label2.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            Timer timer = new Timer();
            timer.Interval = 1000; // 设置计时器间隔为1秒  
            timer.Tick += Timer_Tick; // 绑定Tick事件处理器  
            timer.Start(); // 启动计时器  
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"); // 更新Label文本为当前时间  
        }
        public string StationName
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }
        public string StationData = "";

        private void HandleNameChanged(string newName)
        {
            this.StationName = newName;
        }
        private void HandleimportingData(string newData)
        {
            this.StationData = newData;
        }
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var About = new About();
            About.ShowDialog();
        }


        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {
            ToolStripStatusLabel ClickedStatusLabel = (ToolStripStatusLabel)sender;
            int i = int.Parse(ClickedStatusLabel.Name.Substring(20));
            if (ClickedStatusLabel.BackColor == Color.White)
                ClickedStatusLabel.BackColor = Color.Silver;
            else
                ClickedStatusLabel.BackColor = Color.White;
            for (int j = 1; j <= 9; j++)
            {
                if (j != i)
                {
                    ToolStripStatusLabel Otherlabel = (ToolStripStatusLabel)statusStrip1.Items["toolStripStatusLabel" + j];
                    Otherlabel.BackColor = Color.White;
                }
            }
        }

        private void 直接输入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var importingDataForm = new ImportingData();
            importingDataForm.textBox1.Text = StationData;
            importingDataForm.importingData += HandleimportingData;
            importingDataForm.ShowDialog(this); 
        }

        private void 修改站场名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var nameChangeForm = new NameChange();
            nameChangeForm.textBox1.Text = StationName;
            nameChangeForm.NameChanged += HandleNameChanged;
            nameChangeForm.ShowDialog(this);
        }

        private void 从文件导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); // 创建 OpenFileDialog 的实例
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";// 设置过滤器，只允许选择文本文件
            openFileDialog.FilterIndex = 1;// 设置默认文件类型显示为文本文件
            if (openFileDialog.ShowDialog() == DialogResult.OK)// 显示对话框，如果用户点击了“打开”按钮则继续执行
            {
                if (File.Exists(openFileDialog.FileName))// 检查文件是否确实存在
                {
                    using (StreamReader reader = new StreamReader(openFileDialog.FileName))// 使用 StreamReader 读取文件内容
                    {
                        StationData = reader.ReadToEnd();
                    }
                }
            }
        }

        private void 站场数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();// 创建 SaveFileDialog 的实例
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";// 设置过滤器，只允许保存文本文件
            saveFileDialog.DefaultExt = "txt";// 设置默认文件扩展名为 .txt
            if (saveFileDialog.ShowDialog() == DialogResult.OK)// 显示对话框，如果用户点击了“保存”按钮则继续执行
            {
                string filePath = saveFileDialog.FileName;// 获取要保存的文件路径
                string fileContent = StationData;// 要写入文件的字符串
                using (StreamWriter writer = new StreamWriter(filePath))// 使用 StreamWriter 写入文件内容
                {
                    writer.Write(fileContent);
                }
                MessageBox.Show("文件已成功保存到: " + filePath);// 可选：显示消息框确认文件已保存
            }
        }
    }
}
