using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SelfControlApplication
{
    public partial class BlockList : Form
    {
        private string blockListPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SelfControlApp\blocklist.rtf";

        public BlockList()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            rtbList.SaveFile(blockListPath);
            this.Hide();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbList.Clear();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            rtbList.AppendText(tbAdd.Text + "\n");
            tbAdd.Clear();
        }

        private void tbAdd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnAdd_Click(new object(), new EventArgs());
            }
        }

        private void BlockList_Load(object sender, EventArgs e)
        {
            if (File.Exists(blockListPath))
            {
                rtbList.LoadFile(blockListPath);
            }
            else
            {
                rtbList.SaveFile(blockListPath);
                rtbList.LoadFile(blockListPath);
            }
        }

        public RichTextBox rtbListP
        {
            get
            {
                return rtbList;
            }
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            rtbList.SaveFile(blockListPath);
            this.Hide();
        }

        private void tsmiClear_Click(object sender, EventArgs e)
        {
            rtbList.Clear();
        }
    }
}
