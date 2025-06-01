namespace EasySaveApp.model
{
    // La classe DataLogs est maintenant public
    public class DataLogs
    {
        // D�claration des propri�t�s publiques
        public string SourceLog { get; set; }
        public string TargetLog { get; set; }
        public string SaveNameLog { get; set; }
        public string BackupDateLog { get; set; }
        public string TransactionTimeLog { get; set; }
        public long TotalSizeLog { get; set; }
    }
}
