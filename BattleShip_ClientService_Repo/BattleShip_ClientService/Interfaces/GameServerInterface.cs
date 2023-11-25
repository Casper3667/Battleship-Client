﻿using BattleShip_ClientService.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Interfaces
{
    public class GameServerInterface
    {
        

        const string END_OF_MESSAGE = "#END#";
        bool useEndMessage;
        bool first = true;
        bool isCleared = false;

        bool KeyHasBeenPressed = false;
        bool IsInInputField = false;

        ConsoleColor DefaultBackgroundsColor = ConsoleColor.Black;
        ConsoleColor DefaultForegroundColor = ConsoleColor.Gray;


        TcpClient client;
        NetworkStream stream;

        Thread listeningThread;

        /// <summary>
        /// THIS MAY CHANGE
        /// </summary>
        bool running = true;

        Screen currentScreen;
       
        public Screen CurrentScreen { get { return currentScreen; } set{ currentScreen = value; } }

        int oldWidth;

        Stack<Screen> previousScreens = new Stack<Screen>();


        bool LogFeedback = false;

        
        public GameServerInterface() 
        {
            
        }
        private void TEST_Make_Random_Move()
        {
           var rnd=new Random();

            int screen= rnd.Next(0,2);
            int position= rnd.Next(0,100);
            int value = rnd.Next(0, 3);
            if (screen == 0)
                AttackScreen[position] = (byte)value;
            else if(screen==1)
                DefenceScreen[position] = (byte)value;


        }

        public void Run(string adress,string token)
        {
            if(ConnectToGameServer(adress,token))
            {
                running = true;
                Console.Clear();
                Console.BackgroundColor=DefaultBackgroundsColor;
                Console.ForegroundColor=DefaultForegroundColor;
                currentScreen= Settings.Settings.Screens[2];
                StartReadInputLoop();

                while (running)
                {


                    DrawCurrentScreen();
                    
                    
                    
                }
                
                
                

            }
            else
            {
                Console.WriteLine("Couldnt Connect To Game Server");
                throw new Exception("Couldnt Connect To Game Server ");
            }
        }
        
        
        public bool ConnectToGameServer(string address, string token)
        {
            /// Create TCP Client
            client = new TcpClient();
            /// Connect Client To Server
            
            if(true/*ConnectToServer(address, token)*/)
            {

            }


            ///Setup Listening Thread
            //SetupListeningThread();
            
            StartHandelingThreads();


            return true;
        }



        #region TCP Stuff

        private bool ConnectToServer(string address,string token)
        {
            try
            {
                client.Connect(address, 00000000000000000000000000000000000); // We need to get the Port from somewhere
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Connect To Server, error given: " + e);
                return false;
                throw;
            }
            stream = client.GetStream();

            /// Send Token
            SendJWTToken(token);
            string message = RecieveJWTFeedback();

            Thread.Sleep(1000);
            bool connected = client.Connected;
            if (connected)
            {
                HandleMessage(message);
            } 


            return connected;


            /// Listen for Feedback to Token, And if Allowed In
            
        }
        private void SendJWTToken(string jwt)
        {
            StreamWriter writer=new StreamWriter(stream);
            writer.Write(jwt);
            writer.Flush();
        }
        private string RecieveJWTFeedback()
        {
            
            StreamReader reader = new StreamReader(stream);

            

            return reader.ReadLine();
        }

        public void RecieveData(NetworkStream stream)
        {
            while (client.Connected)
            {
                StringBuilder sb = new StringBuilder();
                byte[] tempBytes = new byte[1];
                while (true && client.Connected)
                {
                    int bytesRead = stream.Read(tempBytes);
                    if (bytesRead == 0) { break; }
                    string recievedData = Encoding.UTF8.GetString(tempBytes);
                    if (useEndMessage)
                    {
                        if (recievedData.Contains(END_OF_MESSAGE)) { break; }
                        sb.Append(recievedData);
                    }
                    else
                    {
                        sb.Append(recievedData);
                        break;
                    }

                    
                }
                string message = sb.ToString();
                HandleMessage(message);
            }
            
           
        }
        private void SendMessage(string msg, NetworkStream stream)
        {
            Byte[] data = Encoding.UTF8.GetBytes((useEndMessage) ? msg + END_OF_MESSAGE : msg);
            stream.Write(data, 0, data.Length);
        }
        private void SetupListeningThread()
        {
            try
            {
                listeningThread = new Thread(() => RecieveData(stream))
                {
                    IsBackground = true
                };
                listeningThread.Start();

                
            }
            catch (Exception e)
            {
                Debug.WriteLine("Eroor Ocurred when setting up Listening THread\nExeption: " + e);
                throw;
            }
        }
        #endregion
        #region Message Handeling
        List<string> ServerMessages = new List<string>();
        Queue<RawGameStateMessage> RawGameStateMessages = new Queue<RawGameStateMessage>();
        Queue<RawChatMessage> RawChatMessages = new Queue<RawChatMessage>();
        Mutex RawGameStateMsgMutex= new Mutex();
        Mutex RawChatMsgMutex = new Mutex();

        Thread RawGameStateHandelingThread;
        Thread RawChatHandelingThread;


        public string SerializeMessage(object message)
        {

            //var props = message.GetType().GetProperties();
            //foreach(var prop in props)
            //{
            //    Console.Write(prop.Name +" : ");
            //    try
            //    {
            //        Console.WriteLine(prop.GetValue(message).ToString());
            //    }
            //    catch
            //    {
            //        Console.WriteLine();
            //    }
            //}

            //var options = new JsonSerializerOptions { IncludeFields = true };

            
            string jsonstring = JsonSerializer.Serialize(message/*,options*/);
            return  jsonstring;
        }
        public IServerMessage? HandleMessage(string message)
        {
            message=message.Replace(END_OF_MESSAGE, "");
            //Console.WriteLine("Handeling this message: "+message);
            IServerMessage? serverMessage=null;
            try
            {
                IServerMessage? temp;
                temp =JsonSerializer.Deserialize<RawGameStateMessage>(message);
                if(temp!=null)
                {
                    //Console.WriteLine("Checking if Game State Message");
                    var gameStateIndicator = ((RawGameStateMessage)temp).Opponent;
                    //Console.WriteLine($"- Indicator: {gameStateIndicator}");
                    if (gameStateIndicator== null || gameStateIndicator=="") 
                        temp = null;
                    else
                    {
                        Register_RawGameStateMessage((RawGameStateMessage)temp);
                        
                    }
                }
                if (temp == null)
                {
                    temp = JsonSerializer.Deserialize<RawChatMessage>(message);
                    //Console.WriteLine("Checking if Chat Message");
                    string chatindicator = ((RawChatMessage)temp).From;
                    //Console.WriteLine($"- Indicator: {chatindicator}");
                    if (chatindicator == null || chatindicator=="")
                        temp = null;
                    else
                    {
                        Register_RawChatMessage((RawChatMessage)temp);
          
                    }
                }
                if (temp == null)
                {
                    temp = JsonSerializer.Deserialize<StartupMessage>(message);
                    //Console.WriteLine("Checking if Startup Message");
                    string startupIndicator = ((StartupMessage)temp).ClientID;
                    //Console.WriteLine($"- Indicator: {startupIndicator}");
                    if (startupIndicator == null || startupIndicator == "")
                        temp = null;
                    else
                    {
                        HandleStartupMessage((StartupMessage)temp);
                    }
                }

                serverMessage = temp;
            }
            catch (Exception ex)
            {

            }

            return serverMessage;
        }


        public void StartHandelingThreads()
        {
            RawGameStateHandelingThread = new Thread(HandleRawGameStateMessageLoop) { IsBackground = true };
            RawGameStateHandelingThread.Start();
            RawChatHandelingThread = new Thread(HandleRawChatMessageLoop) { IsBackground = true };
            RawChatHandelingThread.Start();

        }
        #region Handle GameState Messages
        public void HandleRawGameStateMessageLoop()
        {
            while(client.Connected)
            {
                if (RawGameStateMessages.Any())
                {
                    var message = Extract_RawGameStateMessage();
                    HandleRawGameStateMessage(message);
                }

                else
                    Thread.Sleep(100);
            }

        }
        public void HandleRawGameStateMessage(RawGameStateMessage message)
        {

        }
        private void Register_RawGameStateMessage(RawGameStateMessage message)
        {
            RawGameStateMsgMutex.WaitOne();
            RawGameStateMessages.Enqueue(message);
            RawGameStateMsgMutex.ReleaseMutex();
        }
        private RawGameStateMessage Extract_RawGameStateMessage()
        {
            RawGameStateMsgMutex.WaitOne();
            var message = RawGameStateMessages.Dequeue();
            RawGameStateMsgMutex.ReleaseMutex();
            return message;
        }
        #endregion
        #region Handle Chat Messages
        public void HandleRawChatMessageLoop()
        {
            while (client.Connected)
            {
                if (RawChatMessages.Any())
                {
                    var message = Extract_RawChatMessage();
                    HandleRawChatMessage(message);
                }

                else
                    Thread.Sleep(100);
            }
        }
        public void HandleRawChatMessage(RawChatMessage message)
        {

        }
        private void Register_RawChatMessage(RawChatMessage message)
        {
            RawChatMsgMutex.WaitOne();
            RawChatMessages.Enqueue(message);
            RawChatMsgMutex.ReleaseMutex();
        }
        private RawChatMessage Extract_RawChatMessage()
        {
            RawChatMsgMutex.WaitOne();
            var message = RawChatMessages.Dequeue();
            RawChatMsgMutex.ReleaseMutex();
            return message;
        }

        #endregion
        #region Handle Startup Message
        public void HandleStartupMessage(StartupMessage message)
        {

        }
        #endregion





        #endregion

        #region Input Loop
        Thread readInputThread;
     


        public void StartReadInputLoop()
        {
            readInputThread=new Thread(ReadInputLoop) { IsBackground = true };
            readInputThread.Start();
        }


        public void ReadInputLoop()
        {

            while (running)
            {
                ConsoleKey input = Console.ReadKey(true).Key;
                //if(KeyHasBeenPressed==false && IsInInputField==false)
                //    KeyHasBeenPressed = true;
                HandleInput(input);

            }

        }
        public string HandleInput(ConsoleKey key)
        {
            //TEST_Make_Random_Move(); //THIS INDICATES THAT THIS POINT IS REACHED
            string result = "";
            List<KeyBind> temp = new();
            TestPrint("Key: " + key.ToString());
            KeyBinds.ForEach(kb =>
            {
                if (kb.ConsoleKey == key) 
                    if((kb.AssignedScreen == null || (int)kb.AssignedScreen == currentScreen.ID))    
                        temp.Add(kb);

            });

            TestPrint("Commands in Temp:");
            temp.ForEach(x=>TestPrint("- "+x.Command.ToString()));


            if(temp.Count ==1) {
                result=ExecuteCommand(temp[0].Command);
                
            }
            else if(temp.Count > 1){
                throw (new Exception($"This Key [{key.ToString()}] Has multiple Keybinds in this screen: " + currentScreen.Name));
            }
            else
                result = "No Commands Found";
           TestPrint("Handle_Input result: " + result);
            return result;
        }

        public void GetMethods()
        {
            var publicMethods = this.GetType().GetMethods();
            var nonPublicMethods=this.GetType().GetMethods(BindingFlags.Default);

            TestPrint($"Public Methods [{publicMethods.Length}]:" );
            publicMethods.ToList().ForEach(x => { TestPrint("- " + x.Name); });
            TestPrint($"Non Public Methods [{nonPublicMethods.Length}]:");
            nonPublicMethods.ToList().ForEach(x => { TestPrint("- " + x.Name); });
        }

        public string ExecuteCommand(Command command)
        {
            //TEST_CHAT_MESSAGE_BASIC();
            string result = "";
            MethodInfo? CommandMethod=null;

            string theoreticalMethodName = "Command_" + command.ToString();
            
            TestPrint("Looking for Method with name: " + theoreticalMethodName);

            CommandMethod = this.GetType().GetMethod(theoreticalMethodName);

            //this.GetType().GetMethods(BindingFlags.NonPublic).ToList().ForEach(method =>
            //{
            //    if (CommandMethod == null)
            //    {
            //        string name = method.Name;
            //        Console.WriteLine("- MethodName: " + name);
            //        if (name == theoreticalMethodName)
            //            CommandMethod = method;
            //    }

            //});

            if (CommandMethod != null)
            {

                string response=(string)CommandMethod.Invoke(this, null);
                
                
                result = "METHOD FOUND"+" "+response;
            }
            else
                result = "METHOD NOT FOUND";
            // CommandMethod.CreateDelegate(Delegate)
            return result;
        }
        #region CommmandMethods
        public string Command_GoBack()
        {

            if (GoToPreviousScreen() == false)
                running = false;
            CurrentFeedback = "Command Go Back";
            return "GoBack";
        }
        public string Command_OpenShootingScreen()
        {
            //TEST_Make_Random_Move();
            OpenNewScreen(Settings.Settings.Screens[(int)ScreenID.Shooting_Screen]);
            
            
            CurrentFeedback = "Command Open Shooting Screen";
            return "OpenShootingScreen";
        }
        public string Command_SendShot()
        {

            CurrentFeedback = "Send Shot";
            return "SendShot";
        }
        public string Command_OpenPrivateChatScreen()
        {
            currentChatType = ChatType.Private;
            OpenNewScreen(Settings.Settings.Screens[(int)ScreenID.Chat_Screen]);
            //TEST_CHAT_MESSAGE_BASIC();
            CurrentFeedback = "Command Open Private Chat Screen";
            return "OpenPrivateChatScreen";
        }
        public string Command_OpenGroupChatScreen()
        {
            currentChatType=ChatType.Group;
            OpenNewScreen(Settings.Settings.Screens[(int)ScreenID.Chat_Screen]);
            //TEST_CHAT_MESSAGE_BASIC();
            CurrentFeedback = "Command Open Group Chat Screen";
            return "OpenGroupChatScreen";
        }
        public string Command_SendChatMessage()
        {
            CurrentFeedback = "Send Chat Messsage";
            return "SendChatMessage";
        }
        #endregion

        public enum Command
        {
            GoBack,
            OpenShootingScreen,
            SendShot,
            OpenPrivateChatScreen,
            OpenGroupChatScreen,
            SendChatMessage
        }
       /// <summary>
       /// This Might have Been Better as A JSON FILE
       /// </summary>
        public List<KeyBind> KeyBinds = new()
        {
            new KeyBind(Command.GoBack,ConsoleKey.Escape,null,"Go Back To Previous Screen"),
            new KeyBind(Command.OpenShootingScreen,ConsoleKey.S,ScreenID.Game_Screen,"Open Shooting Screen"),
            new KeyBind(Command.SendShot,ConsoleKey.Enter,ScreenID.Shooting_Screen, "Send Shot"),
            new KeyBind(Command.OpenPrivateChatScreen,ConsoleKey.P,ScreenID.Game_Screen, "Open Private Chat Screen"),
            new KeyBind(Command.OpenGroupChatScreen,ConsoleKey.G,ScreenID.Game_Screen, "Open Group Chat Screen"),
            new KeyBind(Command.SendChatMessage,ConsoleKey.Enter,ScreenID.Chat_Screen, "Sends Chat Message")


        };
        public class KeyBind
        {
            public Command Command { get; private set; }
            public ConsoleKey ConsoleKey { get; private set; }
            public ScreenID? AssignedScreen { get; private set; }
            public string Description { get; private set; }
            public KeyBind(Command command, ConsoleKey consoleKey, ScreenID? assignedScreen, string description)
            {
                Command = command;
                ConsoleKey = consoleKey;
                AssignedScreen = assignedScreen;
                Description = description;
            }
        }


        #endregion

        #region Chat Stuff
        private List<ChatMessage> OldChatLog=new List<ChatMessage>();
        public Queue<ChatMessage> ChatMessages { get; private set; }=new Queue<ChatMessage> ();
        public Queue<ChatMessage> NewChatMessages { get; private set; } = new Queue<ChatMessage>();
        private Mutex NewChatMessageMutex = new Mutex();

        public ChatType currentChatType=ChatType.Private;
        public enum ChatType
        {
            Private,
            Group
        }

        public void AddNewChatMessage(string message)
        {
            NewChatMessageMutex.WaitOne();
            NewChatMessages.Enqueue(new ChatMessage(message));
            NewChatMessageMutex.ReleaseMutex();
        }
        private ChatMessage RemoveMessageFromNewChatMessage()
        {

            NewChatMessageMutex.WaitOne();
            var message = NewChatMessages.Dequeue();
            NewChatMessageMutex.ReleaseMutex();
            return message;
        }
        /// <summary>
        /// Checks for new Messages and if new messages are there it transfers them
        /// </summary>
        /// <returns></returns>
        public bool CheckForNewMessages(ScreenViewData viewData)
        {
            if (NewChatMessages.Any()) 
            { 
                while (NewChatMessages.Any())
                {
                    var message = RemoveMessageFromNewChatMessage();
                    AddChatMessageToQueue(message, viewData);
                }
                return true; 
            }
            else return false;
        }
        /// <summary>
        /// Adds to Queue with max space
        /// </summary>
        private void AddChatMessageToQueue(ChatMessage message,ScreenViewData viewData)
        {
            if (ChatMessages.Count < viewData.View.Height)
            {
                ChatMessages.Enqueue(message);
            }
            else
            {
                OldChatLog.Add(ChatMessages.Dequeue());
                ChatMessages.Enqueue(message);
            }
        }
        private Queue<ChatMessage> GetAllMessagesThatFit(ScreenViewData viewData)
        {
            Queue<ChatMessage> tempQueue = new Queue<ChatMessage>(ChatMessages);
            Queue<ChatMessage> result = new Queue<ChatMessage>();
            int MaxLines = viewData.View.Height;
            int linesSum = 0;

            


            while (tempQueue.Any())
            {
                var message= tempQueue.Dequeue();
                
                int lines = message.Calculate_Lines_In_Chat("", viewData);
                if ((linesSum + lines) <= MaxLines)
                {
                    result.Enqueue(message);
                    linesSum += lines;
                    if (linesSum == MaxLines) break;
                }
                else
                    break;
                

            }
            return result;
        }
        

        int testNumber = 0;
        private void TEST_CHAT_MESSAGE_BASIC()
        {
            AddNewChatMessage($"[{testNumber}: TEST MESSAGE BASIC]");
            testNumber++; 
        }

        #endregion
        #region Game Stuff
        byte[] AttackScreen = new byte[100];
        byte[] DefenceScreen = new byte[100];
        //byte[,] AttackScreen = new byte[10, 10];
        //byte[,] DefenceScreen = new byte[10, 10];

        public List<string> FeedbackLog = new List<string>();
        private string currentFeedback;
        public string CurrentFeedback { 
            get 
            {
                if (currentFeedback == null || currentFeedback == "")
                    currentFeedback = "No Feedback";
                    
                return currentFeedback; 
            } 
            set
            {
                if (LogFeedback)
                {
                    if ((currentFeedback == null || currentFeedback == "")==false)
                        FeedbackLog.Add(currentFeedback);
                }
                currentFeedback = value;
            } }

        #endregion

        #region Screen Stuff

        Mutex ChangeScreenMutex = new Mutex();

        private void DrawCurrentScreen()
        {
            ChangeScreenMutex.WaitOne();

            if (first)
            {
                try
                {
                    Console.SetWindowSize((currentScreen.Width == null) ? Console.WindowWidth : (int)currentScreen.Width, (int)currentScreen.ScreenHeight);
                    Console.BufferHeight = Console.WindowHeight;
                }
                catch (Exception)
                {


                }

                oldWidth = Console.WindowWidth;
            }

            if (Console.WindowWidth != oldWidth || KeyHasBeenPressed)
            {
                ClearScreen();
                
                if(KeyHasBeenPressed) { KeyHasBeenPressed=false; }
            }

            if (currentScreen.ID == 2)
            {
                //TEST_CHAT_MESSAGE_BASIC();

                //TEST_Make_Random_Move();

            }
            if (currentScreen!=null)
            {
                PrintScreen(currentScreen);
            }
            


            if (first) first = false;
            if (isCleared) isCleared = false;
            ChangeScreenMutex.ReleaseMutex();
        }
        private void ClearScreen()
        {
            Console.Clear();
            isCleared = true;
            oldWidth = Console.WindowWidth;
        }
        private void PrintScreen(Screen screen)
        {
            Console.CursorVisible = false;
            foreach (var view in screen.Views)
            {
                switch (view.View.ID)
                {
                    case 0: //Seperator
                        DrawSeperator(view);
                        break;
                    case 1: // Chat Widow
                        DrawChatWindow(view);
                        break;
                    case 2: // AttackScreen
                        Draw_AttackScreen(view);
                        break;
                    case 3: // DefenceScreen
                        Draw_DefenceScreen(view);
                        break;
                    case 4: // Feedback Area 
                        DrawFeedback(view);
                        break;
                    case 5: // Shot Area
                        break;
                    case 6: // Chat Input Area
                        break;
                    case 7: // Header (3 Lines)
                        DrawHeader(view);
                        break;
                    case 8: // Lable (1 Line)
                        DrawLable(view);
                        break;
                    case 9:
                        DrawControls(view);
                        break;
                }
            }
        }
        private void ChangeScreen(Screen newScreen)
        {
            ChangeScreenMutex.WaitOne();
            currentScreen = newScreen;
            first = true;
            ClearScreen();
            ChangeScreenMutex.ReleaseMutex();
        }
        private void OpenNewScreen(Screen newScreen)
        {
            previousScreens.Push(currentScreen);

           ChangeScreen(newScreen);
        }
        private bool GoToPreviousScreen()
        {
            if(previousScreens.Count > 0)
            {
                var screen=previousScreens.Pop();
                ChangeScreen(screen);
                return true;
            }
            else
                return false;
            
        }
        #endregion
        #region View Drawing Stuff
        //static string gab = "      ";// 5
        static string gab = "";// None
        
        

        #region -----------------0 Seperator-----------------
        public void DrawSeperator(ScreenViewData viewData)
        {
            if(first ||isCleared)
            {
                int width = (viewData.ScreenWidth == null) ? Console.WindowWidth : (int)viewData.ScreenWidth;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < width; i++)
                {
                    sb.Append("-");
                }
                Console.SetCursorPosition(0, viewData.StartHeight);
                Console.Write(sb.ToString());
            }
            

        }
        #endregion
        #region -----------------1 Chat Window-----------------
        public void DrawChatWindow(ScreenViewData viewData)
        {

            if(CheckForNewMessages(viewData)||isCleared)
            {
                //if(!isCleared) ClearLines(viewData.StartHeight, viewData.StartHeight + viewData.View.Height);

                var messages = GetAllMessagesThatFit(viewData);
                int CurrentLine = 0;
                int width = (viewData.ScreenWidth == null) ? Console.WindowWidth : (int)viewData.ScreenWidth;
                while (messages.Any())
                {
                    Console.SetCursorPosition(0, viewData.StartHeight + CurrentLine);
                    var message = messages.Dequeue();
                    int lines = message.Calculate_Lines_In_Chat("",viewData);
                    WriteFullChatMessage(message.Message,width);
                    //Console.Write(message.Message);
                    CurrentLine+=lines;
                }



            }

        }
        /// <summary>
        /// Used to Avoid Old Messages peeking behind new ones, and to Avoid Glichy behaviour connected to Clear Lines
        /// </summary>
        /// <param name="message"></param>
        /// <param name="width"></param>
        private void WriteFullChatMessage(string message, int width)
        {
            var length = message.Length;
            int fullLines = (int)Math.Floor((decimal)length / (decimal)width);
            var leftoverLength = length - (fullLines * width);
            var extraSpace=width- leftoverLength;
            Console.Write(message);
            Console.Write(new string(' ', extraSpace));


        }
        #endregion
        #region -----------------2 & 3 Attack And Defence Screens----------------- 
        byte[] OldAttackScreen = new byte[100];
        byte[] OldDefenceScreen = new byte[100];
        //byte[,] OldAttackScreen = new byte[10, 10];
        //byte[,] OldDefenceScreen = new byte[10, 10];
        //int oldWidth=0;
        
        private void DrawScreen(ScreenType type, ScreenViewData viewData)
        {

            string Title = "";
            byte[] screen = new byte[100];
            string[] symbols = new string[3];
            ConsoleColor[] colors = new ConsoleColor[3];

            ConsoleColor oldFColor = Console.ForegroundColor;
            ConsoleColor oldBColor = Console.BackgroundColor;

            switch (type)
            {
                case (ScreenType.Attack):
                    Title = "Attack";
                    screen = AttackScreen;
                    colors = new ConsoleColor[3] {    ConsoleColor.DarkCyan,
                                                    ConsoleColor.Blue,
                                                    ConsoleColor.Red};
                    symbols = new string[3] { "~ ", "O ", "X " };

                    break;
                case ScreenType.Defence:
                    Title = "Defence";
                    screen = DefenceScreen;
                    colors = new ConsoleColor[3] {    ConsoleColor.DarkCyan,
                                                    ConsoleColor.DarkGray,
                                                    ConsoleColor.Red};
                    symbols = new string[3] { "~ ", "■ ", "■ " };

                    break;
            }
            int width = (viewData.ScreenWidth == null) ? Console.WindowWidth : (int)viewData.ScreenWidth;
            //if (first)
            //    oldWidth= width; 
            //if(width!=oldWidth)
            //{
            //    ClearLines(viewData.StartHeight, viewData.StartHeight + viewData.View.Height);
            //    oldWidth = width;
            //}



            int line = 0;
            Console.CursorTop = viewData.StartHeight;
            Console.CursorLeft = (width - Title.Length) / 2;
            Console.WriteLine(Title);
            line++;
            int screenwidth = 2 + (symbols[0].Length * 10);
            int startLeftScreen = (width - screenwidth) / 2;
            Console.SetCursorPosition(startLeftScreen, viewData.StartHeight + line);
            PlaceXGrid();
            int screenIndex = 0;

            //IEnumerable<byte[]> temp = screen.Chunk(10);
            //byte[] ROW = temp;
            for (int y = 0; y < 10; y++)
            {
                line++;
                Console.SetCursorPosition(startLeftScreen, viewData.StartHeight + line);
                screenIndex = 10 * y;
                byte[] row = screen.Take(new Range(screenIndex, 10 * (y + 1))).ToArray();
                Console.Write(gab + $"{y}");
                Console.BackgroundColor = ConsoleColor.Cyan;
                Console.Write(" ");
                for (int x = 0; x < 10; x++)
                {
                    byte value = row[x];
                    Console.ForegroundColor = colors[value];
                    Console.Write(symbols[value]);
                    Console.ForegroundColor = oldFColor;

                }
                Console.BackgroundColor = oldBColor;

            }

        }
        static void PlaceXGrid()
        {
            Console.Write(gab + "  ");
            for (int x = 0; x < 10; x++)
                Console.Write($"{x} ");
            //Console.WriteLine();
        }
        /// <summary>
        /// 2
        /// </summary>
        private void Draw_AttackScreen(ScreenViewData viewData)
        {
            if (CompareScreens(AttackScreen, OldAttackScreen) == false || first || isCleared)
            {
                DrawScreen(ScreenType.Attack, viewData);
                OldAttackScreen = (byte[])AttackScreen.Clone();
            }

        }
        /// <summary>
        /// 3
        /// </summary>
        private void Draw_DefenceScreen(ScreenViewData viewData)
        {
            if (CompareScreens(DefenceScreen, OldDefenceScreen) == false || first || isCleared)
            {
                DrawScreen(ScreenType.Defence, viewData);
                OldDefenceScreen = (byte[]) DefenceScreen.Clone();
            }

        }

        public bool CompareScreens(byte[] newScreen, byte[] oldScreen)
        {

            //    var equal =
            //newScreen.Rank == oldScreen.Rank &&
            //Enumerable.Range(0, newScreen.Rank).All(dimension => newScreen.GetLength(dimension) == oldScreen.GetLength(dimension)) &&
            //newScreen.Cast<double>().SequenceEqual(oldScreen.Cast<double>());
            //    // Gotten From: https://stackoverflow.com/questions/12446770/how-to-compare-multidimensional-arrays-in-c
            //return equal;
            return Enumerable.SequenceEqual(newScreen, oldScreen);
        }
        #endregion
        #region -----------------4 Feedback Screen-----------------
        
        public void DrawFeedback(ScreenViewData viewData)
        {
            
            Console.BackgroundColor = DefaultForegroundColor; 
            Console.ForegroundColor=DefaultBackgroundsColor;

            //string text = CurrentFeedback;
            int width = oldWidth;
            //int left = (int)Math.Ceiling((width - text.Length) / (decimal)2);
            Console.SetCursorPosition(0, viewData.StartHeight);
            string text = MakeStringWithSides(width, CurrentFeedback, ' ');
            Console.Write(text);

            Console.BackgroundColor=DefaultBackgroundsColor; 
            Console.ForegroundColor=DefaultForegroundColor;
        }

        #endregion
        #region -----------------5,6 Input Areas (Shot and Chat)-----------------
        // REMEMBER IsInInputField needs to be set to True if here and False After input section


        #endregion
        #region -----------------7,8,9 Text Displays-----------------
        public void DrawHeader(ScreenViewData viewData)
        {
            if (first || isCleared)
            {
                switch (viewData.Screen.ID)
                {
                    case ((int)ScreenID.Shooting_Screen):
                        DrawHeaderBase(viewData, "Take Your Shot");
                        break;
                    case ((int)ScreenID.Chat_Screen):
                        if (currentChatType == ChatType.Private)
                            DrawHeaderBase(viewData, "Write Private Message");
                        else if (currentChatType == ChatType.Group)
                            DrawHeaderBase(viewData, "Write Group Message");
                        break;

                    default:
                        DrawHeaderBase(viewData, viewData.Screen.Name);
                        break;
                }
            }
            


        }
        private void DrawHeaderBase(ScreenViewData viewData,string header)
        {
            int width = oldWidth;

            int middley = viewData.StartHeight + ((int)Math.Ceiling((decimal)viewData.View.Height / (decimal)2));
            Console.SetCursorPosition(0, viewData.StartHeight);

            Console.SetCursorPosition(0, middley);
            //int LeftSide = (int)Math.Ceiling((width - header.Length) / (decimal)2);
            //int RightSide = width - (header.Length + LeftSide);
            //string left=new string('-', LeftSide);
            //string right = new string('-', RightSide);
            //string text = left + header + right;
            string text = MakeStringWithSides(width, header,'-');
            Console.Write(text);
            Console.SetCursorPosition(0, viewData.StartHeight+(viewData.View.Height-1));




        }
        public void DrawLable(ScreenViewData viewData)
        {
            if(first || isCleared)
            {
                switch (viewData.Screen.ID)
                {
                    case ((int)ScreenID.Game_Screen):
                         DrawLableBase(viewData, "Text TO show how to open shoot and chat");
                        break;
                    case ((int)ScreenID.Chat_Screen):
                        DrawLableBase(viewData, "NO TEXT SPECIFIED");
                        break;
                    default:
                        DrawLableBase(viewData, "NO TEXT SPECIFIED");
                            break;
                }
            }
            



        }
        private void DrawLableBase(ScreenViewData viewData,string lable)
        {
            int width = oldWidth;
            int left = (int)Math.Ceiling((width - lable.Length) / (decimal)2);
            Console.SetCursorPosition(left, viewData.StartHeight);
            Console.Write(lable);
        }
        private void DrawControls(ScreenViewData viewData)
        {
            if(first || isCleared)
            {
                int line = 0;

                int width=oldWidth;
                // DRAW Title:
                Console.SetCursorPosition(0, viewData.StartHeight);
                var text = MakeStringWithSides(width, "Controls",'-');
                Console.Write(text);

                // Get Controls for screen
                var controls = new List<KeyBind>();
                KeyBinds.ForEach(kb =>
                {
                    if (kb.AssignedScreen == null || (int)kb.AssignedScreen == viewData.Screen.ID)
                        controls.Add(kb);                    
                });
                line++;
                foreach(var control in controls)
                {
                    Console.SetCursorPosition(0, viewData.StartHeight + line);
                    Console.Write($"{control.ConsoleKey.ToString()} : {control.Description}");

                    line++;
                    if (line >= viewData.View.Height)
                    {
                        break;
                    }

                }

            }
        }
        private string MakeStringWithSides(int width,string text,char charOnSide)
        {
            int LeftSide = (int)Math.Ceiling((width - text.Length) / (decimal)2);
            int RightSide = width - (text.Length + LeftSide);
            string left = new string(charOnSide, LeftSide);
            string right = new string(charOnSide, RightSide);
            return  left + text + right;

        }
            
        #endregion


        /// <summary>
        /// Fundet HER: https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console
        /// </summary>
        public static void ClearCurrentConsoleLine()
        {
            (int CurrentLeft, int CurrentTop) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(CurrentLeft, CurrentTop);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="From">Inclusive From</param>
        /// <param name="To">Exclusive To</param>
        public static void ClearLines(int From, int To)
        {
            if(From<To)
            {
                for (int i = From; i < To; i++)
                {
                    Console.SetCursorPosition(0, i);
                    Console.Write(new string(' ', Console.WindowWidth));
                }
            }
            


        }
        #endregion


        #region Testing
        public bool IsTest { get; set; } = false;
        public void TestPrint(string text)
        {
            if(IsTest)
            {
                Console.WriteLine(text);
            }
        }

        #endregion


    }
    public enum ScreenType
    {
        Attack,
        Defence
    }
    public enum Attack
    {
        Unknown, Miss, Hit
    }
    public enum Defence
    {
        Empty, Ship, HitShip
    }

    public class ChatMessage
    {
        public string Message { get; set; }
        public int Length { get { return Message.Length; } }
        public DateTime TimeRecieved { get; private set; }

        public int Calculate_Lines_In_Chat(string textInFront,ScreenViewData viewData)
        {
            int width = (viewData.ScreenWidth == null) ? Console.WindowWidth : (int)viewData.ScreenWidth;

            int lines =(int) Math.Ceiling((decimal)(Length+textInFront.Length) / (decimal)width);
            return lines;
        }

        public ChatMessage(string message)
        {
            Message = message;
            TimeRecieved= DateTime.Now;

        }
    }

    public enum ScreenID:int
    {
        Login_Screen=0,
        WaitForOpponent_Screen=1,
        Game_Screen=2,
        Chat_Screen=3,
        Shooting_Screen=4
    }

    public interface IServerMessage
    {

    }
    public class GameServerMessage
    {
        public string MessageType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string? LastAction { get; set; }
        public string Message { get; set; }
        public string? Turn { get; set; }


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


        public StartupMessage() { }

    }
    public class RawChatMessage:IServerMessage
    {
        [JsonInclude]
        public string From { get; set; }
        [JsonInclude]
        public string To { get; set; }
        [JsonInclude]
        public string Message { get; set; }
        
       // public RawChatMessage() { }
    }
    //[System.AttributeUsage(System.AttributeTargets.Field| System.AttributeTargets.Property,AllowMultiple =false)]
    //[JsonAttribute(JsonIncludeAttribute)]
   
    public class RawGameStateMessage:IServerMessage
    {
        [JsonInclude]
        public string Opponent { get; set; }
        [JsonInclude]
        public string LastAction { get; set; }
        [JsonInclude]
        public string AttackScreen { get; set; }
        [JsonInclude]
        public string DefenceScreen { get; set; }
        [JsonInclude]
        public bool GameDone { get; set; } 
        [JsonInclude]
        public bool IsLeading { get; set; }
        [JsonInclude]
        public bool PlayerTurn { get; set; }
        //public RawGameStateMessage() { }
    }
    
    

}
