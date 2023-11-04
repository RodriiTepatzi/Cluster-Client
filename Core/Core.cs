using System.IO;
using Cluster_Client.Core.Events;
using Cluster_Client.Model;
using Cluster_Client.ViewModel;
using Cluster_Client.View;
using System.Net;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Client.Core
{
    public class CoreHandler
    {
        private readonly static CoreHandler _instace = new();

        private Connection _localConnection = new();
        private Connection _serverConnection = new();
        private TcpClient _client = new TcpClient();
        private TcpClient _localClient = new TcpClient();
        private TcpClient _currentRemoteClient = new TcpClient();

        public Connection LocalConnection { get { return _localConnection; } }
        public bool IsConnected { get; private set; }
        public string ServerDisponibility { get; private set; }

        private bool _clientClosing = false;

        //Eventos para actualizar la interfaz
        public event EventHandler<ConnectedStatusEventArgs>? ConnectedStatusEvent;
        public event EventHandler<MessageReceivedEventArgs>? MessageReceivedEvent;
        public event EventHandler<ServerDisponibilityEventArgs>? ServerDisponibilityEvent;

        private void OnStatusConnectedChanged(ConnectedStatusEventArgs e) => ConnectedStatusEvent?.Invoke(this, e);
        private void OnMessageReceived(MessageReceivedEventArgs e) => MessageReceivedEvent?.Invoke(this, e);
        private void OnServerDisponibility(ServerDisponibilityEventArgs e) => ServerDisponibilityEvent?.Invoke(this, e);

        private void HandleConnectionStatus(bool value) => OnStatusConnectedChanged(new ConnectedStatusEventArgs(value));
        private void HandleMessageReceived(string value) => OnMessageReceived(new MessageReceivedEventArgs(value));
        private void HandleServerDisponibility(string value) => OnServerDisponibility(new ServerDisponibilityEventArgs(value));

        private CoreHandler()
        {
            IsConnected = false;
            ServerDisponibility = "";
        }

        public static CoreHandler Instance
        {
            get { return _instace; }
        }

        public async void ConnectToServerAsync(string ipAddress, int port)
        {
            _serverConnection.IpAddress = ipAddress;
            _serverConnection.Port = port;

            try
            {
                await _client.ConnectAsync(IPAddress.Parse(_serverConnection.IpAddress), _serverConnection.Port);

                if (_client.Connected)
                {
                    _serverConnection.Stream = _client.GetStream();
                    _serverConnection.StreamWriter = new StreamWriter(_serverConnection.Stream);
                    _serverConnection.StreamReader = new StreamReader(_serverConnection.Stream);

                    var connection = new Connection();

                    connection.Port = _localConnection.Port;
                    connection.IpAddress = _localConnection.IpAddress;

                    var message = new Message
                    {
                        Type = MessageType.User,
                        Content = JsonConvert.SerializeObject(connection),
                        Connection = connection
                    };

                    var json = JsonConvert.SerializeObject(message);

                    _serverConnection.StreamWriter.WriteLine(json);
                    _serverConnection.StreamWriter.Flush();

                    Thread thread = new Thread(ListenToServerAsync);
                    thread.Start();
                }
            }
            catch
            {

            }
        }

        public void ListenToServerAsync()
        {
            while (_client.Connected)
            {
                try
                {
                    var dataFromClient = _serverConnection.StreamReader!.ReadLine();
                    var model = JsonConvert.DeserializeObject<Message>(dataFromClient!);

                    if (model!.Type == MessageType.Status)
                    {
                        ServerDisponibility = model!.Content! as string;

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            HandleServerDisponibility(ServerDisponibility);
                        }));
                    }
                }
                catch
                {

                }
            }
        }
    }

}
