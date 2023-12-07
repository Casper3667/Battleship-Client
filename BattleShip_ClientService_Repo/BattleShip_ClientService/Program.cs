// See https://aka.ms/new-console-template for more information
using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings;
using System.Diagnostics;

Settings.LoadSettings();
//Console.WriteLine("Hello, World!");
LoginServiceInterface loginServiceInterface = new LoginServiceInterface();
LobbyServiceInterface lobbyServiceInterface = new LobbyServiceInterface();
GameServerInterface gameServerInterface = new GameServerInterface();

    Task<string> loginTask= loginServiceInterface.LoginScreen();
//loginTask.Start();
loginTask.Wait();
if(loginTask.IsCompletedSuccessfully)
{
    string token=loginTask.Result;

    Debug.WriteLine("After LoginScreen\nToken: " + token);
    
    string? adress = lobbyServiceInterface.JoinLobby(token);
    if(adress != null)
    {
        Console.WriteLine("After Game Server Lobby\nGame Server Adress: " + adress);
        gameServerInterface.Run(adress, token);
    }
    else
    {
        Console.WriteLine("Didn't Connect");
    }
   
}
