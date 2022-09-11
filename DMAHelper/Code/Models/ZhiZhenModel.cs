using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMAHelper.Code.Models
{
    public  class ZhiZhenModel
    { 
        public ulong pObjPointer { get; set; }
        public int actorId { get; set; }
        public uint objId { get; set; }

        public ulong fNamePtr { get; set; }
        public ulong fName { get; set; }
        public string className { get; set; }
    }
}
