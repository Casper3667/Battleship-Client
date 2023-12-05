using BattleShip_ClientService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Handlers
{
    public class ShotHandler
    {
        public const int gridSize = 10; // This is For Validating Shot, BUT if it doesnt match with Game Servers Definition, Game Server will Ivalidate Players turn so no use editing it on the Client Side
        //public string CurrentShotFeedback = "x,y";
        public GameServerInterface GameServerInterface { get; private set; }


        public ShotHandler(/*ref string feedback,*/ GameServerInterface gsi)
        {
            //CurrentShotFeedback = feedback;
            this.GameServerInterface = gsi;

        }


        
        public const string useFormat = "Use Format: X,Y ";
        public const string formatExample = "Example: 6,8";
        public const string ErrorType_WrongFormat = "Wrong Format";
        public const string WrongFormat_NoComma = "There needs to be a [,]";
        public const string WrongFormat_TooManyCommas = "there was to Many [,]'s";
        public const string WrongFormat_CommaNotInMiddle = "[,] must be in the middle";
        public const string WrongFormat_NotNumbers = "X and Y must be Numbers";
        public const string ErrorType_OutOfBounds = "Out Of Bounds";
        

        public (bool valid, string feedback, Vector2? shot) CheckShot(string shot)
        {
            bool valid = true;
            string[] elements;
            if (shot.Contains(','))
            {
                elements = shot.Split(',');
            }
            else 
                return (false, ErrorType_WrongFormat+" . "+WrongFormat_NoComma+" . "+useFormat,null);
            
            
            if(elements.Length==2)
            {
                string Xstring = elements[0];
                string Ystring = elements[1];

                int X; int Y;

        
                if(int.TryParse(Xstring, out X))
                    if (int.TryParse(Ystring, out Y))
                    {
                        if(CheckIfShotIsInsideBounds(X, Y))
                        {
                            return (true,"",new Vector2(X,Y));
                        }
                       else
                        {
                            return(false, ErrorType_OutOfBounds+" . "+"One of the Values are outside bounds, they must be 0 or higher and lower than "+gridSize+" . "+formatExample, null);
                        }
                    }
                // if We Get to Here one of the Try Parse Failed

                return (false, ErrorType_WrongFormat + " . " + WrongFormat_NotNumbers + " . " + formatExample, null);
               


            }
            else if(elements.Length>2)
            {
                return (false, ErrorType_WrongFormat + " . " + WrongFormat_TooManyCommas + " . " + useFormat, null);
            }
            else if(elements.Length<2) 
            {
                return (false, ErrorType_WrongFormat + " . " + WrongFormat_CommaNotInMiddle + " . " + useFormat, null);
            }

            GameServerInterface.CurrentFeedback = "Shot at" + shot;
            return (false, "Invalid Shot", null);
        }

        public static bool CheckIfShotIsInsideBounds(int X,int Y )
        {
            bool valid = false;
            int sX = X;
            int sY = Y;

            int gX = gridSize;
            int gY = gridSize;

            if (sX >= 0 && sX < gX)
                if (sY >= 0 && sY < gY)
                    valid = true;




            return valid;
        }

    }
}
