using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRenamer.Classes {
    interface IExifOperator : IDisposable {
        string GetCreateDateString(string fileFullName);
    }
}
