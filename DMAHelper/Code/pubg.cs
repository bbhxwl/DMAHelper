﻿using System;
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

namespace DMAHelper
{
    public class pubg
    {
        Vmm vmm;
        uint pid = 0;
        #region 偏移
        ulong moduleBase;
        ulong GNamesAddress;

        ulong Offset_GWorld = 0x08D9B380;
        ulong Offset_XenuineDecrypt = 0x07178E28;
        ulong Offset_FNameEntry = 0x08F70440;
        int Offset_ChunkSize = 0x41F4;
        ulong Offset_CharacterName = 0x0EE0;
        ulong Offset_ObjID = 0x0018;
        ulong Offset_CurrentLevel = 0x0808;
        ulong Offset_Actors = 0x0250;
        ulong Offset_ItemPackage = 0x0568;
        ulong Offset_AimOffsets = 0x1750;
        ulong Offset_ItemInformationComponent = 0x00A8;
        ulong Offset_ItemID = 0x0248;
        ulong Offset_DroppedItem = 0x0428;
        ulong Offset_DroppedItemGroup = 0x0190;
        ulong Offset_DroppedItemGroup_UItem = 0x0728;
        ulong Offset_SpectatedCount = 0x21E8;
        ulong Offset_WorldToMap = 0x0268;
        ulong Offset_Mesh = 0x0510;
        ulong Offset_PlayerState = 0x0430;
        ulong Offset_LastTeamNum = 0x1B60;
        ulong Offset_PlayerController = 0x0030;
        ulong Offset_LocalPlayersPTR = 0x08ED3600;
        ulong Offset_PlayerCameraManager = 0x04A0;
        ulong Offset_CameraLocation = 0x1744;
        public static uint Offset_XorKey1 = 0x29A03FC5;
        public static long Offset_XorKey2 = 0x65160B55;
        public static int Offset_RorValue = 0x0D;
        public static bool Offset_IsingRor = false;
        ulong Offset_PlayerStatistics = 0x04D8;
        ulong Offset_RootComponent = 0x0260;
        ulong Offset_ComponentLocation = 0x0240;
        #endregion 
        public delegate ulong DecryptData(ulong c);
        public event Action<PubgModel> OnPlayerListUpdate;
        DecryptData decryptFunc;
        public pubg(Vmm m, uint pid)
        {
            this.vmm = m;
            this.pid = pid;
            moduleBase = vmm.ProcessGetModuleBase(pid, "TslGame.exe");
        }
        public bool Init()
        {
            try
            {
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
        public void Start()
        {
            Task.Run(() =>
            {
                while (true)
                {
                     
                        PubgModel model = new PubgModel();
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                        ulong world = decryptFunc(vmm.MemReadInt64(pid, moduleBase + Offset_GWorld));
                        ulong ULocalPlayer = vmm.MemReadInt64(pid, moduleBase + Offset_LocalPlayersPTR);
                        ulong PlayerController = decryptFunc(vmm.MemReadInt64(pid, ULocalPlayer + Offset_PlayerController));
                        ulong CameraManager = vmm.MemReadInt64(pid, PlayerController + Offset_PlayerCameraManager);
                        ulong PersistentLevel = decryptFunc(vmm.MemReadInt64(pid, world + Offset_CurrentLevel));
                        ulong ActorsArray = decryptFunc(vmm.MemReadInt64(pid, PersistentLevel + Offset_Actors));
                        uint Actorscount = vmm.MemReadInt32(pid, ActorsArray + 0x08);
                        ulong actorBase = vmm.MemReadInt64(pid, ActorsArray);
                        ulong GNames = decryptFunc(vmm.MemReadInt64(pid, moduleBase + Offset_FNameEntry));
                        GNamesAddress = decryptFunc(vmm.MemReadInt64(pid, GNames));

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
                                ulong pObjPointer = vmm.MemReadInt64(pid, actorBase + (ulong)i * 8);
                                if (pObjPointer < 0x100000)
                                    continue;
                                int actorId = (int)vmm.MemReadInt32(pid, pObjPointer + 0x10);
                                uint objId = Common.dec_objid(actorId);
                                string objName = GetObjName(objId);

                                if (objName == "PlayerMale_A_C" || objName == "PlayerFemale_A_C" || objName == "AIPawn_Base_Female_C" || objName == "AIPawn_Base_Male_C" || objName == "UltAIPawn_Base_Female_C" || objName == "UltAIPawn_Base_Male_C")
                                {
                                    #region 读取血量和坐标
                                    PlayerModel player = new PlayerModel();
                                    string name = vmm.MemReadString(pid, vmm.MemReadInt64(pid, pObjPointer + Offset_CharacterName), 64);
                                    player.Name = name;
                                    float hp = vmm.MemReadFloat(pid, pObjPointer + 0x0AE4);
                                    player.HP = hp;
                                    #region 读取骨骼
                                    ulong MeshAddr = vmm.MemReadInt64(pid, pObjPointer + Offset_Mesh);
                                    byte[] 敌人坐标 = vmm.MemRead(pid, MeshAddr + Offset_ComponentLocation, 12);
                                    float x = BitConverter.ToSingle(敌人坐标, 0);
                                    float y = BitConverter.ToSingle(敌人坐标, 4);
                                    float z = BitConverter.ToSingle(敌人坐标, 8);
                                    Vector3D actorLocation = new Vector3D(x, y, z);
                                    player.ActorLocation = actorLocation;
                                    int w = vmm.MemReadInt(pid, world + Offset_WorldToMap);
                                    int h = vmm.MemReadInt(pid, world + Offset_WorldToMap + 0x4);
                                    player.x = x + w;
                                    player.y = y + h;
                                    player.z = z;
                                    if (objName == "PlayerMale_A_C" || objName == "PlayerFemale_A_C")
                                    {
                                        player.isBot = false;
                                    }
                                    else if (objName == "AIPawn_Base_Female_C" || objName == "AIPawn_Base_Male_C" || objName == "UltAIPawn_Base_Female_C" || objName == "UltAIPawn_Base_Male_C")
                                    {
                                        player.isBot = true;
                                    }
                                    if (player.x < 0)
                                    {
                                        player.x = -player.x;
                                    }
                                    if (player.y < 0)
                                    {
                                        player.y = -player.y;
                                    }
                                    if (player.z < 0)
                                    {
                                        player.z = -player.z;
                                    }
                                    #endregion

                                    //ListPlayer.Add(player);
                                    //continue;
                                    #region 读取观战人数
                                    player.SpectatedCount = vmm.MemReadInt(pid, pObjPointer + Offset_SpectatedCount);
                                    #endregion
                                    Vector v1 = new Vector(1, 1);
                                    Vector v2 = new Vector(1, 1);
                                    //团队编号
                                    int teamNum = vmm.MemReadInt(pid, pObjPointer + Offset_LastTeamNum);
                                    if (teamNum == 100000 || teamNum > 100000)
                                    {
                                        player.TeamId = teamNum - 100000;
                                    }

                                    //读取杀敌数量
                                    ulong PlayerState = decryptFunc(vmm.MemReadInt64(pid, pObjPointer + Offset_PlayerState));
                                    if (PlayerState > 0x1000)
                                    {
                                        player.KillCount = vmm.MemReadInt(pid, PlayerState + Offset_PlayerStatistics);
                                    }
                                    else
                                    {
                                        player.KillCount = 0;
                                    }
                                    //  Common.dec_objid();
                                    //读取方向
                                    float orientation = vmm.MemReadFloat(pid, pObjPointer + Offset_AimOffsets + 0x4);

                                    player.Orientation = orientation;
                                    //
                                    Vector3D cameraLocation = vmm.MemReadVector(pid, CameraManager + Offset_CameraLocation);


                                    Vector3D aimFov = (actorLocation - cameraLocation);
                                    var tempV = (actorLocation - cameraLocation);
                                    float Radpi = (float)(180 / 3.1415926535f);
                                    float Yaw = (float)Math.Atan2(tempV.Y, tempV.X) * Radpi;
                                    float Pitch = (float)Math.Atan2(z, Math.Sqrt((tempV.X * tempV.X) + (tempV.Y * tempV.Y))) * Radpi;
                                    float Roll = 0;
                                    aimFov = new Vector3D(Yaw, Pitch, Roll);
                                    float AmiMz = vmm.MemReadFloat(pid, pObjPointer + Offset_AimOffsets);
                                    float AimX = (float)Math.Abs(aimFov.X - AmiMz);

                                    bool bIsAimed = (AimX > -5 && AimX < 5);
                                    player.bIsAimed = bIsAimed;

                                    float Distance = (float)(cameraLocation - actorLocation).Length / 100;
                                    player.Distance = Distance;

                                    //ulong PlayerMesh = vmm.MemReadInt64(pid, pObjPointer + Offset_Mesh);
                                    //int actorLocationX = vmm.MemReadInt(pid,PlayerMesh + Offset_ComponentLocation);
                                    //int actorLocationY = vmm.MemReadInt(pid, PlayerMesh + Offset_ComponentLocation+4);
                                    //Console.WriteLine();
                                    //int X = vmm.MemReadInt(pid,world + Offset_WorldToMap);
                                    //int Y = vmm.MemReadInt(pid, world + Offset_WorldToMap + 0x4);
                                    //读取坐标
                                    //ulong RootComponent = decryptFunc(vmm.MemReadInt64(pid, pObjPointer + Offset_RootComponent));
                                    //byte[] temp = vmm.MemRead(pid, RootComponent + Offset_ComponentLocation, 12);
                                    //float x = BitConverter.ToSingle(temp, 0);
                                    //float y = BitConverter.ToSingle(temp, 4);
                                    //float z = BitConverter.ToSingle(temp, 8);
                                    //player.x = x;
                                    //player.y = y;
                                    //player.z = z;
                                    ListPlayer.Add(player);
                                    #endregion

                                }
                                //else if (objName == "DroppedItemGroup")
                                //{
                                //    //这个地方就是物资的
                                //    var ItemGroupPtr = vmm.MemReadInt64(pid, pObjPointer + Offset_DroppedItemGroup);

                                //    var ItemCount = vmm.MemReadInt32(pid, pObjPointer + Offset_DroppedItemGroup + 0x8);
                                //    if (ItemGroupPtr > 0 && ItemCount > 0)
                                //    {
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

                                //                        //auto pObjName = Tsl::GetGNamesByObjID(UItemID);
                                //                    }
                                //                }
                                //            }

                                //        }
                                //    }
                                //}

                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine("11111:" + ex.Message);
                            }


                            // Console.WriteLine(objName);


                        }
                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds);
                        model.Player = ListPlayer;
                        if (OnPlayerListUpdate != null)
                        {
                            OnPlayerListUpdate(model);
                        }
                    
                  
                  
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

                    return name.Substring(0,name.IndexOf('\0')>=0? name.IndexOf('\0'): name.Length);
                }


            }
            return null;
        }
    }
}
