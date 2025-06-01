using EasySaveApp.model;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace EasySaveApp.Socket
{
    public class Connection : IDisposable
    {
        private bool disposedValue;

        public TcpClient? connection { get; set; }
        public Thread? thread = null;


        public Connection(TcpClient client)
        {
            connection = client;
            thread = new(() =>
            {
                this.HandleConnection();
            });
        }

        internal void HandleConnection()
        {
            Byte[] bytes = new byte[1024];
            String? data = null;

            // Send list of
                var message = File.ReadAllText(System.Environment.CurrentDirectory + @"\Works\backupList.json");
                var viewModel = EasySaveApp.viewmodel.ViewModel.getInstance();

                var msgToSend = JsonSerializer.SerializeToUtf8Bytes(new MessageContent { Type = MessageType.ConnectionInit, Body = message });

            if (connection != null)
            {
                connection.GetStream().Write(msgToSend);
            }

            while (true)
            {
                int i;
                try
                {
                    while (connection != null && connection.Connected && (i = connection.GetStream().Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        var msg = JsonSerializer.Deserialize<MessageContent>(data);
                        string BackupName = msg.Body;
                        BackupWithProgress backup = viewModel._backupsWithProgress.Single(b => b.SaveName == BackupName);

                        switch (msg?.Type)
                        {
                            case MessageType.ClientStartTask:
                                if (backup.IsSuspended)
                                {
                                    backup.ResetEvent.Set();
                                    backup.IsSuspended = false;
                                    backup.IsAborted = false;
                                }
                                else
                                {
                                    backup.IsSuspended = false;
                                    backup.IsAborted = false;
                                    backup.IsRunning = true;

                                    new Thread(() =>
                                    {
                                        viewModel.LoadBackup(backup, "en", new ManualResetEvent(true), new ManualResetEvent(true), new ManualResetEvent(true), (progress) =>
                                        {
                                            backup.Progress = progress;

                                            var msgToSend = JsonSerializer.SerializeToUtf8Bytes(new MessageContent { Type = MessageType.BackupProgress, Body = progress.ToString().Replace(",", ".") });

                                            if (connection != null && connection.Connected)
                                            {
                                                connection.GetStream().Write(msgToSend);
                                            }
                                        });
                                    }).Start();
                                }
                                break;

                            case MessageType.ClientPauseTask:
                                if (backup.IsRunning)
                                {
                                    backup.ResetEvent.Reset();
                                    backup.IsSuspended = true;
                                    backup.IsRunning = false;
                                }
                                break;

                            case MessageType.ClientStopTask:
                                backup.IsAborted = true;
                                backup.IsRunning = false;
                                break;

                            case MessageType.ClientStopConnection:
                                ConnectionPool.GetInstance().RemoveConnection(this);
                                connection.GetStream().Close();
                                connection.Close();
                                connection.Dispose();
                                connection = null;
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[Client Connection] Error : {e.Message}");
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~Connection()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
