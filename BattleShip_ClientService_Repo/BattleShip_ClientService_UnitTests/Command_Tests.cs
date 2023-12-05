using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings;
using BattleShip_ClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService_UnitTests
{
    internal class Command_Tests
    {
        GameServerInterface GSI;
        [SetUp]
        public void Setup()
        {
            Testing.IsTesting = true;
            Settings.LoadSettings();
            GSI = new GameServerInterface();
            GSI.IsTest = true;
        }

        [Test]
        public void Test_Command_Setup()
        {
            Assert.Pass();
        }
        [Test]
        public void Test_Command_GoBack()
        {
            string expect = "GoBack";
            string result = GSI.Command_GoBack();

            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Command_OpenShootingScreen() 
        {
            string expect = "OpenShootingScreen";
            string result = GSI.Command_OpenShootingScreen();

            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Command_SendShot()
        {
            string expect = "SendShot";
            string result = GSI.Command_SendShot();

            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Command_OpenPrivateChatScreen()
        {
            string expect = "OpenPrivateChatScreen";
            string result = GSI.Command_OpenPrivateChatScreen();

            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Command_OpenGroupChatScreen()
        {
            string expect = "OpenGroupChatScreen";
            string result = GSI.Command_OpenGroupChatScreen();

            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Command_SendChatMessage()
        {
            string expect = "SendChatMessage";
            string result = GSI.Command_SendChatMessage();

            Assert.That(result, Is.EqualTo(expect));
        }
    }
}
