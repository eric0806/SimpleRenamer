using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace SimpleRenamer.Classes {
    /// <summary>
    /// 使用exiftool讀取媒體檔資訊的工具
    /// </summary>
    class ExifOP_Exiftool : IExifOperator {
        readonly List<string> allowTypes;
        const string FILE_NAME = "exiftool.exe";
        string exifFullName = Path.Combine(Environment.CurrentDirectory, FILE_NAME);

        JArray exifObj;

        /// <summary>
        /// 指出暫存的exiftool.exe是否存在
        /// </summary>
        bool ExiftoolIsExists {
            get {
                return File.Exists(exifFullName);
            }
        }

        public ExifOP_Exiftool() {
            exifObj = null;
            allowTypes = new List<string> { "video", "image" };
            if (!ExiftoolIsExists) {
                WriteExiftoolToFile();
            }
        }

        /// <summary>
        /// 使用Exiftool.exe取得相片/影片Exif內拍攝日期的新檔名(不含附檔名)，若無的話則使用檔案系統的產生日期
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public string GetCreateDateString(string fileFullName) {
            var str = "";
            var exifList = new Dictionary<string, string>();

            ParseExif(fileFullName);

            //檢查檔案格式
            var type = GetExifValue("MIMEType");
            if (string.IsNullOrEmpty(type)) { return null; }
            type = type.ToLower();
            var passType = false;
            foreach(var allowType in allowTypes) {
                if (type.ToLower().Contains(allowType)) {
                    passType = true;
                }
            }
            if (!passType) { return null; }

            /* *
             * 日期挑選順序：
             * 1. DateTimeOriginal
             * 2. CreateDate
             * 3. ModifyDate
             * 4. FileModifyDate
             * */
            if (!string.IsNullOrEmpty(GetExifValue("DateTimeOriginal"))) {
                str = GetExifDateStr(GetExifValue("DateTimeOriginal"));
            }
            else if (!string.IsNullOrEmpty(GetExifValue("CreateDate"))) {
                str = GetExifDateStr(GetExifValue("CreateDate"));
            }
            else if (!string.IsNullOrEmpty(GetExifValue("ModifyDate"))) {
                str = GetExifDateStr(GetExifValue("ModifyDate"));
            }
            else if (!string.IsNullOrEmpty(GetExifValue("FileModifyDate"))) {
                str = GetExifDateStr(GetExifValue("FileModifyDate"));
            }

            //都找不到，則從檔案系統內的修改日期決定
            if (str == string.Empty) {
                str = GetFilesystemDateStr(fileFullName);
            }
            return str;
        }

        #region 私有函數
        /// <summary>
        /// 載入exiftool並解析檔案內容
        /// </summary>
        /// <param name="fileFullName"></param>
        void ParseExif(string fileFullName) {
            using (var p = new Process()) {
                p.StartInfo.FileName = exifFullName;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = $"-j -charset UTF8 -d \"%Y-%m-%d %H:%M:%S\" \"{fileFullName}\""; //採用Json輸出
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                using (var output = p.StandardOutput) {
                    var data = output.ReadToEnd();
                    exifObj = JsonConvert.DeserializeObject<JArray>(data);
                }

                p.Close();
            }
        }

        /// <summary>
        /// 取得Exif指定key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetExifValue(string key) {
            if (exifObj != null && exifObj[0].HasValues) {
                try {
                    return exifObj[0][key].ToString();
                }
                catch {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 將Exiftool內的日期資訊回傳成日期字串
        /// </summary>
        /// <param name="dateStr"></param>
        /// <returns></returns>
        string GetExifDateStr(string dateStr) {
            if (DateTime.TryParse(dateStr, out DateTime date)) {
                return GetDateStrFromDate(date);
            }
            return dateStr;
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
        /// 將資源內的Exiftool二進位資料寫入一個暫存執行檔
        /// </summary>
        /// <returns></returns>
        void WriteExiftoolToFile() {
            if (ExiftoolIsExists) {
                DeleteExiftoolFile();
            }
            var exiftool = Properties.Resources.exiftool;
            using (var file = new FileStream(exifFullName, FileMode.CreateNew, FileAccess.ReadWrite)) {
                file.Write(exiftool, 0, exiftool.Length);
                file.Flush();
            }
        }

        /// <summary>
        /// 刪除Exiftool暫存執行檔
        /// </summary>
        void DeleteExiftoolFile() {
            try {
                File.Delete(exifFullName);
            }
            catch {

            }
        }
        #endregion

        public void Dispose() => DeleteExiftoolFile();
    }
}
