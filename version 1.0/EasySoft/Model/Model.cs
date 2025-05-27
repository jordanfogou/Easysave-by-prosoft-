using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySoft.controller;
using EasySoft.view;
using Newtonsoft.Json;


namespace EasySoft.model
{
    class Model
    {

        public int CheckDataBackup;
        private string SerializeObj;
        public string BackupListFile = System.Environment.CurrentDirectory + @"\Works\";
        public string StateFile = System.Environment.CurrentDirectory + @"\State\";
        public DataState DataState { get; set; }
        public string NameStateFile { get; set; }
        public string BackupNameState { get; set; }
        public string Resource { get; set; }
        public int NbFileMmax { get; set; }
        public int Nbfiles { get; set; }
        public long Size { get; set; }
        public float Progs { get; set; }
        public string Targetresource { get; set; }
        public string SaveName { get; set; }
        public string Type { get; set; }
        public string SourceFile { get; set; }
        public string TypeString { get; set; }
        public long TotalSize { get; set; }
        public TimeSpan TimeTransfert { get; set; }
        public string UserMenuInput { get; set; }
        public string MirrorResource { get; set; }


        public Model()
        {
            UserMenuInput = " ";

            if (!Directory.Exists(BackupListFile))
            {
                DirectoryInfo resource = Directory.CreateDirectory(BackupListFile);
            }
            BackupListFile += @"backupList.json";

            if (!Directory.Exists(StateFile))
            {
                DirectoryInfo resource = Directory.CreateDirectory(StateFile);
            }
            StateFile += @"state.json";
        }

        /// <summary>
        /// function called when full backup is selected
        /// </summary>
        /// <param name="inputpathsave"></param>
        /// <param name="inputDestToSave"></param>
        /// <param name="copyDir"></param>
        /// <param name="verif"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>


        public void CompleteSave(string inputpathsave, string inputDestToSave, bool copyDir, bool verif)
        {
            DataState = new DataState(NameStateFile);
            DataState.SaveState = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TotalSize = 0;
            NbFileMmax = 0;
            Size = 0;
            Nbfiles = 0;
            Progs = 0;

            DirectoryInfo resource = new DirectoryInfo(inputpathsave);

            if (!resource.Exists)
            {
                throw new DirectoryNotFoundException("ERROR: Directory Not Found ! " + inputpathsave);
            }

            DirectoryInfo[] Resource = resource.GetDirectories();
            Directory.CreateDirectory(inputDestToSave); // if already exist do nothing

            FileInfo[] files = resource.GetFiles();

            if (!verif)
            {
                foreach (FileInfo file in files) // Calcul resource size
                {
                    TotalSize += file.Length;
                    NbFileMmax++;
                }
                foreach (DirectoryInfo subresource in Resource) // Calcul subresource size
                {
                    FileInfo[] Maxfiles = subresource.GetFiles();
                    foreach (FileInfo file in Maxfiles)
                    {
                        TotalSize += file.Length;
                        NbFileMmax++;
                    }
                }

            }


            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(inputDestToSave, file.Name);

                if (Size > 0)
                {
                    Progs = ((float)Size / TotalSize) * 100;
                }

                //Systems which allows to insert the values ​​of each file in the report file.
                DataState.SourceFileState = Path.Combine(inputpathsave, file.Name);
                DataState.TargetFileState = tempPath;
                DataState.TotalSizeState = NbFileMmax;
                DataState.TotalFileState = TotalSize;
                DataState.TotalSizeRestState = TotalSize - Size;
                DataState.FileRestState = NbFileMmax - Nbfiles;
                DataState.ProgressState = Progs;

                UpdateStateFile();

                file.CopyTo(tempPath, true);
                Nbfiles++;
                Size += file.Length;

            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copyDir)
            {
                foreach (DirectoryInfo subdir in Resource)
                {
                    string tempPath = Path.Combine(inputDestToSave, subdir.Name);
                    CompleteSave(subdir.FullName, tempPath, copyDir, true);
                }
            }
            ResetValue();
            UpdateStateFile();
            stopwatch.Stop();
            TimeTransfert = stopwatch.Elapsed; // Note the time passed
        }

        public void AddSave(Backup backup)
        {
            List<Backup> backupList = new List<Backup>();
            SerializeObj = null;

            if (!File.Exists(BackupListFile))
            {
                File.WriteAllText(BackupListFile, SerializeObj);
            }

            string jsonString = File.ReadAllText(BackupListFile);

            if (jsonString.Length != 0)
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);
                foreach (var obj in list)
                {
                    backupList.Add(obj);
                }
            }
            backupList.Add(backup);

            SerializeObj = JsonConvert.SerializeObject(backupList.ToArray(), Formatting.Indented) + Environment.NewLine;
            File.WriteAllText(BackupListFile, SerializeObj);

            DataState = new DataState(SaveName)
            {
                BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };
            AddState();
        }

        /// <summary>
        /// //Allows you to add a backup job to the report file.
        /// </summary>
        public void AddState()
        {
            List<DataState> stateList = new List<DataState>();
            SerializeObj = null;

            if (!File.Exists(StateFile))
            {
                File.Create(StateFile).Close();
            }

            string jsonString = File.ReadAllText(StateFile);

            if (jsonString.Length != 0)
            {
                DataState[] list = JsonConvert.DeserializeObject<DataState[]>(jsonString);
                foreach (var backup in list)
                {
                    stateList.Add(backup);
                }
            }
            DataState.SaveState = false;
            stateList.Add(DataState);

            SerializeObj = JsonConvert.SerializeObject(stateList.ToArray(), Formatting.Indented) + Environment.NewLine;
            File.WriteAllText(StateFile, SerializeObj);
        }

        /// <summary>
        /// load backup registered
        /// </summary>
        /// <param name="backupname"></param>
        public void LoadSave(string BackupName)
        {
            Backup backup = null;
            TotalSize = 0;
            BackupNameState = BackupName;

            string jsonString = File.ReadAllText(BackupListFile);


            if (jsonString.Length != 0)
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);
                foreach (var obj in list)
                {
                    if (obj.SaveName == BackupName)
                    {
                        backup = new Backup(obj.SaveName, obj.ResourceBackup, obj.TargetBackup, obj.Type, obj.MirrorBackup);
                    }
                }
            }

            if (backup.Type == "full")
            {
                NameStateFile = backup.SaveName;
                CompleteSave(backup.ResourceBackup, backup.TargetBackup, true, false);
                UpdateLogFile(backup.SaveName, backup.ResourceBackup, backup.TargetBackup);
                Console.WriteLine("Saved Successfull !");
            }
            else
            {
                NameStateFile = backup.SaveName;
                DifferentialSave(backup.ResourceBackup, backup.MirrorBackup, backup.TargetBackup);
                UpdateLogFile(backup.SaveName, backup.ResourceBackup, backup.TargetBackup);
                Console.WriteLine("Saved Successfull !");
            }

        }

        /// <summary>
        /// update the status file
        /// </summary>
        private void UpdateStateFile()
        {
            List<DataState> stateList = new List<DataState>();
            SerializeObj = null;
            if (!File.Exists(StateFile))
            {
                File.Create(StateFile).Close();
            }

            string jsonString = File.ReadAllText(StateFile);

            if (jsonString.Length != 0)
            {
                DataState[]? list = JsonConvert.DeserializeObject<DataState[]>(jsonString);

                foreach (var obj in list)
                {
                    if (obj.SaveNameState == NameStateFile)
                    {
                        obj.SourceFileState = DataState.SourceFileState;
                        obj.TargetFileState = DataState.TargetFileState;
                        obj.TotalFileState = DataState.TotalFileState;
                        obj.TotalSizeState = DataState.TotalSizeState;
                        obj.FileRestState = DataState.FileRestState;
                        obj.TotalSizeRestState = DataState.TotalSizeRestState;
                        obj.ProgressState = DataState.ProgressState;
                        obj.BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        obj.SaveState = DataState.SaveState;
                    }

                    stateList.Add(obj);

                }

                SerializeObj = JsonConvert.SerializeObject(stateList.ToArray(), Formatting.Indented) + Environment.NewLine;

                File.WriteAllText(StateFile, SerializeObj);
            }
        }

        /// <summary>
        /// function called when differential backup is selected
        /// </summary>
        /// <param name="pathA"></param>
        /// <param name="pathB"></param>
        /// <param name="pathC"></param>
        public void DifferentialSave(string pathA, string pathB, string pathC)
        {
            DataState = new DataState(NameStateFile);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataState.SaveState = true;
            TotalSize = 0;
            NbFileMmax = 0;

            System.IO.DirectoryInfo resource1 = new System.IO.DirectoryInfo(pathA);
            System.IO.DirectoryInfo resource2 = new System.IO.DirectoryInfo(pathB);

            // Take a snapshot of the file system.  
            IEnumerable<System.IO.FileInfo> list1 = resource1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> list2 = resource2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            //A custom file comparer defined below  
            FileCompare myFileCompare = new FileCompare();

            var queryList1Only = (from file in list1 select file).Except(list2, myFileCompare);
            Size = 0;
            Nbfiles = 0;
            Progs = 0;

            foreach (var v in queryList1Only)
            {
                TotalSize += v.Length;
                NbFileMmax++;

            }


            foreach (var v in queryList1Only)
            {
                string tempPath = Path.Combine(pathC, v.Name);
                DataState.SourceFileState = Path.Combine(pathA, v.Name);
                DataState.TargetFileState = tempPath;
                DataState.TotalSizeState = NbFileMmax;
                DataState.TotalFileState = TotalSize;
                DataState.TotalSizeRestState = TotalSize - Size;
                DataState.FileRestState = NbFileMmax - Nbfiles;
                DataState.ProgressState = Progs;
                UpdateStateFile();
                v.CopyTo(tempPath, true);
                Size += v.Length;
                Nbfiles++;
            }

            ResetValue();
            UpdateStateFile();
            stopwatch.Stop();
            TimeTransfert = stopwatch.Elapsed;
        }


        private void ResetValue()
        {
            DataState.SourceFileState = null;
            DataState.TargetFileState = null;
            DataState.TotalFileState = 0;
            DataState.TotalSizeState = 0;
            DataState.TotalSizeRestState = 0;
            DataState.FileRestState = 0;
            DataState.ProgressState = 0;
            DataState.SaveState = false;
        }


        /// <summary>
        /// update log file on: \EasySoft\bin
        /// </summary>
        /// <param name="savename"></param>
        /// <param name="sourcedir"></param>
        /// <param name="targetdir"></param>
        public void UpdateLogFile(string savename, string sourcelog, string targetlog)
        {
             string Time = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", TimeTransfert.Hours, TimeTransfert.Minutes, TimeTransfert.Seconds, TimeTransfert.Milliseconds / 10);

            DataLogs datalogs = new DataLogs
            {
                SaveNameLog = savename,
                SourceLog = sourcelog,
                TargetLog = targetlog,
                BackupDateLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                TotalSizeLog = TotalSize,
                TransactionTimeLog = Time
            };

            string path = System.Environment.CurrentDirectory;
            var directory = System.IO.Path.GetDirectoryName(path); 

            string serializeObj = JsonConvert.SerializeObject(datalogs, Formatting.Indented) + Environment.NewLine;
            File.AppendAllText(directory + @"DailyLogs_" + DateTime.Now.ToString("dd-MM-yyyy") + ".json", serializeObj);

        }


        /// <summary>
        /// count the number of backup in json file
        /// </summary>
        public void CheckDataFile()
        {
            CheckDataBackup = 0;

            if (File.Exists(BackupListFile))
            {
                string jsonstring = File.ReadAllText(BackupListFile);
                if (jsonstring.Length != 0)
                {
                    Backup[]? list = JsonConvert.DeserializeObject<Backup[]>(jsonstring);
                    CheckDataBackup = list.Length;
                }
            }
        }
    }
}
