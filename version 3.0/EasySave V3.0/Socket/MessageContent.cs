namespace EasySaveApp.Socket
{
    public enum MessageType
    {
        ConnectionInit,
        BackupList,
        ClientStartTask,
        BackupProgress,
        ClientPauseTask,
        ClientStopTask,
        ClientStopConnection
    }

    public class MessageContent
    {
        public MessageType Type { get; set; }
           
        public string Body { get; set; }

        public MessageContent() { }
    }
}
