using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace SimpleRenamer.Classes {
    /// <summary>
    /// 操作exiftool的工具
    /// </summary>
    class Exiftool : IDisposable {
        const string FILE_NAME = "exiftool.exe";
        string exifFullName = Path.Combine(Environment.CurrentDirectory, FILE_NAME);

        /// <summary>
        /// 指出暫存的exiftool.exe是否存在
        /// </summary>
        bool ExiftoolIsExists {
            get {
                return File.Exists(exifFullName);
            }
        }

        public Exiftool() {
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
            var tempStr = "";
            using (var p = new Process()) {
                p.StartInfo.FileName = exifFullName;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = $"\"{fileFullName}\"";
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                using (var output = p.StandardOutput) {
                    while(!output.EndOfStream) {
                        var line = output.ReadLine();
                        if (line.IndexOf("File Modification Date", 0, StringComparison.CurrentCulture) >= 0) {
                            tempStr = GetDateStr(line);
                        }
                        if (line.IndexOf("Create Date", 0, StringComparison.CurrentCulture) >= 0) {
                            str = GetDateStr(line);
                            break;
                        }
                    }
                    if (str == string.Empty) {
                        str = tempStr;
                    }
                }
                p.Close();
            }
            if (str == string.Empty) {
                str = GetFilesystemDateStr(fileFullName);
            }
            return str;
        }

        #region 私有函數
        /// <summary>
        /// 將Exiftool內的日期資訊回傳成日期字串
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        string GetDateStr(string line) {
            var fullDateAry = line.Substring(line.IndexOf(":") + 1, line.Length - (line.IndexOf(":") + 1)).Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return $"{fullDateAry[0].Replace(":", "-")}_{fullDateAry[1].Replace(":", "")}";
        }

        /// <summary>
        /// 將檔案系統內的產生日期傳回成日期字串
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        string GetFilesystemDateStr(string fileFullName) {
            var file = new FileInfo(fileFullName);
            return string.Format("{0}-{1}-{2}_{3}{4}{5}",
                file.CreationTime.Year,
                file.CreationTime.Month.ToString().PadLeft(2, '0'),
                file.CreationTime.Day.ToString().PadLeft(2, '0'),
                file.CreationTime.Hour.ToString().PadLeft(2, '0'),
                file.CreationTime.Minute.ToString().PadLeft(2, '0'),
                file.CreationTime.Second.ToString().PadLeft(2, '0')
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
            using(var file = new FileStream(exifFullName, FileMode.CreateNew, FileAccess.ReadWrite)) {
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
