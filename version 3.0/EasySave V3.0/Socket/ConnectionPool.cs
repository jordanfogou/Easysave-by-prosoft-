using System.Collections.Generic;

namespace EasySaveApp.Socket
{
    public class ConnectionPool
    {
        public List<Connection> Connections { get; set; }
        private static ConnectionPool _instance = null;

        public ConnectionPool()
        {
            Connections = new();
        }

        public static ConnectionPool GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ConnectionPool();
            }
            return _instance;
        }

        public void AddConnection(Connection connection)
        {
            Connections.Add(connection);
        }

        public void RemoveConnection(Connection connection)
        {
            Connections.Remove(connection);
        }
    }
}
