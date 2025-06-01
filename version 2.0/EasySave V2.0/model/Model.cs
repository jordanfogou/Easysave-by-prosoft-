using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace EasySaveApp.model
{
    class Model
    {
        // ----------------------------------------------------
        // 1) PROPRIÉTÉS AJOUTÉES POUR LA GESTION DU CRYPTAGE
        // ----------------------------------------------------

        /// <summary>
        /// Chemin du dossier cible (équivalent à "TargetResource").
        /// </summary>
        public string TargetResource { get; set; }

        /// <summary>
        /// Tableau des extensions de fichiers à chiffrer (ex. : { ".bmp", ".txt", ".log", ".docx" }).
        /// </summary>
        public string[] ExtensionsToEncrypt { get; set; }

        /// <summary>
        /// Mot de passe de cryptage à utiliser pour les fichiers (passé à CryptoSoft).
        /// </summary>
        public string EncryptionPassword { get; set; }


        // ----------------------------------------------------
        // 2) PROPRIÉTÉS EXISTANTES (inchangées, hormis la casse)
        // ----------------------------------------------------

        public int checkDataBackup;
        private string serializeObj;
        public string backupListFile = Environment.CurrentDirectory + @"\Works\";
        public string stateFile = Environment.CurrentDirectory + @"\State\";

        public DataState DataState { get; set; }
        public string NameStateFile { get; set; }
        public string BackupNameState { get; set; }
        public string Resource { get; set; }
        public int Nbfilesmax { get; set; }
        public int Nbfiles { get; set; }
        public long TotalSize { get; set; }
        public float Progs { get; set; }
        public string SaveName { get; set; }
        public int Type { get; set; }
        public string SourceFile { get; set; }
        public string TypeString { get; set; }
        public long TotalSizeState { get; set; }
        public int TotalFileState { get; set; }
        public int FileRestState { get; set; }
        public long TotalSizeRestState { get; set; }
        public bool SaveState { get; set; }
        public TimeSpan TimeTransfert { get; set; }
        public string MirrorResource { get; set; }
        public bool Format { get; set; }

        /// <summary>
        /// Constructeur de la classe Model.
        /// </summary>
        public Model()
        {
            // Création des dossiers Works et State si nécessaires
            if (!Directory.Exists(backupListFile))
            {
                Directory.CreateDirectory(backupListFile);
            }
            backupListFile += @"backupList.json";

            if (!Directory.Exists(stateFile))
            {
                Directory.CreateDirectory(stateFile);
            }
            stateFile += @"state.json";

            // Initialisation des propriétés de cryptage par défaut
            ExtensionsToEncrypt = new string[0];
            EncryptionPassword = string.Empty;
        }


        /// <summary>
        /// Effectue une sauvegarde complète (mirror) / récursive.
        /// </summary>
        public void CompleteSave(string inputpathsave, string inputDestToSave, bool copyDir, bool verif)
        {
            DataState = new DataState(NameStateFile);
            DataState.SaveState = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            TotalSize = 0;
            Nbfilesmax = 0;
            long Size = 0;
            int Nbfiles = 0;
            float Progs = 0;

            DirectoryInfo resource = new DirectoryInfo(inputpathsave);
            if (!resource.Exists)
            {
                throw new DirectoryNotFoundException($"ERROR: Directory Not Found ! {inputpathsave}");
            }

            DirectoryInfo[] subdirs = resource.GetDirectories();
            Directory.CreateDirectory(inputDestToSave);

            FileInfo[] files = resource.GetFiles();
            if (!verif)
            {
                foreach (FileInfo file in files)
                {
                    TotalSize += file.Length;
                    Nbfilesmax++;
                }
                foreach (DirectoryInfo subdir in subdirs)
                {
                    FileInfo[] subFiles = subdir.GetFiles();
                    foreach (FileInfo file in subFiles)
                    {
                        TotalSize += file.Length;
                        Nbfilesmax++;
                    }
                }
            }

            // Copie des fichiers du dossier racine
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(inputDestToSave, file.Name);

                if (Size > 0)
                {
                    Progs = ((float)Size / TotalSize) * 100;
                }

                DataState.SourceFileState = Path.Combine(inputpathsave, file.Name);
                DataState.TargetFileState = tempPath;
                DataState.TotalSizeState = TotalSize;
                DataState.TotalFileState = Nbfilesmax;
                DataState.TotalSizeRestState = TotalSize - Size;
                DataState.FileRestState = Nbfilesmax - Nbfiles;
                DataState.ProgressState = Progs;
                DataState.BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                DataState.SaveState = true;
                UpdateStateFile();

                // Copie du fichier
                file.CopyTo(tempPath, true);

                // Si l’extension du fichier figure dans ExtensionsToEncrypt, lancer CryptoSoft
                string extension = file.Extension.ToLower();
                if (ExtensionsToEncrypt.Contains(extension))
                {
                    // Exemple d’appel à CryptoSoft :
                    // CryptoSoft.exe –in "cheminFichierSource" –out "cheminFichierChiffré" –pwd "motDePasse"
                    // Ici, on suppose que CryptoSoft.exe se trouve côte-à-côte avec EasySaveApp.exe
                    string cryptExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
                    string arguments = $"-in \"{tempPath}\" -out \"{tempPath}\" -pwd \"{EncryptionPassword}\"";
                    try
                    {
                        var p = new Process();
                        p.StartInfo.FileName = cryptExe;
                        p.StartInfo.Arguments = arguments;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        p.WaitForExit();

                        // Récupérer le code de sortie pour renseigner EncryptionTimeState
                        int exitCode = p.ExitCode;
                        // Si exitCode == 0, la console a déjà affiché le temps en ms, mais on peut le récupérer
                        // depuis la sortie standard si nécessaire. Pour simplifier, on suppose que le programme
                        // écrit directement la durée en ms sur sa sortie, qu’on ignore ici, et on ne gère que l’ExitCode.
                        // Si besoin plus précis, utilisez RedirectStandardOutput = true.
                        DataState.EncryptionTimeState = exitCode == 0 ? (int)stopwatch.ElapsedMilliseconds : -exitCode;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Erreur lors de l'appel à CryptoSoft : {ex.Message}",
                            "Erreur de cryptage",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        DataState.EncryptionTimeState = -1;
                    }
                }
                else
                {
                    // Aucune extension à chiffrer : on marque 0 ms
                    DataState.EncryptionTimeState = 0;
                }

                Nbfiles++;
                Size += file.Length;
            }

            // Récursion pour copier les sous-dossiers si nécessaire
            if (copyDir)
            {
                foreach (DirectoryInfo subdir in subdirs)
                {
                    string tempPath = Path.Combine(inputDestToSave, subdir.Name);
                    CompleteSave(subdir.FullName, tempPath, copyDir, true);
                }
            }

            ResetValue();
            UpdateStateFile();

            stopwatch.Stop();
            TimeTransfert = stopwatch.Elapsed;
        }


        /// <summary>
        /// Effectue une sauvegarde différentielle entre le dossier A (source) et B (mirror), puis copie dans C.
        /// </summary>
        public void DifferentialSave(string pathA, string pathB, string pathC)
        {
            DataState = new DataState(NameStateFile);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataState.SaveState = true;
            TotalSize = 0;
            Nbfilesmax = 0;
            long Size = 0;
            int Nbfiles = 0;
            float Progs = 0;

            DirectoryInfo resource1 = new DirectoryInfo(pathA);
            DirectoryInfo resource2 = new DirectoryInfo(pathB);

            IEnumerable<FileInfo> list1 = resource1.GetFiles("*.*", SearchOption.AllDirectories);
            IEnumerable<FileInfo> list2 = resource2.GetFiles("*.*", SearchOption.AllDirectories);

            FileCompare myFileCompare = new FileCompare();
            var queryList1Only = list1.Except(list2, myFileCompare);

            foreach (var v in queryList1Only)
            {
                TotalSize += v.Length;
                Nbfilesmax++;
            }

            foreach (var v in queryList1Only)
            {
                string tempPath = Path.Combine(pathC, v.Name);

                if (Size > 0)
                {
                    Progs = ((float)Size / TotalSize) * 100;
                }

                DataState.SourceFileState = Path.Combine(pathA, v.Name);
                DataState.TargetFileState = tempPath;
                DataState.TotalSizeState = TotalSize;
                DataState.TotalFileState = Nbfilesmax;
                DataState.TotalSizeRestState = TotalSize - Size;
                DataState.FileRestState = Nbfilesmax - Nbfiles;
                DataState.ProgressState = Progs;
                DataState.BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                DataState.SaveState = true;
                UpdateStateFile();

                // Copie du fichier
                v.CopyTo(tempPath, true);

                // Chiffrement si extension à gérer
                string extension = v.Extension.ToLower();
                if (ExtensionsToEncrypt.Contains(extension))
                {
                    string cryptExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
                    string arguments = $"-in \"{tempPath}\" -out \"{tempPath}\" -pwd \"{EncryptionPassword}\"";
                    try
                    {
                        var p = new Process();
                        p.StartInfo.FileName = cryptExe;
                        p.StartInfo.Arguments = arguments;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        p.WaitForExit();

                        int exitCode = p.ExitCode;
                        DataState.EncryptionTimeState = exitCode == 0 ? (int)stopwatch.ElapsedMilliseconds : -exitCode;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Erreur lors de l'appel à CryptoSoft : {ex.Message}",
                            "Erreur de cryptage",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        DataState.EncryptionTimeState = -1;
                    }
                }
                else
                {
                    DataState.EncryptionTimeState = 0;
                }

                Nbfiles++;
                Size += v.Length;
            }

            ResetValue();
            UpdateStateFile();

            stopwatch.Stop();
            TimeTransfert = stopwatch.Elapsed;
        }


        /// <summary>
        /// Réinitialise les compteurs de DataState.
        /// </summary>
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
            DataState.EncryptionTimeState = 0;
        }


        /// <summary>
        /// Met à jour le fichier d’état JSON (state.json) à chaque étape.
        /// </summary>
        private void UpdateStateFile()
        {
            List<DataState> stateList = new List<DataState>();
            this.serializeObj = null;

            if (!File.Exists(stateFile))
            {
                File.Create(stateFile).Close();
            }

            string jsonString = File.ReadAllText(stateFile);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                DataState[] list = JsonConvert.DeserializeObject<DataState[]>(jsonString);
                foreach (var obj in list)
                {
                    if (obj.SaveNameState == this.NameStateFile)
                    {
                        obj.SourceFileState = this.DataState.SourceFileState;
                        obj.TargetFileState = this.DataState.TargetFileState;
                        obj.TotalFileState = this.DataState.TotalFileState;
                        obj.TotalSizeState = this.DataState.TotalSizeState;
                        obj.FileRestState = this.DataState.FileRestState;
                        obj.TotalSizeRestState = this.DataState.TotalSizeRestState;
                        obj.ProgressState = this.DataState.ProgressState;
                        obj.BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        obj.SaveState = this.DataState.SaveState;
                        obj.EncryptionTimeState = this.DataState.EncryptionTimeState;
                    }
                    stateList.Add(obj);
                }

                this.serializeObj = JsonConvert.SerializeObject(
                    stateList.ToArray(),
                    Newtonsoft.Json.Formatting.Indented
                ) + Environment.NewLine;
                File.WriteAllText(stateFile, this.serializeObj);
            }
        }


        /// <summary>
        /// Met à jour le fichier de logs (journaliers) en JSON ou XML selon Model.Format.
        /// </summary>
        public void UpdateLogFile(string savename, string sourcelog, string targetlog)
        {
            string Time = string.Format(
                "{0:00}:{1:00}:{2:00}.{3:00}",
                TimeTransfert.Hours,
                TimeTransfert.Minutes,
                TimeTransfert.Seconds,
                TimeTransfert.Milliseconds / 10
            );

            DataLogs datalogs = new DataLogs
            {
                SaveNameLog = savename,
                SourceLog = sourcelog,
                TargetLog = targetlog,
                BackupDateLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                TotalSizeLog = this.TotalSize,
                TransactionTimeLog = Time
            };

            // On construit les chemins pour DailyLogs_<date>.json/.xml
            string path = Environment.CurrentDirectory;
            string directory = Path.GetDirectoryName(path);
            string pathfileJson = Path.Combine(
                directory,
                "DailyLogs_" + DateTime.Now.ToString("dd-MM-yyyy") + ".json"
            );
            string pathfileXml = Path.Combine(
                directory,
                "DailyLogs_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xml"
            );

            if (Format)
            {
                // Format = true signifie : on produit un XML
                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.Load(pathfileXml);
                }
                catch
                {
                    // Si le fichier n’existe pas, on crée le nœud racine <Logs>
                    XElement Logs = new XElement("Logs");
                    using (var sr = new StreamWriter(pathfileXml))
                    {
                        sr.WriteLine(Logs);
                    }
                    xdoc.Load(pathfileXml);
                }

                XmlNode Log = xdoc.CreateElement("log");
                XmlNode Name = xdoc.CreateElement("name");
                XmlNode SourceFile = xdoc.CreateElement("sourceFile");
                XmlNode TargetFile = xdoc.CreateElement("TargetFile");
                XmlNode Date = xdoc.CreateElement("date");
                XmlNode SizeOctet = xdoc.CreateElement("size");
                XmlNode TransfertTime = xdoc.CreateElement("transfertTime");

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

                xdoc.DocumentElement.PrependChild(Log);
                xdoc.Save(pathfileXml);
            }
            else
            {
                // Format = false signifie : on produit du JSON
                List<object> jsonContent = new List<object>();
                if (File.Exists(pathfileJson))
                {
                    string oldJsonFileContent = File.ReadAllText(pathfileJson);
                    if (!string.IsNullOrWhiteSpace(oldJsonFileContent))
                    {
                        jsonContent = JsonConvert.DeserializeObject<List<object>>(oldJsonFileContent);
                    }
                }
                jsonContent.Add(datalogs);
                string serializedObj = JsonConvert.SerializeObject(
                    jsonContent,
                    Newtonsoft.Json.Formatting.Indented
                );
                File.WriteAllText(pathfileJson, serializedObj);
            }
        }


        /// <summary>
        /// Ajoute une définition de sauvegarde au fichier backupList.json.
        /// </summary>
        public void AddSave(Backup backup)
        {
            List<Backup> backupList = new List<Backup>();
            this.serializeObj = null;

            if (!File.Exists(backupListFile))
            {
                File.WriteAllText(backupListFile, string.Empty);
            }

            string jsonString = File.ReadAllText(backupListFile);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);
                backupList.AddRange(list);
            }

            backupList.Add(backup);
            this.serializeObj = JsonConvert.SerializeObject(
                backupList.ToArray(),
                Newtonsoft.Json.Formatting.Indented
            ) + Environment.NewLine;
            File.WriteAllText(backupListFile, this.serializeObj);

            // Ajoute un état pour cette nouvelle sauvegarde
            DataState = new DataState(this.SaveName);
            DataState.BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            AddState();
        }


        /// <summary>
        /// Ajoute un nouvel état dans state.json lors de la création d'un job.
        /// </summary>
        public void AddState()
        {
            List<DataState> stateList = new List<DataState>();
            this.serializeObj = null;

            if (!File.Exists(stateFile))
            {
                File.Create(stateFile).Close();
            }

            string jsonString = File.ReadAllText(stateFile);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                DataState[] list = JsonConvert.DeserializeObject<DataState[]>(jsonString);
                stateList.AddRange(list);
            }

            this.DataState.SaveState = false;
            stateList.Add(this.DataState);
            this.serializeObj = JsonConvert.SerializeObject(
                stateList.ToArray(),
                Newtonsoft.Json.Formatting.Indented
            ) + Environment.NewLine;
            File.WriteAllText(stateFile, this.serializeObj);
        }


        /// <summary>
        /// Charge et exécute le job de sauvegarde dont le nom est backupname.
        /// </summary>
        public void LoadSave(string backupname)
        {
            Backup selectedBackup = null;
            TotalSize = 0;
            BackupNameState = backupname;

            string jsonString = File.ReadAllText(backupListFile);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);
                foreach (var obj in list)
                {
                    if (obj.SaveName == backupname)
                    {
                        // Reconstruction de l’objet Backup à 7 paramètres
                        selectedBackup = new Backup(
                            obj.SaveName,
                            obj.ResourceBackup,
                            obj.TargetBackup,
                            obj.Type,
                            obj.MirrorBackup,
                            obj.ExtensionsToEncrypt,
                            obj.EncryptionPassword
                        );
                        break;
                    }
                }
            }

            if (selectedBackup != null)
            {
                NameStateFile = selectedBackup.SaveName;

                if (selectedBackup.Type == "full")
                {
                    CompleteSave(
                        selectedBackup.ResourceBackup,
                        selectedBackup.TargetBackup,
                        true,
                        false
                    );
                }
                else
                {
                    DifferentialSave(
                        selectedBackup.ResourceBackup,
                        selectedBackup.MirrorBackup,
                        selectedBackup.TargetBackup
                    );
                }

                UpdateLogFile(
                    selectedBackup.SaveName,
                    selectedBackup.ResourceBackup,
                    selectedBackup.TargetBackup
                );
            }
        }


        /// <summary>
        /// Renvoie la liste de tous les backups (pour lister leurs noms dans l’UI).
        /// </summary>
        public List<Backup> NameList()
        {
            if (!File.Exists(backupListFile))
            {
                File.WriteAllText(backupListFile, string.Empty);
            }

            List<Backup> names = new List<Backup>();
            string jsonString = File.ReadAllText(backupListFile);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);
                names.AddRange(list);
            }
            return names;
        }


        /// <summary>
        /// Supprime un job de sauvegarde de backupList.json.
        /// </summary>
        public void DeleteSave(string backupname)
        {
            List<Backup> backupList = new List<Backup>();
            this.serializeObj = null;

            if (!File.Exists(backupListFile))
            {
                File.WriteAllText(backupListFile, string.Empty);
            }

            string jsonString = File.ReadAllText(backupListFile);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                Backup[] list = JsonConvert.DeserializeObject<Backup[]>(jsonString);
                foreach (var obj in list)
                {
                    if (obj.SaveName != backupname)
                    {
                        backupList.Add(obj);
                    }
                }
            }

            this.serializeObj = JsonConvert.SerializeObject(
                backupList.ToArray(),
                Newtonsoft.Json.Formatting.Indented
            ) + Environment.NewLine;
            File.WriteAllText(backupListFile, this.serializeObj);
        }


        /// <summary>
        /// Lit le JSON Resources\JailApps.json pour connaître la liste des applications bloquées.
        /// </summary>
        public static string[] GetJailApps()
        {
            using StreamReader reader = new StreamReader(@"..\..\..\Resources\JailApps.json");
            string json = reader.ReadToEnd();
            JailAppsFormat[] item_jailapps = JsonConvert.DeserializeObject<JailAppsFormat[]>(json);
            return item_jailapps[0].jailed_apps.Split(',');
        }


        /// <summary>
        /// Vérifie si l’une des applications bloquées (jail apps) tourne actuellement.
        /// </summary>
        /// <returns>
        /// true si au moins un des noms donnés dans blacklist_app est en cours d'exécution ; false sinon.
        /// </returns>
        public static bool CheckSoftware(string[] blacklist_app)
        {
            bool abortSave = false;
            foreach (string App in blacklist_app)
            {
                Process[] ps = Process.GetProcessesByName(App);
                if (ps.Length > 0)
                {
                    abortSave = true;
                    break;
                }
            }
            return abortSave;
        }


        /// <summary>
        /// Définit le format du log (true pour XML, false pour JSON).
        /// </summary>
        public void ModelFormat(bool extension)
        {
            Format = extension;
        }
    }
}
