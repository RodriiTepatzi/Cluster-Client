using Cluster_Client.Core;
using Cluster_Client.Core.Events;
using Cluster_Client.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Markup;
using System.Net;
using Cluster_Client.Model;

namespace Cluster_Client.ViewModel
{
    public class VideoActionsViewModel : BaseViewModel
    {
        private CoreHandler _coreHandler;
        private ICommand _executeCloseWindowCommand;
        private ICommand _executeLoadVideoCommand;
        private ICommand _executePSLocalVideoCommand;

        private bool _isConnected;
        private bool _isVideoLoaded;
        private Status _serverDisponibility;
        private int _clientsBefore;
        private string _messagesPath;
        private string _messages;
        private Grid _allContent;
        private UIElement _defaultContent;
        private List<RowDefinition> _defaultRowDefinitions;
        private byte[] _localVideo;
        private bool _isLocalVideoStop;

        public ICommand ExecuteCloseWindowCommand
        {
            get { return _executeCloseWindowCommand; }
        }
        public ICommand ExecuteLoadVideoCommand
        { 
            get { return _executeLoadVideoCommand; } 
        }
        public ICommand ExecutePSLocalVideoCommand
        {
            get { return _executePSLocalVideoCommand; }
        }
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }
        public bool IsVideoLoaded
        {
            get { return _isVideoLoaded; }
            set
            {
                _isVideoLoaded = value;
                OnPropertyChanged(nameof(IsVideoLoaded));
            }
        }
        public Status ServerDisponibility
        {
            get { return _serverDisponibility; }
            set
            {
                _serverDisponibility = value;
                OnPropertyChanged(nameof(ServerDisponibility));
            }
        }
        public int ClientsBefore
        {
            get { return _clientsBefore; }
            set
            {
                _clientsBefore = value;
                OnPropertyChanged(nameof(ClientsBefore));
            }
        }
        public string MessagesPath
        {
            get { return _messagesPath; }
            set
            {
                _messagesPath = value;
                OnPropertyChanged(nameof(MessagesPath));
            }
        }
        public string Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }
        public byte[] LocalVideo
        {
            get { return _localVideo; }
            set
            {
                _localVideo = value;
                OnPropertyChanged(nameof(LocalVideo));
            }
        }
        public bool IsLocalVideoStop
        {
            get { return _isLocalVideoStop; }
            set
            {
                _isLocalVideoStop = value;
                OnPropertyChanged(nameof(IsLocalVideoStop));
            }
        }

        public VideoActionsViewModel(Grid allContent)
        {
            _isConnected = true;

            _coreHandler = CoreHandler.Instance;
            _executeCloseWindowCommand = new CommandViewModel(CloseWindowAction);
            _executeLoadVideoCommand = new CommandViewModel(LoadVideoAction);
            _executePSLocalVideoCommand = new CommandViewModel(PSLocalVideo);

            _coreHandler.ConnectedStatusEvent += _coreHandler_ConnectedStatusEvent;
            _coreHandler.ServerDisponibilityEvent += _coreHandler_ServerDisponibilityEvent;
            _coreHandler.ClientsBeforeEvent += _coreHandler_ClientsBeforeEvent;
            _coreHandler.VideoLoadedEvent += _coreHandler_VideoLoadedEvent;
            _coreHandler.LocalVideoEvent += _coreHandler_LocalVideoEvent;
            
            _messagesPath = "Resources/Messages.xaml";
            _messages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MessagesPath);

            _allContent = allContent;
            _defaultContent = allContent.Children.Cast<UIElement>().FirstOrDefault();
            _defaultRowDefinitions = allContent.RowDefinitions.ToList();

            HandleContentChange();
        }

        private void _coreHandler_LocalVideoEvent(object? sender, LocalVideoEventArgs e)
        {
            LocalVideo = e.LocalVideo;
            ShowLocalVideo(LocalVideo);
        }

        private void ShowLocalVideo(byte[] videoBytes)
        {
            using (MemoryStream stream = new MemoryStream(videoBytes))
            {
                Uri videoUri = new Uri(stream.GetHashCode().ToString());
                MediaElement mediaElementVideoLoaded = (MediaElement)Application.Current.MainWindow.FindName("mediaElementVideoLoaded");
                mediaElementVideoLoaded.Source = videoUri;
                mediaElementVideoLoaded.Stop();
                IsLocalVideoStop = true;
            }
        }

        private void _coreHandler_VideoLoadedEvent(object? sender, VideoLoadedEventArgs e)
        {
            IsVideoLoaded = e.IsVideoLoaded;

            if (IsVideoLoaded)
            {
                Button btnPSbtnPSLoadVideo = (Button)Application.Current.MainWindow.FindName("btnPSLoadVideo");
                btnPSbtnPSLoadVideo.Visibility = Visibility.Visible;
                Button btnSendVideo = (Button)Application.Current.MainWindow.FindName("btnSendVideo");
                btnSendVideo.Visibility = Visibility.Visible;
            }
        }

        private void ChangeGridContent(UIElement newContent)
        {
            _allContent.Children.Clear();

            if (newContent is TextBlock textBlock)
            {
                _allContent.RowDefinitions.Clear();
                _allContent.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(textBlock, 0);
                _allContent.Children.Add(textBlock);
            }
            else
            {
                _allContent.RowDefinitions.Clear();
                foreach (var rowDefinition in _defaultRowDefinitions)
                {
                    _allContent.RowDefinitions.Add(rowDefinition);
                }
                _allContent.Children.Add(newContent);
            }
        }

        private void HandleContentChange()
        {
            ResourceDictionary resourceDic;
            using (FileStream fs = new FileStream(Messages, FileMode.Open))
            {
                resourceDic = (ResourceDictionary)XamlReader.Load(fs);
            }
            TextBlock rMessage = (TextBlock)resourceDic["ResponseMessage"];
            ChangeGridContent(rMessage);
        }

        private void _coreHandler_ConnectedStatusEvent(object? sender, ConnectedStatusEventArgs e)
        {
            IsConnected = e.Connected;
        }

        private void _coreHandler_ClientsBeforeEvent(object? sender, ClientsBeforeEventArgs e)
        {
            ClientsBefore = e.clientsBefore;
        }

        private void _coreHandler_ServerDisponibilityEvent(object? sender, ServerDisponibilityEventArgs e)
        {
            ServerDisponibility = e.serverDisponibility;

            if (ServerDisponibility == Status.Waiting)
            {
                ResourceDictionary resourceDic;
                using (FileStream fs = new FileStream(Messages, FileMode.Open))
                {
                    resourceDic = (ResourceDictionary)XamlReader.Load(fs);
                }
                TextBlock wMessage = (TextBlock)resourceDic["WaitingMessage"];
                ChangeGridContent(wMessage);
            } 
            
            else if (ServerDisponibility == Status.Busy)
            {
                ResourceDictionary resourceDic;
                using (FileStream fs = new FileStream(Messages, FileMode.Open))
                {
                    resourceDic = (ResourceDictionary)XamlReader.Load(fs);
                }
                TextBlock bMessage = (TextBlock)resourceDic["BusyMessage"];
                ChangeGridContent(bMessage);
            } else if (ServerDisponibility == Status.Ready)
            {

            }
        }

        private void CloseWindowAction(object sender)
        {
            if (IsConnected)
            {
                CoreHandler.Instance.StopClientAsync();
            }
            Application.Current.Shutdown();
        }

        private void LoadVideoAction(object sender)
        {
            CoreHandler.Instance.LoadVideo();
        }

        private void PSLocalVideo(object sender)
        {
            MediaElement mediaElementVideoLoaded = (MediaElement)Application.Current.MainWindow.FindName("mediaElementVideoLoaded");

            if (IsLocalVideoStop)
            {
                mediaElementVideoLoaded.Play();
                IsLocalVideoStop = false;
            }
            else
            {
                mediaElementVideoLoaded.Stop();
                IsLocalVideoStop = true;
            }
        }
    }
}
