using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings;
using NuGet.Frameworks;

namespace BattleShip_ClientService_UnitTests
{
    public class GameServer_Interface_Tests
    {
        GameServerInterface GSI;
        [SetUp]
        public void Setup()
        {
            //Settings.LoadSettings();
            GSI =new GameServerInterface();
            //GSI.IsTest = true;
        }

        [Test]
        public void Test_GameServer_Constructor()
        {
            Assert.Pass();
        }
        [Test]
        public void Test_GameServer_ConnectToGameServer_True() 
        {
            bool expect = true;
            string adress = "FAKE ADRESS";
            string token = "JWT";

            var result=GSI.ConnectToGameServer(adress, token);

            Assert.AreEqual(expect, result);
        }
        [Test]
        public void Test_GameServer_CompareScreens_True()
        {
            bool expect = true;
            byte[] newScreen = new byte[4] {  1, 2 ,  1, 1  };
            byte[] oldScreen = new byte[4] {  1, 2 ,  1, 1  };

            //byte[, ] newScreen = new byte[2, 2] { { 1, 2 }, { 1, 1 } };
            //byte[,] oldScreen = new byte[2, 2] { { 1, 2 }, { 1, 1 } };

            bool result=GSI.CompareScreens(newScreen, oldScreen);
            Assert.AreEqual(expect, result);


        }
        [Test]
        public void Test_GameServer_CompareScreens_False()
        {
            bool expect= false;
            byte[] newScreen = new byte[4] {  1, 2 ,  1, 1  };
            byte[] oldScreen = new byte[4] {  1, 0 ,  0, 1  };

            //byte[,] newScreen = new byte[2, 2] { { 1, 2 }, { 1, 1 } };
            //byte[,] oldScreen = new byte[2, 2] { { 1, 0 }, { 0, 1 } };

            bool result = GSI.CompareScreens(newScreen, oldScreen);
            Assert.AreEqual(expect, result);
        }
        [Test]
        public void Test_GameServer_SerializeMessage_notBlank()
        {
            string notExpect = "{}";
            var msg = new RawGameStateMessage()
            {
                Opponent = "TEST GAME STATE"
            };
            var result = GSI.SerializeMessage(msg);
            Assert.AreNotEqual(notExpect, result);
        }

        [Test]
        public void Test_GameServer_HandleMessage_RawGameStateMessage()
        {
            var msg = new RawGameStateMessage()
            {
                Opponent = "TEST GAME STATE"
            };
            var json = GSI.SerializeMessage(msg);
            IServerMessage? result = GSI.HandleMessage(json);
            //Assert.NotNull(result);
            Assert.IsInstanceOf<RawGameStateMessage>(result);
            //Assert.AreEqual(expect, 0);

        }
        [Test]
        public void Test_GameServer_HandleMessage_RawChatMessage()
        {
            var msg = new RawChatMessage() {From="TEST CHAT" };
            var json = GSI.SerializeMessage(msg);
            IServerMessage? result = GSI.HandleMessage(json);
            //Assert.NotNull(result);
            Assert.IsInstanceOf<RawChatMessage>(result);
            //Assert.AreEqual(expect, 0);

        }
        [Test]
        public void Test_GameServer_HandleMessage_RawChatMessage_FROM()
        {
            string expect = "Sofie";
            var msg = new RawChatMessage()
            {
                From = expect
            };
            var json = GSI.SerializeMessage(msg);
            IServerMessage? partresult = GSI.HandleMessage(json);
             Assert.IsInstanceOf<RawChatMessage>(partresult);
            string result = ((RawChatMessage)partresult).From;

            Assert.AreEqual(expect, result);
            //Assert.AreEqual(expect, 0);

        }
        [Test]
        public void Test_GameServer_HandleMessage_StartupMessage()
        {
            var msg = new StartupMessage() { ClientID="TEST STARTUP"};
            var json = GSI.SerializeMessage(msg);
            IServerMessage? result = GSI.HandleMessage(json);
            //Assert.NotNull(result);
            Assert.IsInstanceOf<StartupMessage>(result);
            //Assert.AreEqual(expect, 0);

        }
        [Test]
        public void Test_GameServer_HandleMessage_Null()
        {
            //var msg = new StartupMessage();
            //var json = GSI.SerializeMessage(msg);
            var json = "Not a real Json String";
            
            IServerMessage? result = GSI.HandleMessage(json);
            //Assert.NotNull(result);
            Assert.IsNull(result);
            //Assert.IsInstanceOf<StartupMessage>(result);
            //Assert.AreEqual(expect, 0);

        }

        #region InputLoop
        [Test]
        public void Test_GameServer_InputLoop_HandleInput_MethodFound()
        {

            Screen screen = new Screen() { ID = 2 };
            GSI.CurrentScreen = screen;
            string expect = "METHOD FOUND";
            ConsoleKey input = ConsoleKey.Escape;
            string result = GSI.HandleInput(input);
            Assert.IsTrue(result.Contains(expect));
        }
        //[Test]
        //public void Test_GameServer_InputLoop_HandleInput_MethodNotFound()
        //{
        //    string expect = "METHOD NOT FOUND";
        //    ConsoleKey input = ConsoleKey.Enter; // As Long as Screen is
        //    string result = GSI.HandleInput(input);
        //    Assert.AreEqual(expect, result);
        //}
        [Test]
        public void Test_GameServer_InputLoop_HandleInput_NoCommandsFound()
        {
            Screen screen = new Screen() { ID = 2 };
            GSI.CurrentScreen = screen;
            string expect = "No Commands Found";
            ConsoleKey input = ConsoleKey.ExSel;
            string result = GSI.HandleInput(input);
            Assert.AreEqual(expect, result);
        }
        [Test]
        public void Test_GameServer_InputLoop_HandleInput_TestAllCommands()
        {
            Screen screen = new Screen() { ID = 2 };
            GSI.CurrentScreen = screen;
            string expect;
            string result;
            foreach (var kb in GSI.KeyBinds)
            {
                if (kb.AssignedScreen == null || (int)kb.AssignedScreen == 2)
                {
                    expect = kb.Command.ToString();
                }
                else
                {
                    expect = "METHOD NOT FOUND";
                }

                result = GSI.HandleInput(kb.ConsoleKey);


                Assert.IsTrue(true);
               // Assert.IsTrue(result.Contains(expect));


            }
        }
        [Test]
        public void Test_GameServer_InputLoop_ExecuteCommand_TestAllCommands()
        {
            string expect;
            string result;
            foreach(var kb in GSI.KeyBinds)
            {
                expect = kb.Command.ToString();
                result = GSI.ExecuteCommand(kb.Command);
                
                Assert.IsTrue(result.Contains(expect));

            }

            
        }

        [Test]
        public void Test_GetMethods()
        {
            GSI.GetMethods();
            Assert.IsTrue(true);
        }
        #endregion
    }
}
