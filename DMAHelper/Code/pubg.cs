using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vmmsharp;
using DMAHelper.Code.Models;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Timers;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace DMAHelper
{
    public class pubg
    {
        public bool isKaiWuZi = false;
        Vmm vmm;
        DispatcherTimer timer;
        uint pid = 0;
        #region 偏移
        ulong moduleBase;
        ulong GNamesAddress;
        ulong Offset_AcknowledgedPawn = 0x0480;
        ulong Offset_GWorld = 0x08D92500;
        ulong Offset_XenuineDecrypt = 0x07168728;
        ulong Offset_FNameEntry = 0x08F675A0;
        int Offset_ChunkSize = 0x4040;
        ulong Offset_ObjID = 0x000C;
        public static uint Offset_XorKey1 = 0xD6657937;
        public static long Offset_XorKey2 = 0x52AF7AF6;
        public static int Offset_RorValue = 0x03;
        public static bool Offset_IsingRor = false;
        ulong Offset_CharacterName = 0x1388;

        ulong Offset_CurrentLevel = 0x03D0;
        ulong Offset_Actors = 0x00C0;
        ulong Offset_ItemPackage = 0x0568;
        ulong Offset_AimOffsets = 0x15C0;
        ulong Offset_ItemInformationComponent = 0x00B0;
        ulong Offset_ItemID = 0x0248;
        ulong Offset_DroppedItem = 0x0428;
        ulong Offset_DroppedItemGroup = 0x0048;
        ulong Offset_DroppedItemGroup_UItem = 0x0738;
        ulong Offset_SpectatedCount = 0x1314;
        ulong Offset_LerpSafetyZoneRadius = 0x04D8;
        ulong Offset_LerpSafetyZonePosition = 0x0778;
        ulong Offset_PoisonGasWarningPosition = 0x04BC;
        ulong Offset_PoisonGasWarningRadius = 0x04AC;
        ulong Offset_BlackZonePosition = 0x0AF0;
        ulong Offset_BlackZoneRadius = 0x0AFC;
        ulong Offset_RedZonePosition = 0x0710;
        ulong Offset_RedZoneRadius = 0x04D4;
        ulong Offset_GameState = 0x0360;
        ulong Offset_WorldLocation = 0x08C4;
        ulong Offset_Mesh = 0x04C0;
        ulong Offset_Health = 0x1958;
        ulong Offset_GroggyHealth = 0x1BA4;
        ulong Offset_PlayerState = 0x0430;
        ulong Offset_LastTeamNum = 0x2228;
        ulong Offset_PlayerController = 0x0038;
        ulong Offset_LocalPlayersPTR = 0x08ECA770;
        ulong Offset_PlayerCameraManager = 0x04A0;
        ulong Offset_CameraLocation = 0x1CBC;
        ulong Offset_PlayerStatistics = 0x0A2C;
        ulong Offset_RootComponent = 0x01C8;
        ulong Offset_ComponentLocation = 0x02B0;
        #endregion

        private List<CarModel> listCar = new List<CarModel>();
        public delegate ulong DecryptData(ulong c);
        public event Action<PubgModel> OnPlayerListUpdate;
        DecryptData decryptFunc;
        PlayerModel myModel = null;
        public event Action<long> OnExecTime;
        List<GoodItem> goodItems = new List<GoodItem>();
        bool isLocal = false;
        public pubg()
        {
            #region 载具列表
            listCar.Add(new CarModel()
            {
                CarClass = "AirBoat_V2_C",
                CarName = "汽艇"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "AquaRail_A_01_C",
                CarName = "摩托艇"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "AquaRail_A_02_C",
                CarName = "摩托艇"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "AquaRail_A_03_C",
                CarName = "摩托艇"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "AquaRail_C",
                CarName = "摩托艇"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_ATV_C",
                CarName = "全地形车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_BRDM_C",
                CarName = "装甲车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Bicycle_C",
                CarName = "自行车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_CoupeRB_C",
                CarName = "跑车RB"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_DO_Circle_Train_Merged_C",
                CarName = "火车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_DO_Line_Train_Dino_Merged_C",
                CarName = "火车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_DO_Line_Train_Merged_C",
                CarName = "火车"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_Dirtbike_C",
                CarName = "越野摩托"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Food_Truck_C",
                CarName = "食品运输车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_EmergencyPickupVehicle_C",
                CarName = "紧急取件"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_KillTruck_C",
                CarName = "杀戮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_LootTruck_C",
                CarName = "物资车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_M_Rony_A_01_C",
                CarName = "罗尼车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_M_Rony_A_02_C",
                CarName = "罗尼车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_M_Rony_A_03_C",
                CarName = "罗尼车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_McLarenGT_Lx_Yellow_C",
                CarName = "迈凯轮"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_McLarenGT_St_black_C",
                CarName = "迈凯轮"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_McLarenGT_St_white_C",
                CarName = "迈凯轮"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Mirado_A_02_C",
                CarName = "跑车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Mirado_A_03_Esports_C",
                CarName = "跑车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Mirado_Open_03_C",
                CarName = "跑车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Mirado_Open_04_C",
                CarName = "跑车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Mirado_Open_05_C",
                CarName = "跑车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorbike_04_C",
                CarName = "摩托车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorbike_04_Desert_C",
                CarName = "摩托车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorbike_Solitario_C",
                CarName = "摩托车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorbike_04_SideCar_C",
                CarName = "三轮摩托"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorbike_04_SideCar_C",
                CarName = "三轮摩托"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorglider_C",
                CarName = "滑翔机"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Motorglider_Green_C",
                CarName = "滑翔机"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_01_C",
                CarName = "雪地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_02_C",
                CarName = "雪地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_03_C",
                CarName = "雪地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_04_C",
                CarName = "雪地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_05_C",
                CarName = "雪地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_06_C",
                CarName = "雪地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Niva_07_C",
                CarName = "雪地车"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_A_01_C",
                CarName = "皮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_A_02_C",
                CarName = "皮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_A_03_C",
                CarName = "皮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_A_04_C",
                CarName = "皮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_A_05_C",
                CarName = "皮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_A_esports_C",
                CarName = "皮卡车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_B_01_C",
                CarName = "皮卡车(敞篷)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_B_02_C",
                CarName = "皮卡车(敞篷)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_B_03_C",
                CarName = "皮卡车(敞篷)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_B_04_C",
                CarName = "皮卡车(敞篷)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PickupTruck_B_05_C",
                CarName = "皮卡车(敞篷)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Pillar_Car_C",
                CarName = "●警车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_PonyCoupe_C",
                CarName = "新能源"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_Porter_C",
                CarName = "货拉拉"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Scooter_01_A_C",
                CarName = "滑板车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Scooter_02_A_C",
                CarName = "滑板车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Scooter_03_A_C",
                CarName = "滑板车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Scooter_04_A_C",
                CarName = "滑板车"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_Snowbike_01_C",
                CarName = "雪地自行车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Snowbike_02_C",
                CarName = "雪地自行车"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_Snowmobile_01_C",
                CarName = "雪地摩托"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Snowmobile_02_C",
                CarName = "雪地摩托"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Snowmobile_03_C",
                CarName = "雪地摩托"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "BP_TukTukTuk_A_01_C",
                CarName = "三轮车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_TukTukTuk_A_02_C",
                CarName = "三轮车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_TukTukTuk_A_03_C",
                CarName = "三轮车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Van_A_01_C",
                CarName = "面包车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Van_A_02_C",
                CarName = "面包车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "BP_Van_A_03_C",
                CarName = "面包车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Boat_PG117_C",
                CarName = "冲锋艇"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "PG117_A_01_C",
                CarName = "冲锋艇"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_01_C",
                CarName = "山地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_02_C",
                CarName = "山地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_03_C",
                CarName = "山地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_04_C",
                CarName = "山地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_05_C",
                CarName = "山地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_06_C",
                CarName = "山地车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Buggy_A_07_C",
                CarName = "山地车"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "Dacia_A_01_v2_C",
                CarName = "轿车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Dacia_A_01_v2_snow_C",
                CarName = "轿车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Dacia_A_02_v2_C",
                CarName = "轿车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Dacia_A_03_v2_C",
                CarName = "轿车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Dacia_A_03_v2_Esports_C",
                CarName = "轿车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Dacia_A_04_v2_C",
                CarName = "轿车"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "DummyTransportAircraft_C",
                CarName = "飞机"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "WarModeTransportAircraft_C",
                CarName = "空投飞机"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "EmergencyAircraft_Tiger_C",
                CarName = "应急飞机"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "MortarPawn_C",
                CarName = "迫击炮"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "ParachutePlayer_C",
                CarName = "降落伞"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "ParachutePlayer_Warmode_C",
                CarName = "降落伞"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "RedeployAircraft_Tiger_C",
                CarName = "直升机"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "TransportAircraft_Chimera_C",
                CarName = "直升机"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "TransportAircraft_Tiger_C",
                CarName = "直升机"
            });

            listCar.Add(new CarModel()
            {
                CarClass = "Uaz_A_01_C",
                CarName = "吉普车(敞篷)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Uaz_Armored_C",
                CarName = "吉普车(armored)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Uaz_B_01_C",
                CarName = "吉普车(软)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Uaz_B_01_esports_C",
                CarName = "吉普车(软)"
            });
            listCar.Add(new CarModel()
            {
                CarClass = "Uaz_C_01_C",
                CarName = "吉普车(硬)"
            });

            #endregion


        }

        public bool Init(bool isLocal, out string msg)
        {
            msg = "";
            try
            {
                
                this.isLocal = isLocal;
                try
                {
                    if (File.Exists("itemfilter.json"))
                    {

                        string jsonStr = File.ReadAllText("itemfilter.json");
                        var jo = JsonConvert.DeserializeObject<JObject>(jsonStr);
                        var v = jo.Properties();
                        foreach (var item in v)
                        {
                            JToken token = item.Root[item.Name];
                            goodItems.Add(new GoodItem() { className = item.Name, shortName = token["shortName"].Value<string>(), showItem = token["showItem"].Value<bool>(), group = token["group"].Value<int>() });
                        }

                    }

                }
                catch (Exception ex)
                {


                }
                if (isLocal)
                {
                    try
                    {
                        vmm = new Vmm("", "-device", "pmem");
                    }
                    catch (Exception e)
                    {
                        msg += "11:" + e.Message;
                        return false;

                    }
                }
                else
                {
                    try
                    {
                        vmm = new Vmm("", "-device", "fpga");
                    }
                    catch (Exception e)
                    {

                    }

                    try
                    {
                        if (vmm == null)
                        {
                            vmm = new Vmm("", "-device", "fpga", "-memmap", "auto");
                        }

                    }
                    catch (Exception e)
                    {
                        msg += "1:" + e.Message;
                        return false;

                    }

                }


                // GetMemMap();
                vmm.PidGetFromName("tslgame.exe", out uint pid);
                this.pid = pid;
                moduleBase = vmm.ProcessGetModuleBase(pid, "TslGame.exe");
                var DecryptThis = vmm.MemReadInt64(pid, moduleBase + Offset_XenuineDecrypt);
                if (DecryptThis > 0)
                {
                    var val2 = vmm.MemReadInt32(pid, DecryptThis + 3);
                    ulong LeaAddr = DecryptThis + val2 + 7;
                    //在自身进程申请的内存
                    var buff = vmm.MemRead(pid, DecryptThis + 7, 100);
                    #region 申请内存，调用方法
                    List<byte> l = new List<byte>();
                    //第一个字节
                    l.Add(0x48);
                    l.Add(0x8B);
                    l.Add(0xD1);
                    l.Add(0x48);
                    l.Add(0xB8);
                    l.AddRange(BitConverter.GetBytes(LeaAddr));
                    l.AddRange(buff);
                    var DecryptData = l.ToArray();
                    //两种申请内存空间
                    //IntPtr addr = Marshal.AllocHGlobal(DecryptData.Length);
                    IntPtr addr = Common.VirtualAlloc(IntPtr.Zero, DecryptData.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
                    //数据拷贝到申请的内存空间
                    Marshal.Copy(DecryptData, 0, addr, DecryptData.Length);
                    //指针转换成方法
                    decryptFunc = (DecryptData)Marshal.GetDelegateForFunctionPointer(addr, typeof(DecryptData));
                    #endregion 
                    msg = "";
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg += "2:" + ex.Message;
                return false;

            }
            
            msg += "未知错误";
            return false;
        }
        Thread t;
        DateTime dt = DateTime.Now;
        DateTime dtWuZi = DateTime.Now;
        DateTime dtCar = DateTime.Now;
        public void Start()
        {
            Task.Run(() =>
            {
                VmmScatter scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                List<ZhiZhenModel> ListZhiZhenModel = new List<ZhiZhenModel>();
                List<PlayerModel> ListPlayer = new List<PlayerModel>();
                List<PubgGood> goods = new List<PubgGood>();
                List<CarModel> listCarModel = new List<CarModel>();

                while (true)
                {
                    if (isLocal)
                    {
                        Thread.Sleep(10);
                    }
                    ListZhiZhenModel.Clear();
                    ListPlayer.Clear();
                    
                    listCarModel.Clear();
                    dt = DateTime.Now;
                    try
                    {
                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        if (scatter != null)
                        {

                            PubgModel model = new PubgModel();
                            ulong world = decryptFunc(vmm.MemReadInt64(pid, moduleBase + Offset_GWorld));
                            ulong ULocalPlayer = vmm.MemReadInt64(pid, moduleBase + Offset_LocalPlayersPTR);
                            ulong PlayerController = decryptFunc(vmm.MemReadInt64(pid, ULocalPlayer + Offset_PlayerController));
                            ulong CameraManager = vmm.MemReadInt64(pid, PlayerController + Offset_PlayerCameraManager);
                            Vector3D cameraLocation = vmm.MemReadVector(pid, CameraManager + Offset_CameraLocation);
                            ulong PersistentLevel = decryptFunc(vmm.MemReadInt64(pid, world + Offset_CurrentLevel));
                            ulong ActorsArray = decryptFunc(vmm.MemReadInt64(pid, PersistentLevel + Offset_Actors));
                            uint Actorscount = vmm.MemReadInt32(pid, ActorsArray + 0x08);
                            ulong actorBase = vmm.MemReadInt64(pid, ActorsArray);
                            ulong GNames = decryptFunc(vmm.MemReadInt64(pid, moduleBase + Offset_FNameEntry));
                            GNamesAddress = decryptFunc(vmm.MemReadInt64(pid, GNames));
                            // int h = vmm.MemReadInt(pid, world + Offset_WorldLocation + 0x4);
                            uint MapId = Common.dec_objid(vmm.MemReadInt(pid, world + Offset_ObjID));
                            ulong LocalPlayerPawn = decryptFunc(vmm.MemReadInt64(pid, PlayerController + Offset_AcknowledgedPawn));
                            ulong CharacterId = vmm.MemReadInt64(pid, LocalPlayerPawn + Offset_CharacterName);
                            var MyName = vmm.MemReadString(pid, CharacterId, 64);
                            string mapName = GetObjName(MapId);
                            ulong GameState = decryptFunc(vmm.MemReadInt64(pid, world + Offset_GameState));
                            if (mapName == "TslLobby_Persistent_Main")
                            {
                                continue;
                            }
                            if (string.IsNullOrEmpty(MyName) && myModel != null)
                            {
                                MyName = myModel.Name;
                            }
                            model.MapName = mapName;
                            scatter.Prepare(GameState + Offset_LerpSafetyZoneRadius, 4);
                            scatter.Prepare(GameState + Offset_LerpSafetyZonePosition, 8);
                            scatter.Prepare(GameState + Offset_PoisonGasWarningPosition, 8);
                            scatter.Prepare(GameState + Offset_PoisonGasWarningRadius, 4);
                            scatter.Prepare(GameState + Offset_RedZonePosition, 8);
                            scatter.Prepare(GameState + Offset_RedZoneRadius, 4);
                            #region 读取所有类名

                            if (Actorscount > 20000)
                            {
                                continue;
                            }
                            for (int i = 0; i < Actorscount; i++)
                            {
                                try
                                {
                                    scatter.Prepare(actorBase + (ulong)i * 8, 8);

                                }
                                catch (Exception ex)
                                {

                                    Console.WriteLine("lei:" + ex.Message + ex.StackTrace);
                                }
                            }
                            bool isExec = scatter.Execute();
                            var lerpSafetyGasRadius = scatter.ReadFloat(GameState + Offset_LerpSafetyZoneRadius);
                            var lerpSafetyPosition = scatter.ReadVector(GameState + Offset_LerpSafetyZonePosition);
                            var poisonGasPosition = scatter.ReadVector(GameState + Offset_PoisonGasWarningPosition);
                            var poisonGasRadius = scatter.ReadFloat(GameState + Offset_PoisonGasWarningRadius);
                            var redPosition = scatter.ReadVector(GameState + Offset_RedZonePosition);
                            var redRadius = scatter.ReadFloat(GameState + Offset_RedZoneRadius);
                            for (int i = 0; i < Actorscount; i++)
                            {
                                ulong pObjPointer = scatter.ReadUInt64(actorBase + (ulong)i * 8);
                                if (pObjPointer > 0x100000)
                                {
                                    ListZhiZhenModel.Add(new ZhiZhenModel() { pObjPointer = pObjPointer });


                                }
                            }
                            //准备actorId
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);
                            //  scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                            foreach (var item in ListZhiZhenModel)
                            {
                                scatter.Prepare(item.pObjPointer + Offset_ObjID, 4);
                            }
                            isExec = scatter.Execute();
                            //读取actorId
                            foreach (var item in ListZhiZhenModel)
                            {
                                int actorId = scatter.ReadInt(item.pObjPointer + Offset_ObjID);
                                uint objId = Common.dec_objid(actorId);
                                item.actorId = actorId;
                                item.objId = objId;
                            }
                            //准备fNamePtr
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);
                            //scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                            foreach (var item in ListZhiZhenModel)
                            {
                                scatter.Prepare((GNamesAddress + (ulong)(item.objId / Offset_ChunkSize) * 0x8), 8);
                            }
                            isExec = scatter.Execute();
                            //读取fNamePtr 
                            foreach (var item in ListZhiZhenModel)
                            {
                                ulong fNamePtr = scatter.ReadUInt64((GNamesAddress + (ulong)(item.objId / Offset_ChunkSize) * 0x8));
                                if (fNamePtr > 0)
                                {
                                    item.fNamePtr = fNamePtr;
                                    scatter.Prepare(fNamePtr + (ulong)(item.objId % Offset_ChunkSize) * 0x8, 8);
                                }
                            }
                            ListZhiZhenModel = ListZhiZhenModel.Where(x => x.fNamePtr > 0).ToList();
                            //准备fName
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);
                            foreach (var item in ListZhiZhenModel)
                            {
                                item.fNamePtr = item.fNamePtr;
                                scatter.Prepare(item.fNamePtr + (ulong)(item.objId % Offset_ChunkSize) * 0x8, 8);
                            }
                            isExec = scatter.Execute();
                            //读取fName，
                            foreach (var item in ListZhiZhenModel)
                            {
                                ulong fName = scatter.ReadUInt64(item.fNamePtr + (ulong)(item.objId % Offset_ChunkSize) * 0x8);
                                if (fName > 0)
                                {
                                    item.fName = fName;
                                }
                            }
                            //准备className
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);
                            ListZhiZhenModel = ListZhiZhenModel.Where(x => x.fName > 0).ToList();
                            foreach (var item in ListZhiZhenModel)
                            {
                                scatter.Prepare(item.fName + 0x10, 64);
                            }
                            scatter.Execute();
                            //读取className
                            foreach (var item in ListZhiZhenModel)
                            {
                                string className = scatter.ReadStringASCII(item.fName + 0x10, 64);
                                item.className = className;
                            }
                            #endregion
                            #region 读取玩家名字 
                            //准备读取CharacterId
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            var listPlay = ListZhiZhenModel.Where(item =>
                                    (!string.IsNullOrEmpty(item.className) && (item.className == "PlayerMale_A_C" ||
                                                                               item.className == "PlayerFemale_A_C" ||
                                                                               item.className == "AIPawn_Base_Female_C" ||
                                                                               item.className == "AIPawn_Base_Male_C" ||
                                                                               item.className ==
                                                                               "UltAIPawn_Base_Female_C" ||
                                                                               item.className == "UltAIPawn_Base_Male_C")))
                                .ToList();
                            foreach (var item in listPlay)
                            {

                                scatter.Prepare(item.pObjPointer + Offset_CharacterName, 8);

                            }
                            scatter.Execute();
                            //读取CharacterId
                            foreach (var item in listPlay)
                            {
                                item.CharacterId = scatter.ReadUInt64(item.pObjPointer + Offset_CharacterName);
                            }
                            //准备读取CharacterName

                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.CharacterId, 64);
                            }
                            scatter.Execute();
                            //读取CharacterName
                            foreach (var item in listPlay)
                            {
                                item.Name = scatter.ReadStringUnicode(item.CharacterId, 64);
                            }
                            #endregion

                            #region 读取hp
                            //准备hp+读取倒地hp+读取观战人数 
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.pObjPointer + Offset_Health, 4);
                                scatter.Prepare(item.pObjPointer + Offset_GroggyHealth, 4);
                                scatter.Prepare(item.pObjPointer + Offset_SpectatedCount, 4);
                                scatter.Prepare(item.pObjPointer + Offset_LastTeamNum, 4);
                                scatter.Prepare(item.pObjPointer + Offset_AimOffsets + 0x4, 4);

                            }
                            //读取hp+读取倒地hp+读取观战人数 
                            scatter.Execute();
                            foreach (var item in listPlay)
                            {
                                item.Hp = scatter.ReadFloat(item.pObjPointer + Offset_Health);
                                item.groggyHp = scatter.ReadFloat(item.pObjPointer + Offset_GroggyHealth);
                                item.SpectatedCount = scatter.ReadInt(item.pObjPointer + Offset_SpectatedCount);
                                int teamNum = scatter.ReadInt(item.pObjPointer + Offset_LastTeamNum);
                                if (teamNum == 100000 || teamNum > 100000)
                                {
                                    item.teamNum = teamNum - 100000;
                                }
                                else
                                {
                                    item.teamNum = teamNum;
                                }
                                item.Orientation = scatter.ReadFloat(item.pObjPointer + Offset_AimOffsets + 0x4);

                            }
                            listPlay = listPlay.Where(s => s.Hp > 0 || s.groggyHp > 0.1).ToList();

                            #endregion




                            #region 读取杀敌数量
                            //准备读取PlayerState
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.pObjPointer + Offset_PlayerState, 8);
                            }
                            //读取PlayerState
                            scatter.Execute();
                            foreach (var item in listPlay)
                            {
                                item.PlayerState = scatter.ReadUInt64(item.pObjPointer + Offset_PlayerState);
                            }

                            //准备读取KillCount
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.PlayerState + Offset_PlayerStatistics, 4);

                            }
                            //读取KillCount
                            scatter.Execute();
                            foreach (var item in listPlay)
                            {
                                item.KillCount = scatter.ReadInt(item.PlayerState + Offset_PlayerStatistics);
                            }
                            #endregion


                            Console.WriteLine("zuobiao");
                            #region 读取坐标
                            //准备读取MeshAddr
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.pObjPointer + Offset_Mesh, 8);
                            }
                            //读取MeshAddr
                            scatter.Execute();
                            foreach (var item in listPlay)
                            {
                                item.MeshAddr = scatter.ReadUInt64(item.pObjPointer + Offset_Mesh);
                            }
                            //准备读取Offset_ComponentLocation
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.MeshAddr + Offset_ComponentLocation, 12);

                            }
                            //读取Offset_ComponentLocation
                            scatter.Prepare(world + Offset_WorldLocation, 4);
                            scatter.Prepare(world + Offset_WorldLocation + 0x04, 4);
                            scatter.Execute();
                            foreach (var item in listPlay)
                            {

                                float X = scatter.ReadFloat(item.MeshAddr + Offset_ComponentLocation);
                                float Y = scatter.ReadFloat(item.MeshAddr + Offset_ComponentLocation + 0x4);
                                float Z = scatter.ReadFloat(item.MeshAddr + Offset_ComponentLocation + 0x8);
                                float w = scatter.ReadInt(world + Offset_WorldLocation);
                                float h = scatter.ReadInt(world + Offset_WorldLocation + 0x4);
                                item.actorLocation = new Vector3D(X, Y, Z);
                                Vector3D aimFov = (item.actorLocation - cameraLocation);
                                var tempV = (item.actorLocation - cameraLocation);
                                float Radpi = (float)(180 / 3.1415926535f);
                                float Yaw = (float)Math.Atan2(tempV.Y, tempV.X) * Radpi;
                                float Pitch = (float)Math.Atan2(item.z, Math.Sqrt((tempV.X * tempV.X) + (tempV.Y * tempV.Y))) * Radpi;
                                float Roll = 0;
                                aimFov = new Vector3D(Yaw, Pitch, Roll);
                                item.aimFov = aimFov;

                                item.x = X + w;
                                item.y = Y + h;
                                item.z = Z;


                                if (item.className == "PlayerMale_A_C" || item.className == "PlayerFemale_A_C")
                                {
                                    item.isBot = false;
                                }
                                else if (item.className == "AIPawn_Base_Female_C" || item.className == "AIPawn_Base_Male_C" || item.className == "UltAIPawn_Base_Female_C" || item.className == "UltAIPawn_Base_Male_C")
                                {
                                    item.isBot = true;
                                }
                                if (item.x < 0)
                                {
                                    item.x = -item.x;
                                }
                                if (item.y < 0)
                                {
                                    item.y = -item.y;
                                }
                                if (item.z < 0)
                                {
                                    item.z = -item.z;
                                }
                            }

                            #endregion
                            Console.WriteLine("amimz");
                            #region 读取AmiMz
                            //准备读取AmiMz
                            scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                            foreach (var item in listPlay)
                            {
                                scatter.Prepare(item.pObjPointer + Offset_AimOffsets, 4);

                            }
                            //读取AmiMz
                            scatter.Execute();
                            foreach (var item in listPlay)
                            {
                                item.AmiMz = scatter.ReadFloat(item.pObjPointer + Offset_AimOffsets);
                                float AimX = (float)Math.Abs(item.aimFov.X - item.AmiMz);
                                item.bIsAimed = (AimX > -5 && AimX < 5);
                                float Distance = (float)(cameraLocation - item.actorLocation).Length / 100;
                                item.Distance = Distance;
                                ListPlayer.Add(new PlayerModel()
                                {
                                    Name = item.Name,
                                    HP = item.Hp,
                                    TeamId = item.teamNum,
                                    isBot = item.isBot,
                                    bIsAimed = item.bIsAimed,
                                    Distance = item.Distance,
                                    x = item.x,
                                    y = item.y,
                                    z = item.z,
                                    KillCount = item.KillCount,
                                    Orientation = item.Orientation,
                                    SpectatedCount = item.SpectatedCount,
                                    ActorLocation = item.actorLocation,

                                });
                            }

                            #endregion

                            #region 读取物资
                            goods.Clear();
                            if (isKaiWuZi==true)
                            {
                                
                                   
                                    dtWuZi = DateTime.Now;
                                    var listgoods = ListZhiZhenModel.Where(item =>
                                       (!string.IsNullOrEmpty(item.className) && item.className == "DroppedItemGroup"))
                                   .ToList();
                                    if (listgoods != null && listgoods.Count() > 0)
                                    {
                                        //准备读取ItemGroupPtr
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);
                                        foreach (var item in listgoods)
                                        {
                                            scatter.Prepare(item.pObjPointer + Offset_DroppedItemGroup, 8);
                                        }
                                        //读取ItemGroupPtr
                                        scatter.Execute();
                                        foreach (var item in listgoods)
                                        {
                                            item.ItemGroupPtr = scatter.ReadUInt64(item.pObjPointer + Offset_DroppedItemGroup);
                                        }
                                        //准备读取ItemCount 
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in listgoods)
                                        {
                                            if (item.ItemGroupPtr > 0)
                                            {
                                                scatter.Prepare(item.pObjPointer + Offset_DroppedItemGroup + 0x8, 4);
                                            }
                                        }
                                        //读取ItemCount
                                        scatter.Execute();

                                        foreach (var item in listgoods)
                                        {
                                            if (item.ItemGroupPtr > 0)
                                            {

                                                item.ItemCount = scatter.ReadInt(item.pObjPointer + Offset_DroppedItemGroup + 0x8);
                                            }
                                        }

                                        //准备ItemObject
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);


                                        foreach (var item in listgoods)
                                        {
                                            if (item.ItemGroupPtr > 0 && item.ItemCount > 0 && item.ItemCount < 5000)
                                            {
                                                for (int itemIndex = 0; itemIndex < item.ItemCount; itemIndex++)
                                                {
                                                    scatter.Prepare(item.ItemGroupPtr + (ulong)(itemIndex * 0x10), 8);
                                                }
                                            }
                                        }
                                        //读取ItemObject
                                        scatter.Execute();
                                        foreach (var item in listgoods)
                                        {
                                            if (item.ItemGroupPtr > 0 && item.ItemCount > 0 && item.ItemCount < 5000)
                                            {
                                                for (int itemIndex = 0; itemIndex < item.ItemCount; itemIndex++)
                                                {
                                                    ulong ItemObject = scatter.ReadUInt64(item.ItemGroupPtr + (ulong)(itemIndex * 0x10));
                                                    goods.Add(new PubgGood()
                                                    {
                                                        ItemObject = ItemObject
                                                    });

                                                }
                                            }
                                        }
                                        //准备UItemAddress 
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in goods)
                                        {
                                            if (item.ItemObject > 0)
                                            {
                                                scatter.Prepare(item.ItemObject + Offset_DroppedItemGroup_UItem, 8);
                                            }
                                        }

                                        //读取UItemAddress
                                        scatter.Execute();
                                        foreach (var item in goods)
                                        {
                                            if (item.ItemObject > 0)
                                            {
                                                item.UItemAddress = scatter.ReadUInt64(item.ItemObject + Offset_DroppedItemGroup_UItem);
                                            }
                                        }
                                        //准备读取UItemIDAddress
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in goods)
                                        {
                                            if (item.UItemAddress > 0)
                                            {
                                                scatter.Prepare(item.UItemAddress + Offset_ItemInformationComponent, 8);
                                            }
                                        }
                                        //读取UItemIDAddress
                                        scatter.Execute();
                                        foreach (var item in goods)
                                        {
                                            if (item.UItemAddress > 0)
                                            {
                                                item.UItemIDAddress = scatter.ReadUInt64(item.UItemAddress + Offset_ItemInformationComponent);
                                            }
                                        }
                                        //准备读取UItemID
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in goods)
                                        {
                                            if (item.UItemIDAddress > 0)
                                            {
                                                scatter.Prepare(item.UItemIDAddress + Offset_ItemID, 4);
                                            }
                                        }
                                        //读取UItemID
                                        scatter.Execute();
                                        foreach (var item in goods)
                                        {
                                            if (item.UItemIDAddress > 0)
                                            {
                                                item.UItemID = scatter.ReadUInt(item.UItemIDAddress + Offset_ItemID);
                                            }
                                        }
                                        //准备读取UItem坐标
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in goods)
                                        {
                                            if (item.UItemID > 0 && item.UItemID < 0xfff0ff)
                                            {
                                                scatter.Prepare(item.ItemObject + Offset_ComponentLocation, 12);
                                            }
                                        }
                                        //读取UItem坐标
                                        scatter.Prepare(world + Offset_WorldLocation, 4);
                                        scatter.Prepare(world + Offset_WorldLocation + 0x04, 4);
                                        scatter.Execute();
                                        float ww = scatter.ReadInt(world + Offset_WorldLocation);
                                        float hh = scatter.ReadInt(world + Offset_WorldLocation + 0x4);
                                        foreach (var item in goods)
                                        {
                                            if (item.UItemID > 0 && item.UItemID < 0xfff0ff)
                                            {
                                                var zuobiao = scatter.Read(item.ItemObject + Offset_ComponentLocation, 12);
                                                Vector3D v3d = new Vector3D(BitConverter.ToSingle(zuobiao, 0), BitConverter.ToSingle(zuobiao, 4), BitConverter.ToSingle(zuobiao, 8));
                                                var tempv3 = new Vector3D(ww, hh, 0) + v3d;
                                                item.x = (int)tempv3.X;
                                                item.y = (int)tempv3.Y;
                                            }
                                        }
                                        
                                        //准备fNamePtr
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in goods)
                                        {
                                            scatter.Prepare((GNamesAddress + (ulong)(item.UItemID / Offset_ChunkSize) * 0x8), 8);
                                        }
                                        isExec = scatter.Execute();
                                        //读取fNamePtr 
                                        foreach (var item in goods)
                                        {
                                            ulong fNamePtr = scatter.ReadUInt64((GNamesAddress + (ulong)(item.UItemID / Offset_ChunkSize) * 0x8));
                                            if (fNamePtr > 0)
                                            {
                                                item.fNamePtr = fNamePtr;
                                            }
                                        }
                                        goods = goods.Where(x => x.fNamePtr > 0).ToList();
                                        //准备fName
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        foreach (var item in goods)
                                        {
                                            scatter.Prepare(item.fNamePtr + (ulong)(item.UItemID % Offset_ChunkSize) * 0x8, 8);
                                        }
                                        isExec = scatter.Execute();
                                        //读取fName，
                                        foreach (var item in goods)
                                        {
                                            ulong fName = scatter.ReadUInt64(item.fNamePtr + (ulong)(item.UItemID % Offset_ChunkSize) * 0x8);
                                            if (fName > 0)
                                            {
                                                item.fName = fName;
                                            }
                                        }
                                        //准备读取物资名字
                                        scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                        goods = goods.Where(x => x.fName > 0).ToList();
                                        foreach (var item in goods)
                                        {
                                            scatter.Prepare(item.fName + 0x10, 64);
                                        }
                                        scatter.Execute();
                                        //读取物资名字
                                        foreach (var item in goods)
                                        {
                                            string className = scatter.ReadStringASCII(item.fName + 0x10, 64);
                                            var tempM = goodItems.Where(s => s.className == className).FirstOrDefault();
                                            if (tempM != null)
                                            {
                                                item.Name = tempM.shortName;
                                                item.isShow = tempM.showItem;
                                                item.ClassName = className;
                                                item.group = tempM.group;                                            }
                                            //else
                                            //{
                                            //    item.ClassName = className;
                                            //    item.isShow = true;
                                            //    item.Name = className;
                                            //}

                                        }
                                    }
                                 
                            }
                            #endregion
                            #region 读取载具

                            var listtempcar = ListZhiZhenModel.Where(item =>
                                (!string.IsNullOrEmpty(item.className) && (listCar.Any(h => h.CarClass == item.className)))).ToList();
                            Console.WriteLine("listtempcar " + listtempcar.Count());
                            if (listtempcar.Count() == 0)
                            {

                            }

                            if (listtempcar != null && listtempcar.Count() > 0)
                            {
                                //准备读取载具RootComponent
                                scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                foreach (var item in listtempcar)
                                {

                                    var tempCarModel = listCar.Where(s => s.CarClass == item.className).FirstOrDefault();
                                    if (tempCarModel != null)
                                    {
                                        listCarModel.Add(new CarModel()
                                        {
                                            CarClass = item.className,
                                            CarName = tempCarModel.CarName,
                                            pObjPointer = item.pObjPointer
                                        });
                                    }

                                    scatter.Prepare(item.pObjPointer + Offset_RootComponent, 12);
                                }
                                //读取载具RootComponent
                                scatter.Execute();
                                foreach (var item in listCarModel)
                                {
                                    var RootComponentAddress = scatter.ReadUInt64(item.pObjPointer + Offset_RootComponent);
                                    if (RootComponentAddress > 0)
                                    {
                                        item.RootComponent = decryptFunc(RootComponentAddress);
                                    }
                                }
                                //准备读取坐标
                                scatter.Clear(pid, Vmm.FLAG_NOCACHE);

                                foreach (var item in listCarModel)
                                {
                                    scatter.Prepare(item.RootComponent + Offset_ComponentLocation, 12);
                                }
                                //读取坐标
                                scatter.Prepare(world + Offset_WorldLocation, 4);
                                scatter.Prepare(world + Offset_WorldLocation + 0x04, 4);
                                scatter.Execute();
                                foreach (var item in listCarModel)
                                {
                                    var zuobiao = scatter.Read(item.RootComponent + Offset_ComponentLocation, 12);
                                    Vector3D v3d = new Vector3D(BitConverter.ToSingle(zuobiao, 0), BitConverter.ToSingle(zuobiao, 4), BitConverter.ToSingle(zuobiao, 8));
                                    float w = scatter.ReadInt(world + Offset_WorldLocation);
                                    float h = scatter.ReadInt(world + Offset_WorldLocation + 0x4);
                                    var tempv3 = new Vector3D(w, h, 0) + v3d;
                                    item.x = (int)tempv3.X;
                                    item.y = (int)tempv3.Y;
                                }
                            }


                            #endregion
                           
                            var tempMyModel = ListPlayer.Where(s => s.Name == MyName).FirstOrDefault();

                            if (tempMyModel != null)
                            {
                                myModel = tempMyModel;
                                foreach (var item in ListPlayer)
                                {

                                    if (item.TeamId == myModel.TeamId)
                                    {
                                        item.IsMyTeam = true;
                                    }
                                }
                            }
                            model.Cars = listCarModel;
                            model.Player = ListPlayer;
                            model.MyTeam = ListPlayer.Where(s => s.IsMyTeam == true).ToList();
                            model.Game.Add(new List<object>() { lerpSafetyPosition.X, lerpSafetyPosition.Y, lerpSafetyGasRadius });

                            model.Game.Add(new List<object>() { poisonGasPosition.X, poisonGasPosition.Y, poisonGasRadius });
                            model.Game.Add(new List<object>() { redPosition.X, redPosition.Y, redRadius });
                            if (tempMyModel != null)
                            {
                                model.MyName = tempMyModel.Name;
                            }
                            model.MyName = MyName;
                            if (ListPlayer.Count == 0)
                            {
                                continue;
                            }
                            model.PubgGoods = goods.Where(s => s.isShow).ToList();
                            if (OnPlayerListUpdate != null)
                            {
                                OnPlayerListUpdate(model);
                            }
                        }
                        sw.Stop();
                        if (OnExecTime != null)
                        {
                            OnExecTime(sw.ElapsedMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("11:" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    // GC.Collect();
                }
            });
            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(2);
            //timer.Tick += (ss, ee) =>
            //{
            //    if ((DateTime.Now - dt).TotalSeconds > 2)
            //    {
            //        try
            //        {
            //            if (t != null)
            //            {
            //                t.Abort();
            //            }
            //        }
            //        catch (Exception)
            //        {


            //        }

            //    }
            //};

            //timer.Start();


        }

        string GetObjName(uint objId)
        {
            //获取类名地址
            ulong fNamePtr = vmm.MemReadInt64(pid, (GNamesAddress + (ulong)(objId / Offset_ChunkSize) * 0x8));
            if (fNamePtr > 0)
            {
                //获取类名地址
                ulong fName = vmm.MemReadInt64(pid, fNamePtr + (ulong)(objId % Offset_ChunkSize) * 0x8);
                if (fName > 0)
                {
                    var nameByte = vmm.MemRead(pid, fName + 0x10, 64);

                    //获取类名
                    string name = Encoding.ASCII.GetString(nameByte.ToArray());

                    return name.Substring(0, name.IndexOf('\0') >= 0 ? name.IndexOf('\0') : name.Length);
                }
            }
            return null;
        }
    }
}
