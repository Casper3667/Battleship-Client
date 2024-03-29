﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BattleShip_ClientService.Interfaces.GameServerInterface;

namespace BattleShip_ClientService.Messages
{
    public interface IClientMessage
    {
    }

    public class ShotMessage : IClientMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool ValidShotMessage { get; set; }

        public ShotMessage(int x, int y, bool validShotMessage=true)
        {
            X = x;
            Y = y;
            ValidShotMessage = validShotMessage;
        }
    }
    public class RawChatMessageFromClient : IClientMessage
    {
        public string From { get; set; }
        public ChatType To { get; set; }
        public string Message { get; set; }
        public bool ValidRawChatMessageFromClient { get; set; }
        public RawChatMessageFromClient(string from,ChatType to, string message,bool valid=true)
        {
            From= from;
            To= to;
            Message= message;
            ValidRawChatMessageFromClient = valid;

        }
    }
}
