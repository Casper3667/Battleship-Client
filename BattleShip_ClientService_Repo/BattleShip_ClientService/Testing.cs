using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService
{
    public static class Testing
    {
        public static bool IsTesting  { get; set; }
    
        public static void Print(string message)
        {
            if(IsTesting)
                Console.WriteLine(message);
        }
    }
}
