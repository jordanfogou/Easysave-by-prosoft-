namespace EasySaveApp.model
{
    class DataLogs
    {
        //Declaration of the properties that are used for the program log file
        public string ?SourceLog { get; set; }
        public string ?TargetLog { get; set; }
        public string ?SaveNameLog { get; set; }
        public string ?BackupDateLog { get; set; }
        public string ?TransactionTimeLog { get; set; }
        public long ?TotalSizeLog { get; set; }
        public string ?CryptTime { get; set; }


    }
}