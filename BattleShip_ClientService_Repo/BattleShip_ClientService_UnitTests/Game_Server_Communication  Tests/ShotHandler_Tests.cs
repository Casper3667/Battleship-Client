using BattleShip_ClientService.Handlers;
using BattleShip_ClientService.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService_UnitTests.Game_Server_Communication__Tests
{
    internal class ShotHandler_Tests
    {
        GameServerInterface GSI;
        ShotHandler SH;
        [SetUp]
        public void Setup()
        {
            GSI = new GameServerInterface();
            GSI.IsTest = true;
            SH = new ShotHandler(GSI);
        }

        [Test]
        public void Test_GSC_Setup()
        {
            Assert.Pass();
        }
        #region - CheckShot TESTS
        [Test]
        public void Test_GSC_CheckShot_valid()
        {
            string input = "5,5";
            bool expectValid = true;
            Vector2? expectShot = new Vector2(5, 5);

            (bool resultValid, string resultFeedback, Vector2? resultShot) = SH.CheckShot(input);

            Assert.Multiple(() =>
            {
                Assert.That(resultValid, Is.EqualTo(expectValid), "Check is valid");
                Assert.That(resultFeedback, Is.EqualTo(""), "Feedback is Correct");
                Assert.That(resultShot, Is.EqualTo(expectShot), "Check Shot is Correct");
            });

           
        }
        public void Test_GSC_CheckShot_Invalid_WrongFormat_NoComma()
        {
            string input = "5 5";
            bool expectValid = false;
            string expectType = ShotHandler.ErrorType_WrongFormat;
            string expectText = ShotHandler.WrongFormat_NoComma;
            Vector2? expectShot = null;

            (bool resultValid, string resultFeedback, Vector2? resultShot) = SH.CheckShot(input);

            Assert.Multiple(() =>
            {
                Assert.That(resultValid, Is.EqualTo(expectValid), "Check isValid");
                Assert.That(resultFeedback.Contains(expectType), Is.True, "Feedback Error Type is Correct");
                Assert.That(resultFeedback.Contains(expectText), Is.True, "Feedback Error Text is Correct");
                Assert.That(resultShot, Is.EqualTo(expectShot), "Check Shot is Correct");
            });


        }
        public void Test_GSC_CheckShot_Invalid_WrongFormat_TooManyCommas()
        {
            string input = "5,3,5";
            bool expectValid = false;
            string expectType = ShotHandler.ErrorType_WrongFormat;
            string expectText = ShotHandler.WrongFormat_TooManyCommas;
            Vector2? expectShot = null;

            (bool resultValid, string resultFeedback, Vector2? resultShot) = SH.CheckShot(input);

            Assert.Multiple(() =>
            {
                Assert.That(resultValid, Is.EqualTo(expectValid), "Check isValid");
                Assert.That(resultFeedback.Contains(expectType), Is.True, "Feedback Error Type is Correct");
                Assert.That(resultFeedback.Contains(expectText), Is.True, "Feedback Error Text is Correct");
                Assert.That(resultShot, Is.EqualTo(expectShot), "Check Shot is Correct");
            });

        }
        public void Test_GSC_CheckShot_Invalid_WrongFormat_CommaNotInMiddle()
        {
            string input = "55,";
            bool expectValid = false;
            string expectType = ShotHandler.ErrorType_WrongFormat;
            string expectText = ShotHandler.WrongFormat_CommaNotInMiddle;
            Vector2? expectShot = null;

            (bool resultValid, string resultFeedback, Vector2? resultShot) = SH.CheckShot(input);

            Assert.Multiple(() =>
            {
                Assert.That(resultValid, Is.EqualTo(expectValid), "Check isValid");
                Assert.That(resultFeedback.Contains(expectType), Is.True, "Feedback Error Type is Correct");
                Assert.That(resultFeedback.Contains(expectText), Is.True, "Feedback Error Text is Correct");
                Assert.That(resultShot, Is.EqualTo(expectShot), "Check Shot is Correct");
            });

        }
        public void Test_GSC_CheckShot_Invalid_WrongFormat_NotNumbers()
        {
            string input = "u,s";
            bool expectValid = false;
            string expectType = ShotHandler.ErrorType_WrongFormat;
            string expectText = ShotHandler.WrongFormat_NotNumbers;
            Vector2? expectShot = null;

            (bool resultValid, string resultFeedback, Vector2? resultShot) = SH.CheckShot(input);

            Assert.Multiple(() =>
            {
                Assert.That(resultValid, Is.EqualTo(expectValid), "Check isValid");
                Assert.That(resultFeedback.Contains(expectType), Is.True, "Feedback Error Type is Correct");
                Assert.That(resultFeedback.Contains(expectText), Is.True, "Feedback Error Text is Correct");
                Assert.That(resultShot, Is.EqualTo(expectShot), "Check Shot is Correct");
            });

        }
        /// <summary>
        /// This Is Tested more Deeply in CheckIfShotIsInsideBounds TESTS
        /// </summary>
        public void Test_GSC_CheckShot_Invalid_OutOfBounds()
        {

            string input = "-1,-1";
            bool expectValid = false;
            string expectType = ShotHandler.ErrorType_OutOfBounds;

            Vector2? expectShot = null;

            (bool resultValid, string resultFeedback, Vector2? resultShot) = SH.CheckShot(input);

            Assert.Multiple(() =>
            {
                Assert.That(resultValid, Is.EqualTo(expectValid), "Check isValid");
                Assert.That(resultFeedback.Contains(expectType), Is.True, "Feedback Error Type is Correct");
                Assert.That(resultShot, Is.EqualTo(expectShot), "Check Shot is Correct");
            });
        }
        #region - CheckIfShotIsInsideBounds Tests
        [Test]
        public void Test_ShotHandler_CheckIfShotIsInsideBounds_GridSize10_True_X5Y5()
        {
            Point minimum = new Point(0, 0);
            Point middle = new Point(5,5); 
            Point maximum = new Point(9, 9);

            Assert.Multiple(() =>
            {
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds( minimum.X,minimum.Y), Is.True, "Minimum Value For True(0,0)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(middle.X, middle.Y), Is.True, "Middle Value For True(5,5)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(maximum.X, maximum.Y), Is.True, "Maximum Value For True(9,9)");
            });
            //Assert.Inconclusive();
        }
        [Test]
        public void Test_ShotHandler_CheckIfShotIsInsideBounds_GridSize10_False()
        {
            Point closestToMinimum = new Point(-1, -1);
            Point extreme = new Point(100, 42); //100,42
            Point closestToMaximum = new Point(10, 10);

            Assert.Multiple(() =>
            {
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(closestToMinimum.X, closestToMinimum.Y), Is.False, "Closest False Value to Minimum Value For True(-1,-1)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(extreme.X, extreme.Y), Is.False, "Extreme Value For False(100,42)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(closestToMaximum.X, closestToMaximum.Y), Is.False, "Closest False Value to Maximum Value For True(10,10)");
            });
        }
        [Test]
        public void Test_ShotHandler_CheckIfShotIsInsideBounds_GridSizeAny_True_X5Y5()
        {
            Point minimum = new Point(0, 0);
            Point middle = new Point(ShotHandler.gridSize-5, ShotHandler.gridSize - 5);
            Point maximum = new Point(ShotHandler.gridSize-1, ShotHandler.gridSize-1);

            Assert.Multiple(() =>
            {
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(minimum.X, minimum.Y), Is.True, "Minimum Value For True(0,0)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(middle.X, middle.Y), Is.True, "Middle Value For True(5,5)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(maximum.X, maximum.Y), Is.True, "Maximum Value For True(9,9)");
            });
            //Assert.Inconclusive();
        }
        [Test]
        public void Test_ShotHandler_CheckIfShotIsInsideBounds_GridSizeAny_False()
        {
            Point closestToMinimum = new Point(-1, -1);
            Point extreme = new Point(ShotHandler.gridSize + 90, ShotHandler.gridSize + 32); //100,42
            Point closestToMaximum = new Point(ShotHandler.gridSize, ShotHandler.gridSize);

            Assert.Multiple(() =>
            {
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(closestToMinimum.X, closestToMinimum.Y), Is.False, "Closest False Value to Minimum Value For True(-1,-1)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(extreme.X, extreme.Y), Is.False, "Extreme Value For False(100,42)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(closestToMaximum.X, closestToMaximum.Y), Is.False, "Closest False Value to Maximum Value For True(10,10)");
            });
        }
        #endregion
        #endregion





    }
}
