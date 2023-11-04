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

        private string _ipAddress;
        private int _port;
        private bool _isConnected;
        private string _serverDisponibility;

        public ICommand ExecuteConnectToServerCommand
        {
            get { return _executeConnectToServerCommand; }
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

        public string HowIsServerDisponibility
        {
            get { return _serverDisponibility; }
            set
            {
                _serverDisponibility = value;
                OnPropertyChanged(nameof(HowIsServerDisponibility));
            }
        }

        public ConnectionViewModel()
        {
            _coreHandler = CoreHandler.Instance;
            _executeConnectToServerCommand = new CommandViewModel(ConnectAction);
            _ipAddress = "";
            _port = 6969;

            _coreHandler.ConnectedStatusEvent += _coreHandler_ConnectedStatusEvent;
            _coreHandler.ServerDisponibilityEvent += _coreHandler_ServerDisponibilityEvent;
        }

        private void _coreHandler_ServerDisponibilityEvent(object? sender, ServerDisponibilityEventArgs e)
        {
            HowIsServerDisponibility = e.serverDisponibility;

            if (HowIsServerDisponibility == "Busy")
            {
                string busyServerMessagePath = "Resources/BusyServerMessage.xaml";
                string busyServerMessage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, busyServerMessagePath);

                using (FileStream fs = new FileStream(busyServerMessage, FileMode.Open))
                {
                    ResourceDictionary resourceDict = (ResourceDictionary)XamlReader.Load(fs);

                    StackPanel bsMessage = (StackPanel)resourceDict["BusyMessage"];

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        StackPanel Content = (StackPanel)Application.Current.MainWindow.FindName("Content");

                        Content.Children.Clear();

                        Content.Children.Add(bsMessage);
                    }));
                }
            }
            else if (HowIsServerDisponibility == "Waiting")
            {
                string waitingServerMessagePath = "Resources/WaitingServerMessage.xaml";
                string waitingServerMessage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, waitingServerMessagePath);

                using (FileStream fs = new FileStream(waitingServerMessage, FileMode.Open))
                {
                    ResourceDictionary resourceDict = (ResourceDictionary)XamlReader.Load(fs);

                    StackPanel wsMessage = (StackPanel)resourceDict["WaitingMessage"];

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        StackPanel Content = (StackPanel)Application.Current.MainWindow.FindName("Content");

                        Content.Children.Clear();

                        Content.Children.Add(wsMessage);
                    }));
                }
            }
            else if (HowIsServerDisponibility == "Ready")
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

        private void _coreHandler_ConnectedStatusEvent(object? sender, ConnectedStatusEventArgs e)
        {
            IsConnected = e.Connected;
        }
        private void ConnectAction(object sender)
        {
            if (Port > 0 && !string.IsNullOrEmpty(IpAddress))
            {
                CoreHandler.Instance.ConnectToServerAsync(IpAddress, Port);
            }
        }
    }
}
