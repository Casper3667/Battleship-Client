using BattleShip_ClientService.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Interfaces
{
    internal class LobbyServiceInterface
    {
        TcpClient client;
        string LobbyIp;
        int LobbyPort;
        string Token;

        const string SucsessMessage = "Success";
        const string GetServerErrorMessage = "404 Timer for Getting Server IP Ran Out";
        public LobbyServiceInterface() 
        {
            client = new TcpClient();
            LobbyIp = Settings.Settings.NetworkSettings.LobbyIP;
            LobbyPort = Settings.Settings.NetworkSettings.LobbyPORT;

        }


        /// <summary>
        /// Joins Game Server Lobby 
        /// </summary>
        /// <returns>Adress for Game Server</returns>
        public  string? JoinLobby(string Token)
        {
            Console.Clear();
            Console.WriteLine("Joining Game Lobby");
            this.Token = Token;
            
            try
            {
                Console.WriteLine("Connecting To Game Lobby");
                client.Connect(LobbyIp, LobbyPort);
                Debug.WriteLine("Connected to Game Lobby");
                if(client.Connected)
                {
                    NetworkStream stream=client.GetStream();
                    //Console.WriteLine("Waiting For GoAhead to Send JWT");
                    //RecieveMessage(stream);
                    
                    Console.WriteLine("Sending Token to Game Lobby");
                    JWTHandler.SendJWTToken(Token, stream);

                    Console.WriteLine("Waiting for Response on Token");
                    string response =  JWTHandler.RecieveJWTFeedback(stream,TimeSpan.FromSeconds(10));
                    if (response == SucsessMessage)
                    {
                        Console.WriteLine("Got Positive Response about Token from Lobby");
                        Console.WriteLine("Waiting For Server Adress");
                        string adress = ListenForServerAdress(stream, TimeSpan.FromSeconds(Settings.Settings.NetworkSettings.LobbyMaxWaitTimeSeconds));
                        if (adress != GetServerErrorMessage)
                        {
                            Console.WriteLine("Got Server Adress");
                            return HandleServerAdress(adress);
                        }
                        else
                        {
                            Console.WriteLine("Lobbby didnt Send a Server Adress within the Time Limit");
                            return null;
                        }
                    }
                    else
                        Console.WriteLine("Got Negative Response to Token");

                }
            }
            catch (Exception)
            {

                throw;
            }


            return null;
        }
        private string? RecieveMessage(NetworkStream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();
        }
        private string ListenForServerAdress(NetworkStream stream,TimeSpan Timeout)
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
                message = GetServerErrorMessage;
                 Debug.Fail("Didnt Get A Server IP in Given Time");
            }
            Debug.WriteLine("Message Regarding Server IP: " + message);

            return message;



            //TimeSpan timer = Timeout;
            //bool gottenMessage = false;

            //StreamReader reader = new StreamReader(stream);

            //string message = "";
            //while (gottenMessage == false && timer > TimeSpan.Zero)
            //{
            //    string? response = reader.ReadLine();
            //    if (response != null)
            //    {
            //        message = (string)response;
            //        gottenMessage = true;
            //    }
            //    else
            //    {
            //        Thread.Sleep(100);
            //        timer.Subtract(TimeSpan.FromMilliseconds(100));
            //    }

            //}

            //if (message == "")
            //{
            //    message = GetServerErrorMessage;
            //    Debug.Fail("Didnt Get A Server IP in Given Time");
            //}

            //Debug.WriteLine("Message Regarding Server IP: " + message);

            //return message;
        }
        

        public string HandleServerAdress(string adressMessage)
        {
            return adressMessage;
        }
    }
}
