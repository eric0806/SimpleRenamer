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
using System.Configuration;

namespace SimpleRenamer {
    public partial class MainForm : Form {
        Classes.Exiftool exiftool;

        public MainForm() {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            exiftool = new Classes.Exiftool();
            folderBrowserDialog1.SelectedPath = ConfigurationManager.AppSettings["LastFolder"];
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
            if (string.IsNullOrWhiteSpace(txtFolderName.Text)) {
                ShowErrorMessage("請選擇資料夾!!");
            }
            else if (!Directory.Exists(txtFolderName.Text)) {
                ShowErrorMessage("資料夾不存在!!");
            }
            else {
                btnStart.Enabled = false;
                btnCancel.Enabled = true;
                txtResult.Text = "";
                toolStripProgressBar1.Value = 0;
                var dir = new DirectoryInfo(txtFolderName.Text);
                lblRemaining.Text = dir.GetFiles().Length.ToString();
                bgWorker.RunWorkerAsync(dir);
            }
        }

        /// <summary>
        /// 取消目前正在跑的重命名處理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e) {
            bgWorker.CancelAsync();
            btnCancel.Enabled = false;
        }

        /// <summary>
        /// 資料夾變動立即儲存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFolderName_TextChanged(object sender, EventArgs e) {
            ConfigurationManager.AppSettings.Set("LastFolder", txtFolderName.Text);
        }


        void ShowErrorMessage(string message) {
            MessageBox.Show(message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            var worker = new Classes.RenameWorker(ref exiftool);
            worker.Rename(sender as BackgroundWorker, e);
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            var state = e.UserState as Classes.RenameState;
            toolStripProgressBar1.Value = e.ProgressPercentage;
            txtResult.Text += $"{state.OriginalName} 更名為 {state.NewName} => {(state.IsSuccess ? "成功" : "失敗")} {(state.IsSuccess ? "" : state.ErrorMessage)}{Environment.NewLine}";
            lblRemaining.Text = state.Remaining.ToString();
            ScrollTextToEnd();
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                txtResult.Text += $"發生錯誤:{e.Error.Message}";
            }
            else if (e.Cancelled) {
                txtResult.Text += "使用者已取消";
            }
            else {
                toolStripProgressBar1.Value = 0;
                txtResult.Text += "已完成";
            }
            ScrollTextToEnd();
            lblRemaining.Text = "";
            btnStart.Enabled = true;
            btnCancel.Enabled = false;
        }

        void ScrollTextToEnd() {
            txtResult.SelectionStart = txtResult.Text.Length;
            txtResult.ScrollToCaret();
        }
    }
}
