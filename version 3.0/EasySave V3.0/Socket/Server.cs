using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace EasySaveApp.Socket
{
    public class Server
    {
        private static TcpListener _listener;
        private Thread _serverThread;

        private Semaphore _pool;

        public Server(int maxConnection)
        {
            _serverThread = new Thread(Listen);
            _serverThread.Start();
            _pool = new Semaphore(1, maxConnection);
        }

        private void Listen()
        {
            _listener = new TcpListener(IPAddress.Any, 66);
            _listener.Start();

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Connection connection = new(client);
                ConnectionPool.GetInstance().AddConnection(connection);
                
                connection.thread.Start();
            }
        }
    }
}
