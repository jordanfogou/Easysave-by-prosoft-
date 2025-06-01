using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EasySaveApp.viewmodel;

namespace EasySaveApp.view
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel viewmodel;
        private string language = "fr";
        private bool formatmw = false;

        public MainWindow()
        {
            viewmodel = new ViewModel();
            InitializeComponent();
            ShowListBox();
        }

        private void ButtonClickFr(object sender, RoutedEventArgs e)
        {
            result.Text = "";
            language = "fr";
            ChooseLanguage(language);
        }

        private void ButtonClickEn(object sender, RoutedEventArgs e)
        {
            result.Text = "";
            language = "en";
            ChooseLanguage(language);
        }

        private void ChooseLanguage(string language)
        {
            var dict = new ResourceDictionary();
            if (language == "fr")
            {
                dict.Source = new Uri("Resources\\fr-FR.xaml", UriKind.Relative);
            }
            else
            {
                dict.Source = new Uri("Resources\\en-GB.xaml", UriKind.Relative);
            }

            // On vide d’abord les dictionnaires précédents pour éviter les doublons
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void ButtonAddSaveClick(object sender, RoutedEventArgs e)
        {
            string saveName = name_save.Text.Trim();
            string sourceDir = SoureDir.Text.Trim();
            string targetDir = TargetDir.Text.Trim();
            string mirrorDir = MirrorDir.Text.Trim();

            // ────────────────────────────────────────────────────────────
            // Récupère la liste d’extensions saisie, ex. ".bmp;.txt;.log;.docx"
            // Si l’utilisateur n’a rien saisi, on prend un tableau vide par défaut
            string rawExt = ExtensionsTextBox.Text.Trim();
            string[] extensionsToEncrypt = Array.Empty<string>();
            if (!string.IsNullOrWhiteSpace(rawExt))
            {
                extensionsToEncrypt = rawExt
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(ext => ext.ToLowerInvariant())
                    .ToArray();
            }

            // Récupère le mot de passe entré (si vide, on le passe vide)
            string encryptionPassword = PasswordBox.Password;
            // ────────────────────────────────────────────────────────────

            if (mirror_button.IsChecked.Value) // sauvegarde “full”
            {
                if (string.IsNullOrWhiteSpace(saveName)
                    || string.IsNullOrWhiteSpace(sourceDir)
                    || string.IsNullOrWhiteSpace(targetDir))
                {
                    result.Text = (string)FindResource("msg_emptyfield");
                }
                else
                {
                    string type = "full";
                    viewmodel.AddSaveModel(
                        type,
                        saveName,
                        sourceDir,
                        targetDir,
                        "",                    // mirrorDir vide pour full
                        extensionsToEncrypt,
                        encryptionPassword
                    );
                    result.Text = (string)FindResource("msg_saveaddedfull");
                    ShowListBox();
                }
            }
            else if (diff_button.IsChecked.Value) // sauvegarde “diff”
            {
                if (string.IsNullOrWhiteSpace(saveName)
                    || string.IsNullOrWhiteSpace(sourceDir)
                    || string.IsNullOrWhiteSpace(targetDir)
                    || string.IsNullOrWhiteSpace(mirrorDir))
                {
                    result.Text = (string)FindResource("msg_emptyfield");
                }
                else
                {
                    string type = "diff";
                    viewmodel.AddSaveModel(
                        type,
                        saveName,
                        sourceDir,
                        targetDir,
                        mirrorDir,
                        extensionsToEncrypt,
                        encryptionPassword
                    );
                    result.Text = (string)FindResource("msg_saveaddeddiff");
                    ShowListBox();
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
            if (Json_button.IsChecked.Value)
            {
                formatmw = false;
                viewmodel.ModelFormat(formatmw);
            }
        }

        private void SourceResourceClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SoureDir.Text = dialog.FileName;
            }
        }

        private void TargetResourceClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TargetDir.Text = dialog.FileName;
            }
        }

        private void MirrorResourceClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                MirrorDir.Text = dialog.FileName;
            }
        }

        private void ShowListBox()
        {
            Save_work.Items.Clear();
            var names = viewmodel.ListBackup();
            foreach (string name in names)
            {
                Save_work.Items.Add(name);
            }
        }

        private void MirrorButtonChecked(object sender, RoutedEventArgs e)
        {
            // pas d’action supplémentaire pour l’instant
        }

        private void ButtonExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GridMenuMouseDown(object sender, RoutedEventArgs e)
        {
            DragMove();
        }

        private void ButtonStartSaveClick(object sender, RoutedEventArgs e)
        {
            if (Save_work.SelectedItem != null)
            {
                foreach (string item in Save_work.SelectedItems)
                {
                    bool backupSucceeded = viewmodel.LoadBackup(item);
                    result.Text = backupSucceeded
                        ? (string)FindResource("msg_successave")
                        : (string)FindResource("msg_failsave");
                }
            }
        }

        private void OpenBlacklist(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"..\..\..\Resources\JailApps.json");
        }

        private void ButtonSelectAll(object sender, RoutedEventArgs e)
        {
            Save_work.SelectAll();
        }

        private void ButtonDeleteSave(object sender, RoutedEventArgs e)
        {
            if (Save_work.SelectedItem != null)
            {
                foreach (string item in Save_work.SelectedItems)
                {
                    viewmodel.DeleteBackup(item);
                }
                ShowListBox();
            }
        }
    }
}
