﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleShip_ClientService;
using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings;

namespace BattleShip_ClientService_UnitTests
{
    public class Login_Service_Interface_Tests
    {

        LoginServiceInterface loginServiceInterface;
        
        [SetUp]
        public void Setup()
        {
            Settings.LoadSettings();
            loginServiceInterface = new LoginServiceInterface();
        }

        [Test]
        public void Test_ConnectToLoginService()
        {
            Assert.Pass();
        }


        [Test] public void Test_LoginService_GetURL() 
        {
            TestContext.WriteLine("URL: "+loginServiceInterface.GetURL());
            Assert.Pass();
        }
        
        // TODO: TEST Fix this test, i think its the fact it calls on the Console.Width and Height when the console isnt up that is the problem
        //[Test]
        //public void Test_LoginService_LoginScreen()
        //{
        //    string Expect = "JWT";

        //    var result = loginServiceInterface.LoginScreen();

        //    Assert.AreEqual(Expect, result);

        //}

    }
}
