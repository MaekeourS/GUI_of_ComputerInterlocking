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
    public partial class LabelDisplay : Form
    {
        public delegate void LabelDisplayEventHandler(bool newSignalLabelDisplay, bool newTurningLabelDisplay, bool newRailLabelDisplay);

        // 定义一个事件，基于上面的委托
        public event LabelDisplayEventHandler LabelDisplayChanged;
        public LabelDisplay()
        {
            InitializeComponent();
        }

        public bool SignalLabelDisplayFlag;
        public bool TurningLabelDisplayFlag;
        public bool RailLabelDisplayFlag;
        protected virtual void OnLabelDisplayChanged(bool newSignalLabelDisplay, bool newTurningLabelDisplay, bool newRailLabelDisplay)
        {
            LabelDisplayChanged?.Invoke(newSignalLabelDisplay, newTurningLabelDisplay, newRailLabelDisplay);
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            OnLabelDisplayChanged(SignalName.Checked,TurningName.Checked,RailName.Checked);
        }
        private void LabelDisplay_Load(object sender, EventArgs e)
        {
            var screen = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
            SignalName.Checked = SignalLabelDisplayFlag;
            TurningName.Checked = TurningLabelDisplayFlag;
            RailName.Checked = RailLabelDisplayFlag;
            this.Location = new Point(screen.WorkingArea.Width - this.Width, screen.WorkingArea.Height - this.Height);
        }
    }
}
