using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService_UnitTests.Game_Server_Communication__Tests
{
    internal class ServerMessageHandler_Tests
    {
        GameServerInterface GSI;
        [SetUp]
        public void Setup()
        {
            //Settings.LoadSettings();
            GSI = new GameServerInterface();
            GSI.IsTest = true;
        }

        [Test]
        public void Test_SMH_Setup()
        {
            Assert.Pass();
        }

        #region - HandeRawGameStateMessage TESTS

        [Test]
        public void Test_SMH_HandleRawGameStateMessage_GameNotDone()
        {
            RawGameStateMessage input = new RawGameStateMessage()
            {
                Opponent = "Casper",
                LastAction = "Standin",
                AttackScreen = "0,0,0,0",
                DefenceScreen = "1,1,0,0",
                GameDone = false,
                IsLeading = false,
                PlayerTurn = true
            };
            byte[] expectAttack = new byte[] { 0, 0, 0, 0 };
            byte[] expectDefence = new byte[] { 1, 1, 0, 0 };

            GSI.HandleRawGameStateMessage(input);

            Assert.Multiple(() =>
            {
                Assert.That(GSI.OtherPlayerName, Is.EqualTo(input.Opponent), "Check OtherPlayerName");
                Assert.That(GSI.CurrentFeedback, Is.EqualTo(input.LastAction), "Check CurrentFeedback");
                Assert.That(GSI.AttackScreen, Is.EqualTo(expectAttack), "Check AttackScreen");
                Assert.That(GSI.DefenceScreen, Is.EqualTo(expectDefence), "Check Defence");
                Assert.That(GSI.IsGameDone, Is.EqualTo(input.GameDone), "Check IsGameDone");
                Assert.That(GSI.IsLeading, Is.EqualTo(input.IsLeading), "Check IsLeading");
                Assert.That(GSI.IsClientTurn, Is.EqualTo(input.PlayerTurn), "Check IsClientTurn");

            });
        }
        [Test]
        public void Test_SMH_HandleRawGameStateMessage_GameDone()
        {
            RawGameStateMessage input = new RawGameStateMessage()
            {
                Opponent = "Casper",
                LastAction = "Standin",
                AttackScreen = "0,0,0,0",
                DefenceScreen = "1,1,0,0",
                GameDone = true,
                IsLeading = true,
                PlayerTurn = true
            };
            byte[] expectAttack = new byte[] { 0, 0, 0, 0 };
            byte[] expectDefence = new byte[] { 1, 1, 0, 0 };

            GSI.HandleRawGameStateMessage(input);

            Assert.Multiple(() =>
            {
                Assert.That(GSI.OtherPlayerName, Is.EqualTo(input.Opponent), "Check OtherPlayerName");
                Assert.That(GSI.CurrentFeedback==input.LastAction,Is.False, "Check CurrentFeedback, Should not be last action");
                Assert.That(GSI.AttackScreen, Is.EqualTo(expectAttack), "Check AttackScreen");
                Assert.That(GSI.DefenceScreen, Is.EqualTo(expectDefence), "Check Defence");
                Assert.That(GSI.IsGameDone, Is.EqualTo(input.GameDone), "Check IsGameDone");
                Assert.That(GSI.IsLeading, Is.EqualTo(input.IsLeading), "Check IsLeading");
                Assert.That(GSI.IsClientTurn, Is.False, "Check IsClientTurn since if game is Done it Should be false now"); 

            });
        }
        #region - TranslateScreen TESTS
        [Test]
        public void Test_SMH_TranslateScreen() {
            string input = "1,0,1,2,0,0";
            byte[] expect = new byte[] { 1, 0, 1, 2, 0, 0 };
            byte[] result = GSI.TranslateScreen(input);
            Assert.That(result,Is.EquivalentTo(expect));
        }
        #endregion
        #region - GameDoneCode TESTS
        [Test] public void Test_SMH_GameDoneCode() { Assert.Inconclusive(); }
        #endregion
        #endregion
        #region - HandeRawChatMessage TESTS
        /// <summary>
        /// Doesnt Test With format, So That Tests doesnt break if i change the formatting
        /// </summary>
        [Test]
        public void Test_SMH_HandleRawChatMessage()
        {
            RawChatMessage input=new RawChatMessage() { From="Casper", To="Sofie", Message="Hello World"};
            GSI.HandleRawChatMessage(input);

           ChatMessage message= GSI.RemoveMessageFromNewChatMessage();

            string result = message.Message;
            Assert.Multiple(() =>
            {
                Assert.That(result.Contains(input.From), Is.True, "Contains From");
                Assert.That(result.Contains(input.To), Is.True, "Contains To");
                Assert.That(result.Contains(input.Message), Is.True, "Contains Message");

            });


        }
        #endregion
        #region - HandeStartupMessage TESTS
        [Test]
        public void Test_SMH_HandleStartupMessage_Alone()
        {
            StartupMessage input= new() { ClientID="Sofie", GameReady=false, OtherPlayer=null};

            GSI.HandleStartupMessage(input);

            Assert.Multiple(() =>
            {
                Assert.That(GSI.Name,Is.EqualTo(input.ClientID), "Check Name");
                Assert.That(GSI.IsGameReady, Is.EqualTo(input.GameReady), "Check IsGameReady");
                Assert.That(GSI.OtherPlayerName, Is.EqualTo(input.OtherPlayer), "Check OtherPlayerName");

            });
        }
        [Test]
        public void Test_SMH_HandleStartupMessage_HaveOpponent()
        {
            StartupMessage input = new() { ClientID = "Sofie", GameReady = true, OtherPlayer = "Casper" };

            GSI.HandleStartupMessage(input);

            Assert.Multiple(() =>
            {
                Assert.That(GSI.Name, Is.EqualTo(input.ClientID), "Check Name");
                Assert.That(GSI.IsGameReady, Is.EqualTo(input.GameReady), "Check IsGameReady");
                Assert.That(GSI.OtherPlayerName, Is.EqualTo(input.OtherPlayer), "Check OtherPlayerName");

            });
        }
        #endregion
    }
}
