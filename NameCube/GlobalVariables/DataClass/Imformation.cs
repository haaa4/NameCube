using Newtonsoft.Json;
<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d

namespace NameCube.GlobalVariables.DataClass
{
    public class ImformationData
    {
        /// <summary>
        /// 推荐过更好的颜色
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsedBetterColor { get; set; } = false;
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
