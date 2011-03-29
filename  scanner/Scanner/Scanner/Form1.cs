using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImageProcess;
using NxtController;

namespace Scanner
{
    public partial class Form1 : Form
    {
        Nxt nxt;
        public Form1()
        {
            InitializeComponent();
            this.nxt = new Nxt();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.nxt.RotateMotor();
        }
    }
}
