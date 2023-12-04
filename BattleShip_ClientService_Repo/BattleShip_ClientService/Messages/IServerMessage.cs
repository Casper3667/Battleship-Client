using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Messages
{
    
        public interface IServerMessage
        {

        }
        public class StartupMessage : IServerMessage
        {
            [JsonInclude]
            public string ClientID { get; set; }
            //public string DefenceScreen { get; set; }
            [JsonInclude]
            public bool GameReady { get; set; }
            [JsonInclude]
            public string? OtherPlayer { get; set; }




        }
        public class RawChatMessage : IServerMessage
        {
            [JsonInclude] public string From { get; set; } = "";
            [JsonInclude] public string To { get; set; } = "";
            [JsonInclude] public string Message { get; set; } = "";

            // public RawChatMessage() { }
        }
        //[System.AttributeUsage(System.AttributeTargets.Field| System.AttributeTargets.Property,AllowMultiple =false)]
        //[JsonAttribute(JsonIncludeAttribute)]

        public class RawGameStateMessage : IServerMessage
        {
            [JsonInclude] public string Opponent { get; set; } = "";
            [JsonInclude] public string LastAction { get; set; } = "";
            [JsonInclude] public string AttackScreen { get; set; } = "";
            [JsonInclude] public string DefenceScreen { get; set; } = "";
            [JsonInclude] public bool GameDone { get; set; }
            [JsonInclude] public bool IsLeading { get; set; }
            [JsonInclude] public bool PlayerTurn { get; set; }
            //public RawGameStateMessage() { }
        }

    
}
