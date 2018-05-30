using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;

namespace SimpleRenamer.Classes {
    /// <summary>
    /// 使用Windows API Code Pack讀取媒體檔資訊的工具
    /// </summary>
    class ExifOP_ApiCodePack : IExifOperable {
        ShellObject shellFile;
        readonly List<string> allowTypes;

        public ExifOP_ApiCodePack() {
            allowTypes = new List<string> { "圖片", "視訊" };
        }

        public string GetCreateDateString(string fileFullName) {
            var str = "";
            shellFile = ShellFile.FromParsingName(fileFullName);
            /*
             * 從shellFile.Properties.DefaultPropertyCollection裡面去挑
             * 日期優先順序:
             * System.Kind == 圖片：
             * 1. System.Photo.DateTaken
             * 2. System.ItemDate
             * 
             * System.Kind == 視訊
             * 1. System.Media.DateEncoded - 8
             * 2. System.ItemDate - 8
             * 
             * 共用
             * 3. System.Document.DateSaved
             * 4. System.DateModified
             * */

            //檢查檔案類型
            var (isPass, type) = PassType();
            if (!isPass) { return null; }

            //不同類型分開處理
            switch (type) {
                case "圖片": {
                        if (HasKey("System.Photo.DateTaken")) {
                            str = GetDateStrFromDate((GetExifValue("System.Photo.DateTaken") as DateTime?).Value);
                        }
                        else if (HasKey("System.ItemDate")) {
                            str = GetDateStrFromDate((GetExifValue("System.ItemDate") as DateTime?).Value);
                        }
                        break;
                    }
                case "視訊": {
                        if (HasKey("System.Media.DateEncoded")) {
                            str = GetDateStrFromDate((GetExifValue("System.Media.DateEncoded") as DateTime?).Value.AddHours(-8));
                        }
                        else if (HasKey("System.ItemDate")) {
                            str = GetDateStrFromDate((GetExifValue("System.ItemDate") as DateTime?).Value.AddHours(-8));
                        }
                        break;
                    }
            }

            //找不到日期，則查找共通的部分
            if (!string.IsNullOrEmpty(str)) { return str; }
            if (HasKey("System.Document.DateSaved")) {
                str = GetDateStrFromDate((GetExifValue("System.Document.DateSaved") as DateTime?).Value);
            }
            else if (HasKey("System.DateModified")) {
                str = GetDateStrFromDate((GetExifValue("System.DateModified") as DateTime?).Value);
            }


            return str;
        }

        #region 私有方法
        /// <summary>
        /// 檢查shellFile是否有該key的屬性
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasKey(string key) {
            return shellFile.Properties.DefaultPropertyCollection.Count(p => p.CanonicalName == key) > 0;
        }

        /// <summary>
        /// 取得系統屬性資料
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetExifValue(string key) {
            if (shellFile != null) {
                try {
                    return shellFile.Properties.DefaultPropertyCollection[key].ValueAsObject;
                }
                catch {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 檢查檔案類型
        /// </summary>
        /// <returns></returns>
        (bool isPass, string type) PassType() {
            //檢查System.KindText
            var type = GetExifValue("System.KindText") as string;
            if (string.IsNullOrEmpty(type)) { return (false, null); }
            return (allowTypes.Contains(type), type);
        }

        /// <summary>
        /// 從日期轉成日期字串
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        string GetDateStrFromDate(DateTime date) {
            return string.Format("{0}-{1}-{2}_{3}{4}{5}",
                    date.Year,
                    date.Month.ToString().PadLeft(2, '0'),
                    date.Day.ToString().PadLeft(2, '0'),
                    date.Hour.ToString().PadLeft(2, '0'),
                    date.Minute.ToString().PadLeft(2, '0'),
                    date.Second.ToString().PadLeft(2, '0')
                );
        }

        /// <summary>
        /// 將檔案系統內的產生日期傳回成日期字串
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        string GetFilesystemDateStr(string fileFullName) {
            var file = new FileInfo(fileFullName);
            return GetDateStrFromDate(file.CreationTime);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: 處置受控狀態 (受控物件)。
                }

                // TODO: 釋放非受控資源 (非受控物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放非受控資源的程式碼時，才覆寫完成項。
        // ~ExifOP_ApiCodePack() {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose() {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
