using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySoft.model
{
    internal class DataLogs
    {
        //Declaration of the properties that are used for the program log file
        public string SourceLog { get; set; }
        public string TargetLog { get; set; }
        public string SaveNameLog { get; set; }
        public string BackupDateLog { get; set; }
        public string TransactionTimeLog { get; set; }
        public long TotalSizeLog { get; set; }


    }
}
