using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube
{
    internal class InitializationAll
    {
        public static void InitializationData()
        {
            if(GlobalVariables.json.AllSettings==null)
            {
                GlobalVariables.json.AllSettings=new allSettings();
            }
            else
            {
                if(GlobalVariables.json.AllSettings.Name==null) 
                    GlobalVariables.json.AllSettings.Name = new List<string> { "张三" };
                if(GlobalVariables.json.AllSettings.Dark==null)    
                    GlobalVariables.json.AllSettings.Dark = false;
                if (GlobalVariables.json.AllSettings.Volume== null)
                    GlobalVariables.json.AllSettings.Volume  = 100;
                if (GlobalVariables.json.AllSettings.Speed== null)
                    GlobalVariables.json.AllSettings.Speed  = 0;
                if (GlobalVariables.json.AllSettings.Start== null)
                    GlobalVariables.json.AllSettings.Start  = false;
                if (GlobalVariables.json.AllSettings.SystemSpeech== null)
                    GlobalVariables.json.AllSettings.SystemSpeech = false;
                if (GlobalVariables.json.AllSettings.Top== null)
                    GlobalVariables.json.AllSettings.Top = true;
    }
        }
    }
}
