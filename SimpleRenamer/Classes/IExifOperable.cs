using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRenamer.Classes {
    /// <summary>
    /// 可操作Exif的介面
    /// </summary>
    interface IExifOperable : IDisposable {
        string GetCreateDateString(string fileFullName);
    }
}
