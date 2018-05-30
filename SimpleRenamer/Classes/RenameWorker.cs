using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace SimpleRenamer.Classes {
    /// <summary>
    /// 重新命名的處理
    /// </summary>
    class RenameWorker {
        RenameState state;
        readonly IExifOperable exiftool;
        readonly Dictionary<string, int> fileNameList;

        public RenameWorker(ref IExifOperable exiftool) {
            this.exiftool = exiftool;
            fileNameList = new Dictionary<string, int>();
        }

        /// <summary>
        /// 背景處理每個檔案的重新命名處理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Rename(object sender, DoWorkEventArgs e) {
            var worker = sender as BackgroundWorker;
            var dir = e.Argument as DirectoryInfo;
            var newFilename = "";
            var index = 0;
            var remaining = dir.GetFiles().Length;
            var total = remaining;

            //更新進度
            void ReportProgress() {
                remaining--;
                state.Remaining = remaining;
                worker.ReportProgress((int)((float)index / (float)total * 100), state);
            }

            foreach(var file in dir.GetFiles()) {
                if (worker.CancellationPending) {
                    e.Cancel = true;
                    return;
                }
                index++;

                state = new RenameState {
                    OriginalName = file.Name
                };

                //取得新檔名
                newFilename = exiftool.GetCreateDateString(file.FullName);
                //如果檔名空白，表示不支援(只支援影片和圖片)，跳過處理
                if (string.IsNullOrEmpty(newFilename)) {
                    state.IsSuccess = false;
                    state.ErrorMessage = "不支援的檔案類型";
                    ReportProgress();
                    continue;
                }
                newFilename = $"{exiftool.GetCreateDateString(file.FullName)}{file.Extension.ToLower()}";

                //判斷檔名是否已重複，如果重複必須在後方加上序號
                if (fileNameList.ContainsKey(newFilename)) {
                    fileNameList[newFilename] = fileNameList[newFilename] + 1;
                    newFilename = $"{exiftool.GetCreateDateString(file.FullName)}_{(fileNameList[newFilename].ToString().PadLeft(2, '0'))}{file.Extension.ToLower()}";
                }
                else {
                    fileNameList.Add(newFilename, 0);
                }

                //設定新檔名
                state.NewName = newFilename;

                //檔案更名
                try {
                    file.MoveTo(Path.Combine(Path.GetDirectoryName(file.FullName), newFilename));
                    state.IsSuccess = true;
                }
                catch(Exception ex) {
                    state.IsSuccess = false;
                    state.ErrorMessage = ex.Message;
                }

                ReportProgress();
            }
        }

        
    }


    /// <summary>
    /// 重新命名的處理狀態
    /// </summary>
    class RenameState {
        public string OriginalName { get; set; }
        public string NewName { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int Remaining { get; set; }
    }
}
