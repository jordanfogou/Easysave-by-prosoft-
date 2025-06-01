using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using EasySaveApp.model;
using System.Linq;
using EasySaveApp;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EasySaveApp.view
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EasySaveApp.viewmodel.ViewModel viewmodel;
        
        public string language = "fr";
        private bool formatmw = false;
        public IEnumerable<BackupWithProgress> backupsWithProgress { get { return viewmodel._backupsWithProgress; } }
        public ManualResetEvent REProcessingPrioritizedFiles = new ManualResetEvent(true);
        public ManualResetEvent RETransferingHeavyFile = new ManualResetEvent(true);
        public ManualResetEvent REBusinessSoftwareOpened = new ManualResetEvent(true);
        System.Threading.Timer CheckForBusinessAppsTimer;

        public MainWindow()
        {
            Process Proc_EnCours = Process.GetCurrentProcess(); //Obtains the current process of the application

            Process[] Les_Proc = Process.GetProcesses(); //Collection of currently launched processes

            foreach (Process Processus in Les_Proc)
            {
                if (Proc_EnCours.Id != Processus.Id) ////If the IDs are different but of the same name
                {
                    if (Proc_EnCours.ProcessName == Processus.ProcessName)
                    {
                        MessageBox.Show("The EasySeave software is already running.");
                        this.Close();
                    }
                }
            }

            CheckForBusinessAppsTimer = new(CheckForBusinessSoftwares, null, 0, 1000);

            viewmodel = EasySaveApp.viewmodel.ViewModel.getInstance();
            InitializeComponent();
            ShowListBox();
        }

        public bool IsBusinessSoftwareOpened()
        {
            string[] businessApps = Model.GetJailApps();

            bool isBusinessAppOpened = false;
            foreach (string App in businessApps)
            {
                Process[] ps = Process.GetProcessesByName(App);
                if (ps.Length > 0)
                {
                    isBusinessAppOpened = true;
                }
            }
            return isBusinessAppOpened;
        }

        public void CheckForBusinessSoftwares(object? state)
        {
            if (IsBusinessSoftwareOpened())
            {
                REBusinessSoftwareOpened.Reset();
            }
            else
            {
                REBusinessSoftwareOpened.Set();
            }
        }

        private void ButtonClickFr(object sender, RoutedEventArgs e)//Function to translate the software into French
        {
            result.Text = "";
            language = "fr";
            ChooseLanguage(language);
        }

        private void ButtonClickEn(object sender, RoutedEventArgs e)//Function to translate the software into English
        {
            result.Text = "";
            language = "en";
            ChooseLanguage(language);
        }

        private void ChooseLanguage(string language)//Function that allows you to choose the language of the software
        {
            ResourceDictionary dict = new ResourceDictionary();
            if (language == "fr")
            {
                dict.Source = new Uri("Resources\\fr-FR.xaml", UriKind.Relative);
            }
            else
            {
                dict.Source = new Uri("Resources\\en-GB.xaml", UriKind.Relative);
            }
            this.Resources.MergedDictionaries.Add(dict);
        }
        private void ButtonAddSaveClick(object sender, RoutedEventArgs e)//Function that allows the button to add a backup
        {
            string saveName = "";
            string sourceDir = "";
            string targetDir = "";
            string mirrorDir = "";

            saveName = name_save.Text;
            sourceDir = SoureDir.Text;
            targetDir = TargetDir.Text;
            mirrorDir = MirrorDir.Text;

            if (mirror_button.IsChecked.Value) //If the button of the full backup is selected
            {
                if (name_save.Text.Length.Equals(0) || SoureDir.Text.Length.Equals(0) || TargetDir.Text.Length.Equals(0))
                {
                    result.Text = (string)FindResource("msg_emptyfield");
                }
                else
                {
                    string type = "full";
                    viewmodel.AddSaveModel(type, saveName, sourceDir, targetDir, ""); //Function to add the backup

                    result.Text = (string)FindResource("msg_saveaddedfull");

                    ShowListBox();//Function to update the list.
                }

            }
            else if (diff_button.IsChecked.Value)//If the button of the full backup is selected
            {
                if (name_save.Text.Length.Equals(0) || SoureDir.Text.Length.Equals(0) || TargetDir.Text.Length.Equals(0) || MirrorDir.Text.Length.Equals(0))
                {
                    result.Text = (string)FindResource("msg_emptyfield");
                }
                else
                {
                    string type = "diff";
                    viewmodel.AddSaveModel(type, saveName, sourceDir, targetDir, mirrorDir);//Function to add the backup

                    result.Text = (string)FindResource("msg_saveaddeddiff");

                    ShowListBox();//Function to update the list.
                }

            }

        }
        private void XmlLog(object sender, RoutedEventArgs e)
        {
            if (xml_button.IsChecked.Value)
            {
                formatmw = true;
                viewmodel.ModelFormat(formatmw);
            }
        }

        private void JsonLog(object sender, RoutedEventArgs e)
        {
            if ((Json_button.IsChecked.Value))
            {
                formatmw = false;
                viewmodel.ModelFormat(formatmw);
            }
        }


        private void SourceResourceClick(object sender, RoutedEventArgs e)//Function to retrieve the path to the source folder
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //Declaration of the method to open the window to choose the folder path.
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SoureDir.Text = dialog.FileName; //Displays the path in the window text.
            }

        }

        private void TargetResourceClick(object sender, RoutedEventArgs e)//Function to retrieve the path to the destination folder
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //Declaration of the method to open the window to choose the folder path.
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TargetDir.Text = dialog.FileName; //Displays the path in the window text.
            }

        }

        private void MirrorResourceClick(object sender, RoutedEventArgs e)//Function to retrieve the folder path of the mirror backup.
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //Declaration of the method to open the window to choose the folder path.
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                MirrorDir.Text = dialog.FileName; //Displays the path in the window text.
            }

        }

        private void calculateGlobalIsProcessingPrioritizedFilesFlag()
        {
            bool isProcessingPrioritizedFiles = false;
            foreach (BackupWithProgress backupWithProgress in backupsWithProgress)
            {
                if (backupWithProgress.IsProcessingPrioritizedFile)
                {
                    isProcessingPrioritizedFiles = true;
                    break;
                }
            }
            if (isProcessingPrioritizedFiles)
            {
                REProcessingPrioritizedFiles.Reset();
            }
            else
            {
                REProcessingPrioritizedFiles.Set();
            }
        }

        private void calculateGlobalIstransferingHeavyFileFlag()
        {
            bool isTransferingHeavyFiles = false;
            foreach (BackupWithProgress backupWithProgress in backupsWithProgress)
            {
                if (backupWithProgress.IsTransferingHeavyFile)
                {
                    isTransferingHeavyFiles = true;
                    break;
                }
            }
            if (isTransferingHeavyFiles)
            {
                RETransferingHeavyFile.Reset();
            }
            else
            {
                RETransferingHeavyFile.Set();
            }
        }

        private void ShowListBox() //Function that displays the names of the backups in the list.
        {
            viewmodel._backupsWithProgress.Clear();

            List<string> names = viewmodel.ListBackup();
            foreach (string name in names)//Loop that allows you to manage the names in the list.
            {
                BackupWithProgress backupWithProgress = new BackupWithProgress(name, 0, new ManualResetEvent(true));
                backupWithProgress.IsProcessingPrioritizedFileChanged += (object? sender, PropertyChangedEventArgs e) =>
                {
                    calculateGlobalIsProcessingPrioritizedFilesFlag();
                };
                backupWithProgress.IsTransferingHeavyFileChanged += (object? sender, PropertyChangedEventArgs e) =>
                {
                    calculateGlobalIstransferingHeavyFileFlag();
                };

                viewmodel._backupsWithProgress.Add(backupWithProgress); //Function that allows you to insert the names of the backups in the list.
            }
            Save_work.ItemsSource = backupsWithProgress;
        }

        private void ButtonExit(object sender, RoutedEventArgs e)//Function of the button to close the software
        {
            Process.GetCurrentProcess().Kill();//Function that turns off the software
        }

        private void GridMenuMouseDown(object sender, RoutedEventArgs e)//Function that allows you to move the software window.
        {
            DragMove();//Function that allows movement
        }

        private void ButtonStartSaveClick(object sender, RoutedEventArgs e)//Function that launches the backup
        {
            if (Save_work.SelectedItem != null) //Condition that allows to check if the user has selected a backup.
            {
                foreach (BackupWithProgress item in Save_work.SelectedItems)//Loop that allows you to select multiple saves
                {
                    string saveName = item.SaveName.ToString();
                    BackupWithProgress backup = backupsWithProgress.Single(b => b.SaveName == saveName);

                    if (backup.IsRunning)
                    {
                        MessageBox.Show("Sauvegarde déjà en cours d'exécution");
                        break;
                    }
                    
                    if (backup.IsSuspended)
                    {
                        if (backup.IsRunning)
                        {
                            backup.ResetEvent.Set();
                            backup.IsSuspended = false;
                            backup.IsAborted = false;
                        }
                    }
                    else
                    {
                        backup.IsSuspended = false;
                        backup.IsAborted = false;
                        backup.IsRunning = true;

                        Thread t = new(() =>
                        {
                            viewmodel.LoadBackup(backup, language, REProcessingPrioritizedFiles, RETransferingHeavyFile, REBusinessSoftwareOpened, (progress) =>
                            {
                                BackupWithProgress bk = viewmodel._backupsWithProgress.Single(x => x.SaveName == saveName);
                                bk.Progress = progress;
                            });
                        });
                        t.Name = saveName;
                        t.Start();
                    }
                }
            }
        }

        private void OpenBlacklist(object sender, RoutedEventArgs e)//Function that allows the button to open the file of blacklisted software
        {
            Process.Start("notepad.exe", @"Resources\JailApps.json");
        }

        private void OpenCryptExt(object sender, RoutedEventArgs e)//Function that allows the button to open the file of blacklisted software
        {
            Process.Start("notepad.exe", @"Resources\CryptExtension.json");
        }


        private void ButtonSelectAll(object sender, RoutedEventArgs e)
        {
            Save_work.SelectAll();
        }

        private void ButtonUnselectAll(object sender, RoutedEventArgs e)
        {
            Save_work.UnselectAll();
        }

        private void ButtonDeleteSave(object sender, RoutedEventArgs e)//Function that allows the deletion of a backup
        {
            if (Save_work.SelectedItem != null) //Condition that allows to check if the user has selected a backup.
            {
                foreach (BackupWithProgress item in Save_work.SelectedItems)//Loop that allows you to select multiple saves
                {
                    string saveName = item.SaveName;
                    viewmodel.DeleteBackup(saveName);
                }

                ShowListBox();//Function to update the list.
            }
        }

        private void Button_Play_click(object sender, RoutedEventArgs e)
        {
            viewmodel.PlayButton_click();
        }

        public void Button_Pause_click(object sender, RoutedEventArgs e)
        {
            BackupWithProgress backup = ((BackupWithProgress)Save_work.SelectedItem);
            backup.ResetEvent.Reset();
            backup.IsSuspended = true;
            backup.IsRunning = false;
        }
        
        private void Button_Stop_click(object sender, RoutedEventArgs e)
        {
            BackupWithProgress backup = ((BackupWithProgress)Save_work.SelectedItem);
            backup.IsAborted = true;
            viewmodel.StopButton_click();
        }

        private void Open_extensionprio(object sender, RoutedEventArgs e)//Function allowing the button to open the file for priority extensions
        {
            Process.Start("notepad.exe", @"Resources\PriorityExtensions.json");
        }
        private void Open_logs(object sender, RoutedEventArgs e)//Function allowing the button to open the file for priority extensions
        {
            Process.Start("notepad.exe", @"State\state.json");
        }

        private void Button_minimize(object sender, RoutedEventArgs e)//Function to reduce the window.
        {
            WindowState = (WindowState)FormWindowState.Minimized;
        }

        private void Cryptosoft_check_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as System.Windows.Controls.CheckBox;
            viewmodel.IsCryptChecked((bool) checkbox.IsChecked);
        }

        private void OpenMaxSizeFile(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", @"Resources\LimitSize.json");
        }
    }
}
