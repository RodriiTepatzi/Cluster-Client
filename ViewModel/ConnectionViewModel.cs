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

namespace Cluster_Client.ViewModel
{
    public class ConnectionViewModel : BaseViewModel
    {
        private CoreHandler _coreHandler;

        private ICommand _executeConnectToServerCommand;
        private ICommand _executeCloseWindowCommand;

        private string _ipAddress;
        private int _port;
        private bool _isConnected;
        private string _messagesPath;
        private string _messages;

        public ICommand ExecuteConnectToServerCommand
        {
            get { return _executeConnectToServerCommand; }
        }

        public ICommand ExecuteCloseWindowCommand
        {
            get { return _executeCloseWindowCommand; }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
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

        public ConnectionViewModel()
        {
            _coreHandler = CoreHandler.Instance;
            _ipAddress = "";
            _port = 6969;
            _messagesPath = "Resources/Messages.xaml";
            _messages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MessagesPath);

            _executeConnectToServerCommand = new CommandViewModel(ConnectAction);
            _executeCloseWindowCommand = new CommandViewModel(CloseWindowAction);
            _coreHandler.ConnectedStatusEvent += _coreHandler_ConnectedStatusEvent;
        }

        private void _coreHandler_ConnectedStatusEvent(object? sender, ConnectedStatusEventArgs e)
        {
            IsConnected = e.Connected;

            if (IsConnected)
            {
                var newWindow = new VideoActionsView();
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = newWindow;
                    newWindow.Show();
                }));
            }

        }
        private void ConnectAction(object sender)
        {
            if (Port > 0 && !string.IsNullOrEmpty(IpAddress))
            {
                
                using (FileStream fs = new FileStream(Messages, FileMode.Open))
                {
                    ResourceDictionary resourceDic = (ResourceDictionary)XamlReader.Load(fs);
                    StackPanel conMessage = (StackPanel)resourceDic["ConnectingMessage"];
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        StackPanel Content = (StackPanel)Application.Current.MainWindow.FindName("Content");
                        Content.Children.Clear();
                        Content.Children.Add(conMessage);
                    }));
                }

                CoreHandler.Instance.ConnectToServerAsync(IpAddress, Port);

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
    }
}
