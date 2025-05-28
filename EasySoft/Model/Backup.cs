using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EasySoft.model
{
    [Serializable]
    public class Backup
    {
        public string SaveName { get; set; }
        public string ResourceBackup { get; set; }
        public string TargetBackup { get; set; }
        public string Type { get; set; }
        public string MirrorBackup { get; set; }

        public Backup() { } // Constructeur sans paramètre pour XML

        public Backup(string saveName, string resourceBackup, string targetBackup, string type, string mirrorBackup)
        {
            SaveName = saveName;
            ResourceBackup = resourceBackup;
            TargetBackup = targetBackup;
            Type = type;
            MirrorBackup = mirrorBackup;
        }
    }
}
