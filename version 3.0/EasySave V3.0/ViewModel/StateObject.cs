using System.Text;

namespace EasySaveApp.ViewModel

{
    public class StateObject// State object for reading client data asynchronously  
    {
        // Client socket.  
        // Size of receive buffer.  
        public const int BufferSize = 4096;
        // Receive buffer.  
        public byte[] buffer = new byte[4096];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
}
