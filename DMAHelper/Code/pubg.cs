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

namespace DMAHelper
{
    public class pubg
    {
        Vmm vmm;
        uint pid = 0;
        #region 偏移
        ulong moduleBase;
        ulong GNamesAddress;

        ulong Offset_GWorld = 0x09031380;
        ulong Offset_XenuineDecrypt = 0x07429928;
        ulong Offset_FNameEntry = 0x09206410;
        int Offset_ChunkSize = 0x3F6C;
        ulong Offset_ObjID = 0x000C;

        ulong Offset_CharacterName = 0x1AE8;

        ulong Offset_CurrentLevel = 0x01A8;
        ulong Offset_Actors = 0x0048;
        ulong Offset_ItemPackage = 0x0560;
        ulong Offset_AimOffsets = 0x16B0;
        ulong Offset_ItemInformationComponent = 0x00A8;
        ulong Offset_ItemID = 0x0248;
        ulong Offset_DroppedItem = 0x0420;
        ulong Offset_DroppedItemGroup = 0x01A8;
        ulong Offset_DroppedItemGroup_UItem = 0x0738;
        ulong Offset_SpectatedCount = 0x1438;
        ulong Offset_WorldLocation = 0x02DC;
        ulong Offset_Mesh = 0x0480;
        ulong Offset_Health = 0x11E8;
        ulong Offset_PlayerState = 0x0410;
        ulong Offset_LastTeamNum = 0x1408;
        ulong Offset_PlayerController = 0x0030;
        ulong Offset_LocalPlayersPTR = 0x091695F0;
        ulong Offset_PlayerCameraManager = 0x04A0;
        ulong Offset_CameraLocation = 0x15C8;
        public static uint Offset_XorKey1 = 0x520D75E6;
        public static long Offset_XorKey2 = 0xEB27ADE8;
        public static int Offset_RorValue = 0x0C;
        public static bool Offset_IsingRor = true;
        ulong Offset_PlayerStatistics = 0x0A54;
        ulong Offset_RootComponent = 0x0270;
        ulong Offset_ComponentLocation = 0x0310;
        #endregion 
        public delegate ulong DecryptData(ulong c);
        public event Action<PubgModel> OnPlayerListUpdate;
        DecryptData decryptFunc;
        public pubg()
        {


        }
        private void GetMemMap()
        {
            try
            {
                var map = vmm.Map_GetPhysMem();
                if (map.Length == 0) throw new Exception("Map_GetPhysMem() returned no entries!");
                string mapOut = "";
                for (int i = 0; i < map.Length; i++)
                {
                    mapOut += $"{map[i].pa.ToString("x")} {(map[i].pa + map[i].cb - 1).ToString("x")}\n";
                }
                File.WriteAllText("mmap.txt", mapOut);
            }
            catch (Exception ex)
            {
                // handle error
            }
        }
        public bool Init()
        {
            try
            {
                vmm = new Vmm("", "-device", "fpga");
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
                    return true;
                }
            }
            catch (Exception ex)
            {


            }

            return false;
        }
        DispatcherTimer timer;
        public void Start()
        {
            Task.Run(() =>
            {
                while (true)
                {

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    VmmScatter scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
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

                        string mapName = GetObjName(MapId);
                        if (mapName == "TslLobby_Persistent_Main")
                        {
                            return;
                        }

                        
                        model.MapName = mapName;
                        List<PlayerModel> ListPlayer = new List<PlayerModel>();
                        for (int i = 0; i < Actorscount; i++)
                        {
                            try
                            {
                                scatter.Prepare(actorBase + (ulong)i * 8, 8);
                                
                                
                                //if (objName == "PlayerMale_A_C" || objName == "PlayerFemale_A_C" || objName == "AIPawn_Base_Female_C" || objName == "AIPawn_Base_Male_C" || objName == "UltAIPawn_Base_Female_C" || objName == "UltAIPawn_Base_Male_C")
                                //{
                                //    #region 读取血量和坐标
                                //    PlayerModel player = new PlayerModel();
                                //    string name = vmm.MemReadString(pid, vmm.MemReadInt64(pid, pObjPointer + Offset_CharacterName), 64);
                                //    player.Name = name;
                                //    float hp = vmm.MemReadFloat(pid, pObjPointer + Offset_Health);
                                //    player.HP = hp;
                                //    #region 读取骨骼
                                //    ulong MeshAddr = vmm.MemReadInt64(pid, pObjPointer + Offset_Mesh);
                                //    byte[] 敌人坐标 = vmm.MemRead(pid, MeshAddr + Offset_ComponentLocation, 12);
                                //    float x = BitConverter.ToSingle(敌人坐标, 0);
                                //    float y = BitConverter.ToSingle(敌人坐标, 4);
                                //    float z = BitConverter.ToSingle(敌人坐标, 8);
                                //    Vector3D actorLocation = new Vector3D(x, y, z);
                                //    player.ActorLocation = actorLocation;
                                //    int w = vmm.MemReadInt(pid, world + Offset_WorldLocation);
                                //    int h = vmm.MemReadInt(pid, world + Offset_WorldLocation + 0x4);
                                //    player.x = x + w;
                                //    player.y = y + h;
                                //    player.z = z;
                                //    if (objName == "PlayerMale_A_C" || objName == "PlayerFemale_A_C")
                                //    {
                                //        player.isBot = false;
                                //    }
                                //    else if (objName == "AIPawn_Base_Female_C" || objName == "AIPawn_Base_Male_C" || objName == "UltAIPawn_Base_Female_C" || objName == "UltAIPawn_Base_Male_C")
                                //    {
                                //        player.isBot = true;
                                //    }
                                //    if (player.x < 0)
                                //    {
                                //        player.x = -player.x;
                                //    }
                                //    if (player.y < 0)
                                //    {
                                //        player.y = -player.y;
                                //    }
                                //    if (player.z < 0)
                                //    {
                                //        player.z = -player.z;
                                //    }
                                //    #endregion

                                //    //ListPlayer.Add(player);
                                //    //continue;
                                //    #region 读取观战人数
                                //    player.SpectatedCount = vmm.MemReadInt(pid, pObjPointer + Offset_SpectatedCount);
                                //    #endregion
                                //    Vector v1 = new Vector(1, 1);
                                //    Vector v2 = new Vector(1, 1);
                                //    //团队编号
                                //    int teamNum = vmm.MemReadInt(pid, pObjPointer + Offset_LastTeamNum);
                                //    if (teamNum == 100000 || teamNum > 100000)
                                //    {
                                //        player.TeamId = teamNum - 100000;
                                //    }

                                //    //读取杀敌数量
                                //    ulong PlayerState = decryptFunc(vmm.MemReadInt64(pid, pObjPointer + Offset_PlayerState));
                                //    if (PlayerState > 0x1000)
                                //    {
                                //        player.KillCount = vmm.MemReadInt(pid, PlayerState + Offset_PlayerStatistics);
                                //    }
                                //    else
                                //    {
                                //        player.KillCount = 0;
                                //    }
                                //    //  Common.dec_objid();
                                //    //读取方向
                                //    float orientation = vmm.MemReadFloat(pid, pObjPointer + Offset_AimOffsets + 0x4);

                                //    player.Orientation = orientation;
                                //    //
                                //    Vector3D cameraLocation = vmm.MemReadVector(pid, CameraManager + Offset_CameraLocation);


                                //    Vector3D aimFov = (actorLocation - cameraLocation);
                                //    var tempV = (actorLocation - cameraLocation);
                                //    float Radpi = (float)(180 / 3.1415926535f);
                                //    float Yaw = (float)Math.Atan2(tempV.Y, tempV.X) * Radpi;
                                //    float Pitch = (float)Math.Atan2(z, Math.Sqrt((tempV.X * tempV.X) + (tempV.Y * tempV.Y))) * Radpi;
                                //    float Roll = 0;
                                //    aimFov = new Vector3D(Yaw, Pitch, Roll);
                                //    float AmiMz = vmm.MemReadFloat(pid, pObjPointer + Offset_AimOffsets);
                                //    float AimX = (float)Math.Abs(aimFov.X - AmiMz);

                                //    bool bIsAimed = (AimX > -5 && AimX < 5);
                                //    player.bIsAimed = bIsAimed;

                                //    float Distance = (float)(cameraLocation - actorLocation).Length / 100;
                                //    player.Distance = Distance;

                                //    //ulong PlayerMesh = vmm.MemReadInt64(pid, pObjPointer + Offset_Mesh);
                                //    //int actorLocationX = vmm.MemReadInt(pid,PlayerMesh + Offset_ComponentLocation);
                                //    //int actorLocationY = vmm.MemReadInt(pid, PlayerMesh + Offset_ComponentLocation+4);
                                //    //Console.WriteLine();
                                //    //int X = vmm.MemReadInt(pid,world + Offset_WorldToMap);
                                //    //int Y = vmm.MemReadInt(pid, world + Offset_WorldToMap + 0x4);
                                //    //读取坐标
                                //    //ulong RootComponent = decryptFunc(vmm.MemReadInt64(pid, pObjPointer + Offset_RootComponent));
                                //    //byte[] temp = vmm.MemRead(pid, RootComponent + Offset_ComponentLocation, 12);
                                //    //float x = BitConverter.ToSingle(temp, 0);
                                //    //float y = BitConverter.ToSingle(temp, 4);
                                //    //float z = BitConverter.ToSingle(temp, 8);
                                //    //player.x = x;
                                //    //player.y = y;
                                //    //player.z = z;
                                //    ListPlayer.Add(player);
                                //    #endregion

                                //}
                                //else if (objName == "DroppedItemGroup")
                                //{
                                //    //这个地方就是物资的
                                //    var ItemGroupPtr = vmm.MemReadInt64(pid, pObjPointer + Offset_DroppedItemGroup);

                                //    var ItemCount = vmm.MemReadInt32(pid, pObjPointer + Offset_DroppedItemGroup + 0x8);
                                //    if (ItemGroupPtr > 0 && ItemCount > 0)
                                //    {
                                //        List<PubgGood> goods = new List<PubgGood>();
                                //        for (int itemIndex = 0; itemIndex < ItemCount; itemIndex++)
                                //        {
                                //            var ItemObject = vmm.MemReadInt64(pid, ItemGroupPtr + (ulong)(itemIndex * 0x10));
                                //            if (ItemObject > 0)
                                //            {
                                //                var UItemAddress = vmm.MemReadInt64(pid, ItemObject + Offset_DroppedItemGroup_UItem);
                                //                if (UItemAddress > 0)
                                //                {
                                //                    var UItemID = vmm.MemReadInt32(pid, vmm.MemReadInt64(pid, UItemAddress + Offset_ItemInformationComponent) + Offset_ItemID);
                                //                    if (UItemID > 0 && UItemID < 0xfff0ff)
                                //                    {
                                //                        string UItemName = GetObjName(UItemID);
                                //                        var v3d = vmm.MemReadVector(pid, ItemObject + Offset_ComponentLocation);
                                //                        PubgGood good = new PubgGood();
                                //                        good.ClassName = UItemName;

                                //                        int w = vmm.MemReadInt(pid, world + Offset_WorldLocation);
                                //                        int h = vmm.MemReadInt(pid, world + Offset_WorldLocation + 0x4);
                                //                        var tempv3 = new Vector3D(w, h, 0) + v3d;
                                //                        good.x = (int)tempv3.X;
                                //                        good.y = (int)tempv3.Y;
                                //                        goods.Add(good);
                                //                        //auto pObjName = Tsl::GetGNamesByObjID(UItemID);
                                //                    }
                                //                }
                                //            }

                                //        }
                                //        model.PubgGoods = goods;
                                //    }
                                //}

                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine("11111:" + ex.Message);
                            }
                        }
                        bool isExec = scatter.Execute();
                        List<ZhiZhenModel> ListZhiZhenModel = new List<ZhiZhenModel>();
                        #region 读取所有类名
                        for (int i = 0; i < Actorscount; i++)
                        {
                            ulong pObjPointer = scatter.ReadUInt64(actorBase + (ulong)i * 8);
                            if (pObjPointer > 0x100000)
                            {
                                ListZhiZhenModel.Add(new ZhiZhenModel() { pObjPointer = pObjPointer });
                               
                               
                            }
                        }
                        //准备actorId
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
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
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
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
                        ListZhiZhenModel=ListZhiZhenModel.Where(x => x.fNamePtr > 0).ToList();
                        //准备fName
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in ListZhiZhenModel)
                        {
                            item.fNamePtr = item.fNamePtr;
                            scatter.Prepare(item.fNamePtr + (ulong)(item.objId % Offset_ChunkSize) * 0x8,8);
                        }
                          isExec = scatter.Execute();
                        //读取fName，
                        foreach (var item in ListZhiZhenModel)
                        {
                            ulong fName = scatter.ReadUInt64(item.fNamePtr + (ulong)(item.objId % Offset_ChunkSize) * 0x8);
                            if (item.fName > 0)
                            {
                                item.fName = fName;
                            }
                        }
                        //准备className
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        ListZhiZhenModel=ListZhiZhenModel.Where(x => x.fName > 0).ToList();
                        foreach (var item in ListZhiZhenModel)
                        {
                            scatter.Prepare(item.fName + 0x10, 64);
                        }
                        scatter.Execute();
                        //读取className
                        foreach (var item in ListZhiZhenModel)
                        {
                            string className = scatter.ReadStringASCII(item.fName + 0x10, 64);
                                item.className= className;
                        }
                        #endregion
                        #region 读取玩家名字 
                        //准备读取CharacterId
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
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
                             
                                scatter.Prepare(item.pObjPointer + Offset_CharacterName,8);
                             
                        }
                        scatter.Execute();
                        //读取CharacterId
                        foreach (var item in listPlay)
                        {
                            item.CharacterId= scatter.ReadUInt64(item.pObjPointer + Offset_CharacterName);
                        }
                        //准备读取CharacterName
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in listPlay)
                        {
                            if (item.CharacterId>0)
                            {
                                scatter.Prepare(item.CharacterId, 64);
                            }
                        }
                        scatter.Execute();
                        //读取CharacterName
                        foreach (var item in listPlay)
                        {
                            if (item.CharacterId>0)
                            {
                                item.Name = scatter.ReadStringUnicode(item.CharacterId, 64);
                            }
                        }
                        #endregion
                        
                        #region 读取hp
                        //准备读取hp
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in listPlay)
                        {
                            scatter.Prepare(item.pObjPointer + Offset_Health, 4);
                        }
                        //读取hp
                        scatter.Execute();
                        foreach (var item in listPlay)
                        {
                            item.Hp = scatter.ReadFloat(item.pObjPointer + Offset_Health);
                        }
                        #endregion
                        #region 读取观战人数 
                        //准备读取观战人数
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in listPlay)
                        {
                            scatter.Prepare(item.pObjPointer + Offset_SpectatedCount, 4);
                        }
                        //读取观战人数
                        scatter.Execute();
                        foreach (var item in listPlay)
                        {
                            item.SpectatedCount = scatter.ReadInt(item.pObjPointer + Offset_SpectatedCount);
                        }
                        #endregion
                        #region 读取团队编号
                        //准备读取团队编号
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in listPlay)
                        {
                            scatter.Prepare(item.pObjPointer + Offset_LastTeamNum, 4);
                        }
                        //读取团队编号
                        scatter.Execute();
                        foreach (var item in listPlay)
                        {
                            int teamNum= scatter.ReadInt(item.pObjPointer + Offset_LastTeamNum);
                            if (teamNum == 100000 || teamNum > 100000)
                            {
                                item.teamNum= teamNum - 100000;
                            }
                        }
                        #endregion
                        #region 读取杀敌数量
                        //准备读取PlayerState
scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
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
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in listPlay)
                        {
                            if (item.PlayerState > 0x1000)
                            {
                                scatter.Prepare(item.PlayerState + Offset_PlayerStatistics, 4);
                            }
                        }
                        //读取KillCount
                        scatter.Execute();
                        foreach (var item in listPlay)
                        {
                            if (item.PlayerState > 0)
                            {
                                item.KillCount = scatter.ReadInt(item.PlayerState + Offset_PlayerStatistics);
                            }
                        }
                        #endregion  
                        #region 读取方向
//准备读取方向
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        
                        foreach (var item in listPlay)
                        {
                            scatter.Prepare(item.pObjPointer + Offset_AimOffsets + 0x4, 4);
                        }
                        //读取方向
                        scatter.Execute();
                        foreach (var item in listPlay)
                        {
                            item.Orientation = scatter.ReadFloat(item.pObjPointer + Offset_AimOffsets + 0x4);
                        }
                        #endregion
                         
                        #region 读取坐标
//准备读取MeshAddr
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);    
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
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
                        foreach (var item in listPlay)
                        {
                            if (item.MeshAddr > 0)
                            {
                                scatter.Prepare(item.MeshAddr + Offset_ComponentLocation, 12);
                            }
                        }
                        //读取Offset_ComponentLocation
                        scatter.Prepare(world + Offset_WorldLocation, 12);
                        scatter.Execute();
                        foreach (var item in listPlay)
                        {
                            if (item.MeshAddr > 0)
                            {
                                float X = scatter.ReadFloat(item.MeshAddr + Offset_ComponentLocation);
                                float Y = scatter.ReadFloat(item.MeshAddr + Offset_ComponentLocation + 0x4);
                                float Z = scatter.ReadFloat(item.MeshAddr + Offset_ComponentLocation + 0x8);
                                float w= scatter.ReadInt(world + Offset_WorldLocation);
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
                            }

                            if (item.className=="PlayerMale_A_C"||item.className=="PlayerFemale_A_C")
                            {
                                item.isBot = false;
                            } else if (item.className == "AIPawn_Base_Female_C" || item.className == "AIPawn_Base_Male_C" || item.className == "UltAIPawn_Base_Female_C" || item.className == "UltAIPawn_Base_Male_C")
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
                         #region 读取AmiMz
                        //准备读取AmiMz
                        scatter = vmm.Scatter_Initialize(pid, Vmm.FLAG_NOCACHE);
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
                           item.bIsAimed= (AimX > -5 && AimX < 5);
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
                                 ActorLocation =item.actorLocation
                           });
                        }

                         #endregion
                        model.Player = ListPlayer;
                        if (OnPlayerListUpdate != null)
                        {
                            OnPlayerListUpdate(model);
                        }
                    }
                    sw.Stop();
                    Console.WriteLine("dma:" + sw.ElapsedMilliseconds);

                }
            });



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
