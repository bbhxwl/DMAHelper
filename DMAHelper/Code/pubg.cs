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

namespace DMAHelper
{
    public class pubg
    {
        Vmm vmm;
        uint pid = 0;
        #region 偏移
        ulong moduleBase;
        ulong GNamesAddress;

        ulong Offset_GWorld = 0x08CC1F50;
        ulong Offset_XenuineDecrypt = 0x0708D028;
        ulong Offset_FNameEntry = 0x08E961A0;
        int Offset_ChunkSize = 0x3EB0;
        ulong Offset_CharacterName = 0x0F88;
        ulong Offset_ObjID = 0x0010;
        ulong Offset_CurrentLevel = 0x00D0;
        ulong Offset_Actors = 0x00D0;
        ulong Offset_ItemPackage = 0x0560;
        ulong Offset_ItemInformationComponent = 0x00B0;
        ulong Offset_ItemID = 0x0248;
        ulong Offset_DroppedItem = 0x0420;
        ulong Offset_DroppedItemGroup = 0x0280;
        ulong Offset_DroppedItemGroup_UItem = 0x0728;

        public static uint Offset_XorKey1 = 0x6428D89B;
        public static long Offset_XorKey2 = 0xB3BC2464;
        public static int Offset_RorValue = 0x09;
        public static bool Offset_IsingRor = true;
        ulong Offset_RootComponent = 0x0388;
        ulong Offset_ComponentLocation = 0x02C0;
        #endregion 
        public delegate ulong DecryptData(ulong c);
        public event Action<List<PlayerModel>> OnPlayerListUpdate;
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
                    ulong world = decryptFunc(vmm.MemReadInt64(pid, moduleBase + Offset_GWorld));
                    ulong PersistentLevel = decryptFunc(vmm.MemReadInt64(pid, world + Offset_CurrentLevel));
                    ulong ActorsArray = decryptFunc(vmm.MemReadInt64(pid, PersistentLevel + Offset_Actors));
                    uint Actorscount = vmm.MemReadInt32(pid, ActorsArray + 0x08);
                    ulong actorBase = vmm.MemReadInt64(pid, ActorsArray);
                    ulong GNames = decryptFunc(vmm.MemReadInt64(pid, moduleBase + Offset_FNameEntry));
                    GNamesAddress = decryptFunc(vmm.MemReadInt64(pid, GNames));
                    List<PlayerModel> ListPlayer = new List<PlayerModel>();
                    for (int i = 0; i < Actorscount; i++)
                    {
                        ulong pObjPointer = vmm.MemReadInt64(pid, actorBase + (ulong)i * 8);
                        if (pObjPointer < 0x100000)
                            continue;
                        int actorId = (int)vmm.MemReadInt32(pid, pObjPointer + 0x10);
                        uint objId = Common.dec_objid(actorId);
                        string objName = GetObjName(objId);
                        if (objName == "PlayerMale_A_C" || objName == "PlayerFemale_A_C")
                        {
                            #region 读取血量和坐标
                            PlayerModel player = new PlayerModel();
                            string name = vmm.MemReadString(pid, vmm.MemReadInt64(pid, pObjPointer + Offset_CharacterName), 64);
                            player.Name = name;
                            float hp = vmm.MemReadFloat(pid, pObjPointer + 0x0AE4);
                            player.HP = hp;
                            //读取坐标
                            ulong RootComponent = decryptFunc(vmm.MemReadInt64(pid, pObjPointer + Offset_RootComponent));
                            byte[] temp = vmm.MemRead(pid, RootComponent + Offset_ComponentLocation, 12);
                            float x = BitConverter.ToSingle(temp, 0);
                            float y = BitConverter.ToSingle(temp, 4);
                            float z = BitConverter.ToSingle(temp, 8);
                            player.x = x;
                            player.y = y;
                            player.z = z;
                            ListPlayer.Add(player);
                            #endregion

                        }
                        else if (objName == "DroppedItemGroup")
                        {
                            //这个地方就是物资的
                            var ItemGroupPtr = vmm.MemReadInt64(pid, pObjPointer + Offset_DroppedItemGroup);

                            var ItemCount = vmm.MemReadInt32(pid, pObjPointer + Offset_DroppedItemGroup + 0x8);
                            if (ItemGroupPtr>0&& ItemCount>0)
                            {
                                for (int itemIndex = 0; itemIndex < ItemCount; itemIndex++)
                                {
                                    var ItemObject = vmm.MemReadInt64(pid, ItemGroupPtr + (ulong)(itemIndex * 0x10));
                                    if (ItemObject > 0)
                                    {
                                        var UItemAddress = vmm.MemReadInt64(pid, ItemObject + Offset_DroppedItemGroup_UItem);
                                        if (UItemAddress > 0)
                                        {
                                            var UItemID = vmm.MemReadInt32(pid, vmm.MemReadInt64(pid, UItemAddress + Offset_ItemInformationComponent) + Offset_ItemID);
                                            if (UItemID > 0 && UItemID < 0xfff0ff)
                                            {
                                                //auto pObjName = Tsl::GetGNamesByObjID(UItemID);
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        // Console.WriteLine(objName);


                    }

                    if (OnPlayerListUpdate != null)
                    {
                        OnPlayerListUpdate(ListPlayer);
                    }
                    Thread.Sleep(10);
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
                    List<byte> newByte = new List<byte>();
                    for (int ii = 0; ii < nameByte.Count(); ii++)
                    {
                        if (nameByte[ii] == 0)
                            break;
                        newByte.Add(nameByte[ii]);
                    }


                    //获取类名
                    string name = Encoding.ASCII.GetString(newByte.ToArray());

                    return name;
                }


            }
            return null;
        }
    }
}
