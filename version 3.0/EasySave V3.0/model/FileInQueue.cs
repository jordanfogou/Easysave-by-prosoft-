using System.IO;

namespace EasySaveApp.model
{
    internal class FileInQueue
    {
        public FileInfo MFileInfo { get; set; }
        public bool IsPrioritized { get; set; }

        public FileInQueue(FileInfo fileInfo, bool isPrioritized)
        {
            MFileInfo = fileInfo;
            IsPrioritized = isPrioritized;
        }
    }
}
