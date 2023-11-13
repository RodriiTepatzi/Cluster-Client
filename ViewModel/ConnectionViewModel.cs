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
            _port = 6969; //Deafault port
            _messagesPath = "Resources/Messages.xaml";
            _messages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MessagesPath); //Advice messages path
            _executeConnectToServerCommand = new CommandViewModel(ConnectAction);
            _executeCloseWindowCommand = new CommandViewModel(CloseWindowAction);
            _coreHandler.ConnectedStatusEvent += _coreHandler_ConnectedStatusEvent;
        }

        private void _coreHandler_ConnectedStatusEvent(object? sender, ConnectedStatusEventArgs e)
        {
            IsConnected = e.Connected; 
            if (IsConnected) //If client is connected close the connection window and show the video window
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
                //While client try connect to server, show this message for don't allow that user clicks the connect btn more than one time
                //This could be removed
                using (FileStream fs = new FileStream(Messages, FileMode.Open))
                {
                    ResourceDictionary resourceDic = (ResourceDictionary)XamlReader.Load(fs); //Open the resource file of advice messages
                    StackPanel conMessage = (StackPanel)resourceDic["ConnectingMessage"]; //Find the Connecting Message
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        //Find the StackPanel where the message will placed
                        StackPanel Content = (StackPanel)Application.Current.MainWindow.FindName("Content");
                        Content.Children.Clear();
                        Content.Children.Add(conMessage);
                    }));
                }
                //Connect to server
                CoreHandler.Instance.ConnectToServerAsync(IpAddress, Port);
            }
        }

        private void CloseWindowAction(object sender)
        {
            if (IsConnected)
            {
                //If client is connected, stop it
                CoreHandler.Instance.StopClientAsync();
            }
            //Stop the app
            Application.Current.Shutdown();
        }
    }
}
