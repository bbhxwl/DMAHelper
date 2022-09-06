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
using DMAHelper.Code.Models;

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

        private void P_OnPlayerListUpdate(PubgModel obj)
        {
            //PubgMqttModel model = new PubgMqttModel();
            //model.Map = obj.MapName;
            // List<dynamic> l = new List<dynamic>();
            //foreach (var item in obj.Player)
            //{
            //    List<object> listobj = new List<object>();
            //    listobj.Add(item.x);
            //    listobj.Add(item.y);
            //    listobj.Add(item.Distance);
            //    listobj.Add(item.TeamId);
            //    listobj.Add(item.HP);
            //    listobj.Add(item.KillCount);
            //    listobj.Add(item.SpectatedCount);
            //    listobj.Add(item.Orientation);
            //    //是不是队友，1=是队友，0不是队友
            //    listobj.Add(0);
            //    listobj.Add(item.isBot ?1:0);
            //    listobj.Add(0);
            //    listobj.Add(0);
            //    listobj.Add(item.bIsAimed);
            //    listobj.Add(0);
            //    listobj.Add(item.Name);
            //    listobj.Add(1);
            //    model.Player.Add(listobj);
            //}
            //Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ffff"));

            //Task.Run(() =>
            //{
            //    mqtt.PublishAsync(new MqttApplicationMessageBuilder().WithTopic("470138890").WithQualityOfServiceLevel( MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce).WithPayload(JsonConvert.SerializeObject(model)).Build());

            //});
            this.Dispatcher.Invoke(() =>
            {

                this.txt.Text = JsonConvert.SerializeObject(obj);
            });
        }
    }
}
