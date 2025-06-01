using System;

namespace EasySaveApp.model
{
    /// <summary>
    /// Classe repr�sentant une configuration de sauvegarde
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
        /// Chemin du dossier miroir (utilis� uniquement pour les sauvegardes diff�rentielles)
        /// </summary>
        public string MirrorBackup { get; set; }

        /// <summary>
        /// Tableau des extensions de fichiers � chiffrer (ex: [".txt", ".bmp", ".docx"])
        /// </summary>
        public string[] ExtensionsToEncrypt { get; set; }

        /// <summary>
        /// Mot de passe utilis� pour le chiffrement
        /// </summary>
        public string EncryptionPassword { get; set; }

        /// <summary>
        /// Constructeur complet avec tous les param�tres
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
        /// Constructeur par d�faut (n�cessaire pour la d�s�rialisation JSON)
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