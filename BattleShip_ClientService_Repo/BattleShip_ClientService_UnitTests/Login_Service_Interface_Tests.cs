using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleShip_ClientService;
using BattleShip_ClientService.Interfaces;

namespace BattleShip_ClientService_UnitTests
{
    public class Login_Service_Interface_Tests
    {

        LoginServiceInterface loginServiceInterface;
        
        [SetUp]
        public void Setup()
        {
            loginServiceInterface = new LoginServiceInterface();
        }

        [Test]
        public void Test_ConnectToLoginService()
        {
            Assert.Pass();
        }

        [Test]
        public void Test_LoginService_LoginScreen()
        {
            string Expect = "JWT";

            var result = loginServiceInterface.LoginScreen();

            Assert.AreEqual(Expect, result);

        }

    }
}
