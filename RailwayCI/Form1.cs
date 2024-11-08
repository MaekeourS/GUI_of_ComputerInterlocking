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
using System.Drawing.Imaging;
using Microsoft.VisualBasic.PowerPacks;

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
        public string StationData = "";
        public string Password = "";
        public bool PasswordFlag = false;
        public int SectionNumber = 0;
        public enum Types { track, turnout, frog, trainSignal, shunttingSignal, multifunctionSignal };
        public enum OccupancyStates { available, occupied, breakdown };
        public enum RoutePoints { starting, turning, ending };
        public class PartsOfStations
        {
            public PartsOfStations Up;
            public PartsOfStations Down;
            public PartsOfStations Left;
            public PartsOfStations Right;
            public string UpName;
            public string DownName;
            public string LeftName;
            public string RightName;
            public string NameOfParts;
            public Types TypeOfParts;
            public int Length;
            public string Directions;
            public int Conditions;
            public OccupancyStates OccupancyState;
            public RoutePoints RoutePoint;
            public LineShape Rail;
            public SignalPaintings SignalPainting;
        }
        PartsOfStations[] PartsOfStation = new PartsOfStations[100];
        public class SignalPaintings
        {
            //信号机绘图部件，待补全
        }

        public string StationName
        {
            get { return label1.Text; }
            set
            {
                label1.Text = value;
                this.Text = "计算机联锁控显端仿真 —— " + value + " 站";
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"); // 更新Label文本为当前时间  
        }
        private void HandleNameChanged(string newName)
        {
            this.StationName = newName;
        }
        private void HandleimportingData(string newData)
        {
            this.StationData = newData;
            DataTransforming();
        }
        private void HandlePassword(string newPassword)
        {
            this.Password = newPassword;
        }
        private void HandlePasswordFlag(bool newFlag)
        {
            this.PasswordFlag = newFlag;
        }
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var About = new About();
            About.ShowDialog();
        }

        public void DataTransforming()
        {

            string[] EachDatas = StationData.Split(';');
            int i = 0;
            foreach (string EachData in EachDatas)
            {
                string[] Details = EachData.Split(',');
                int typeFlag = 0;
                PartsOfStation[i] = new PartsOfStations();
                switch (Details[0])
                {
                    case "轨道":
                        PartsOfStation[i].TypeOfParts = Types.track; typeFlag = 0; break;
                    case "道岔":
                        PartsOfStation[i].TypeOfParts = Types.turnout; typeFlag = 1; break;
                    case "辙叉":
                        PartsOfStation[i].TypeOfParts = Types.frog; typeFlag = 2; break;
                    case "列车信号机":
                        PartsOfStation[i].TypeOfParts = Types.trainSignal; typeFlag = 3; break;
                    case "调车信号机":
                        PartsOfStation[i].TypeOfParts = Types.shunttingSignal; typeFlag = 4; break;
                    case "列车调车信号机":
                        PartsOfStation[i].TypeOfParts = Types.multifunctionSignal; typeFlag = 5; break;
                }
                PartsOfStation[i].NameOfParts = Details[1];
                switch (typeFlag)
                {
                    case 0:
                        PartsOfStation[i].Length = int.Parse(Details[2]);
                        PartsOfStation[i].LeftName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].RightName = Details[4] == "null" ? "" : Details[4];
                        break;
                    case 1:
                        PartsOfStation[i].Directions = Details[2];
                        PartsOfStation[i].UpName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].DownName = Details[4] == "null" ? "" : Details[4];
                        break;
                    case 2:
                        PartsOfStation[i].Directions = Details[2];
                        PartsOfStation[i].LeftName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].RightName = Details[4] == "null" ? "" : Details[4];
                        PartsOfStation[i].UpName = Details[5] == "null" ? "" : Details[5];
                        PartsOfStation[i].DownName = Details[6] == "null" ? "" : Details[6];
                        break;
                    case 3:
                    case 4:
                    case 5:
                        PartsOfStation[i].Directions = Details[2];
                        if (Details[4] == "L")
                            PartsOfStation[i].LeftName = Details[3];
                        else PartsOfStation[i].RightName = Details[3];
                        break;
                }
                i++;
            }
            SectionNumber = i;
            DataConnecting();
            PartPainting();
        }
        public void DataConnecting()//建立部件间引用
        {
            for (int i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].TypeOfParts <= Types.frog)
                {
                    if (PartsOfStation[i].LeftName != "" && PartsOfStation[i].Left != null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].LeftName)
                            {
                                PartsOfStation[i].Left = PartsOfStation[j];
                                PartsOfStation[j].Right = PartsOfStation[i];
                            }
                        }
                    if (PartsOfStation[i].RightName != "" && PartsOfStation[i].Right != null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].RightName)
                            {
                                PartsOfStation[i].Right = PartsOfStation[j];
                                PartsOfStation[j].Left = PartsOfStation[i];
                            }
                        }
                    if (PartsOfStation[i].UpName != "" && PartsOfStation[i].Up != null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].UpName)
                            {
                                PartsOfStation[i].Up = PartsOfStation[j];
                                PartsOfStation[j].Down = PartsOfStation[i];
                            }
                        }
                    if (PartsOfStation[i].DownName != "" && PartsOfStation[i].Down != null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].DownName)
                            {
                                PartsOfStation[i].Down = PartsOfStation[j];
                                PartsOfStation[j].Up = PartsOfStation[i];
                            }
                        }
                }
                else
                {
                    if (PartsOfStation[i].LeftName != "")
                        for (int j = 0; j < SectionNumber; j++)
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].LeftName)
                                PartsOfStation[i].Left = PartsOfStation[j];
                    else
                        for (int k = 0; k < SectionNumber; k++)
                            if (PartsOfStation[k].NameOfParts == PartsOfStation[i].RightName)
                                PartsOfStation[i].Right = PartsOfStation[k];
                }
            }
        }
        public void PartPainting()//绘图
        {

        }
        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {
            ToolStripStatusLabel ClickedStatusLabel = (ToolStripStatusLabel)sender;
            int i = int.Parse(ClickedStatusLabel.Name.Substring(20));
            if (ClickedStatusLabel.BackColor != Color.White)
            {
                ClickedStatusLabel.BackColor = Color.White;
                return;
            }
            for (int j = 1; j <= 9; j++)
            {
                if (j != i)
                {
                    ToolStripStatusLabel Otherlabel = (ToolStripStatusLabel)statusStrip1.Items["toolStripStatusLabel" + j];
                    Otherlabel.BackColor = Color.White;
                }
            }
            var PasswordForm = new Password();
            PasswordForm.Text = "验证保护口令";
            PasswordForm.OldPassword = Password;
            PasswordForm.SettingNewPassword = false;
            PasswordForm.FlagChecked += HandlePasswordFlag;
            PasswordForm.ShowDialog();
            if (!PasswordFlag) return; else ClickedStatusLabel.BackColor = SystemColors.GradientActiveCaption;
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
            saveFileDialog.FileName = StationName + "站 站场数据";
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

        private void 修改保护口令ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Password == "")
            {
                var PasswordForm = new Password();
                PasswordForm.Text = "设置保护口令";
                PasswordForm.PasswordChanged += HandlePassword;
                PasswordForm.ShowDialog();
            }
            else
            {
                var PasswordForm = new Password();
                PasswordForm.Text = "验证保护口令";
                PasswordForm.OldPassword = Password;
                PasswordForm.SettingNewPassword = true;
                PasswordForm.PasswordChanged += HandlePassword;
                PasswordForm.ShowDialog();
            }

        }

        private void 重置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StationData = "";
        }

        private void 站场图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Bitmap bitmap = new Bitmap(this.Width, this.Height))// 创建一个Bitmap对象，其大小与当前窗口相同
            {
                using (Graphics g = Graphics.FromImage(bitmap))// 使用Graphics对象从当前窗口绘制图像到Bitmap
                {
                    g.CopyFromScreen(this.Location, Point.Empty, this.Size);
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog// 使用SaveFileDialog让用户选择保存文件的位置
                {
                    Filter = "JPEG Image|*.jpg;*.jpeg|PNG Image|*.png|BMP Image|*.bmp",
                    FileName = StationName + "站站场图  " + DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"),
                    Title = "导出为"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    switch (Path.GetExtension(saveFileDialog.FileName).ToLower())// 根据用户选择的文件格式保存图像
                    {
                        case ".jpg":
                        case ".jpeg":
                            bitmap.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                            break;
                        case ".png":
                            bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
                            break;
                        case ".bmp":
                            bitmap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                            break;
                        default:
                            MessageBox.Show("Unsupported file format.");
                            break;
                    }
                }
            }
        }
    }
}
