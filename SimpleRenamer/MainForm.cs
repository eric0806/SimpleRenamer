using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SimpleRenamer {
    public partial class MainForm : Form {
        Classes.Exiftool exiftool;

        public MainForm() {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            exiftool = new Classes.Exiftool();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            exiftool.Dispose();
        }

        /// <summary>
        /// 選取資料夾按鈕點下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, EventArgs e) {
            var result = folderBrowserDialog1.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath)) {
                txtFolderName.Text = folderBrowserDialog1.SelectedPath;
            }
            else {
                //folderBrowserDialog1.Reset();
            }
        }

        /// <summary>
        /// 開始按鈕按下，開始重新命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e) {

        }
    }
}
