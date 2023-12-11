using BattleShip_ClientService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService_UnitTests
{
    internal class GameServerCommunication_Tests
    {
        GameServerInterface GSI;
        [SetUp]
        public void Setup()
        {
            GSI = new GameServerInterface();
            GSI.IsTest = true;
        }

        [Test]
        public void Test_GSC_Setup()
        {
            Assert.Pass();
        }
        #region - CheckShot TESTS
        [Test]
        public void Test_GSC_CheckShot_valid() {
            Assert.Inconclusive();
        }
        public void Test_GSC_CheckShot_Invalid_WrongFormat()
        {
            Assert.Inconclusive();
        }
        public void Test_GSC_CheckShot_Invalid_OutOfBounds()
        {
            Assert.Inconclusive();
        }
        #endregion
    }
}
