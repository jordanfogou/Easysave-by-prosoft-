namespace EasySaveApp.model
{
    class DataState
    {
        // Declaration of properties that are used for saving information for the report file in JSON
        public string SaveNameState { get; set; }
        public string BackupDateState { get; set; }
        public bool SaveState { get; set; }
        public string SourceFileState { get; set; }
        public string TargetFileState { get; set; }
        public float TotalFileState { get; set; }
        public long TotalSizeState { get; set; }
        public float ProgressState { get; set; }
        public long FileRestState { get; set; }
        public long TotalSizeRestState { get; set; }

        public DataState(string saveNameState)
        {
            SaveNameState = saveNameState;
        }
    }
}