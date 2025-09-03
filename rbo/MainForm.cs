using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rbo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void RbtControl_Click_1(object sender, EventArgs e)
        {
            RbtHome myrbthome = new RbtHome();
            myrbthome.TopLevel = false;
            this.rbtcontrolpanel.Controls.Add(myrbthome);
            myrbthome.FormBorderStyle = FormBorderStyle.None;
            myrbthome.BringToFront();
            myrbthome.Show();
        }

        private void DataGet_Click_1(object sender, EventArgs e)
        {
            Dataview mydataview = new Dataview();
            mydataview.TopLevel = false;
            this.rbtcontrolpanel.Controls.Add(mydataview);
            mydataview.FormBorderStyle = FormBorderStyle.None;
            mydataview.BringToFront();
            mydataview.Show();
        }

        private void SimulationBtn_Click_1(object sender, EventArgs e)
        {
            SimulationFun mysimulationFun = new SimulationFun();
            mysimulationFun.TopLevel = false;
            this.rbtcontrolpanel.Controls.Add(mysimulationFun);
            mysimulationFun.FormBorderStyle = FormBorderStyle.None;
            mysimulationFun.BringToFront();
            mysimulationFun.Show();
        }

        private void SetupBtn_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RbtHome myrbthome = new RbtHome();
            myrbthome.TopLevel = false;
            this.rbtcontrolpanel.Controls.Add(myrbthome);
            myrbthome.FormBorderStyle = FormBorderStyle.None;
            myrbthome.BringToFront();
            myrbthome.Show();
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine(e.KeyChar.ToString());
        }
    }
}
