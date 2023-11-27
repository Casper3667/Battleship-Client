// See https://aka.ms/new-console-template for more information
using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings;


Settings.LoadSettings();
Console.WriteLine("Hello, World!");
LoginServiceInterface loginServiceInterface = new LoginServiceInterface();
LobbyServiceInterface lobbyServiceInterface = new LobbyServiceInterface();
GameServerInterface gameServerInterface = new GameServerInterface();
string token= loginServiceInterface.LoginScreen();
Console.WriteLine("After LoginScreen\nToken: "+token);
string adress=lobbyServiceInterface.JoinLobby(token);
Console.WriteLine("After Game Server Lobby\nGame Server Adress: " + adress);
gameServerInterface.Run(adress,token);