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

namespace Cluster_Client.ViewModel
{
    public class VideoActionsViewModel : BaseViewModel
    {
        private CoreHandler _coreHandler;
        private ICommand _executeCloseWindowCommand;
        private bool _isConnected;

        public ICommand ExecuteCloseWindowCommand
        {
            get { return _executeCloseWindowCommand; }
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

        public VideoActionsViewModel()
        {
            _coreHandler = CoreHandler.Instance;
            _executeCloseWindowCommand = new CommandViewModel(CloseWindowAction);
            _coreHandler.ConnectedStatusEvent += _coreHandler_ConnectedStatusEvent;
        }

        private void _coreHandler_ConnectedStatusEvent(object? sender, ConnectedStatusEventArgs e)
        {
            IsConnected = e.Connected;
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
