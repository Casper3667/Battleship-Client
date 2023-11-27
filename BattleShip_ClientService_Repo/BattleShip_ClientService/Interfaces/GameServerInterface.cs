using BattleShip_ClientService.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
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

        TcpClient client;
        NetworkStream stream;

        Thread listeningThread;

        /// <summary>
        /// THIS MAY CHANGE
        /// </summary>
        bool running = true;

        Screen currentScreen;
        int oldWidth;

        Stack<Screen> previousScreens = new Stack<Screen>();

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
                Console.Clear();
                currentScreen= Settings.Settings.Screens[2];
                
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
        #region Chat Stuff
        private List<ChatMessage> OldChatLog=new List<ChatMessage>();
        public Queue<ChatMessage> ChatMessages { get; private set; }=new Queue<ChatMessage> ();
        public Queue<ChatMessage> NewChatMessages { get; private set; } = new Queue<ChatMessage>();
        private Mutex NewChatMessageMutex = new Mutex();
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



        #endregion

        #region Screen Stuff
        private void DrawCurrentScreen()
        {
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

            if (Console.WindowWidth != oldWidth)
            {
                Console.Clear();
                isCleared = true;
                oldWidth = Console.WindowWidth;
            }


            if (currentScreen.ID == 2)
            {
                TEST_CHAT_MESSAGE_BASIC();

                TEST_Make_Random_Move();
                PrintGameScreen(currentScreen);
            }


            if (first) first = false;
            if (isCleared) isCleared = false;
        }
        private void ChangeScreen(Screen newScreen)
        {
            currentScreen = newScreen;
            first = true;
        }
        private void OpenNewScreen(Screen newScreen)
        {
            previousScreens.Push(currentScreen);

           ChangeScreen(newScreen);
        }
        private void GoToPreviousScreen()
        {
            if(previousScreens.Count > 0)
            {
                var screen=previousScreens.Pop();
                ChangeScreen(screen);
            }

            
        }
        #endregion
        #region Game Screen Stuff
        //static string gab = "      ";// 5
        static string gab = "";// None
        
        private void PrintGameScreen(Screen screen)
        {
            
           
            


            foreach(var view in screen.Views)
            {
                switch(view.View.ID)
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
                        break;
                        case 5: // Shot Area
                        break;
                }

            }

            



            
        }

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
            Console.CursorVisible = false;
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
            Console.CursorVisible=true;
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
            Console.CursorVisible = false;
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
            Console.CursorVisible = true;
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

        #endregion
        #region -----------------5,6,7 Input Areas (Shot and Chat)-----------------

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
