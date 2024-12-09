using System.Collections.ObjectModel;
using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using Secs4Net;
using Secs4Net.Sml;
using SecsI4net;

namespace Example.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {

        private ISecsIConnection? secs;

        public ObservableCollection<string> PortList { get; } = new ObservableCollection<string>();

        public string[] MessageList { get; } = mapper.Keys.ToArray();


        private static Dictionary<string, SecsMessage> mapper = new Dictionary<string, SecsMessage>()
        {
            ["S1F1"] = new SecsMessage(1, 1),
            ["S2F41"] = new SecsMessage(2, 41) ,
        };

        [ObservableProperty]
        private string currenKey;

        [ObservableProperty]
        private string currenMassage;


        public MainViewModel()
        {
            PortList = new ObservableCollection<string>(SerialPort.GetPortNames());
        }

        partial void OnCurrenKeyChanged(string value)
        {
            CurrenMassage=mapper[value].ToSml();
        }

    }
}
