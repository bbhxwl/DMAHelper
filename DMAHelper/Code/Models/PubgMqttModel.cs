using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMAHelper.Code.Models
{
    public class PubgMqttModel
    {
        public string Map { get; set; }
        public List<List<int>> Game { get; set; }
        public List<List<int>> Car { get; set; }
        public List<string> Box { get; set; }
        public List<string> Goods { get; set; }
        public List<List<object>> Player { get; set; }
    }
}
