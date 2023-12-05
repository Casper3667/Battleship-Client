using BattleShip_ClientService;
using BattleShip_ClientService.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService_UnitTests
{
    internal class Settings_Tests
    {
        [SetUp]
        public void Setup()
        {
            Testing.IsTesting = true;
            Settings.LoadSettings();
        }

        [Test]
        public void Test_Settings_Setup()
        {
            Assert.Pass();
        }

        [Test]
        public void Test_Settings_SeeViews() 
        {
            TestContext.WriteLine("Views:");
            foreach(var view in Settings.Views) 
            {
                TestContext.WriteLine("- "+view.Value.Name);
            }
            Assert.Pass();
        }
        [Test]
        public void Test_Settings_SeeScreens()
        {
            TestContext.WriteLine("Views:");
            foreach (var screen in Settings.Screens)
            {
                TestContext.WriteLine("- " + screen.Value.Name);
            }
            Assert.Pass();
        }
    }
}
