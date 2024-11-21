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
        public string StationData = "轨道,1G,150,null,1;辙叉,1,上撇,1G,2G,1/3,null;轨道,2G,280,1,null;道岔,1/3,撇形,1,3;辙叉,3,下撇,4G,3G,null,1/3;轨道,3G,150,3,null;轨道,4G,280,null,3;列车信号机,X,下方,2G,R;调车信号机,D1,上方,4G,L;列车调车信号机,D2,上方,3G,R";
        public string Password = "000000";//默认口令：六个零
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
            public Boolean Painted = false;
            public Boolean Changeable = false;
            public OccupancyStates OccupancyState;
            public RoutePoints RoutePoint;
            public LineShape Rail;
            public SignalPaintings SignalPainting;
            public Label NameLabel;
        }
        PartsOfStations[] PartsOfStation = new PartsOfStations[100];
        public class SignalPaintings
        {
            public LineShape BaseLine;
            public OvalShape DownLight;
            public OvalShape UpLight;
            public RectangleShape TrainButton;
            public RectangleShape ShuntingButton;
            //信号机绘图部件，待补全
        }

        public string StationName
        {
            get { return label1.Text; }
            set
            {
                label1.Text = value;
                this.Text = "计算机联锁控显端仿真 —— " + value + "站";
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"); // 更新Label文本为当前时间  
        }
        private void HandleNameChanged(string newName)
        {
            this.StationName = newName;
            NewTitleLocation();
            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "设置新站场名为：" + StationName);
        }
        private void HandleimportingData(string newData)
        {
            this.StationData = newData;
            DataTransforming();
            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "导入新站场数据");
        }
        private void HandlePassword(string newPassword)
        {
            this.Password = newPassword;
            if (this.Password == "")
            {
            var PasswordForm = new Password();
            PasswordForm.Text = "设置保护口令";
            PasswordForm.PasswordChanged += HandlePassword;
            PasswordForm.ShowDialog();
            }

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
                        PartsOfStation[i].Length = 100;
                        PartsOfStation[i].Directions = Details[2];
                        PartsOfStation[i].UpName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].DownName = Details[4] == "null" ? "" : Details[4];
                        break;
                    case 2:
                        PartsOfStation[i].Length = 30;
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
                        {
                            PartsOfStation[i].LeftName = Details[3];
                            PartsOfStation[i].RightName = "";
                        }

                        else
                        {
                            PartsOfStation[i].RightName = Details[3];
                            PartsOfStation[i].LeftName = "";
                        }

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
                    if (PartsOfStation[i].LeftName != "" && PartsOfStation[i].Left == null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].LeftName)
                            {
                                PartsOfStation[i].Left = PartsOfStation[j];
                                PartsOfStation[j].Right = PartsOfStation[i];
                                break;
                            }
                        }
                    if (PartsOfStation[i].RightName != "" && PartsOfStation[i].Right == null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].RightName)
                            {
                                PartsOfStation[i].Right = PartsOfStation[j];
                                PartsOfStation[j].Left = PartsOfStation[i];
                                break;
                            }
                        }
                    if (PartsOfStation[i].UpName != "" && PartsOfStation[i].Up == null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].UpName)
                            {
                                PartsOfStation[i].Up = PartsOfStation[j];
                                PartsOfStation[j].Down = PartsOfStation[i];
                                break;
                            }
                        }
                    if (PartsOfStation[i].DownName != "" && PartsOfStation[i].Down == null)
                        for (int j = 0; j < SectionNumber; j++)
                        {
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].DownName)
                            {
                                PartsOfStation[i].Down = PartsOfStation[j];
                                PartsOfStation[j].Up = PartsOfStation[i];
                                break;
                            }
                        }
                }
                else
                {
                    if (PartsOfStation[i].RightName == "")
                    {
                        for (int j = 0; j < SectionNumber; j++)
                            if (PartsOfStation[j].NameOfParts == PartsOfStation[i].LeftName)
                            {
                                PartsOfStation[i].Left = PartsOfStation[j];
                                break;
                            }
                    }
                    else
                    {
                        for (int k = 0; k < SectionNumber; k++)
                            if (PartsOfStation[k].NameOfParts == PartsOfStation[i].RightName)
                            {
                                PartsOfStation[i].Right = PartsOfStation[k];
                                break;
                            }
                    }


                }
            }
        }
        public void PartPainting()//绘图
        {
            int Xpoint = 200, Ypoint = 500;
            ShapeContainer shapeContainer = new ShapeContainer();
            shapeContainer.Location = new System.Drawing.Point(0, 0);
            shapeContainer.Size = this.Size;
            EachPartPainting(PartsOfStation[0], Xpoint, Ypoint, shapeContainer, true);
            LightPainting(shapeContainer);
            this.Controls.Add(shapeContainer);
        }
        public void LightPainting(ShapeContainer shapeContainer)
        {
            for (int i = 0; i < SectionNumber; i++)
            {
                PartsOfStations thisPart = PartsOfStation[i];
                int Height = thisPart.Directions == "上方" ? -45 : 20;
                if (thisPart.TypeOfParts >= Types.trainSignal)
                {
                    thisPart.SignalPainting = new SignalPaintings();
                    thisPart.SignalPainting.BaseLine = new LineShape();
                    thisPart.SignalPainting.DownLight = new OvalShape();
                    thisPart.SignalPainting.UpLight = new OvalShape();
                    thisPart.SignalPainting.TrainButton = new RectangleShape();
                    thisPart.SignalPainting.ShuntingButton = new RectangleShape();
                    if (thisPart.Right != null)
                    {
                        thisPart.SignalPainting.BaseLine.X1 = thisPart.Right.Rail.X2;
                        thisPart.SignalPainting.BaseLine.Y1 = thisPart.Right.Rail.Y2 + Height;
                        thisPart.SignalPainting.BaseLine.X2 = thisPart.Right.Rail.X2;
                        thisPart.SignalPainting.BaseLine.Y2 = thisPart.Right.Rail.Y2 + Height + 30;
                        thisPart.SignalPainting.BaseLine.BorderWidth = 2;
                        thisPart.SignalPainting.BaseLine.BorderColor = Color.White;
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.BaseLine);
                        thisPart.SignalPainting.DownLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 30, thisPart.SignalPainting.BaseLine.Y1);
                        thisPart.SignalPainting.DownLight.Size = new Size(30, 30);
                        thisPart.SignalPainting.DownLight.BorderColor = Color.White;
                        thisPart.SignalPainting.DownLight.FillColor = Color.Red;
                        if (thisPart.TypeOfParts == Types.shunttingSignal) thisPart.SignalPainting.DownLight.FillColor = Color.Blue;
                        thisPart.SignalPainting.DownLight.FillStyle = FillStyle.Solid;
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.DownLight);
                        if (thisPart.TypeOfParts != Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.UpLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 60, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.UpLight.Size = new Size(30, 30);
                            thisPart.SignalPainting.UpLight.BorderColor = Color.White;
                            thisPart.SignalPainting.UpLight.FillColor = Color.Black;
                            thisPart.SignalPainting.UpLight.FillStyle = FillStyle.Solid;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.UpLight);
                        }
                        if (thisPart.TypeOfParts == Types.trainSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 10, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(30, 30);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }
                        if (thisPart.TypeOfParts >= Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.ShuntingButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 10, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.ShuntingButton.Size = new Size(30, 30);
                            thisPart.SignalPainting.ShuntingButton.BorderColor = Color.White;
                            thisPart.SignalPainting.ShuntingButton.FillColor = Color.White;
                            thisPart.SignalPainting.ShuntingButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.ShuntingButton.Click += ShuntingButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.ShuntingButton);
                        }
                        if (thisPart.TypeOfParts == Types.multifunctionSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 50, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(30, 30);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }

                    }
                    else if (thisPart.Left != null)
                    {
                        thisPart.SignalPainting.BaseLine.X1 = thisPart.Left.Rail.X1;
                        thisPart.SignalPainting.BaseLine.Y1 = thisPart.Left.Rail.Y1 + Height;
                        thisPart.SignalPainting.BaseLine.X2 = thisPart.Left.Rail.X1;
                        thisPart.SignalPainting.BaseLine.Y2 = thisPart.Left.Rail.Y1 + Height + 30;
                        thisPart.SignalPainting.BaseLine.BorderWidth = 2;
                        thisPart.SignalPainting.BaseLine.BorderColor = Color.White;
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.BaseLine);
                        thisPart.SignalPainting.DownLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1, thisPart.SignalPainting.BaseLine.Y1);
                        thisPart.SignalPainting.DownLight.Size = new Size(30, 30);
                        thisPart.SignalPainting.DownLight.BorderColor = Color.White;
                        thisPart.SignalPainting.DownLight.FillColor = Color.Red;
                        if (thisPart.TypeOfParts == Types.shunttingSignal) thisPart.SignalPainting.DownLight.FillColor = Color.Blue;
                        thisPart.SignalPainting.DownLight.FillStyle = FillStyle.Solid;
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.DownLight);
                        if (thisPart.TypeOfParts != Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.UpLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 30, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.UpLight.Size = new Size(30, 30);
                            thisPart.SignalPainting.UpLight.BorderColor = Color.White;
                            thisPart.SignalPainting.UpLight.FillColor = Color.Black;
                            thisPart.SignalPainting.UpLight.FillStyle = FillStyle.Solid;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.UpLight);
                        }
                        if (thisPart.TypeOfParts == Types.trainSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 40, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(30, 30);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }
                        if (thisPart.TypeOfParts >= Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.ShuntingButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 40, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.ShuntingButton.Size = new Size(30, 30);
                            thisPart.SignalPainting.ShuntingButton.BorderColor = Color.White;
                            thisPart.SignalPainting.ShuntingButton.FillColor = Color.White;
                            thisPart.SignalPainting.ShuntingButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.ShuntingButton.Click += ShuntingButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.ShuntingButton);
                        }
                        if (thisPart.TypeOfParts == Types.multifunctionSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 80, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(30, 30);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }
                    }
                }
            }
        }
        public void EachPartPainting(PartsOfStations thisPart, int Xpoint, int Ypoint, ShapeContainer shapeContainer, Boolean Direction)
        {
            if (thisPart.TypeOfParts == Types.track)
            {
                thisPart.Rail = new LineShape();
                thisPart.Rail.X1 = Xpoint;
                thisPart.Rail.Y1 = Ypoint;
                thisPart.Rail.X2 = Xpoint + thisPart.Length;
                thisPart.Rail.Y2 = Ypoint;
            }
            else if (thisPart.TypeOfParts == Types.frog)
            {
                thisPart.Rail = new LineShape();
                if (thisPart.Left == null)
                    if (thisPart.Directions == "上捺")
                    {
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint - thisPart.Length;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint;
                    }
                    else
                    {
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint + thisPart.Length;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint;
                    }
                else if (thisPart.Right == null)
                    if (thisPart.Directions == "上撇")
                    {
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint - thisPart.Length;
                    }
                    else
                    {
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length;
                    }
                else
                {
                    thisPart.Changeable = true;
                    thisPart.Rail.X1 = Xpoint;
                    thisPart.Rail.Y1 = Ypoint;
                    thisPart.Rail.X2 = Xpoint + thisPart.Length;
                    thisPart.Rail.Y2 = Ypoint;
                }

            }
            else if (thisPart.TypeOfParts == Types.turnout)
            {
                thisPart.Rail = new LineShape();
                if (Direction)
                {
                    Ypoint -= 30;
                    if (thisPart.Directions == "撇形")
                    {
                        Xpoint += 30;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint + thisPart.Length;
                        Xpoint = Xpoint + thisPart.Length;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint;
                    }
                    else
                    {
                        Xpoint -= 30;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        Xpoint = Xpoint - thisPart.Length;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length;
                    }
                }
                else
                {
                    Ypoint += 30;
                    if (thisPart.Directions == "撇形")
                    {
                        Xpoint -= 30;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        Xpoint = Xpoint - thisPart.Length;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length;
                    }
                    else
                    {
                        Xpoint += 30;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        Xpoint = Xpoint + thisPart.Length;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length;
                    }
                }
            }

            if (thisPart.Rail != null)
            {
                thisPart.Rail.BorderWidth = 10;
                thisPart.Rail.BorderColor = Color.Aqua;
                shapeContainer.Shapes.Add(thisPart.Rail);
            }

            if (thisPart.TypeOfParts == Types.track || (thisPart.TypeOfParts == Types.frog && thisPart.Changeable))
            {
                thisPart.NameLabel = new Label
                {
                    Location = new System.Drawing.Point((thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - 15, thisPart.Rail.Y2 + 20),
                    Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))),
                    Size = new System.Drawing.Size(40, 25),
                    Text = thisPart.NameOfParts,
                    ForeColor = System.Drawing.Color.White
                };
                if (thisPart.TypeOfParts == Types.frog && (thisPart.Directions == "下撇" || thisPart.Directions == "下捺"))
                    thisPart.NameLabel.Location = new System.Drawing.Point((thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - 15, thisPart.Rail.Y2 - 40);
                this.Controls.Add(thisPart.NameLabel);
            }

            /*
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(60, 23);
            this.toolStripStatusLabel1.Text = " 总定位 ";
            */
            /*
            各部件绘图，待补全
            */

            thisPart.Painted = true;
            if (thisPart.Right != null && !thisPart.Right.Painted) EachPartPainting(thisPart.Right, Xpoint + thisPart.Length, Ypoint, shapeContainer, false);
            if (thisPart.Up != null && !thisPart.Up.Painted) EachPartPainting(thisPart.Up, Xpoint, Ypoint - thisPart.Up.Length, shapeContainer, true);
            if (thisPart.Down != null && !thisPart.Down.Painted) EachPartPainting(thisPart.Down, Xpoint, Ypoint + thisPart.Length, shapeContainer, false);
            if (thisPart.Left != null && !thisPart.Left.Painted) EachPartPainting(thisPart.Left, Xpoint - thisPart.Left.Length, Ypoint, shapeContainer, false);
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
            PasswordFlag = false;
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
            nameChangeForm.Text = "站场名称修改";
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
                        string NewData = reader.ReadToEnd();
                        if (NewData != "")
                        {
                            StationData = NewData;
                            DataTransforming();
                        }
                        else MessageBox.Show("站场数据不能为空！");
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
            for (int i = 0; i < PartsOfStation.Length; i++)
            {
                if (PartsOfStation[i] != null)// 如果PartsOfStations实例包含LineShape或NameLabel，也需要从父控件中移除它们
                {
                    if (PartsOfStation[i].Rail != null)
                    {
                        PartsOfStation[i].Rail.Parent = null; // 从父控件移除LineShape
                    }
                    if (PartsOfStation[i].SignalPainting != null)
                    {
                        PartsOfStation[i].SignalPainting.BaseLine.Parent = null;
                        PartsOfStation[i].SignalPainting.DownLight.Parent = null;
                        if (PartsOfStation[i].SignalPainting.UpLight != null) PartsOfStation[i].SignalPainting.UpLight.Parent = null;
                        if (PartsOfStation[i].SignalPainting.TrainButton != null) PartsOfStation[i].SignalPainting.TrainButton.Parent = null;
                        if (PartsOfStation[i].SignalPainting.ShuntingButton != null) PartsOfStation[i].SignalPainting.ShuntingButton.Parent = null;
                    }
                    if (PartsOfStation[i].NameLabel != null)
                    {
                        PartsOfStation[i].NameLabel.Dispose(); // 处理NameLabel
                    }
                    PartsOfStation[i] = null;
                }
            }
            if (this.Controls["shapeContainer"] is ShapeContainer shapeContainer)// 移除ShapeContainer中的所有LineShape
            {
                foreach (var shape in shapeContainer.Shapes)
                {
                    shapeContainer.Shapes.Remove((Shape)shape);
                }
            }
            SectionNumber = 0;
            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "重置站场数据");
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

        private void Form1_Load(object sender, EventArgs e)
        {
            var nameChangeForm = new NameChange();
            nameChangeForm.NameChanged += HandleNameChanged;
            nameChangeForm.ShowDialog(this);
            this.WindowState = FormWindowState.Maximized;
            NewTitleLocation();
            NewButtonLocation();
            NewListLocation();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            NewTitleLocation();
            NewButtonLocation();
            NewListLocation();
        }

        private void NewTitleLocation()
        {
            label1.Location = new Point(this.Width / 2 - label1.Size.Width - 80, label1.Location.Y);
            label2.Location = new Point(this.Width / 2 + 50, label2.Location.Y);
        }

        private void NewButtonLocation()
        {
            MessageButton.Location = new Point(this.Width - 30 - MessageButton.Width, this.Height - 55 - MessageButton.Height);
        }
        private void NewListLocation()
        {
            dataGridView1.Location = new Point(MessageButton.Location.X + MessageButton.Width - dataGridView1.Width, MessageButton.Location.Y - dataGridView1.Height);
        }

        private void MessageButton_Click(object sender, EventArgs e)//右下角消息提示框按钮点击方法
        {
            if (dataGridView1.Visible)
                dataGridView1.Visible = false;
            else dataGridView1.Visible = true;
        }

        private void TrainButtonClicked(object sender, EventArgs e)//列车进路开放按钮触发的事件
        {
            RectangleShape ClickedButton = (RectangleShape)sender;
            for (int i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].SignalPainting != null && PartsOfStation[i].SignalPainting.TrainButton == ClickedButton)
                {
                    MessageBox.Show(PartsOfStation[i].NameOfParts);//测试用提示消息，请忽略
                    break;
                }
            }
        }

        private void ShuntingButtonClicked(object sender, EventArgs e)//调车进路开放按钮触发的事件
        {
            RectangleShape ClickedButton = (RectangleShape)sender;
            for (int i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].SignalPainting != null && PartsOfStation[i].SignalPainting.ShuntingButton == ClickedButton)
                {
                    MessageBox.Show(PartsOfStation[i].NameOfParts);//测试用提示消息，请忽略
                    break;
                }
            }
        }

        private void 总定位ButtonClicked()//道岔总定位触发的事件
        {

        }

        private void 总反位ButtonClicked()//道岔总反位触发的事件
        {

        }

        private void 单锁ButtonClicked()//道岔单锁触发的事件
        {

        }

        private void 单解ButtonClicked()//道岔单解触发的事件
        {

        }
    }
}
