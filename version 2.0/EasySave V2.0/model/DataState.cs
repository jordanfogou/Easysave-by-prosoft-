using System;

namespace EasySaveApp.model
{
    public class DataState
    {
        // ─────────────────────────────────────────────────────────────────────────
        // Propriétés utilisées pour le fichier d’état (state.json)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Nom de la sauvegarde (clé pour retrouver l’état correspondant).
        /// </summary>
        public string SaveNameState { get; set; }

        /// <summary>
        /// Date de la dernière mise à jour de l’état (format "dd/MM/yyyy HH:mm:ss").
        /// </summary>
        public string BackupDateState { get; set; }

        /// <summary>
        /// Indique si la sauvegarde est en cours (true) ou terminée (false).
        /// </summary>
        public bool SaveState { get; set; }

        /// <summary>
        /// Chemin complet du fichier source en cours de transfert (peut être null si aucun).
        /// </summary>
        public string? SourceFileState { get; set; }

        /// <summary>
        /// Chemin complet du fichier cible en cours de transfert (peut être null si aucun).
        /// </summary>
        public string? TargetFileState { get; set; }

        /// <summary>
        /// Nombre total de fichiers à traiter pour cette sauvegarde.
        /// On passe en float pour accepter des valeurs JSON comme "0.0".
        /// </summary>
        public float TotalFileState { get; set; }

        /// <summary>
        /// Taille totale (octets) de tous les fichiers à traiter.
        /// </summary>
        public long TotalSizeState { get; set; }

        /// <summary>
        /// Pourcentage d’avancement (de 0 à 100).
        /// </summary>
        public float ProgressState { get; set; }

        /// <summary>
        /// Nombre de fichiers restant à traiter.
        /// </summary>
        public long FileRestState { get; set; }

        /// <summary>
        /// Taille restante (octets) à copier/chiffrer.
        /// </summary>
        public long TotalSizeRestState { get; set; }


        // ─────────────────────────────────────────────────────────────────────────
        // NOUVEAU : Durée du chiffrement du fichier courant (en millisecondes)
        //   •   0   = pas de chiffrement (extension non listée)
        //   •  > 0  = durée du chiffrement en ms
        //   •  < 0  = code d’erreur négatif (-ExitCode) retourné par CryptoSoft
        // ─────────────────────────────────────────────────────────────────────────
        public int EncryptionTimeState { get; set; }


        /// <summary>
        /// Constructeur complet de DataState.
        /// On initialise ici tous les champs non-nullable pour éviter les warnings C#.
        /// </summary>
        public DataState(string saveNameState)
        {
            // Propriétés obligatoires non-nullable
            SaveNameState = saveNameState;
            BackupDateState = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            SaveState = false;

            // Chemins de fichier : peuvent rester null par défaut, la propriété est nullable
            SourceFileState = null;
            TargetFileState = null;

            // Initialisation des compteurs à zéro
            TotalFileState = 0f;
            TotalSizeState = 0L;
            ProgressState = 0f;
            FileRestState = 0L;
            TotalSizeRestState = 0L;

            // Pas de chiffrement par défaut
            EncryptionTimeState = 0;
        }
    }
}
