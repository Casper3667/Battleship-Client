using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Interfaces
{
    public class LoginServiceInterface
    {
        string[] Logo=new string[]
        {
            "┌─┐┌─┐─┬──┬─┬  ┌──┌─┐┬ ┬─┬─┌─┐",
            "├─┤├─┤ │  │ │  ├─ └─┐├─┤ │ ├─┘",
            "└─┘┴ ┴ ┴  ┴ └──└──└─┘┴ ┴─┴─┴  "
        };
        string ErrorText = "Invalid Login";
        private void PrintLogo(Vector2 LogoPos)
        {
            var len = Logo.Length;
            int offset= (int)Math.Floor((decimal)len / 2);
            for(int i = 0; i < len;i++)
            {
                int y=(int)LogoPos.Y-offset;
                Console.SetCursorPosition((int)LogoPos.X, y + i);
                Console.Write(Logo[i]);
            }

        }
        public /*async Task<UserDetails>*/ string LoginScreen()
        {

            //int TitleXPos = (ClientSettings.ConsoleWidth - GameSettings.GameName.Length) / 2;
            
            bool loggingIn = true;
            var username = "";
            var password = "";
            //UserDetails content = new UserDetails();
            bool ShowErrorText = false;
            while (loggingIn)
            {
                int TitleYPos = Console.WindowHeight / 4;
                int TitleXPos = (Console.WindowWidth - Logo[0].Length) / 2;
                (var UPos, var PPos) = WriteGUI(new Vector2(TitleXPos,TitleYPos), ShowErrorText);
                (username, password) = GetUsernameAndPassword(UPos, PPos);

                //var response = await Login(username, password, false);
                //bool LoginSuccessfull = response.IsSuccessStatusCode;
                bool LoginSuccessfull =LoginMockup(username, password);

                if (LoginSuccessfull)
                {
                    //content = await response.Content.ReadFromJsonAsync<UserDetails>();
                    
                    
                    ShowErrorText = false;
                }
                else
                {
                    ShowErrorText = true;
                }
                loggingIn = !LoginSuccessfull;
            }

            Console.WriteLine("Clear standin");
            Console.Clear();
            return "JWT";
           // return content;
        }
        private (Vector2 UsernamePos, Vector2 PasswordPos) WriteGUI(Vector2 LogoPos, bool ShowErrorText)
        {
            Console.Clear();
            PrintLogo(LogoPos);
            var UsernameTextPos = Console.WindowHeight / 2;
            var PasswordTextPos = (Console.WindowHeight / 2) + 1;

            //Console.SetCursorPosition(TitleXPos, TitlePos);
            //Console.WriteLine(GameSettings.GameName);
            Console.SetCursorPosition(0, UsernameTextPos);
            Console.Write("Username: ");
            (var ULeft, var UTop) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, PasswordTextPos);
            Console.Write("Password: ");
            (var PLeft, var PTop) = Console.GetCursorPosition();
            if (ShowErrorText)
            {
                Console.SetCursorPosition(0, PasswordTextPos + 1);
                Console.WriteLine(ErrorText);
            }


            return (new Vector2(ULeft, UTop), new Vector2(PLeft, PTop));
        }
        private (string username, string password) GetUsernameAndPassword(Vector2 Upos, Vector2 PPos)
        {
            Console.SetCursorPosition(((int)Upos.X), (int)Upos.Y);
            var username = Console.ReadLine();
            Console.SetCursorPosition((int)PPos.X, (int)PPos.Y);
            var password = Console.ReadLine();
            return (username, password);
        }
        #region Communication With Login Service Region
        public bool LoginMockup(string username, string password)
        {
            return true;
        }

        //public async Task<HttpResponseMessage?> Login(string username, string password, bool isDebug = true)
        //{
        //    var response = await client.GetAsync($"{username}/{password}");

        //    return response;

        //    //if (response.IsSuccessStatusCode)1
        //    //{
        //    //    var content = await response.Content.ReadAsStringAsync();

        //    //    if(isDebug)
        //    //        Console.WriteLine("Response Sucsess full response: " + content);
        //    //    return true;
        //    //}
        //    //else
        //    //{
        //    //    if(isDebug)
        //    //        Console.WriteLine($"Response failed. Status code {response.StatusCode}");
        //    //    return false;
        //    //}

        //}
        #endregion

    }
}
