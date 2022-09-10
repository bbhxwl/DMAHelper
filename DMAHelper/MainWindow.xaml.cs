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
using System.Diagnostics;
using System.Net.Configuration;

namespace DMAHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Vmm vmm = null;
        pubg p = null;
        IMqttClient mqtt = new MqttFactory().CreateMqttClient();
        public MainWindow()
        {
            InitializeComponent();
          

        }
        private    void P_OnPlayerListUpdate(PubgModel obj)
        {
             
             PubgMqttModel model = new PubgMqttModel();
            model.Map = obj.MapName;
             List<dynamic> l = new List<dynamic>();
            foreach (var item in obj.Player)
            {
                List<object> listobj = new List<object>();
                listobj.Add(item.x);
                listobj.Add(item.y);
                listobj.Add(item.Distance);
                listobj.Add(item.TeamId);
                listobj.Add(item.HP);
                listobj.Add(item.KillCount);
                listobj.Add(item.SpectatedCount);
                listobj.Add(item.Orientation);
                //是不是队友，1=是队友，0不是队友
                listobj.Add(0);
                listobj.Add(item.isBot ?1:0);
                listobj.Add(0);
                listobj.Add(0);
                listobj.Add(item.bIsAimed);
                listobj.Add(0);
                listobj.Add(item.Name);
                listobj.Add(1);
                model.Player.Add(listobj);
            }
            if (obj.PubgGoods!=null&&obj.PubgGoods.Count>0)
            {
               
                foreach (var item in obj.PubgGoods)
                {
                    List<object> listobj = new List<object>();
                    listobj.Add(item.ClassName);
                    listobj.Add(item.x);
                    listobj.Add(item.y);
                   // listobj.Add(0);
                    //listobj.Add(item.ClassName);
                    model.Goods.Add(listobj);
                }
               
            }
            try
            {
                 mqtt.PublishAsync(new MqttApplicationMessageBuilder().WithTopic(zhuti).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce).WithPayload(JsonConvert.SerializeObject(model)).Build()).ContinueWith(rs =>
                 {
                     try
                     {
                         if (rs.Result.ReasonCode == MqttClientPublishReasonCode.Success)
                         {
                          }
                     }
                     catch (Exception ee)
                     {

                      }
                    
                 });
            
            }
            catch (Exception ex)
            {

             }
            
            //this.Dispatcher.Invoke(() =>
            //{

            //    this.txt.Text = JsonConvert.SerializeObject(obj);
            //});
        }
        string zhuti =null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtuid.Content.ToString()=="")
            {
                txtLog.AppendText( "账号不能为空\r\n");
                return;
            }
            zhuti = txtuid.Content.ToString();
            var op = new MqttClientOptionsBuilder().WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithTcpServer("113.107.160.90").Build();
            mqtt.ConnectAsync(op).ContinueWith(rs =>
            {
                if (rs.Result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    txtLog.AppendText("网络连接成功\r\n");

                }
            });
            vmm.PidGetFromName("tslgame.exe", out uint pid);
            if (pid<=0)
            {
                txtLog.AppendText("游戏未运行\r\n");
                return;
            }
 
            string path = vmm.ProcessGetInformationString(pid, Vmm.VMMDLL_PROCESS_INFORMATION_OPT_STRING_PATH_KERNEL);
            //如果程序的路径 转小写后 包括 tslgame.exe，说明是pubg进程
            if (string.IsNullOrEmpty(path))
            {
                txtLog.AppendText("程序获取失败\r\n");
                return;
            }
            if (path.ToLower().Contains("tslgame.exe"))
            {
                try
                {
                    vmm=new Vmm("", "-device", "fpga");
                    p = new pubg(vmm, pid);
                    p.OnPlayerListUpdate += P_OnPlayerListUpdate;
                    if (p.Init())
                    {
                        btnOk.IsEnabled = false;
                        p.Start();
                    }
                    else
                    {
                        txtLog.AppendText("初始化DMA失败\r\n");
                        return;
                    }
                }
                catch (Exception ex)
                {

                    txtLog.AppendText("初始化DMA异常"+ex.Message+"\r\n");
                    return;
                }
               
            }

        }
    }
}
