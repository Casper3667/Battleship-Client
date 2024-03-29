﻿// See https://aka.ms/new-console-template for more information
using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings;
using System.Diagnostics;
const string validTokenforTesting = "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVXNlcm5hbWUiLCJleHAiOjE3MDIyMDY3MjZ9.8GQbviPdgD8J3iwgpqhS39splxsmbnE5IwJl4_Q-dd0";

Settings.LoadSettings();
//Console.WriteLine("Hello, World!");
LoginServiceInterface loginServiceInterface = new LoginServiceInterface();
LobbyServiceInterface lobbyServiceInterface = new LobbyServiceInterface();
GameServerInterface gameServerInterface = new GameServerInterface();

Task<string> loginTask = loginServiceInterface.LoginScreen();
//loginTask.Start();
loginTask.Wait();
if (loginTask.IsCompletedSuccessfully)
{
    string token = loginTask.Result;

    Debug.WriteLine("After LoginScreen\nToken: " + token);

    ServerAdress? adress = lobbyServiceInterface.JoinLobby(token);
    if (adress != null)
    {
        Console.WriteLine("After Game Server Lobby\nGame Server Adress:\nIP: " + adress.IP + "\nPort: " + adress.Port);
        Debug.WriteLine("After Game Server Lobby\nGame Server Adress:\nIP: " + adress.IP + "\nPort: " + adress.Port);
        gameServerInterface.Run(adress, token);
    }
    else
    {
        Console.WriteLine("Didn't Connect");
        Debug.WriteLine("Didn't Connect");
        //Debug.WriteLine("Trying To Connect Anyway");
        //ServerAdress temp=new ServerAdress() { IP = Settings.NetworkSettings.LobbyIP,Port=13000};
        //gameServerInterface.Run(temp, token);
    }

}
