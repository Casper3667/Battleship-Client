using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Settings.NetworkSettings
{
    public class NetworkSettings
    {
        public string Name { get; set; }
        public string LoginIP { get; set; }
        public int LoginPORT { get; set; }
        public string LobbyIP { get; set; }
        public int LobbyPORT { get; set; }
        public int LobbyMaxWaitTimeSeconds { get; set; }
    }
}
