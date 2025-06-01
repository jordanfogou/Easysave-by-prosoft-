using System;

namespace EasySaveApp.model
{
    /// <summary>
    /// Classe représentant une configuration de sauvegarde
    /// </summary>
    public class Backup
    {
        /// <summary>
        /// Nom unique de la sauvegarde
        /// </summary>
        public string SaveName { get; set; }

        /// <summary>
        /// Chemin du dossier source
        /// </summary>
        public string ResourceBackup { get; set; }

        /// <summary>
        /// Chemin du dossier cible
        /// </summary>
        public string TargetBackup { get; set; }

        /// <summary>
        /// Type de sauvegarde ("full" ou "diff")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Chemin du dossier miroir (utilisé uniquement pour les sauvegardes différentielles)
        /// </summary>
        public string MirrorBackup { get; set; }

        /// <summary>
        /// Tableau des extensions de fichiers à chiffrer (ex: [".txt", ".bmp", ".docx"])
        /// </summary>
        public string[] ExtensionsToEncrypt { get; set; }

        /// <summary>
        /// Mot de passe utilisé pour le chiffrement
        /// </summary>
        public string EncryptionPassword { get; set; }

        /// <summary>
        /// Constructeur complet avec tous les paramètres
        /// </summary>
        public Backup(string saveName, string resourceBackup, string targetBackup, string type,
                     string mirrorBackup, string[] extensionsToEncrypt, string encryptionPassword)
        {
            SaveName = saveName;
            ResourceBackup = resourceBackup;
            TargetBackup = targetBackup;
            Type = type;
            MirrorBackup = mirrorBackup;
            ExtensionsToEncrypt = extensionsToEncrypt ?? new string[0];
            EncryptionPassword = encryptionPassword ?? string.Empty;
        }

        /// <summary>
        /// Constructeur par défaut (nécessaire pour la désérialisation JSON)
        /// </summary>
        public Backup()
        {
            SaveName = string.Empty;
            ResourceBackup = string.Empty;
            TargetBackup = string.Empty;
            Type = "full";
            MirrorBackup = string.Empty;
            ExtensionsToEncrypt = new string[0];
            EncryptionPassword = string.Empty;
        }
    }
}