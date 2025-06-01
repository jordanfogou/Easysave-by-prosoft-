using System;

namespace EasySaveApp.model
{
    /// <summary>
    /// Classe représentant une entrée de log pour les sauvegardes
    /// </summary>
    public class DataLogs
    {
        /// <summary>
        /// Nom de la sauvegarde
        /// </summary>
        public string SaveNameLog { get; set; }

        /// <summary>
        /// Chemin complet du fichier/dossier source
        /// </summary>
        public string SourceLog { get; set; }

        /// <summary>
        /// Chemin complet du fichier/dossier cible
        /// </summary>
        public string TargetLog { get; set; }

        /// <summary>
        /// Date et heure de la sauvegarde (format "dd/MM/yyyy HH:mm:ss")
        /// </summary>
        public string BackupDateLog { get; set; }

        /// <summary>
        /// Taille totale en octets
        /// </summary>
        public long TotalSizeLog { get; set; }

        /// <summary>
        /// Temps de transfert (format "HH:MM:SS.MS")
        /// </summary>
        public string TransactionTimeLog { get; set; }

        /// <summary>
        /// Temps de cryptage en millisecondes
        /// 0 = pas de cryptage
        /// > 0 = temps de cryptage en ms
        /// < 0 = code erreur
        /// </summary>
        public int EncryptionTimeLog { get; set; }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public DataLogs()
        {
            SaveNameLog = string.Empty;
            SourceLog = string.Empty;
            TargetLog = string.Empty;
            BackupDateLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            TotalSizeLog = 0;
            TransactionTimeLog = "00:00:00.00";
            EncryptionTimeLog = 0;
        }
    }
}