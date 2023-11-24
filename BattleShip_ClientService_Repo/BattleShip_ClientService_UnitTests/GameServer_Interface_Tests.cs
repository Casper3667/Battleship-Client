using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleShip_ClientService.Interfaces;

namespace BattleShip_ClientService_UnitTests
{
    public class GameServer_Interface_Tests
    {
        GameServerInterface GSI;
        [SetUp]
        public void Setup()
        {
            GSI=new GameServerInterface();

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
    }
}
