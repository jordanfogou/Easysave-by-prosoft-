namespace EasySaveApp.model
{
    // AVANT : class Backup { … }
    // APRÈS : on rend la classe publique
    public class Backup
    {
        // Propriétés existantes
        public string ResourceBackup { get; set; }
        public string TargetBackup { get; set; }
        public string SaveName { get; set; }
        public string Type { get; set; }
        public string MirrorBackup { get; set; }

        // Nouveaux champs pour le chiffrement
        public string[] ExtensionsToEncrypt { get; set; }
        public string EncryptionPassword { get; set; }

        // Constructeur mis à jour pour accepter 7 arguments
        public Backup(
            string saveName,
            string source,
            string target,
            string type,
            string mirror,
            string[] extensionsToEncrypt,
            string encryptionPassword)
        {
            SaveName = saveName;
            ResourceBackup = source;
            TargetBackup = target;
            Type = type;
            MirrorBackup = mirror;
            ExtensionsToEncrypt = extensionsToEncrypt;
            EncryptionPassword = encryptionPassword;
        }
    }
}
