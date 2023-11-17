using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Cluster_Client.Core.Events;
using Cluster_Client.Model;
using Cluster_Client.ViewModel;
using Cluster_Client.View;
using Microsoft.Win32;

namespace Cluster_Client.Core
{
    public class CoreHandler
    {
        private readonly static CoreHandler _instace = new();
        private Connection _localConnection = new();//The client connection
        private Connection _serverConnection = new();//Connection to serve
        private TcpClient _client = new TcpClient();
        public Connection LocalConnection { get { return _localConnection; } }
        public bool IsConnected { get; private set; }
        public bool IsVideoLoaded { get; private set; }
        public Status ServerDisponibility { get; private set; }
        public int ClientsBefore { get; private set; }
        public string LocalVideoPath { get; private set; }
        public bool IsVideoSended { get; private set; }
        public byte[] Video { get; private set; }
        public string VideoFormat { get; private set; }
        public Video VideoProcessed { get; private set; }
        public byte[] VideoProcessedByte { get; private set; }
        public string VideoProcessedFormat { get; private set; }
        public bool IsVideoReceived { get; private set; }
        public string TemporalyPathVideoReceived { get; private set; }

        //Events for update the UI
        public event EventHandler<ConnectedStatusEventArgs>? ConnectedStatusEvent;
        public event EventHandler<VideoLoadedEventArgs>? VideoLoadedEvent;
        public event EventHandler<ServerDisponibilityEventArgs>? ServerDisponibilityEvent;
        public event EventHandler<ClientsBeforeEventArgs>? ClientsBeforeEvent;
        public event EventHandler<LocalVideoEventArgs>? LocalVideoEvent;
        public event EventHandler<IsVideoSendedEventArgs>? IsVideoSendedEvent;
        public event EventHandler<IsVideoReceivedEventArgs>? IsVideoReceivedEvent;

        private void OnStatusConnectedChanged(ConnectedStatusEventArgs e) => ConnectedStatusEvent?.Invoke(this, e);
        private void OnVideoLoaded(VideoLoadedEventArgs e) => VideoLoadedEvent?.Invoke(this, e);
        private void OnServerDisponibility(ServerDisponibilityEventArgs e) => ServerDisponibilityEvent?.Invoke(this, e);
        private void OnClientsBefore(ClientsBeforeEventArgs e) => ClientsBeforeEvent?.Invoke(this, e);
        private void OnLacalVideo(LocalVideoEventArgs e) => LocalVideoEvent?.Invoke(this, e);
        private void OnVideoSended(IsVideoSendedEventArgs e) => IsVideoSendedEvent?.Invoke(this, e);
        private void OnvideoReceived(IsVideoReceivedEventArgs e) => IsVideoReceivedEvent?.Invoke(this, e);

        private void HandleConnectionStatus(bool value) => OnStatusConnectedChanged(new ConnectedStatusEventArgs(value));
        private void HandleVideoLoaded(bool value) => OnVideoLoaded(new VideoLoadedEventArgs(value));
        private void HandleServerDisponibility(Status value) => OnServerDisponibility(new ServerDisponibilityEventArgs(value));
        private void HandleClientsBefore(int value) => OnClientsBefore(new ClientsBeforeEventArgs(value));
        private void HandleLocalVideo(string value) => OnLacalVideo(new LocalVideoEventArgs(value));
        private void HandleVideoSended(bool value) => OnVideoSended(new IsVideoSendedEventArgs(value));
        private void HandleVideoReceived(bool value1, string value2) => OnvideoReceived(new IsVideoReceivedEventArgs(value1, value2));

        private CoreHandler()
        {
            IsConnected = false;
            ServerDisponibility = Status.Waiting;
            ClientsBefore = 0;
            IsVideoLoaded = false;
            LocalVideoPath = "";
            IsVideoSended = false;
            Video = new byte[0];
            VideoFormat = "";
            VideoProcessedByte = new byte[0];
            VideoProcessedFormat = "";
            IsVideoReceived = false;
            TemporalyPathVideoReceived = "";
        }

        public static CoreHandler Instance
        {
            get { return _instace; }
        }

        public async void ConnectToServerAsync(string ipAddress, int port)
        {
            _serverConnection.IpAddress = ipAddress;
            _serverConnection.Port = port;
            //Obtain our IP
            List<string> ips = new List<string>();
            var entry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in entry.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ips.Add(ip.ToString());
            //Obtain our port
            var localport = FreeTcpPort(ips[0]);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(ips[0]), localport);
            //Define our local connection
            _localConnection.Port = localport;
            _localConnection.IpAddress = localEndPoint.Address.ToString();
            try
            {
                //Connect to server
                await _client.ConnectAsync(IPAddress.Parse(_serverConnection.IpAddress), _serverConnection.Port);
                //If is connected, send our connection data to server 
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
                        Connection = connection,
                    };

                    var json = JsonConvert.SerializeObject(message);
                    _serverConnection.StreamWriter.WriteLine(json);
                    _serverConnection.StreamWriter.Flush();

                    IsConnected = true;
                    HandleConnectionStatus(IsConnected);
                    //Start listen to server
                    Thread thread = new Thread(ListenToServerAsync);
                    thread.Start();
                }
            }
            catch
            { }
        }

        public void ListenToServerAsync()
        {
            while (_client.Connected)
            {
                try
                {
                    var dataFromClient = _serverConnection.StreamReader!.ReadLine();
                    var model = JsonConvert.DeserializeObject<Message>(dataFromClient!);
                    //When message from server is a status
                    if (model!.Type == MessageType.Status)
                    {
                        var json = model.Content as string;
                        var content = JsonConvert.DeserializeObject<Status>(json!);
                        Status ServerDisponibility = content;
                        //Send Server disponibility to UI
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            HandleServerDisponibility(ServerDisponibility);
                        }));
                    }
                    //When message from server is a turn
                    if (model!.Type == MessageType.Turn)
                    {
                        ClientsBefore = int.Parse(model!.Content! as string);
                        //Send clients before to UI
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            HandleClientsBefore(ClientsBefore);
                        }));
                    }

                    //Wjen message from serveer is Processed Data
                    if (model!.Type == MessageType.ProcessedData)
                    {
                        VideoProcessed = model!.Content! as Video;
                        VideoProcessedByte = VideoProcessed.data;
                        VideoProcessedFormat = VideoProcessed.format;
                        if (VideoProcessedByte != null)
                        {
                            SaveTemporalyVideoReceived(VideoProcessedByte, VideoProcessedFormat);
                            IsVideoReceived = true;
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                HandleVideoReceived(IsVideoReceived, TemporalyPathVideoReceived);
                            }));
                        }
                    }
                }
                catch
                { }
            }
        }

        public int FreeTcpPort(string ip)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void StopClientAsync()
        {
            if (File.Exists(TemporalyPathVideoReceived))
            {
                File.Delete(TemporalyPathVideoReceived);
            }
            IsConnected = false;
            _client.Close();
        }

        public void LoadVideo()
        {
            string pathLocalVideo = "";
            //Open the dialog to chose the video
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //Filter the files as videos
            openFileDialog.Filter = "Archivos de video|*.avi;*.wmv;*.mp4;*.mkv;*.mpeg;*.flv;*.3gp;*.mov|Todos los archivos (*.*)|*.*";
            openFileDialog.Title = "Seleccionar Video";
            if (openFileDialog.ShowDialog() == true)
            {
                pathLocalVideo = openFileDialog.FileName;
                if (!string.IsNullOrEmpty(pathLocalVideo))
                {
                    Video = File.ReadAllBytes(pathLocalVideo);//get the video in bytes
                    //string videoName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName); //get the video name
                    VideoFormat = System.IO.Path.GetExtension(openFileDialog.FileName).TrimStart('.').ToLower(); //get the video format

                    if (Video.Length > 0)
                    {
                        IsVideoLoaded = true;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            HandleLocalVideo(pathLocalVideo);
                            HandleVideoLoaded(IsVideoLoaded);
                        }));
                    }
                }
            }
        }

        public void SendVideo()
        {
            _serverConnection.Stream = _client.GetStream();
            _serverConnection.StreamWriter = new StreamWriter(_serverConnection.Stream);
            _serverConnection.StreamReader = new StreamReader(_serverConnection.Stream);

            var connection = new Connection();
            connection.Port = _localConnection.Port;
            connection.IpAddress = _localConnection.IpAddress;

            var video = new Video();
            video.format = VideoFormat;
            video.data = Video;

            var message = new Message
            {
                Type = MessageType.Data,
                Content = JsonConvert.SerializeObject(video),
                Connection = connection,
            };

            var json = JsonConvert.SerializeObject(message);
            _serverConnection.StreamWriter.WriteLine(json);
            _serverConnection.StreamWriter.Flush();

            IsVideoSended = true;
            Application.Current.Dispatcher?.Invoke(new Action(() =>
            {
                HandleVideoSended(IsVideoSended);
            }));
        }

        public void SaveTemporalyVideoReceived(byte[] videoReceivedByte, string videoReceivedFormat)
        {
            string temporalyName = "VideoProcesado."+videoReceivedFormat;
            TemporalyPathVideoReceived = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources",temporalyName);

            if (File.Exists(TemporalyPathVideoReceived))
            {
                File.Delete(TemporalyPathVideoReceived);
            }

            using (FileStream fs = new FileStream(TemporalyPathVideoReceived, FileMode.Create))
            {
                fs.Write(videoReceivedByte, 0, videoReceivedByte.Length);
            }
        }

        public void SaveVideo()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"Archivo {VideoProcessedFormat}|*.{VideoProcessedFormat}";
            saveFileDialog.Title = "Guardar Video";
            saveFileDialog.FileName = "video_procesado";

            if (saveFileDialog.ShowDialog() == true)
            {
                string selectedPath = saveFileDialog.FileName;

                using (FileStream fs = new FileStream(selectedPath, FileMode.Create))
                {
                    fs.Write(VideoProcessedByte, 0, VideoProcessedByte.Length);
                }
            }
        }
    }
}
