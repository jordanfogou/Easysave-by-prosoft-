using System.Net.Sockets;
using System.Text;

namespace EasySaveConsole
{
    public class StateObject // State object for reading client data asynchronously  
    {
        public Socket workSocket = null;// Client socket.  
        public const int BufferSize = 4096;// Size of receive buffer.  
        public byte[] buffer = new byte[BufferSize];// Receive buffer.  
        public StringBuilder sb = new StringBuilder();// Received data string.  
    }
}
