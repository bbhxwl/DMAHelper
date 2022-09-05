using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using vmmsharp;
using MQTTnet.Client;
using MQTTnet;
namespace DMAHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Vmm vmm = new Vmm("", "-device", "fpga");
        pubg p = null;
        IMqttClient mqtt = new MqttFactory().CreateMqttClient();
        public MainWindow()
        {
            InitializeComponent();
           var op= new MqttClientOptionsBuilder().WithTcpServer("219.129.239.39").Build();
            mqtt.ConnectAsync(op).ContinueWith(rs =>
            {
                if (rs.Result.ResultCode== MqttClientConnectResultCode.Success)
                {
                    
                }
            });
            var pidList = vmm.PidList();
            Console.WriteLine($"一共有{pidList.Length}个进程");
            foreach (var pid in pidList)
            {
                string path = vmm.ProcessGetInformationString(pid, Vmm.VMMDLL_PROCESS_INFORMATION_OPT_STRING_PATH_KERNEL);
                //如果程序的路径 转小写后 包括 tslgame.exe，说明是pubg进程
                if (path.ToLower().Contains("tslgame.exe"))
                {
                    p = new pubg(vmm, pid);
                    p.OnPlayerListUpdate += P_OnPlayerListUpdate;
                    if (p.Init())
                    {
                        MessageBox.Show("初始化成功");
                        p.Start();
                    }
                }
            }


        }

        private void P_OnPlayerListUpdate(List<Code.Models.PlayerModel> obj)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            List<dynamic> l = new List<dynamic>();
            foreach (var item in obj)
            {
                string j = "["+item.x+","+item.y+",\r\n    45,\r\n    14,\r\n    "+item.HP+",\r\n    0,\r\n    0,\r\n    165.8167,\r\n    0,\r\n    0,\r\n    0,\r\n    0,\r\n    0,\r\n    0,\r\n    \""+item.Name+"\",\r\n    1\r\n]";
                l.Add(j);
            }
            o.Player = l;
            o.Goods =new List<int>();
            o.Box = new List<int>();
            o.Car = new List<int>();
            o.Map = "Kiki_Main";
            o.Goods = new List<int>();
            o.Game = new List<int>();
            mqtt.PublishAsync(new MqttApplicationMessageBuilder().WithTopic("470138890").WithPayload(JsonConvert.SerializeObject(o)).Build());
            this.Dispatcher.Invoke(() =>
            {
                
                this.txt.Text = JsonConvert.SerializeObject(obj);
            });
        }
    }
}
