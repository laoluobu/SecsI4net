using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Secs4Net;
using Secs4Net.Sml;
using SecsI4net;

namespace Example.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private ISecsIConnector? secs;

        [ObservableProperty]
        private string[] portList = Array.Empty<string>();

        public string[] MessageList { get; } = mapper.Keys.ToArray();


        private static Dictionary<string, SecsMessage> mapper = new Dictionary<string, SecsMessage>()
        {
            ["S1F1"] = new SecsMessage(1, 1),
            ["S2F41"] = new SecsMessage(2, 41),
        };

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string? currenKey;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string? currenMassage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
        [NotifyCanExecuteChangedFor(nameof(DisConnectCommand))]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private bool isConnected;

        [ObservableProperty]
        private string smlLog = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
        private string? currentPort;

        public MainViewModel()
        {
            RefreshPortList();
        }

        partial void OnCurrenKeyChanged(string? value)
        {
            if (value == null) return;
            CurrenMassage = mapper[value].ToSml();
        }

        [RelayCommand]
        private void RefreshPortList()
        {
            PortList = SerialPort.GetPortNames();
        }

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private void Connect()
        {
            secs = new SecsIConnector(CurrentPort!, OnRecivedMessage);
            IsConnected = true;
        }

        private void OnRecivedMessage(SecsMessage message)
        {
            WriteSecsLog(message);
        }

        private void WriteSecsLog(SecsMessage message)
        {
            SmlLog += $"[{DateTime.Now}]  {message.ToSml()}\n";
        }

        private bool CanConnect()
        {
            return !IsConnected && CurrentPort != null;
        }

        [RelayCommand(CanExecute = nameof(IsConnected))]
        private void DisConnect()
        {
            secs?.Dispose();
            IsConnected = false;
        }

        private bool CanSendMessage()
        {
            return IsConnected && CurrenMassage != null && CurrenMassage.Length > 0;
        }

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private void SendMessage(string sml)
        {
            var message = sml.ToSecsMessage();
            secs!.SendAsync(message);
            WriteSecsLog(message);
        }
    }
}
