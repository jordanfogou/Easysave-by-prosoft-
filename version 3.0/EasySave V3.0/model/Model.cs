using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Xml;

namespace EasySaveApp.model
{
    class Model
    {
        //Declaration of all variables and properties
        public int HEAVY_FILE_SIZE_THRESHOLD_IN_BYTES = JsonConvert.DeserializeObject<int>(File.ReadAllText(@"Resources\LimitSize.json"));
        public int checkDataBackup;
        private string serializeObj;
        public string backupListFile = System.Environment.CurrentDirectory + @"\Works\";
        public string stateFile = System.Environment.CurrentDirectory + @"\State\";
        public string NameStateFile { get; set; }
        public string BackupNameState { get; set; }
        public string SaveName { get; set; }
        public TimeSpan TimeTransfert { get; set; }
        public TimeSpan CryptTransfert { get; set; }
        public string UserMenuInput { get; set; }
        public bool Format { get; set; }
        public long TotalSize { get; set; }
        public bool Button_play { get; set; }
        public bool Button_pause { get; set; }
        public bool Button_stop { get; set; }
        public string StatusButton { get; set; }

        private static Mutex mut = new Mutex();

        public bool isCheck { get; set; }

        public Model()
        {

            UserMenuInput = " ";

            if (!Directory.Exists(backupListFile)) //Check if the folder is created
            {
                DirectoryInfo di = Directory.CreateDirectory(backupListFile); //Function that creates the folder
            }
            backupListFile += @"backupList.json"; //Create a JSON file

            if (!Directory.Exists(stateFile))//Check if the folder is created
            {
                DirectoryInfo di = Directory.CreateDirectory(stateFile); //Function that creates the folder
            }
            stateFile += @"state.json"; //Create a JSON file
        }

        public void CompleteSave(Backup backup, bool copyDir, bool verif, BackupWithProgress backupWithProgress, ManualResetEvent REProcessingPrioritizedFiles, ManualResetEvent RETransferingHeavyFile, ManualResetEvent REBusinessSoftwareOpened, Action<float> progressChangeFunction)
        {
            DataState dataState = new DataState(NameStateFile);
            dataState.SaveState = true;

            Stopwatch stopwatch = new Stopwatch();
            Stopwatch cryptwatch = new Stopwatch();
            stopwatch.Start();

            long TotalSize = 0;
            int NbFileMmax = 0;
            long Size = 0;
            int Nbfiles = 0;
            float Progs = 0;

            DirectoryInfo resource = new DirectoryInfo(backup.ResourceBackup);

            if (!resource.Exists)
            {
                throw new DirectoryNotFoundException("ERROR: Directory Not Found ! " + backup.ResourceBackup);
            }

            DirectoryInfo[] Resource = resource.GetDirectories();
            Directory.CreateDirectory(backup.TargetBackup); // if already exist do nothing

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
            backup.TotalSize = TotalSize;

            FileInQueue[] filesInQueue = PrioritizeFiles(files);

            foreach (FileInQueue fileInQueue in filesInQueue)
            {
                FileInfo file = fileInQueue.MFileInfo;

                if (fileInQueue.IsPrioritized)
                {
                    backupWithProgress.IsProcessingPrioritizedFile = true;
                }
                else
                {
                    backupWithProgress.IsProcessingPrioritizedFile = false;
                    REProcessingPrioritizedFiles.WaitOne();
                }

                if (file.Length > HEAVY_FILE_SIZE_THRESHOLD_IN_BYTES)
                {
                    RETransferingHeavyFile.WaitOne();
                    backupWithProgress.IsTransferingHeavyFile = true;
                }

                backupWithProgress.ResetEvent.WaitOne();
                if (backupWithProgress.IsAborted)
                {
                    progressChangeFunction(0);
                    break;
                }
                REBusinessSoftwareOpened.WaitOne();

                string sourcePath = Path.Combine(backup.ResourceBackup, file.Name);
                string tempPath = Path.Combine(backup.TargetBackup, file.Name);


                if (isCheck == true && CryptExt(Path.GetExtension(file.Name))) { 
                    CryptFunction(sourcePath, tempPath);
                }
                else
                {
                    file.CopyTo(tempPath, true); //Function that allows you to copy the file to its new folder.
                }
                backupWithProgress.IsTransferingHeavyFile = false;

                Nbfiles++;
                Size += file.Length;

                if (Size > 0)
                {
                    Progs = ((float)Size / TotalSize) * 100;
                }

                //Systems which allows to insert the values ​​of each file in the report file.
                dataState.SourceFileState = Path.Combine(backup.ResourceBackup, file.Name);
                dataState.TargetFileState = tempPath;
                dataState.TotalFileState = NbFileMmax;
                dataState.TotalSizeState = TotalSize;
                dataState.TotalSizeRestState = TotalSize - Size;
                dataState.FileRestState = NbFileMmax - Nbfiles;
                dataState.ProgressState = Progs;
                progressChangeFunction(Progs);

                UpdateStateFile(dataState);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copyDir)
            {
                foreach (DirectoryInfo subdir in Resource)
                {
                    CompleteSave(backup, copyDir, true, backupWithProgress, REProcessingPrioritizedFiles, RETransferingHeavyFile, REBusinessSoftwareOpened, progressChangeFunction);
                }
            }
            cryptwatch.Stop();
            stopwatch.Stop();
            CryptTransfert = stopwatch.Elapsed;
            TimeTransfert = stopwatch.Elapsed; // Note the time passed
        }

        /// <summary>
        /// function called when differential backup is selected
        /// </summary>
        /// <param name="pathA"></param>
        /// <param name="pathB"></param>
        /// <param name="pathC"></param>
        public void DifferentialSave(Backup backup, BackupWithProgress backupWithProgress, ManualResetEvent REProcessingPrioritizedFiles, ManualResetEvent RETransferingHeavyFile, ManualResetEvent REBusinessSoftwareOpened, Action<float> progressChangeFunction)
        {
            DataState dataState = new DataState(NameStateFile);
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch cryptwatch = new Stopwatch();
            stopwatch.Start();

            dataState.SaveState = true;
            long TotalSize = 0;
            int NbFileMmax = 0;

            System.IO.DirectoryInfo resource1 = new System.IO.DirectoryInfo(backup.ResourceBackup);
            System.IO.DirectoryInfo resource2 = new System.IO.DirectoryInfo(backup.MirrorBackup);

            // Take a snapshot of the file system.  
            IEnumerable<System.IO.FileInfo> list1 = resource1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> list2 = resource2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            //A custom file comparer defined below  
            FileCompare myFileCompare = new FileCompare();

            var queryList1Only = (from file in list1 select file).Except(list2, myFileCompare).ToArray();
            long Size = 0;
            int Nbfiles = 0;
            float Progs = 0;


            foreach (var v in queryList1Only)
            {
                TotalSize += v.Length;
                NbFileMmax++;
            }
            backup.TotalSize = TotalSize;

            FileInQueue[] filesInQueue = PrioritizeFiles(queryList1Only);

            foreach (FileInQueue fileInQueue in filesInQueue)
            {
                FileInfo file = fileInQueue.MFileInfo;

                if (fileInQueue.IsPrioritized)
                {
                    backupWithProgress.IsProcessingPrioritizedFile = true;
                }
                else
                {
                    backupWithProgress.IsProcessingPrioritizedFile = false;
                    REProcessingPrioritizedFiles.WaitOne();
                }

                if (file.Length > HEAVY_FILE_SIZE_THRESHOLD_IN_BYTES)
                {
                    RETransferingHeavyFile.WaitOne();
                    backupWithProgress.IsTransferingHeavyFile = true;
                }

                backupWithProgress.ResetEvent.WaitOne();

                if (backupWithProgress.IsAborted)
                {
                    progressChangeFunction(0);
                    break;
                }
                REBusinessSoftwareOpened.WaitOne();

                string sourcePath = Path.Combine(backup.ResourceBackup, file.Name);
                string tempPath = Path.Combine(backup.TargetBackup, file.Name);

                if (isCheck == true && CryptExt(Path.GetExtension(file.Name)))
                {
                    CryptFunction(sourcePath, tempPath);
                }
                else
                {
                    file.CopyTo(tempPath, true); //Function that allows you to copy the file to its new folder.
                }

                Nbfiles++;
                Size += file.Length;

                if (Size > 0)
                {
                    Progs = ((float)Size / TotalSize) * 100;
                }
                backupWithProgress.IsTransferingHeavyFile = false;

                dataState.SourceFileState = Path.Combine(backup.ResourceBackup, file.Name);
                dataState.TargetFileState = tempPath;
                dataState.TotalSizeState = NbFileMmax;
                dataState.TotalFileState = TotalSize;
                dataState.TotalSizeRestState = TotalSize - Size;
                dataState.FileRestState = NbFileMmax - Nbfiles;
                dataState.ProgressState = Progs;
                progressChangeFunction(Progs);
                UpdateStateFile(dataState);
            }

            stopwatch.Stop();
            cryptwatch.Stop();
            TimeTransfert = stopwatch.Elapsed; // Note the time passed
            CryptTransfert = stopwatch.Elapsed;
        }

        public void CryptFunction(string sourcePath, string destPath)
        {
            Encrypt(sourcePath, destPath);
        }

        private void UpdateStateFile(DataState dataState)
        {
            mut.WaitOne();
            List<DataState> stateList = new List<DataState>();

            if (!File.Exists(stateFile)) //Checking if the file exists
            {
                File.Create(stateFile).Close();
            }

            string jsonString = File.ReadAllText(stateFile); //Reading the json file

            if (jsonString.Length != 0)
            {
                stateList = System.Text.Json.JsonSerializer.Deserialize<List<DataState>>(jsonString, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            }

            stateList.Add(dataState);

            jsonString = System.Text.Json.JsonSerializer.Serialize<List<DataState>>(stateList);

            File.WriteAllText(stateFile, jsonString); //Function to write to JSON file
            mut.ReleaseMutex();
        }

        /// <summary>
        /// update log file on: \EasySoft\bin
        /// </summary>
        /// <param name="savename"></param>
        /// <param name="sourcedir"></param>
        /// <param name="targetdir"></param>
        public void UpdateLogFile(Backup backup)
        {
            mut.WaitOne();
            Stopwatch stopwatch = new Stopwatch(); //Declaration of the stopwatch
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", TimeTransfert.Hours, TimeTransfert.Minutes, TimeTransfert.Seconds, TimeTransfert.Milliseconds / 10);
            string elapsedCrypt = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", CryptTransfert.Hours, CryptTransfert.Minutes, CryptTransfert.Seconds, CryptTransfert.Milliseconds / 10);
            DataLogs datalogs = new DataLogs
            {
                SaveNameLog = backup.SaveName,
                SourceLog = backup.ResourceBackup,
                TargetLog = backup.TargetBackup,
                BackupDateLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                TotalSizeLog = backup.TotalSize,
                TransactionTimeLog = elapsedTime,
                CryptTime = elapsedCrypt,
            };
            string path = System.Environment.CurrentDirectory;
            var directory = System.IO.Path.GetDirectoryName(path);
            string pathfile = directory + @"DailyLogs_" + DateTime.Now.ToString("dd-MM-yyyy") + ".json";
            string pathfiles = directory + @"DailyLogs_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xml";

            if (Format == true)
            {
                XmlDocument xdoc = new XmlDocument();

                try
                {
                    xdoc.Load(pathfiles);
                }
                catch
                {
                    XElement Logs = new XElement("Logs");
                    StreamWriter sr = new StreamWriter(pathfiles);
                    sr.WriteLine(Logs);
                    sr.Close();
                    xdoc.Load(pathfiles);

                }
                XmlNode Log = xdoc.CreateElement("log");

                XmlNode Name = xdoc.CreateElement("name");
                XmlNode SourceFile = xdoc.CreateElement("sourceFile");
                XmlNode TargetFile = xdoc.CreateElement("TargetFile");
                XmlNode Date = xdoc.CreateElement("date");
                XmlNode SizeOctet = xdoc.CreateElement("size");
                XmlNode TransfertTime = xdoc.CreateElement("transfertTime");
                XmlNode CryptTime = xdoc.CreateElement("cryptTime");

                Name.InnerText = datalogs.SaveNameLog;
                SourceFile.InnerText = datalogs.SourceLog;
                TargetFile.InnerText = datalogs.TargetLog;
                Date.InnerText = datalogs.BackupDateLog;
                SizeOctet.InnerText = datalogs.TotalSizeLog.ToString();
                TransfertTime.InnerText = datalogs.TransactionTimeLog;

                Log.AppendChild(Name);
                Log.AppendChild(SourceFile);
                Log.AppendChild(TargetFile);
                Log.AppendChild(Date);
                Log.AppendChild(SizeOctet);
                Log.AppendChild(TransfertTime);
                Log.AppendChild(CryptTime);

                xdoc.DocumentElement.PrependChild(Log);
                xdoc.Save(pathfiles);
            }
            else
            {
                List<object> jsonContent = new List<object>();
                if (File.Exists(pathfile))
                {
                    string oldJsonFileContent = File.ReadAllText(pathfile);
                    jsonContent = JsonConvert.DeserializeObject<List<object>>(oldJsonFileContent);
                }
                jsonContent.Add(datalogs);
                string serializedObj = JsonConvert.SerializeObject(jsonContent, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathfile, serializedObj);


            }
            stopwatch.Reset(); // Reset of stopwatch
            mut.ReleaseMutex();
        }


        public void AddSave(Backup backup) //Function that creates a backup job
        {
            List<Backup> backupList = new List<Backup>();
            this.serializeObj = null;

            if (!File.Exists(backupListFile)) //Checking if the file exists
            {
                File.WriteAllText(backupListFile, this.serializeObj);
            }

            string jsonString = File.ReadAllText(backupListFile); //Reading the json file

            if (jsonString.Length != 0) //Checking the contents of the json file is empty or not
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString); //Derialization of the json file
                foreach (var obj in list) //Loop to add the information in the json
                {
                    backupList.Add(obj);
                }
            }
            backupList.Add(backup); //Allows you to prepare the objects for the json filling

            this.serializeObj = JsonConvert.SerializeObject(backupList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file
            File.WriteAllText(backupListFile, this.serializeObj); // Writing to the json file

            DataState dataState = new DataState(this.SaveName);//Class initiation

            dataState.BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); //Adding the time in the variable
            AddState(dataState); //Call of the function to add the backup in the report file.

        }

        public void AddState(DataState dataState) //Function that allows you to add a backup job to the report file.
        {
            List<DataState> stateList = new List<DataState>();
            this.serializeObj = null;

            if (!File.Exists(stateFile)) //Checking if the file exists
            {
                File.Create(stateFile).Close();
            }

            string jsonString = File.ReadAllText(stateFile); //Reading the json file

            if (jsonString.Length != 0)
            {
                DataState[] list = JsonConvert.DeserializeObject<DataState[]>(jsonString); //Derialization of the json file
                foreach (var obj in list) //Loop to add the information in the json
                {
                    stateList.Add(obj);
                }
            }
            dataState.SaveState = false;
            stateList.Add(dataState); //Allows you to prepare the objects for the json filling

            this.serializeObj = JsonConvert.SerializeObject(stateList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file
            File.WriteAllText(stateFile, this.serializeObj);// Writing to the json file
        }

        public void LoadSave(BackupWithProgress backup, ManualResetEvent REProcessingPrioritizedFiles, ManualResetEvent RETransferingHeavyFile, ManualResetEvent REBusinessSoftwareOpened, Action<float> progressChangeFunction) //Function that allows you to load backup jobs
        {
            Backup selectedBackup = null;
            BackupNameState = backup.SaveName;

            string jsonString = File.ReadAllText(backupListFile); //Reading the json file


            if (jsonString.Length != 0) //Checking the contents of the json file is empty or not
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);  //Derialization of the json file
                foreach (var obj in list)
                {
                    if (obj.SaveName == backup.SaveName) //Check to have the correct name of the backup
                    {
                        selectedBackup = new Backup(obj.SaveName, obj.ResourceBackup, obj.TargetBackup, obj.Type, obj.MirrorBackup); //Function that allows you to retrieve information about the backup
                    }
                }
            }

            if (selectedBackup != null)
            {
                NameStateFile = selectedBackup.SaveName;

                if (selectedBackup.Type == "full") //If the type is 1, it means it's a full backup
                {
                    CompleteSave(selectedBackup, true, false, backup, REProcessingPrioritizedFiles, RETransferingHeavyFile, REBusinessSoftwareOpened, progressChangeFunction); //Calling the function to run the full backup
                }
                else //If this is the wrong guy then, it means it's a differential backup
                {
                    DifferentialSave(selectedBackup, backup, REProcessingPrioritizedFiles, RETransferingHeavyFile, REBusinessSoftwareOpened, progressChangeFunction); //Calling the function to start the differential backup
                }

                UpdateLogFile(selectedBackup); //Call of the function to start the modifications of the log file
            }
        }

        public List<Backup> NameList()//Function that lets you know the names of the backups.
        {
            List<Backup> backupList = new List<Backup>();

            if (!File.Exists(backupListFile)) //Checking if the file exists
            {
                File.WriteAllText(backupListFile, this.serializeObj);
            }

            List<Backup> names = new List<Backup>();
            string jsonString = File.ReadAllText(backupListFile); //Function to read json file
            Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString); // Function to dezerialize the json file

            if (jsonString.Length != 0)
            {
                foreach (var obj in list) //Loop to display the names of the backups
                {
                    names.Add(obj);
                }

            }

            return names;

        }

        // Function Delete a backup
        public void DeleteSave(string backupname)
        {
            List<Backup> backupList = new List<Backup>();
            this.serializeObj = null;

            if (!File.Exists(backupListFile)) //Checking if the file exists
            {
                File.WriteAllText(backupListFile, this.serializeObj);
            }

            string jsonString = File.ReadAllText(backupListFile); //Reading the json file

            if (jsonString.Length != 0) //Checking the contents of the json file is empty or not
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString); //Derialization of the json file
                foreach (var obj in list) //Loop to add the information in the json
                {
                    if (obj.SaveName != backupname) //Check to have the correct name of the backup
                    {
                        backupList.Add(obj);
                    }
                }
            }

            this.serializeObj = JsonConvert.SerializeObject(backupList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file
            File.WriteAllText(backupListFile, this.serializeObj); // Writing to the json file
        }

        public static string[] GetJailApps()//Function that allows to recover software that is blacklisted.
        {
            using StreamReader reader = new StreamReader(@"Resources\JailApps.json");//Function to read the json file
            JailAppsFormat[] item_jailapps;
            string[] jailapps_array;
            string json = reader.ReadToEnd();
            List<JailAppsFormat> items = JsonConvert.DeserializeObject<List<JailAppsFormat>>(json);
            item_jailapps = items.ToArray();
            jailapps_array = item_jailapps[0].jailed_apps.Split(',');

            return jailapps_array;//We return the names of the softwares which are in the list of the json file.
        }


        public static bool CheckSoftware(string[] blacklist_app)//Function that allows you to compare a program that is in the list is running.
        {
            bool abortSave = false;
            foreach (string App in blacklist_app)
            {
                Process[] ps = Process.GetProcessesByName(App);
                if (ps.Length > 0)
                {
                    abortSave = true;
                }
            }
            return abortSave;
        }

        public void ModelFormat(bool extension)
        {
            Format = extension;
        }

        public static FileInQueue[] PrioritizeFiles(FileInfo[] filesPath)
        {
            List<string> prioritisedExtensions = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(@"Resources\PriorityExtensions.json"));
            FileInQueue[] filesPrioritized = filesPath.Where(x => prioritisedExtensions.Contains(x.Extension)).Select(f => new FileInQueue(f, true)).ToArray();
            FileInQueue[] filesNormal = filesPath.Where(x => !prioritisedExtensions.Contains(x.Extension)).Select(f => new FileInQueue(f, false)).ToArray();
            return filesPrioritized.Concat(filesNormal).ToArray();
        }

        private static string[] getExtensionCrypt()//Function that allows to recover the extensions that the user wants to encrypt in the json file.
        {
            using (StreamReader reader = new StreamReader(@"Resources\CryptExtension.json"))//Function to read the json file
            {
                CryptFormat[] item_crypt;
                string[] crypt_extensions_array;
                string json = reader.ReadToEnd();
                List<CryptFormat> items = JsonConvert.DeserializeObject<List<CryptFormat>>(json);
                item_crypt = items.ToArray();
                crypt_extensions_array = item_crypt[0].extensionCrypt.Split(',');

                return crypt_extensions_array; //We return the variables that are stored in an array
            }
        }
        public static bool CryptExt(string extension)//Function that compares the extensions of the json file and the one of the file being backed up.
        {
            foreach (string extensionExt in getExtensionCrypt())
            {
                if (extensionExt == extension)
                {
                    return true;
                }
            }
            return false;
        }

        public void Encrypt(string sourceDir, string targetDir)//This function allows you to encrypt files. 
        {
            using (Process process = new Process())//Declaration of the process
            {
                process.StartInfo.FileName = @"Resources\CryptoSoft\CryptoSoft.exe"; //Calls the process that is CryptoSoft
                process.StartInfo.Arguments = String.Format("\"{0}\"", sourceDir) + " " + String.Format("\"{0}\"", targetDir); //Preparation of variables for the process.
                process.Start(); //Launching the process
                process.WaitForExit();
                process.Close();
            }
        }

        public bool Play_click()
        {
            Button_play = true;
            return Button_play;
        }

        public bool Pause_click()
        {
            Button_pause = true;
            return Button_pause;
        }
        public bool Stop_click()
        {
            Button_stop = true;
            return Button_stop;
        }
    }
}



