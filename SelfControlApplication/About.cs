using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SelfControlApplication
{
    public partial class About : Form
    {
        private string url = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=FAGAY2FRNEVJ4";
        private string name = "https://vk.com/is.fadeev";
        public About()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void pbDonate_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(url);
        }

        private void llblName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(name);
        }

        private void btnAboutOk_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
