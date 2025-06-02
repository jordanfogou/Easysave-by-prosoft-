using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EasySaveApp.model
{
    public class Backup
    {
        // Déclaration des propriétés utilisées pour enregistrer les informations d’un job de sauvegarde
        public string ResourceBackup { get; set; }
        public string TargetBackup { get; set; }
        public string SaveName { get; set; }
        public string Type { get; set; }
        public string MirrorBackup { get; set; }
        public float Progress { get; set; }
        public long TotalSize { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // NOUVELLES PROPRIÉTÉS POUR LE CRYPTAGE (depuis v2.0)
        // ─────────────────────────────────────────────────────────────────────────
        public string[] ExtensionsToEncrypt { get; set; }
        public string EncryptionPassword { get; set; }

        /// <summary>
        /// Constructeur sans paramètres (initialise les champs de cryptage par défaut).
        /// </summary>
        public Backup()
        {
            ExtensionsToEncrypt = new string[0];
            EncryptionPassword = string.Empty;
        }

        /// <summary>
        /// Constructeur complet avec tous les paramètres (nom, source, cible, type, miroir, extensions à chiffrer, mot de passe, progression).
        /// </summary>
        public Backup(
            string saveName,
            string source,
            string target,
            string type,
            string mirror,
            string[] extensionsToEncrypt,
            string encryptionPassword,
            float progress = 0
        )
        {
            SaveName = saveName;
            ResourceBackup = source;
            TargetBackup = target;
            Type = type;
            MirrorBackup = mirror;
            ExtensionsToEncrypt = extensionsToEncrypt ?? new string[0];
            EncryptionPassword = encryptionPassword ?? string.Empty;
            Progress = progress;
        }

        /// <summary>
        /// Constructeur de compatibilité (5 paramètres) – initialise les nouvelles propriétés à leurs valeurs par défaut.
        /// </summary>
        public Backup(
            string saveName,
            string source,
            string target,
            string type,
            string mirror
        ) : this(
                saveName,
                source,
                target,
                type,
                mirror,
                new string[0],   // Extensions à chiffrer vides
                string.Empty,    // Mot de passe vide
                0                // Progression initiale à 0
            )
        {
        }
    }

    public class BackupWithProgress : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangedEventHandler? IsProcessingPrioritizedFileChanged;
        public event PropertyChangedEventHandler? IsTransferingHeavyFileChanged;

        private string _SaveName;
        private float _Progress;
        private ManualResetEvent _ResetEvent;
        private bool _IsSuspended;
        private bool _IsAborted;
        private bool _IsRunning;
        private bool _IsProcessingPrioritizedFile;
        private bool _IsTransferingHeavyFile;

        public string SaveName { get { return _SaveName; } set { _SaveName = value; OnPropertyChanged(); } }
        public float Progress { get { return _Progress; } set { _Progress = value; OnPropertyChanged(); } }
        public ManualResetEvent ResetEvent { get { return _ResetEvent; } set { _ResetEvent = value; OnPropertyChanged(); } }
        public bool IsSuspended { get { return _IsSuspended; } set { _IsSuspended = value; OnPropertyChanged(); } }
        public bool IsAborted { get { return _IsAborted; } set { _IsAborted = value; OnPropertyChanged(); } }
        public bool IsRunning { get { return _IsRunning; } set { _IsRunning = value; OnPropertyChanged(); } }
        public bool IsProcessingPrioritizedFile { get { return _IsProcessingPrioritizedFile; } set { _IsProcessingPrioritizedFile = value; OnPropertyChanged(); OnIsProcessingPrioritizedFileChanged(); } }
        public bool IsTransferingHeavyFile { get { return _IsTransferingHeavyFile; } set { _IsTransferingHeavyFile = value; OnPropertyChanged(); OnIsTransferingHeavyFileChanged(); } }

        public BackupWithProgress(string saveName, float progress, ManualResetEvent re)
        {
            _SaveName = saveName;
            _Progress = progress;
            _ResetEvent = re;
            _IsSuspended = false;
            _IsAborted = false;
            _IsRunning = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnIsProcessingPrioritizedFileChanged()
        {
            IsProcessingPrioritizedFileChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProcessingPrioritizedFile)));
        }

        protected void OnIsTransferingHeavyFileChanged()
        {
            IsTransferingHeavyFileChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTransferingHeavyFile)));
        }
    }
}
