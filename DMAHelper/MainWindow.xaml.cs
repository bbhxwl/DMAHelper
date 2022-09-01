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

namespace DMAHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Vmm vmm = new Vmm("", "-device", "fpga");
        pubg p = null;
        public MainWindow()
        {
            InitializeComponent();
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
            this.Dispatcher.Invoke(() =>
            {
                this.txt.Text = JsonConvert.SerializeObject(obj);
            });
        }
    }
}
