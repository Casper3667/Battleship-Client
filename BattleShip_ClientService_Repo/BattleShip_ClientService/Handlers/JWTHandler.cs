using BattleShip_ClientService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Handlers
{
    public static class JWTHandler
    {
        
        public static void SendJWTToken(string jwt, NetworkStream stream)
        {
            StreamWriter writer = new StreamWriter(stream);
            Console.WriteLine($"Sending this JWT:[{jwt}]");
            writer.WriteLine(jwt);
            writer.Flush();
        }
        public static string RecieveJWTFeedback(NetworkStream stream, TimeSpan Timeout)
        {
            StreamReader reader = new StreamReader(stream);
            string? response = reader.ReadLine();
            string message;
            if (response != null)
            {
                Console.WriteLine("Got RESPONSE");
                message = (string)response;
                //gottenMessage = true;
            }
            else 
            {
                message = "404 Got A Null Message Back";
                Debug.Fail("Got A Null Message Back");
            }
            Debug.WriteLine("Message Regarding JWT: " + message);

            return message;

            //TimeSpan timer = Timeout;
            //bool gottenMessage = false;

           

            //string message="";

            //(int x,int y)=Console.GetCursorPosition();

            //while (gottenMessage==false && timer>TimeSpan.Zero)
            //{
            //    Console.SetCursorPosition(x, y);
            //    Console.WriteLine($"Getting JWT RESPONSE: Timer is at {timer.TotalSeconds} of {Timeout.TotalSeconds} Seconds");
            //    string? response =  reader.ReadLine();
            //    if(response != null)
            //    {
            //        Console.WriteLine("Got RESPONSE");
            //        message=(string)response; 
            //        gottenMessage = true;
            //    }
            //    else
            //    {
            //        Thread.Sleep(100);
            //        timer.Subtract(TimeSpan.FromMilliseconds(100));
            //    }

            //}

            //if(message=="")
            //{
            //    message = "404 Timer for Getting JWT Ran Out";
            //    Debug.Fail("Didnt Get A Message Back About JWT in Given Time");
            //}


        }
        public static string RecieveJWTFeedbackServer(NetworkStream stream)
        {
            string message = RecieveData(stream);

            return message;

            

        }
        internal static string RecieveData(NetworkStream stream)
        {
            StringBuilder sb = new StringBuilder();
            byte[] tempBytes = new byte[1];
            while (true)
            {
                int bytesRead = stream.Read(tempBytes);
                if (bytesRead == 0) { break; }
                string recievedData = Encoding.UTF8.GetString(tempBytes);
                if (GameServerInterface.useEndMessage)
                {
                    if (recievedData.Contains(GameServerInterface.END_OF_MESSAGE)) { break; }
                    sb.Append(recievedData);
                }
                else
                {
                    sb.Append(recievedData);
                    break;
                }


            }
            return sb.ToString();
        }
    }
}
