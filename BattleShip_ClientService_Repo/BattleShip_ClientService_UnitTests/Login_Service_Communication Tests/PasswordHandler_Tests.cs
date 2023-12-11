using BattleShip_ClientService.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService_UnitTests.Login_Service_Communication_Tests
{
    internal class PasswordHandler_Tests
    {
        string HashValueFor_CAT = "C01AE1A5F122F25CE5675F86028B536A";
        string HashValueFor_PASSWORD = "319F4D26E3C536B5DD871BB2C52E3178";
        string HashValueFor_Password = "DC647EB65E6711E155375218212B3964";
        string HashValueFor_password = "5F4DCC3B5AA765D61D8327DEB882CF99";
        string HashValueFor_Sofie = "D8C5EBEDCA49B89FE1A845B536CADFE2";
        string HashValueFor_Casper = "E864A59D5D3FB7D439DD4DDD4797AB22";
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void Test_PH_Setup()
        {
            Assert.Pass();
        }
        #region - HashPassword TESTS
        [Test]
        public void Test_PH_HashPassword_CAT()
        {
            string input = "CAT";
           // TestContext.WriteLine("Input: " + input);
            string result=PasswordHandler.HashPassword(input);
            TestContext.WriteLine($"HashValue For [{input}]=[{result}]");
            getHash("PASSWORD");
            Assert.Pass();
        }
        [Test]
        public void Test_PH_HashPassword_MultipleVersionsOfPassword()
        {
            getHash("PASSWORD");
            getHash("Password");
            getHash("password");
            Assert.Pass();
        }
        [Test]
        public void Test_PH_HashPassword_CasperAndSofie()
        {
            getHash("Casper");
            getHash("Sofie");
            Assert.Pass();
        }
        private void getHash(string input)
        {
            string result = PasswordHandler.HashPassword(input);
            TestContext.WriteLine($"HashValue For [{input}]=[{result}]");
        }
        #endregion
        #region - CompareHash TESTS
        [Test]
        public void Test_PH_HashPassword_CompareHash_Cat()
        {
            Assert.That(PasswordHandler.HashPassword("CAT"), Is.EqualTo(HashValueFor_CAT), "CAT");
        }
        [Test]
        public void Test_PH_HashPassword_CompareHash_MultipleVersionsOfPassword()
        {
            Assert.Multiple(() =>
            {
                Assert.That(PasswordHandler.HashPassword("PASSWORD"), Is.EqualTo(HashValueFor_PASSWORD), "PASSWORD");
                Assert.That(PasswordHandler.HashPassword("Password"), Is.EqualTo(HashValueFor_Password), "Password");
                Assert.That(PasswordHandler.HashPassword("password"), Is.EqualTo(HashValueFor_password), "Password");
            });
        }
        #endregion




    }
}
