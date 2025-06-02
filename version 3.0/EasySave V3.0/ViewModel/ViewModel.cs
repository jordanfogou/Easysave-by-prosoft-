using System;
using EasySaveApp.model;
using System.Collections.Generic;
using MessageBox = System.Windows.MessageBox;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using EasySaveApp.Socket;

namespace EasySaveApp.viewmodel
{
    public class ViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private Model model;
        string[] jail_apps = Model.GetJailApps();
        public string[] BlackListApp { get => jail_apps; set => jail_apps = value; }
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public ObservableCollection<BackupWithProgress> _backupsWithProgress = new ObservableCollection<BackupWithProgress>();

        private List<string> nameslist;
        public List<Backup> backups;

        public ViewModel()
        {
            model = new Model();

            Server server = new Server(3);
        }

        private static ViewModel? _viewModel;
        public static ViewModel getInstance()
        {
            if (_viewModel == null)
            {
                _viewModel = new ViewModel();
            }
            return _viewModel;
        }


        public void AddSaveModel(string type, string saveName, string sourceDir, string targetDir, string mirrorDir)
        {
            model.SaveName = saveName;

            // Pour la v3.0, on utilise des valeurs par défaut pour le cryptage
            // Tu pourras les rendre configurables plus tard dans l'interface
            string[] extensionsToEncrypt = new string[0]; // Vide par défaut
            string encryptionPassword = string.Empty; // Vide par défaut

            // Si tu veux utiliser les extensions du fichier CryptExtension.json :
            // string[] extensionsToEncrypt = GetExtensionsFromCryptJson();

            Backup backup = new Backup(saveName, sourceDir, targetDir, type, mirrorDir,
                                       extensionsToEncrypt, encryptionPassword);
            model.AddSave(backup);
        }


        public List<string> ListBackup()//Function that lets you know the lists of the names of the backups.
        {

            List<string> nameslist = new List<string>();
            foreach (var obj in model.NameList())
            {
                nameslist.Add(obj.SaveName);
            }
            return nameslist;
        }

        public void LoadBackup(BackupWithProgress backup, string language, ManualResetEvent REProcessingPrioritizedFiles, ManualResetEvent RETransferingHeavyFile, ManualResetEvent REBusinessSoftwareOpened, Action<float> progressChangeFunction)//Function that allows you to load the backups that were selected by the user.
        {
            if (Model.CheckSoftware(BlackListApp))//If a program is in the blacklist we do not start the backup.
            {
                if (language == "fr")
                {
                    MessageBox.Show("ECHEC DE SAUVEGARDE \n" +
                        "ERREUR N°1 : LOGICIEL BLACKLIST \n" +
                        "EN COURS D'EXECUTION");

                }
                else
                {
                    MessageBox.Show("BACKUP FAILURE \n" +
                        "ERROR N°1 : BLACKLIST SOFTWARE\n" +
                        "IN PROGRESS");
                }
                backup.IsRunning = false;
            }
            else
            {
                model.LoadSave(backup, REProcessingPrioritizedFiles, RETransferingHeavyFile, REBusinessSoftwareOpened, progressChangeFunction);//Function that launches backups


                backup.IsRunning = false;
                if (!backup.IsAborted)
                {
                    if (language == "fr")
                    {
                        MessageBox.Show("SAUVEGARDE REUSSIE ");
                    }
                    else
                    {
                        MessageBox.Show("SUCCESSFUL BACKUP ");
                    }
                }
                else
                {
                    if (language == "fr")
                    {
                        MessageBox.Show("SAUVEGARDE ABORTEE ");
                    }
                    else
                    {
                        MessageBox.Show("BACKUP ABORTED ");
                    }
                }
            }
        }

        public void IsCryptChecked(bool state)
        {
            model.isCheck = state;
        }
                
        

        public void DeleteBackup(string backupname)//Function that allows you to delete the backups that were selected by the user.
        {
            model.DeleteSave(backupname);
        }

        public void ModelFormat(bool format)
        {
            model.ModelFormat(format);
        }

        public string Check_buttonStatus()
        {

            return model.StatusButton;
        }

        public void PlayButton_click()
        {
            model.Play_click();
        }
        public void PauseButton_click()
        {
            model.Pause_click();
        }
        public void StopButton_click()
        {
            model.Stop_click();
        }
    }
}
