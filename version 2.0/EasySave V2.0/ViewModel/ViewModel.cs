using System.Collections.Generic;
using EasySaveApp.model;

namespace EasySaveApp.viewmodel
{
    public class ViewModel
    {
        private Model model;
        private string[] jail_apps = Model.GetJailApps();

        /// <summary>
        /// Tableau des applications “blacklistées” (jail apps)
        /// </summary>
        public string[] BlackListApp
        {
            get => jail_apps;
            set => jail_apps = value;
        }

        public ViewModel()
        {
            model = new Model();
        }

        /// <summary>
        /// Ajoute une sauvegarde en transmettant :
        ///   - type ("full" ou "diff")
        ///   - saveName : nom de la sauvegarde
        ///   - sourceDir : dossier source
        ///   - targetDir : dossier cible
        ///   - mirrorDir : dossier miroir (vide si full)
        ///   - extensionsToEncrypt : tableau d’extensions (".bmp", ".txt", etc.)
        ///   - encryptionPassword : mot de passe de chiffrement
        /// </summary>
        public void AddSaveModel(
            string type,
            string saveName,
            string sourceDir,
            string targetDir,
            string mirrorDir,
            string[] extensionsToEncrypt,
            string encryptionPassword
        )
        {
            //↴ on stocke d’abord les valeurs dans le Model
            model.SaveName = saveName;
            model.Resource = sourceDir;
            model.TargetResource = targetDir;      // <<—— ici, R majuscule
            model.MirrorResource = mirrorDir;

            //↴ les nouveaux champs pour le chiffrement
            model.ExtensionsToEncrypt = extensionsToEncrypt;
            model.EncryptionPassword = encryptionPassword;

            //↴ on crée l’objet Backup à 7 paramètres (source, target, etc.)
            Backup backup = new Backup(
                saveName,
                sourceDir,
                targetDir,
                type,
                mirrorDir,
                extensionsToEncrypt,
                encryptionPassword
            );

            //↴ on l’ajoute au fichier backupList.json
            model.AddSave(backup);
        }

        /// <summary>
        /// Renvoie la liste des noms de sauvegarde existants (pour la ListBox).
        /// </summary>
        public List<string> ListBackup()
        {
            var nameslist = new List<string>();
            foreach (var obj in model.NameList())
            {
                nameslist.Add(obj.SaveName);
            }
            return nameslist;
        }

        /// <summary>
        /// Lance la sauvegarde nommée backupname si aucun logiciel blacklisté n’est détecté
        /// </summary>
        public bool LoadBackup(string backupname)
        {
            if (!Model.CheckSoftware(BlackListApp))
            {
                model.LoadSave(backupname);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Supprime la sauvegarde nommée backupname
        /// </summary>
        public void DeleteBackup(string backupname)
        {
            model.DeleteSave(backupname);
        }

        /// <summary>
        /// Bascule entre format XML ou JSON pour le fichier de log
        /// </summary>
        public void ModelFormat(bool format)
        {
            model.ModelFormat(format);
        }
    }
}
