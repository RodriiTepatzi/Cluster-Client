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


        public ConnectionViewModel()
        {
            _coreHandler = CoreHandler.Instance;
            _executeConnectToServerCommand = new CommandViewModel(ConnectAction);
            _executeCloseWindowCommand = new CommandViewModel(CloseWindowAction);
            _ipAddress = "";
            _port = 6969;

            _coreHandler.ConnectedStatusEvent += _coreHandler_ConnectedStatusEvent;
        }

        private void _coreHandler_ConnectedStatusEvent(object? sender, ConnectedStatusEventArgs e)
        {
            IsConnected = e.Connected;

            if (IsConnected)
            {
                var view = new VideoActionsView();
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = view;
                    view.Show();
                }));
            }
        }
        private void ConnectAction(object sender)
        {
            if (Port > 0 && !string.IsNullOrEmpty(IpAddress))
            {
                string connectingMessagePath = "Resources/ConnectingMessage.xaml";
                string connectingMessage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, connectingMessagePath);
                using (FileStream fs = new FileStream(connectingMessage, FileMode.Open))
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
