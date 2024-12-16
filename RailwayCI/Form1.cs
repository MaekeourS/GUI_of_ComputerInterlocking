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
using System.Net;
using System.Threading;

namespace RailwayCI
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label2.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 设置计时器间隔为1秒  
            timer.Tick += Timer_Tick; // 绑定Tick事件处理器  
            timer.Start(); // 启动计时器  
            DelayTimer.Interval = 1000;
            DelayTimer.Tick += DelayTimer_Tick;
            SignalTimer.Interval = 1000;
            SignalTimer.Tick += SignalChange;
        }
        public string StationData = "轨道,0/1DG,50,null,1DG;轨道,1DG,150,0/1DG,1DG/1;轨道,1DG/1,50,1DG,1;辙叉,1,上撇,1DG/1,1/IG,1/3,null;轨道,1/IG,150,1,IG;轨道,IG,380,1/IG,IG/2;轨道,IG/2,150,IG,2;道岔,1/3,撇形,3,1;辙叉,3,下撇,null,3/IIG,null,1/3;轨道,3/IIG,50,3,IIG;轨道,IIG,450,3/IIG,IIG/4;轨道,IIG/4,50,IIG,null;辙叉,4,下捺,IIG/4,null,null,2/4;道岔,2/4,捺形,4,2;辙叉,2,上捺,IG/2,2/2DG,2/4,null;轨道,2/2DG,50,2,null;轨道,2DG,150,2/2DG,2DG/0;轨道,2DG/0,50,2DG,null;列车调车信号机,X,上方,1DG,L;列车调车信号机,S,上方,2DG/0,L;调车信号机,D2,下方,IG/2,L;调车信号机,D1,下方,1/IG,R;调车信号机,D4,上方,IIG/4,L;调车信号机,D3,上方,3/IIG,R";
        public string Password = "123";//默认口令：123
        public bool PasswordFlag = false;
        public bool SignalLabelDisplayFlag = true;
        public bool TurningLabelDisplayFlag = true;
        public bool RailLabelDisplayFlag = true;
        public int TurningFlag = -1;
        public int SectionNumber = 0;
        public int RailNumber = 0;
        public int DelayTime = 3;
        public int CancelPart = -1;
        public System.Windows.Forms.Timer DelayTimer = new System.Windows.Forms.Timer();
        public System.Windows.Forms.Timer SignalTimer = new System.Windows.Forms.Timer();
        public enum Types { track, turnout, frog, trainSignal, shunttingSignal, multifunctionSignal };
        public enum OccupancyStates { available, occupied, breakdown };
        public enum OccupancyDirections { none, left, right };
        public enum RoutePoints { Other, starting, turning, ending, TrainStart, TrainEnd, ShuntingStart, ShuntingEnd, TrainPoint, ShuntingPoint };
        public class PartsOfStations
        {
            public int Number;
            public int LineNumber;
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
            public bool Painted = false;
            public bool Locked = false;
            public bool Changeable = false;
            public bool lineChanged = false;
            public bool Visited = false;
            public OccupancyStates OccupancyState;
            public OccupancyDirections OccupancyDirection;
            public RoutePoints RoutePoint;
            public LineShape Rail;
            public LineShape InsulatedJoint;
            public OvalShape LockSign;
            public SignalPaintings SignalPainting;
            public Label NameLabel;
        }
        public PartsOfStations[] PartsOfStation = new PartsOfStations[300];
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
                this.Text = "计算机联锁控显端仿真 —— " + value;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"); // 更新Label文本为当前时间  
        }
        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            DelayTime--;
            DelayLabel.Text = "剩余延时：" + DelayTime.ToString() + "秒";
            if (DelayTime <= 0)
            {
                DelayTimer.Stop();
                RouteClearing(CancelPart);
                CancelPart = -1;
                DelayLabel.Text = "";
                DelayLabel.Visible = false;
                if (toolStripStatusLabel7.BackColor == SystemColors.GradientActiveCaption)
                {
                    toolStripStatusLabel7.BackColor = Color.White;
                    dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "总取消");
                }
                else if (toolStripStatusLabel8.BackColor == SystemColors.GradientActiveCaption)
                {
                    toolStripStatusLabel8.BackColor = Color.White;
                    dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "总人解");
                }

                return;
            }

        }
        private void SignalChange(object sender, EventArgs e)
        {
            //更新信号机显示
            for (int i = 0; i < SectionNumber; i++)
            {
                PartsOfStations ThisSignal = PartsOfStation[i];
                if (ThisSignal.TypeOfParts == Types.trainSignal)
                {
                    int j = ThisSignal.Left != null ? ThisSignal.Left.Number : ThisSignal.Right.Number;
                    PartsOfStations ThisRail = PartsOfStation[j];
                    if (ThisRail.OccupancyState == OccupancyStates.occupied && (ThisRail.RoutePoint == RoutePoints.TrainPoint || ThisRail.RoutePoint == RoutePoints.TrainStart || ThisRail.RoutePoint == RoutePoints.TrainEnd))
                    {
                        if ((ThisSignal.Right == null && ThisRail.OccupancyDirection == OccupancyDirections.right) || (ThisSignal.Left == null && ThisRail.OccupancyDirection == OccupancyDirections.left))
                        {
                            if (ThisRail.lineChanged)
                                ThisSignal.SignalPainting.DownLight.FillColor = Color.FromArgb(255, 255, 0);
                            else
                                ThisSignal.SignalPainting.DownLight.FillColor = Color.Black;
                            ThisSignal.SignalPainting.UpLight.FillColor = Color.FromArgb(255, 255, 0);
                        }
                    }
                    else
                    {
                        ThisSignal.SignalPainting.DownLight.FillColor = Color.Red;
                        ThisSignal.SignalPainting.UpLight.FillColor = Color.Black;
                    }
                }
                else if (ThisSignal.TypeOfParts == Types.shunttingSignal)
                {
                    int j = ThisSignal.Left != null ? ThisSignal.Left.Number : ThisSignal.Right.Number;
                    PartsOfStations ThisRail = PartsOfStation[j];
                    if (ThisRail.OccupancyState == OccupancyStates.occupied && (ThisRail.RoutePoint == RoutePoints.ShuntingPoint || ThisRail.RoutePoint == RoutePoints.ShuntingStart || ThisRail.RoutePoint == RoutePoints.ShuntingEnd))
                    {
                        if ((ThisSignal.Right == null && ThisRail.OccupancyDirection == OccupancyDirections.right) || (ThisSignal.Left == null && ThisRail.OccupancyDirection == OccupancyDirections.left))
                        {
                            ThisSignal.SignalPainting.DownLight.FillColor = Color.White;
                        }
                    }
                    else
                    {
                        ThisSignal.SignalPainting.DownLight.FillColor = Color.Blue;
                    }
                }
                else if (ThisSignal.TypeOfParts == Types.multifunctionSignal)
                {
                    int j = ThisSignal.Left != null ? ThisSignal.Left.Number : ThisSignal.Right.Number;
                    PartsOfStations ThisRail = PartsOfStation[j];
                    if (ThisRail.OccupancyState == OccupancyStates.occupied && (ThisRail.RoutePoint == RoutePoints.TrainPoint || ThisRail.RoutePoint == RoutePoints.TrainStart || ThisRail.RoutePoint == RoutePoints.TrainEnd))
                    {
                        if ((ThisSignal.Right == null && ThisRail.OccupancyDirection == OccupancyDirections.right) || (ThisSignal.Left == null && ThisRail.OccupancyDirection == OccupancyDirections.left))
                        {
                            if (ThisRail.lineChanged)
                                ThisSignal.SignalPainting.DownLight.FillColor = Color.FromArgb(255, 255, 0);
                            else
                                ThisSignal.SignalPainting.DownLight.FillColor = Color.Black;
                            ThisSignal.SignalPainting.UpLight.FillColor = Color.FromArgb(255, 255, 0);
                        }
                    }
                    else if (ThisRail.OccupancyState == OccupancyStates.occupied && (ThisRail.RoutePoint == RoutePoints.ShuntingPoint || ThisRail.RoutePoint == RoutePoints.ShuntingStart || ThisRail.RoutePoint == RoutePoints.ShuntingEnd))
                    {
                        if ((ThisSignal.Right == null && ThisRail.OccupancyDirection == OccupancyDirections.right) || (ThisSignal.Left == null && ThisRail.OccupancyDirection == OccupancyDirections.left))
                        {
                            ThisSignal.SignalPainting.DownLight.FillColor = Color.White;
                            ThisSignal.SignalPainting.UpLight.FillColor = Color.Black;
                        }
                    }
                    else
                    {
                        ThisSignal.SignalPainting.DownLight.FillColor = Color.Red;
                        ThisSignal.SignalPainting.UpLight.FillColor = Color.Black;
                    }
                }
            }
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
        private void HandleLabelDisplay(bool newSignalLabelDisplay, bool newTurningLabelDisplay, bool newRailLabelDisplay)
        {
            this.SignalLabelDisplayFlag = newSignalLabelDisplay;
            this.TurningLabelDisplayFlag = newTurningLabelDisplay;
            this.RailLabelDisplayFlag = newRailLabelDisplay;
            for (int i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].TypeOfParts >= Types.trainSignal)
                {
                    PartsOfStation[i].NameLabel.Visible = SignalLabelDisplayFlag;
                }
                else if (PartsOfStation[i].TypeOfParts == Types.frog)
                {
                    if (PartsOfStation[i].NameLabel != null)
                        PartsOfStation[i].NameLabel.Visible = TurningLabelDisplayFlag;
                }
                else if (PartsOfStation[i].TypeOfParts == Types.track)
                {
                    if (PartsOfStation[i].NameLabel != null)
                        PartsOfStation[i].NameLabel.Visible = RailLabelDisplayFlag;
                }
            }

        }
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var About = new About();
            About.ShowDialog();
        }

        private void DataTransforming()
        {
            DataClearing();
            string[] EachDatas = StationData.Split(';');
            int i = 0, j = 0;
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
                PartsOfStation[i].Number = i;
                PartsOfStation[i].NameOfParts = Details[1];
                switch (typeFlag)
                {
                    case 0:
                        PartsOfStation[i].Length = int.Parse(Details[2]);
                        PartsOfStation[i].LeftName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].RightName = Details[4] == "null" ? "" : Details[4];
                        j++;
                        break;
                    case 1:
                        PartsOfStation[i].Length = 50;
                        PartsOfStation[i].Directions = Details[2];
                        PartsOfStation[i].UpName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].DownName = Details[4] == "null" ? "" : Details[4];
                        j++;
                        break;
                    case 2:
                        PartsOfStation[i].Length = 15;
                        PartsOfStation[i].Directions = Details[2];
                        PartsOfStation[i].LeftName = Details[3] == "null" ? "" : Details[3];
                        PartsOfStation[i].RightName = Details[4] == "null" ? "" : Details[4];
                        PartsOfStation[i].UpName = Details[5] == "null" ? "" : Details[5];
                        PartsOfStation[i].DownName = Details[6] == "null" ? "" : Details[6];
                        j++;
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
            RailNumber = j;
            DataConnecting();
            PartPaintingAsync();
            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "导入新站场数据");
        }
        private void DataConnecting()//建立部件间引用
        {
            PartsOfStation[0].LineNumber = 5;
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
            PartsOfStations thisPart = PartsOfStation[0];
            int LineNumberAdded = 1;
            //MessageBox.Show(RailNumber.ToString());
            while (LineNumberAdded < RailNumber)
            {
                for (int l = 0; l < SectionNumber; l++)
                {
                    if (PartsOfStation[l].LineNumber == 0 && PartsOfStation[l].TypeOfParts <= Types.frog)
                    {
                        if (PartsOfStation[l].Right != null && PartsOfStation[l].Right.LineNumber != 0)
                        {
                            PartsOfStation[l].LineNumber = PartsOfStation[l].Right.LineNumber;
                            LineNumberAdded++;
                            //MessageBox.Show(LineNumberAdded.ToString());
                        }
                        if (PartsOfStation[l].Left != null && PartsOfStation[l].Left.LineNumber != 0)
                        {
                            PartsOfStation[l].LineNumber = PartsOfStation[l].Left.LineNumber;
                            LineNumberAdded++;
                            //MessageBox.Show(LineNumberAdded.ToString());
                        }
                        if (PartsOfStation[l].Up != null && PartsOfStation[l].Up.LineNumber != 0)
                        {
                            PartsOfStation[l].LineNumber = PartsOfStation[l].Up.LineNumber + 1;
                            LineNumberAdded++;
                            //MessageBox.Show(LineNumberAdded.ToString());
                        }
                        if (PartsOfStation[l].Down != null && PartsOfStation[l].Down.LineNumber != 0)
                        {
                            PartsOfStation[l].LineNumber = PartsOfStation[l].Down.LineNumber - 1;
                            LineNumberAdded++;
                            //MessageBox.Show(LineNumberAdded.ToString());
                        }
                    }
                }
            }

        }
        public async Task PartPaintingAsync()//绘图
        {
            int Xpoint = 100, Ypoint = 500;
            ShapeContainer shapeContainer = new ShapeContainer();
            shapeContainer.Location = new System.Drawing.Point(0, 0);
            shapeContainer.Size = this.Size;
            await EachPartPainting(PartsOfStation[0], Xpoint, Ypoint, shapeContainer, true);
            //LightPainting(shapeContainer);
            this.Controls.Add(shapeContainer);
            SignalTimer.Start();
        }
        private async Task LightPainting(ShapeContainer shapeContainer)//信号机绘制
        {
            for (int i = 0; i < SectionNumber; i++)
            {
                PartsOfStations thisPart = PartsOfStation[i];
                int Height = thisPart.Directions == "上方" ? -37 : 20;
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
                        thisPart.SignalPainting.BaseLine.Y2 = thisPart.Right.Rail.Y2 + Height + 17;
                        thisPart.SignalPainting.BaseLine.BorderWidth = 2;
                        thisPart.SignalPainting.BaseLine.BorderColor = Color.FromArgb(85, 120, 182);
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.BaseLine);
                        thisPart.SignalPainting.DownLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 17, thisPart.SignalPainting.BaseLine.Y1);
                        thisPart.SignalPainting.DownLight.Size = new Size(17, 17);
                        thisPart.SignalPainting.DownLight.BorderColor = Color.FromArgb(85, 120, 182);
                        thisPart.SignalPainting.DownLight.FillColor = Color.Red;
                        if (thisPart.TypeOfParts == Types.shunttingSignal) thisPart.SignalPainting.DownLight.FillColor = Color.Blue;
                        thisPart.SignalPainting.DownLight.FillStyle = FillStyle.Solid;
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.DownLight);
                        if (thisPart.TypeOfParts != Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.UpLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 34, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.UpLight.Size = new Size(17, 17);
                            thisPart.SignalPainting.UpLight.BorderColor = Color.FromArgb(85, 120, 182);
                            thisPart.SignalPainting.UpLight.FillColor = Color.Black;
                            thisPart.SignalPainting.UpLight.FillStyle = FillStyle.Solid;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.UpLight);
                        }
                        if (thisPart.TypeOfParts == Types.trainSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 5, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(17, 17);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }
                        if (thisPart.TypeOfParts >= Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.ShuntingButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 5, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.ShuntingButton.Size = new Size(17, 17);
                            thisPart.SignalPainting.ShuntingButton.BorderColor = Color.White;
                            thisPart.SignalPainting.ShuntingButton.FillColor = Color.FromArgb(128, 128, 128);
                            thisPart.SignalPainting.ShuntingButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.ShuntingButton.Click += ShuntingButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.ShuntingButton);
                        }
                        if (thisPart.TypeOfParts == Types.multifunctionSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 27, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(17, 17);
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
                        thisPart.SignalPainting.BaseLine.Y2 = thisPart.Left.Rail.Y1 + Height + 17;
                        thisPart.SignalPainting.BaseLine.BorderWidth = 2;
                        thisPart.SignalPainting.BaseLine.BorderColor = Color.FromArgb(85, 120, 182);
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.BaseLine);
                        thisPart.SignalPainting.DownLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1, thisPart.SignalPainting.BaseLine.Y1);
                        thisPart.SignalPainting.DownLight.Size = new Size(17, 17);
                        thisPart.SignalPainting.DownLight.BorderColor = Color.FromArgb(85, 120, 182);
                        thisPart.SignalPainting.DownLight.FillColor = Color.Red;
                        if (thisPart.TypeOfParts == Types.shunttingSignal) thisPart.SignalPainting.DownLight.FillColor = Color.Blue;
                        thisPart.SignalPainting.DownLight.FillStyle = FillStyle.Solid;
                        shapeContainer.Shapes.Add(thisPart.SignalPainting.DownLight);
                        if (thisPart.TypeOfParts != Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.UpLight.Location = new Point(thisPart.SignalPainting.BaseLine.X1 + 17, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.UpLight.Size = new Size(17, 17);
                            thisPart.SignalPainting.UpLight.BorderColor = Color.FromArgb(85, 120, 182);
                            thisPart.SignalPainting.UpLight.FillColor = Color.Black;
                            thisPart.SignalPainting.UpLight.FillStyle = FillStyle.Solid;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.UpLight);
                        }
                        if (thisPart.TypeOfParts == Types.trainSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 22, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(17, 17);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }
                        if (thisPart.TypeOfParts >= Types.shunttingSignal)
                        {
                            thisPart.SignalPainting.ShuntingButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 22, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.ShuntingButton.Size = new Size(17, 17);
                            thisPart.SignalPainting.ShuntingButton.BorderColor = Color.White;
                            thisPart.SignalPainting.ShuntingButton.FillColor = Color.FromArgb(128, 128, 128);
                            thisPart.SignalPainting.ShuntingButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.ShuntingButton.Click += ShuntingButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.ShuntingButton);
                        }
                        if (thisPart.TypeOfParts == Types.multifunctionSignal)
                        {
                            thisPart.SignalPainting.TrainButton.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - 44, thisPart.SignalPainting.BaseLine.Y1);
                            thisPart.SignalPainting.TrainButton.Size = new Size(17, 17);
                            thisPart.SignalPainting.TrainButton.BorderColor = Color.White;
                            thisPart.SignalPainting.TrainButton.FillColor = Color.Green;
                            thisPart.SignalPainting.TrainButton.FillStyle = FillStyle.Solid;
                            thisPart.SignalPainting.TrainButton.Click += TrainButtonClicked;
                            shapeContainer.Shapes.Add(thisPart.SignalPainting.TrainButton);
                        }
                    }
                    thisPart.NameLabel = new Label
                    {
                        Font = new Font("宋体", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134))),
                        //Size = new Size(100, 25),
                        Text = thisPart.NameOfParts,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.White
                    };
                    thisPart.NameLabel.Size = new Size(thisPart.NameLabel.Text.Length * 20, 25);
                    if (thisPart.Directions == "上方")
                        thisPart.NameLabel.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - thisPart.NameLabel.Text.Length * 10, thisPart.SignalPainting.BaseLine.Y1 - 35);
                    else thisPart.NameLabel.Location = new Point(thisPart.SignalPainting.BaseLine.X1 - thisPart.NameLabel.Text.Length * 10, thisPart.SignalPainting.BaseLine.Y1 + 27);
                    this.Controls.Add(thisPart.NameLabel);
                }
            }
        }
        private async Task EachPartPainting(PartsOfStations thisPart, int Xpoint, int Ypoint, ShapeContainer shapeContainer, Boolean Direction)//轨道区段绘制
        {
            if (thisPart.TypeOfParts == Types.track)
            {
                thisPart.Rail = new LineShape
                {
                    X1 = Xpoint,
                    Y1 = Ypoint,
                    X2 = Xpoint + thisPart.Length,
                    Y2 = Ypoint
                };
                if (thisPart.Right != null && thisPart.Right.TypeOfParts == Types.track)
                {
                    thisPart.InsulatedJoint = new LineShape
                    {
                        X1 = thisPart.Rail.X2,
                        Y1 = thisPart.Rail.Y2 - 8,
                        X2 = thisPart.Rail.X2,
                        Y2 = thisPart.Rail.Y2 + 8,
                        BorderColor = Color.FromArgb(85, 120, 182),
                        BorderWidth = 2
                    };
                    shapeContainer.Shapes.Add(thisPart.InsulatedJoint);
                }
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
                        //if (Direction) Xpoint -= 30;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint + thisPart.Length;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint;
                    }
                else if (thisPart.Right == null)
                    if (thisPart.Directions == "上撇")
                    {
                        //if (!Direction && thisPart.Up.Painted) Xpoint -= 15;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint - thisPart.Length;
                    }
                    else
                    {
                        if (Direction) Xpoint -= 15;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        thisPart.Rail.X2 = Xpoint + thisPart.Length;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length;
                    }
                else
                {
                    thisPart.Changeable = true;
                    if (Direction && thisPart.Directions == "下捺") Xpoint -= 15;
                    thisPart.Rail.X1 = Xpoint;
                    thisPart.Rail.Y1 = Ypoint;
                    thisPart.Rail.X2 = Xpoint + thisPart.Length;
                    thisPart.Rail.Y2 = Ypoint;
                    thisPart.LockSign = new OvalShape();
                    thisPart.LockSign.Location = new Point(thisPart.Rail.X1, thisPart.Rail.Y1 - thisPart.Length / 2);
                    thisPart.LockSign.Size = new Size(thisPart.Length, thisPart.Length);
                    thisPart.LockSign.BorderColor = Color.Red;
                    thisPart.LockSign.Visible = false;
                    thisPart.LockSign.Click += LockSignClicked;
                    shapeContainer.Shapes.Add(thisPart.LockSign);
                }

            }
            else if (thisPart.TypeOfParts == Types.turnout)
            {
                thisPart.Rail = new LineShape();
                if (Direction)
                {
                    //Ypoint -= 30;
                    if (thisPart.Directions == "撇形")
                    {
                        Xpoint += 15;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint + thisPart.Length - 15;
                        Xpoint = Xpoint + thisPart.Length;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint - 15;
                    }
                    else
                    {
                        //Xpoint -= 30;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length - 15;
                        Xpoint = Xpoint - thisPart.Length;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint - 15;
                    }
                    Ypoint -= 15;
                }
                else
                {
                    //Ypoint += 30;
                    if (thisPart.Directions == "撇形")
                    {
                        //Xpoint -= 30;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint;
                        Xpoint = Xpoint - thisPart.Length;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint + thisPart.Length;
                        Xpoint -= 15;
                    }
                    else
                    {
                        Xpoint += 15;
                        thisPart.Rail.X1 = Xpoint;
                        thisPart.Rail.Y1 = Ypoint;
                        Xpoint = Xpoint + thisPart.Length;
                        thisPart.Rail.X2 = Xpoint;
                        thisPart.Rail.Y2 = Ypoint + thisPart.Length;
                    }
                    Ypoint += 15;
                }
                if (thisPart.Directions == "撇形")
                {
                    thisPart.InsulatedJoint = new LineShape
                    {
                        X1 = (thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - 5,
                        Y1 = (thisPart.Rail.Y1 + thisPart.Rail.Y2) / 2 - 5,
                        X2 = (thisPart.Rail.X1 + thisPart.Rail.X2) / 2 + 5,
                        Y2 = (thisPart.Rail.Y1 + thisPart.Rail.Y2) / 2 + 5,
                        BorderColor = Color.FromArgb(85, 120, 182),
                        BorderWidth = 2
                    };
                    shapeContainer.Shapes.Add(thisPart.InsulatedJoint);
                }
                else
                {
                    thisPart.InsulatedJoint = new LineShape
                    {
                        X1 = (thisPart.Rail.X1 + thisPart.Rail.X2) / 2 + 5,
                        Y1 = (thisPart.Rail.Y1 + thisPart.Rail.Y2) / 2 - 5,
                        X2 = (thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - 5,
                        Y2 = (thisPart.Rail.Y1 + thisPart.Rail.Y2) / 2 + 5,
                        BorderColor = Color.FromArgb(85, 120, 182),
                        BorderWidth = 2
                    };
                    shapeContainer.Shapes.Add(thisPart.InsulatedJoint);
                }
            }

            if (thisPart.Rail != null)
            {
                thisPart.Rail.BorderWidth = 5;
                if (thisPart.TypeOfParts != Types.frog)
                    thisPart.Rail.BorderColor = Color.FromArgb(85, 120, 182);
                else if (!thisPart.Changeable)
                    thisPart.Rail.BorderColor = Color.FromArgb(85, 120, 182);
                else thisPart.Rail.BorderColor = Color.FromArgb(0, 255, 0);
                thisPart.Rail.Click += RailClicked;
                shapeContainer.Shapes.Add(thisPart.Rail);
            }

            if (thisPart.TypeOfParts == Types.track || (thisPart.TypeOfParts == Types.frog && thisPart.Changeable))
            {
                thisPart.NameLabel = new Label
                {
                    //Location = new Point((thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - 60, thisPart.Rail.Y2 + 15),
                    Font = new Font("宋体", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134))),
                    //Size = new Size(120, 25),
                    Text = thisPart.NameOfParts,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White
                };
                if (thisPart.TypeOfParts == Types.frog)
                    thisPart.NameLabel.ForeColor = Color.FromArgb(0, 255, 0);
                thisPart.NameLabel.Location = new Point((thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - thisPart.NameLabel.Text.Length * 10, thisPart.Rail.Y2 + 10);
                thisPart.NameLabel.Size = new Size(thisPart.NameLabel.Text.Length * 20, 25);
                if (thisPart.TypeOfParts == Types.track && (thisPart.Right == null || thisPart.Left == null || (thisPart.Right != null && thisPart.Right.TypeOfParts == Types.frog) || (thisPart.Left != null && thisPart.Left.TypeOfParts == Types.frog)))
                {
                    thisPart.NameLabel.Dispose();
                }
                else
                {
                    if (thisPart.TypeOfParts == Types.frog && (thisPart.Directions == "下撇" || thisPart.Directions == "下捺"))
                        thisPart.NameLabel.Location = new Point((thisPart.Rail.X1 + thisPart.Rail.X2) / 2 - thisPart.NameLabel.Text.Length * 10, thisPart.Rail.Y2 - 35);
                    this.Controls.Add(thisPart.NameLabel);
                }

            }
            thisPart.Painted = true;
            await Task.Delay(100);
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
            for (int j = 1; j <= 8; j++)
            {
                if (j != i)
                {
                    ToolStripStatusLabel Otherlabel = (ToolStripStatusLabel)statusStrip1.Items["toolStripStatusLabel" + j];
                    Otherlabel.BackColor = Color.White;
                }
            }
            if (i >= 5 && i <= 8)
            {
                PasswordFlag = false;
                var PasswordForm = new Password();
                PasswordForm.Text = "验证保护口令";
                PasswordForm.OldPassword = Password;
                PasswordForm.SettingNewPassword = false;
                PasswordForm.FlagChecked += HandlePasswordFlag;
                PasswordForm.ShowDialog();
                if (!PasswordFlag) return; else ClickedStatusLabel.BackColor = SystemColors.GradientActiveCaption;
            }
            else ClickedStatusLabel.BackColor = SystemColors.GradientActiveCaption;

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
                            string TempName = openFileDialog.SafeFileName;
                            int underscoreIndex = TempName.IndexOf('_');
                            if (underscoreIndex != -1)
                            {
                                StationName = TempName.Substring(0, underscoreIndex);
                            }
                            else
                            {
                                var nameChangeForm = new NameChange();
                                nameChangeForm.NameChanged += HandleNameChanged;
                                nameChangeForm.ShowDialog(this);
                            }
                            NewTitleLocation();
                            DataTransforming();
                        }
                        else MessageBox.Show("站场数据不能为空！");
                    }
                }
            }
        }

        private void 站场数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = StationName + "_站场数据"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileContent = StationData;// 要写入文件的字符串
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    writer.Write(fileContent);
                }
                MessageBox.Show("文件已成功保存到: " + saveFileDialog.FileName);
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
            SignalTimer.Stop();
            DataClearing();
            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "重置站场数据");
        }
        private void DataClearing()
        {
            for (int i = 0; i < PartsOfStation.Length; i++)
            {
                if (PartsOfStation[i] != null)// 如果PartsOfStations实例包含LineShape等，也需要从父控件中移除它们
                {
                    if (PartsOfStation[i].Rail != null)
                    {
                        PartsOfStation[i].Rail.Parent = null; // 从父控件移除对应控件
                    }
                    if (PartsOfStation[i].InsulatedJoint != null)
                    {
                        PartsOfStation[i].InsulatedJoint.Parent = null;
                    }
                    if (PartsOfStation[i].LockSign != null)
                    {
                        PartsOfStation[i].LockSign.Parent = null;
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
        }
        private void 站场图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Bitmap bitmap = new Bitmap(this.Width - 50, this.Height - 200))// 创建一个Bitmap对象，其大小与当前窗口相同
            {
                using (Graphics g = Graphics.FromImage(bitmap))// 使用Graphics对象从当前窗口绘制图像到Bitmap
                {
                    g.CopyFromScreen(new Point(this.Location.X + 25, this.Location.Y + 80), Point.Empty, new Size(this.Width - 50, this.Height - 200));
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog// 使用SaveFileDialog让用户选择保存文件的位置
                {
                    Filter = "JPEG Image|*.jpg;*.jpeg|PNG Image|*.png|BMP Image|*.bmp",
                    FileName = StationName + "_站场图  " + DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"),
                    Title = "导出为"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    switch (Path.GetExtension(saveFileDialog.FileName).ToLower())// 根据用户选择的文件格式保存图像
                    {
                        case ".jpg":
                        case ".jpeg":
                            bitmap.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                            MessageBox.Show("文件已成功保存到: " + saveFileDialog.FileName);
                            break;
                        case ".png":
                            bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
                            MessageBox.Show("文件已成功保存到: " + saveFileDialog.FileName);
                            break;
                        case ".bmp":
                            bitmap.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                            MessageBox.Show("文件已成功保存到: " + saveFileDialog.FileName);
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
            LabelDisplayButton.Location = new Point(MessageButton.Location.X - LabelDisplayButton.Width - 10, MessageButton.Location.Y);
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
            int i, j, Found, Found2;
            for (i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].SignalPainting != null && PartsOfStation[i].SignalPainting.TrainButton == ClickedButton)
                {
                    //MessageBox.Show(PartsOfStation[i].NameOfParts);//测试用提示消息，请忽略
                    break;
                }
            }
            if (toolStripStatusLabel7.BackColor == SystemColors.GradientActiveCaption)
            {
                CancelPart = i;
                DelayTime = 3;
                DelayLabel.Text = "剩余延时：" + DelayTime.ToString() + "秒";
                DelayLabel.Visible = true;
                DelayTimer.Start();
                return;
            }
            if (toolStripStatusLabel8.BackColor == SystemColors.GradientActiveCaption)
            {
                CancelPart = i;
                DelayTime = 10;
                DelayLabel.Text = "剩余延时：" + DelayTime.ToString() + "秒";
                DelayLabel.Visible = true;
                DelayTimer.Start();
                return;
            }
            for (j = 0; j < SectionNumber; j++)
            {
                if (PartsOfStation[j].RoutePoint == RoutePoints.starting && PartsOfStation[i].SignalPainting.TrainButton != null) break;
            }
            if (j < SectionNumber)
            {
                if ((PartsOfStation[j].Right != null && PartsOfStation[i].Right != null) || (PartsOfStation[j].Left != null && PartsOfStation[i].Left != null))
                {
                    PartsOfStation[i].RoutePoint = RoutePoints.turning;
                    if (PartsOfStation[i].Right != null) i = PartsOfStation[i].Right.Number;
                    else if (PartsOfStation[i].Left != null) i = PartsOfStation[i].Left.Number;
                    TurningFlag = i;
                }
                else
                {
                    PartsOfStation[i].RoutePoint = RoutePoints.ending;
                    if (PartsOfStation[i].Right != null) i = PartsOfStation[i].Right.Right.Number;
                    else if (PartsOfStation[i].Left != null) i = PartsOfStation[i].Left.Left.Number;
                    if (PartsOfStation[j].Right != null) j = PartsOfStation[j].Right.Number;
                    else if (PartsOfStation[j].Left != null) j = PartsOfStation[j].Left.Number;
                    if (TurningFlag >= 0)
                    {
                        Found = RouteCreating(j, TurningFlag);
                        Found2 = RouteCreating(TurningFlag, i);
                        if (Found != 0 && Found2 != 0)
                        {
                            RouteDisplay(j, TurningFlag, Found, true);
                            RouteDisplay(TurningFlag, i, Found2, true);
                            DoubleYellowSetting(i);
                            PartsOfStation[j].RoutePoint = RoutePoints.TrainStart;
                            PartsOfStation[i].RoutePoint = RoutePoints.TrainEnd;
                            TurningFlag = -1;
                            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "建立了一条列车变通进路");
                        }
                    }
                    else
                    {
                        Found = RouteCreating(j, i);
                        if (Found != 0)
                        {
                            RouteDisplay(j, i, Found, true);
                            DoubleYellowSetting(i);
                            PartsOfStation[j].RoutePoint = RoutePoints.TrainStart;
                            PartsOfStation[i].RoutePoint = RoutePoints.TrainEnd;
                            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "建立了一条列车进路");
                        }
                    }
                    for (int k = 0; k < SectionNumber; k++)
                        if (PartsOfStation[k].TypeOfParts >= Types.trainSignal)
                            PartsOfStation[k].RoutePoint = RoutePoints.Other;
                }
            }
            else PartsOfStation[i].RoutePoint = RoutePoints.starting;
        }

        private void ShuntingButtonClicked(object sender, EventArgs e)//调车进路开放按钮触发的事件
        {
            RectangleShape ClickedButton = (RectangleShape)sender;
            int i, j, Found, Found2;
            for (i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].SignalPainting != null && PartsOfStation[i].SignalPainting.ShuntingButton == ClickedButton)
                {
                    //MessageBox.Show(PartsOfStation[i].NameOfParts);//测试用提示消息，请忽略
                    break;
                }
            }
            if (toolStripStatusLabel7.BackColor == SystemColors.GradientActiveCaption)
            {
                CancelPart = i;
                DelayTime = 3;
                DelayLabel.Text = "剩余延时：" + DelayTime.ToString() + "秒";
                DelayLabel.Visible = true;
                DelayTimer.Start();
                return;
            }
            if (toolStripStatusLabel8.BackColor == SystemColors.GradientActiveCaption)
            {
                CancelPart = i;
                DelayTime = 10;
                DelayLabel.Text = "剩余延时：" + DelayTime.ToString() + "秒";
                DelayLabel.Visible = true;
                DelayTimer.Start();
                return;
            }
            for (j = 0; j < SectionNumber; j++)
            {
                if (PartsOfStation[j].RoutePoint == RoutePoints.starting && PartsOfStation[i].SignalPainting.ShuntingButton != null) break;
            }
            if (j < SectionNumber)
            {
                if ((PartsOfStation[j].Right != null && PartsOfStation[i].Left != null) || (PartsOfStation[j].Left != null && PartsOfStation[i].Right != null))
                {
                    PartsOfStation[i].RoutePoint = RoutePoints.turning;
                    if (PartsOfStation[i].Right != null) i = PartsOfStation[i].Right.Number;
                    else if (PartsOfStation[i].Left != null) i = PartsOfStation[i].Left.Number;
                    TurningFlag = i;
                }
                else
                {
                    PartsOfStation[i].RoutePoint = RoutePoints.ending;
                    if (PartsOfStation[i].Right != null) i = PartsOfStation[i].Right.Right.Number;
                    else if (PartsOfStation[i].Left != null) i = PartsOfStation[i].Left.Left.Number;
                    if (PartsOfStation[j].Right != null) j = PartsOfStation[j].Right.Number;
                    else if (PartsOfStation[j].Left != null) j = PartsOfStation[j].Left.Number;
                    if (TurningFlag >= 0)
                    {
                        Found = RouteCreating(j, TurningFlag);
                        Found2 = RouteCreating(TurningFlag, i);
                        if (Found != 0 && Found2 != 0)
                        {
                            RouteDisplay(j, TurningFlag, Found, false);
                            RouteDisplay(TurningFlag, i, Found2, false);
                            PartsOfStation[j].RoutePoint = RoutePoints.ShuntingStart;
                            PartsOfStation[i].RoutePoint = RoutePoints.ShuntingEnd;
                            TurningFlag = -1;
                            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "建立了一条调车变通进路");
                        }
                    }
                    else
                    {
                        Found = RouteCreating(j, i);
                        if (Found != 0)
                        {
                            RouteDisplay(j, i, Found, false);
                            PartsOfStation[j].RoutePoint = RoutePoints.ShuntingStart;
                            PartsOfStation[i].RoutePoint = RoutePoints.ShuntingEnd;
                            dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "建立了一条调车进路");
                        }
                    }
                    for (int k = 0; k < SectionNumber; k++)
                        if (PartsOfStation[k].TypeOfParts >= Types.trainSignal)
                            PartsOfStation[k].RoutePoint = RoutePoints.Other;
                }
            }
            else PartsOfStation[i].RoutePoint = RoutePoints.starting;
        }

        private void RailClicked(object sender, EventArgs e)
        {
            LineShape ClickedRail = (LineShape)sender;
            //MessageBox.Show("clicked");
            int i;
            for (i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].Rail != null && PartsOfStation[i].Rail == ClickedRail)
                {
                    break;
                }
            }
            if (i < SectionNumber)
            {
                if (toolStripStatusLabel1.BackColor != Color.White)//总定位按钮按下
                {
                    if (PartsOfStation[i].Conditions == 1 && !PartsOfStation[i].Locked && PartsOfStation[i].OccupancyState == OccupancyStates.available)
                    {
                        if (PartsOfStation[i].Up != null && PartsOfStation[i].Up.Up.Changeable && !PartsOfStation[i].Up.Up.Locked && PartsOfStation[i].Up.Up.OccupancyState == OccupancyStates.available)
                        {
                            FrogChange(i);
                            if (PartsOfStation[i].Up.Up.Conditions == 1) FrogChange(PartsOfStation[i].Up.Up.Number);
                        }
                        else if (PartsOfStation[i].Up != null && !PartsOfStation[i].Up.Up.Changeable) FrogChange(i);
                        else if (PartsOfStation[i].Down != null && PartsOfStation[i].Down.Down.Changeable && !PartsOfStation[i].Down.Down.Locked && PartsOfStation[i].Down.Down.OccupancyState == OccupancyStates.available)
                        {
                            FrogChange(i);
                            if (PartsOfStation[i].Down.Down.Conditions == 1) FrogChange(PartsOfStation[i].Down.Down.Number);
                        }
                        else if (PartsOfStation[i].Up != null && !PartsOfStation[i].Down.Down.Changeable) FrogChange(i);
                    }
                    toolStripStatusLabel1.BackColor = Color.White;
                }
                else if (toolStripStatusLabel2.BackColor != Color.White)//总反位按钮按下
                {
                    if (PartsOfStation[i].Conditions == 0 && !PartsOfStation[i].Locked && PartsOfStation[i].OccupancyState == OccupancyStates.available)
                    {
                        if (PartsOfStation[i].Up != null && PartsOfStation[i].Up.Up.Changeable && !PartsOfStation[i].Up.Up.Locked && PartsOfStation[i].Up.Up.OccupancyState == OccupancyStates.available)
                        {
                            FrogChange(i);
                            if (PartsOfStation[i].Up.Up.Conditions == 0) FrogChange(PartsOfStation[i].Up.Up.Number);
                        }
                        else if (PartsOfStation[i].Up != null && !PartsOfStation[i].Up.Up.Changeable) FrogChange(i);
                        else if (PartsOfStation[i].Down != null && PartsOfStation[i].Down.Down.Changeable && !PartsOfStation[i].Down.Down.Locked && PartsOfStation[i].Down.Down.OccupancyState == OccupancyStates.available)
                        {
                            FrogChange(i);
                            if (PartsOfStation[i].Down.Down.Conditions == 0) FrogChange(PartsOfStation[i].Down.Down.Number);
                        }
                        else if (PartsOfStation[i].Up != null && !PartsOfStation[i].Down.Down.Changeable) FrogChange(i);
                    }
                    toolStripStatusLabel2.BackColor = Color.White;
                }
                else if (toolStripStatusLabel3.BackColor != Color.White)//单锁按钮按下
                {
                    if (!PartsOfStation[i].Locked && PartsOfStation[i].OccupancyState == OccupancyStates.available)
                    {
                        FrogLock(i);
                        toolStripStatusLabel3.BackColor = Color.White;
                    }
                }
                else if (toolStripStatusLabel4.BackColor != Color.White)//单解按钮按下
                {
                    if (PartsOfStation[i].Locked && PartsOfStation[i].OccupancyState == OccupancyStates.available)
                    {
                        FrogLock(i);
                        toolStripStatusLabel4.BackColor = Color.White;
                    }
                }
                else if (toolStripStatusLabel5.BackColor != Color.White)
                {
                    if (PartsOfStation[i].OccupancyState == OccupancyStates.available)
                    {
                        PartsOfStation[i].OccupancyState = OccupancyStates.breakdown;
                        PartsOfStation[i].Rail.BorderColor = Color.Red;
                        toolStripStatusLabel5.BackColor = Color.White;
                    }
                }
                else if (toolStripStatusLabel6.BackColor != Color.White)
                {
                    if (PartsOfStation[i].OccupancyState == OccupancyStates.breakdown)
                    {
                        PartsOfStation[i].OccupancyState = OccupancyStates.available;
                        PartsOfStation[i].Rail.BorderColor = Color.FromArgb(85, 120, 182);
                        toolStripStatusLabel6.BackColor = Color.White;
                    }

                }
            }
        }
        private void LockSignClicked(object sender, EventArgs e)
        {
            OvalShape ClickedOval = (OvalShape)sender;
            int i;
            for (i = 0; i < SectionNumber; i++)
            {
                if (PartsOfStation[i].LockSign != null && PartsOfStation[i].LockSign == ClickedOval)
                {
                    break;
                }
            }
            if (i < SectionNumber)
                if (toolStripStatusLabel4.BackColor != Color.White)//单解按钮按下
                {
                    if (PartsOfStation[i].Locked && PartsOfStation[i].OccupancyState == OccupancyStates.available)
                    {
                        FrogLock(i);
                        toolStripStatusLabel4.BackColor = Color.White;
                    }
                }
        }
        private void FrogLock(int FrogChosed)//道岔锁闭
        {
            PartsOfStations ThisPart = PartsOfStation[FrogChosed];
            if (!ThisPart.Locked)
            {
                ThisPart.Locked = true;
                ThisPart.LockSign.Visible = true;
            }
            else
            {
                ThisPart.Locked = false;
                ThisPart.LockSign.Visible = false;
            }

        }
        private void FrogChange(int FrogChosed)//道岔位置转换
        {
            PartsOfStations ThisPart = PartsOfStation[FrogChosed];
            if (ThisPart.Changeable)
            {
                int ChangeLength = 15;
                if (ThisPart.Conditions != 0)//反位转为定位
                {
                    ChangeLength = -ChangeLength;
                    ThisPart.Rail.BorderColor = Color.FromArgb(0, 255, 0);
                    ThisPart.NameLabel.ForeColor = Color.FromArgb(0, 255, 0);
                    ThisPart.Conditions = 0;
                }
                else//定位转为反位
                {
                    ThisPart.Rail.BorderColor = Color.FromArgb(255, 255, 0);
                    ThisPart.NameLabel.ForeColor = Color.FromArgb(255, 255, 0);
                    ThisPart.Conditions = 1;
                }
                if (ThisPart.Directions == "上撇")
                {
                    ThisPart.Rail.Y2 -= ChangeLength;
                }
                else if (ThisPart.Directions == "上捺")
                {
                    ThisPart.Rail.Y1 -= ChangeLength;
                }
                else if (ThisPart.Directions == "下撇")
                {
                    ThisPart.Rail.Y1 += ChangeLength;
                }
                else if (ThisPart.Directions == "下捺")
                {
                    ThisPart.Rail.Y2 += ChangeLength;
                }
            }
        }
        private int RouteCreating(int StartPoint, int EndPoint)//查找是否能建立进路
        {
            int i = StartPoint;
            //EndPoint = PartsOfStation[EndPoint].Left != null ? PartsOfStation[EndPoint].Left.Number : PartsOfStation[EndPoint].Right.Number;
            while (PartsOfStation[i].Left != null || PartsOfStation[i].TypeOfParts != Types.track)
            {
                if (PartsOfStation[i].OccupancyState != OccupancyStates.available) return 0;

                if (PartsOfStation[EndPoint].LineNumber < PartsOfStation[i].LineNumber && PartsOfStation[i].Up != null)
                {
                    if (PartsOfStation[i].TypeOfParts == Types.turnout && PartsOfStation[i].Up.Conditions == 0 && PartsOfStation[i].Up.Locked)
                        return 0;
                    if (PartsOfStation[i].Up.Directions != "撇形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        i = PartsOfStation[i].Up.Number;
                    else i = PartsOfStation[i].Left.Number;
                }
                else if (PartsOfStation[EndPoint].LineNumber > PartsOfStation[i].LineNumber && PartsOfStation[i].Down != null)
                {
                    if (PartsOfStation[i].TypeOfParts == Types.turnout && PartsOfStation[i].Down.Conditions == 0 && PartsOfStation[i].Down.Locked)
                        return 0;
                    if (PartsOfStation[i].Down.Directions != "捺形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        i = PartsOfStation[i].Down.Number;
                    else i = PartsOfStation[i].Left.Number;
                }
                else
                {
                    if (PartsOfStation[i].TypeOfParts == Types.frog && (PartsOfStation[i].Conditions == 1 && PartsOfStation[i].Locked) && (PartsOfStation[i].Directions == "上捺" || PartsOfStation[i].Directions == "下撇"))
                        return 0;
                    i = PartsOfStation[i].Left.Number;
                }
                if (PartsOfStation[i].Left == PartsOfStation[EndPoint])
                {
                    return -1;
                }
            }
            i = StartPoint;

            while (PartsOfStation[i].Right != null || PartsOfStation[i].TypeOfParts != Types.track)
            {
                //MessageBox.Show(PartsOfStation[i].LineNumber.ToString());
                if (PartsOfStation[i].OccupancyState != OccupancyStates.available) return 0;

                if (PartsOfStation[EndPoint].LineNumber < PartsOfStation[i].LineNumber && PartsOfStation[i].Up != null)
                {
                    if (PartsOfStation[i].TypeOfParts == Types.turnout && PartsOfStation[i].Up.Conditions == 0 && PartsOfStation[i].Up.Locked)
                        return 0;
                    if (PartsOfStation[i].Up.Directions != "捺形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        i = PartsOfStation[i].Up.Number;
                    else i = PartsOfStation[i].Right.Number;
                }
                else if (PartsOfStation[EndPoint].LineNumber > PartsOfStation[i].LineNumber && PartsOfStation[i].Down != null)
                {
                    if (PartsOfStation[i].TypeOfParts == Types.turnout && PartsOfStation[i].Down.Conditions == 0 && PartsOfStation[i].Down.Locked)
                        return 0;
                    if (PartsOfStation[i].Down.Directions != "撇形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        i = PartsOfStation[i].Down.Number;
                    else i = PartsOfStation[i].Right.Number;
                }
                else
                {
                    if (PartsOfStation[i].TypeOfParts == Types.frog && (PartsOfStation[i].Conditions == 1 && PartsOfStation[i].Locked) && (PartsOfStation[i].Directions == "上撇" || PartsOfStation[i].Directions == "下捺"))
                        return 0;
                    i = PartsOfStation[i].Right.Number;
                }
                if (PartsOfStation[i].Right == PartsOfStation[EndPoint])
                {
                    //MessageBox.Show(PartsOfStation[i].LineNumber.ToString());
                    return 1;
                }
            }
            return 0;
        }
        private void RouteDisplay(int StartRail, int EndRail, int Flag, bool isTrainRoute)//建立进路并显示
        {
            int i = StartRail;
            //EndRail = PartsOfStation[EndRail].Left != null ? PartsOfStation[EndRail].Left.Number : PartsOfStation[EndRail].Right.Number;
            if (Flag == 0)
                return;
            else if (Flag == 1)
                while (PartsOfStation[i].Right != null || PartsOfStation[i].TypeOfParts != Types.track || PartsOfStation[i] == PartsOfStation[EndRail])
                {
                    PartsOfStation[i].Rail.BorderColor = Color.White;
                    PartsOfStation[i].OccupancyState = OccupancyStates.occupied;
                    PartsOfStation[i].OccupancyDirection = OccupancyDirections.right;
                    if (isTrainRoute) PartsOfStation[i].RoutePoint = RoutePoints.TrainPoint;
                    else PartsOfStation[i].RoutePoint = RoutePoints.ShuntingPoint;
                    if (PartsOfStation[i] == PartsOfStation[EndRail])
                    {
                        i = StartRail;
                        if (isTrainRoute)
                        {
                            PartsOfStation[i].RoutePoint = RoutePoints.TrainStart;
                            PartsOfStation[EndRail].RoutePoint = RoutePoints.TrainEnd;
                        }
                        else
                        {
                            PartsOfStation[i].RoutePoint = RoutePoints.ShuntingStart;
                            PartsOfStation[EndRail].RoutePoint = RoutePoints.ShuntingEnd;
                        }
                        return;
                    }
                    //if (PartsOfStation[i].OccupancyState != OccupancyStates.available) break;
                    if (PartsOfStation[EndRail].LineNumber < PartsOfStation[i].LineNumber && PartsOfStation[i].Up != null)
                    {
                        if (PartsOfStation[i].Up.Directions != "捺形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        {
                            if (PartsOfStation[i].TypeOfParts == Types.frog && PartsOfStation[i].Conditions == 0)
                            {
                                FrogChange(i);
                                PartsOfStation[i].Rail.BorderColor = Color.White;
                            }
                            else if (PartsOfStation[i].Up.TypeOfParts == Types.frog && PartsOfStation[i].Up.Conditions == 0)
                            {
                                FrogChange(PartsOfStation[i].Up.Number);
                            }
                            i = PartsOfStation[i].Up.Number;
                        }
                        else
                        {
                            i = PartsOfStation[i].Right.Number;
                        }
                    }
                    else if (PartsOfStation[EndRail].LineNumber > PartsOfStation[i].LineNumber && PartsOfStation[i].Down != null)
                    {
                        if (PartsOfStation[i].Down.Directions != "撇形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        {
                            if (PartsOfStation[i].TypeOfParts == Types.frog && PartsOfStation[i].Conditions == 0)
                            {
                                FrogChange(i);
                                PartsOfStation[i].Rail.BorderColor = Color.White;
                            }
                            else if (PartsOfStation[i].Down.TypeOfParts == Types.frog && PartsOfStation[i].Down.Conditions == 0)
                            {
                                FrogChange(PartsOfStation[i].Down.Number);
                            }
                            i = PartsOfStation[i].Down.Number;
                        }
                        else
                        {
                            i = PartsOfStation[i].Right.Number;
                        }
                    }
                    else
                    {
                        if (PartsOfStation[i].Right.TypeOfParts == Types.frog && PartsOfStation[i].Right.Conditions != 0 && !PartsOfStation[i].Right.Locked)
                            FrogChange(PartsOfStation[i].Right.Number);
                        i = PartsOfStation[i].Right.Number;
                    }
                }
            else if (Flag == -1)
                while (PartsOfStation[i].Left != null || PartsOfStation[i].TypeOfParts != Types.track || PartsOfStation[i] == PartsOfStation[EndRail])
                {
                    PartsOfStation[i].Rail.BorderColor = Color.White;
                    PartsOfStation[i].OccupancyState = OccupancyStates.occupied;
                    PartsOfStation[i].OccupancyDirection = OccupancyDirections.left;
                    if (isTrainRoute) PartsOfStation[i].RoutePoint = RoutePoints.TrainPoint;
                    else PartsOfStation[i].RoutePoint = RoutePoints.ShuntingPoint;
                    if (PartsOfStation[i] == PartsOfStation[EndRail])
                    {
                        i = StartRail;
                        if (isTrainRoute)
                        {
                            PartsOfStation[i].RoutePoint = RoutePoints.TrainStart;
                            PartsOfStation[EndRail].RoutePoint = RoutePoints.TrainEnd;
                        }
                        else
                        {
                            PartsOfStation[i].RoutePoint = RoutePoints.ShuntingStart;
                            PartsOfStation[EndRail].RoutePoint = RoutePoints.ShuntingEnd;
                        }
                        return;
                    }
                    //if (PartsOfStation[i].OccupancyState != OccupancyStates.available) break;
                    if (PartsOfStation[EndRail].LineNumber < PartsOfStation[i].LineNumber && PartsOfStation[i].Up != null)
                    {
                        if (PartsOfStation[i].Up.Directions != "撇形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        {
                            if (PartsOfStation[i].TypeOfParts == Types.frog && PartsOfStation[i].Conditions == 0)
                            {
                                FrogChange(i);
                                PartsOfStation[i].Rail.BorderColor = Color.White;
                            }
                            else if (PartsOfStation[i].Up.TypeOfParts == Types.frog && PartsOfStation[i].Up.Conditions == 0)
                            {
                                FrogChange(PartsOfStation[i].Up.Number);
                            }
                            i = PartsOfStation[i].Up.Number;
                        }
                        else i = PartsOfStation[i].Left.Number;
                    }
                    else if (PartsOfStation[EndRail].LineNumber > PartsOfStation[i].LineNumber && PartsOfStation[i].Down != null)
                    {
                        if (PartsOfStation[i].Down.Directions != "捺形" && !(PartsOfStation[i].Conditions == 0 && PartsOfStation[i].Locked))
                        {
                            if (PartsOfStation[i].TypeOfParts == Types.frog && PartsOfStation[i].Conditions == 0)
                            {
                                FrogChange(i);
                                PartsOfStation[i].Rail.BorderColor = Color.White;
                            }
                            else if (PartsOfStation[i].Down.TypeOfParts == Types.frog && PartsOfStation[i].Down.Conditions == 0)
                            {
                                FrogChange(PartsOfStation[i].Down.Number);
                            }
                            i = PartsOfStation[i].Down.Number;
                        }
                        else i = PartsOfStation[i].Left.Number;
                    }
                    else
                    {
                        if (PartsOfStation[i].Left.TypeOfParts == Types.frog && PartsOfStation[i].Left.Conditions != 0 && !PartsOfStation[i].Left.Locked)
                            FrogChange(PartsOfStation[i].Left.Number);
                        i = PartsOfStation[i].Left.Number;
                    }
                }
        }
        private void DoubleYellowSetting(int EndPoint)
        {
            int i = EndPoint;
            PartsOfStations ThisPart = PartsOfStation[i];
            //MessageBox.Show(ThisPart.NameOfParts);
            bool DoubleYellowSignal = false;
            bool Cleared = false;
            while (!Cleared)
            {
                ThisPart.Visited = true;
                if (ThisPart.TypeOfParts == Types.frog)
                {
                    if (ThisPart.Conditions != 0 || !ThisPart.Changeable)
                    {
                        DoubleYellowSignal = true;
                        //MessageBox.Show(ThisPart.NameOfParts);
                    }
                    //else DoubleYellowSignal = false;
                }
                if (ThisPart.TypeOfParts == Types.track && DoubleYellowSignal)
                {
                    ThisPart.lineChanged = true;
                }
                if (ThisPart.Up != null && ThisPart.Up.OccupancyState == OccupancyStates.occupied && !ThisPart.Up.Visited)
                {
                    ThisPart = ThisPart.Up;
                    //MessageBox.Show("up");
                }
                else if (ThisPart.Down != null && ThisPart.Down.OccupancyState == OccupancyStates.occupied && !ThisPart.Down.Visited)
                {
                    ThisPart = ThisPart.Down;
                    //MessageBox.Show("down");
                }
                else if (ThisPart.Left != null && ThisPart.Left.OccupancyState == OccupancyStates.occupied && !ThisPart.Left.Visited)
                {
                    ThisPart = ThisPart.Left;
                    //MessageBox.Show("left");

                }
                else if (ThisPart.Right != null && ThisPart.Right.OccupancyState == OccupancyStates.occupied && !ThisPart.Right.Visited)
                {
                    ThisPart = ThisPart.Right;
                    //MessageBox.Show("right");
                }
                else Cleared = true;
                //MessageBox.Show(ThisPart.NameOfParts);

            }
        }
        private void RouteClearing(int StartPoint)//取消进路
        {
            int i = PartsOfStation[StartPoint].Left != null ? PartsOfStation[StartPoint].Left.Number : PartsOfStation[StartPoint].Right.Number;
            PartsOfStations ThisPart = PartsOfStation[i];
            bool Cleared = false;
            if (ThisPart.OccupancyState != OccupancyStates.available)
            {
                dataGridView1.Rows.Add(DateTime.Now.ToString("HH:mm:ss"), "取消了一条进路");
            }
            while (!Cleared)
            {
                ThisPart.OccupancyState = OccupancyStates.available;
                ThisPart.OccupancyDirection = OccupancyDirections.none;
                ThisPart.RoutePoint = RoutePoints.Other;
                ThisPart.lineChanged = false;
                ThisPart.Visited = false;
                if (ThisPart.TypeOfParts != Types.frog)
                    ThisPart.Rail.BorderColor = Color.FromArgb(85, 120, 182);
                else if (!ThisPart.Changeable)
                    ThisPart.Rail.BorderColor = Color.FromArgb(85, 120, 182);
                else if (ThisPart.Conditions != 0)
                    ThisPart.Rail.BorderColor = Color.FromArgb(255, 255, 0);
                else
                    ThisPart.Rail.BorderColor = Color.FromArgb(0, 255, 0);
                if (ThisPart.Up != null && ThisPart.Up.OccupancyState == OccupancyStates.occupied)
                {
                    ThisPart = ThisPart.Up;
                }
                else if (ThisPart.Down != null && ThisPart.Down.OccupancyState == OccupancyStates.occupied)
                {
                    ThisPart = ThisPart.Down;
                }
                else if (ThisPart.Left != null && ThisPart.Left.OccupancyState == OccupancyStates.occupied)
                {
                    ThisPart = ThisPart.Left;
                }
                else if (ThisPart.Right != null && ThisPart.Right.OccupancyState == OccupancyStates.occupied)
                {
                    ThisPart = ThisPart.Right;
                }
                else Cleared = true;
            }
        }
        private void 站场事件记录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "导出为",
                FileName = StationName + "_站场事件记录"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        sw.Write(dataGridView1.Columns[i].HeaderText);
                        if (i < dataGridView1.Columns.Count - 1)
                            sw.Write("\t");
                    }
                    sw.WriteLine();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            for (int j = 0; j < row.Cells.Count; j++)
                            {
                                sw.Write(row.Cells[j].Value?.ToString());
                                if (j < row.Cells.Count - 1)
                                    sw.Write("\t");
                            }
                            sw.WriteLine();
                        }
                    }
                }
                MessageBox.Show("文件已成功保存到: " + saveFileDialog.FileName);
            }
        }

        private void LabelDisplayButton_Click(object sender, EventArgs e)
        {
            var labelDisplay = new LabelDisplay();
            labelDisplay.LabelDisplayChanged += HandleLabelDisplay;
            labelDisplay.SignalLabelDisplayFlag = this.SignalLabelDisplayFlag;
            labelDisplay.TurningLabelDisplayFlag = this.TurningLabelDisplayFlag;
            labelDisplay.RailLabelDisplayFlag = this.RailLabelDisplayFlag;
            labelDisplay.ShowDialog(this);
        }
    }
}
