using BattleShip_ClientService.Handlers;
using BattleShip_ClientService.Messages;
using BattleShip_ClientService.Settings;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Interfaces
{
    public class LoginServiceInterface
    {
        //const int LoginServicePort = 8081;
        const string pathToController = "/api/Users/";
        const string startOFPath = "http://";
        

        HttpClient client;
        
        static string ErrorText = "Invalid Login";

        
        public LoginServiceInterface() {
            //CreateClientWithCircuitBreaker();
            CreateClientWithoutCircuitBreaker();
        }

        #region Try at Circut Breaker Code (Given Up)
        AsyncCircuitBreakerPolicy<HttpResponseMessage> CircutBreakerPolicy;
        static bool IsCircutPopped = false;
        private IHttpClientFactory httpClientFactory;
        // httpClientFactory;
        //private IServiceCollection Services;
        ServiceCollection Services;
        /// <summary>
        /// How Long We Came in Trying to Implement A Circut Breaker, 
        /// Ït Obviously didnt Work, Since we where only tought how to do it from a Web Application NOT a Console Application
        /// This Code might just be the remnant of a Useless attempt at something impossible,
        /// </summary>
        private void CreateClientWithCircuitBreaker()
        {
            CircutBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(10), onBreak, onReset, onHalfOpen);
            // Microsoft.Extensions.Http.DefaultHttpClientFactory



            Services = new ServiceCollection();

            var builder = Services.AddHttpClient<HttpClient>("CircuitBreakerClient").AddPolicyHandler(CircutBreakerPolicy);


            //(ServiceCollection)Services.All();
            Services.GetType();
            //builder.
            //Services.GetService()
            client = httpClientFactory.CreateClient("CircuitBreakerClient");
            client.BaseAddress = new Uri(GetURL());
            client.Timeout = TimeSpan.FromSeconds(10);
        }
        Action<DelegateResult<HttpResponseMessage>, TimeSpan> onBreak = delegate (DelegateResult<HttpResponseMessage> r, TimeSpan t) {
            Debug.WriteLine("CircuitBreaker On BReak ResponseMessage: " + r.Result.Content.ToString() + "Seconds Left: " + t.TotalSeconds.ToString());
            IsCircutPopped = true; ErrorText = $"Too Many Invalid Logins Try Again in {t.TotalSeconds.ToString()} Seconds";
        };

        Action onReset = () =>
        {
            IsCircutPopped = false;
            ErrorText = "Try Logging In Again";
        };
        Action onHalfOpen = () =>
        {
            IsCircutPopped = false;
            ErrorText = "You Have ONE try to Login Again, if you want more wait";
        };
        #endregion

        private void CreateClientWithoutCircuitBreaker()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(GetURL());
            client.Timeout = TimeSpan.FromSeconds(10);
        }


        public string GetURL()
        {
            string ip = Settings.Settings.NetworkSettings.LoginIP;
            int port = Settings.Settings.NetworkSettings.LoginPORT;
            string result = startOFPath +ip + ":" + port + pathToController;
            return result;
        }
        string[] Logo=new string[]
        {
            "┌─┐┌─┐─┬──┬─┬  ┌──┌─┐┬ ┬─┬─┌─┐",
            "├─┤├─┤ │  │ │  ├─ └─┐├─┤ │ ├─┘",
            "└─┘┴ ┴ ┴  ┴ └──└──└─┘┴ ┴─┴─┴  "
        };
       
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
        public async /*Task<UserDetails>*/ Task<string> LoginScreen()
        {

            //int TitleXPos = (ClientSettings.ConsoleWidth - GameSettings.GameName.Length) / 2;
            
            bool loggingIn = true;
            var username = "";
            var password = "";
            //UserDetails content = new UserDetails();
            bool ShowErrorText = false;
            string JWT="";
            while (loggingIn)
            {
                int TitleYPos = Console.WindowHeight / 4;
                int TitleXPos = (Console.WindowWidth - Logo[0].Length) / 2;
                (var UPos, var PPos) = WriteGUI(new Vector2(TitleXPos,TitleYPos), ShowErrorText);

                if(IsCircutPopped==false)
                {
                    (username, password) = GetUsernameAndPassword(UPos, PPos);

                    var response = await Login(username, password, false);
                    bool LoginSuccessfull = response.IsSuccessStatusCode;
                    //bool LoginSuccessfull =LoginMockup(username, password);

                    if (LoginSuccessfull)
                    {
                        //content = await response.Content.ReadFromJsonAsync<UserDetails>();


                        ShowErrorText = false;
                        JWT = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("GOT JWT: " + JWT);
                    }
                    else
                    {
                        ShowErrorText = true;
                        if (response.ReasonPhrase != null)
                            ErrorText = "Invalid Login: " + response.ReasonPhrase;
                        else
                            ErrorText = "Invalid Login";
                        Debug.WriteLine("DIDNT GET JWT");
                    }
                    loggingIn = !LoginSuccessfull;
                }
                else
                {
                    Thread.Sleep(100);
                }
                
            }

            Console.WriteLine("Clear standin");
            Console.Clear();
            
            return JWT;
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

        public async Task<HttpResponseMessage?> Login(string username, string password, bool isDebug = true)
        {
            string hashPassword = PasswordHandler.HashPassword(password);

            var response = await client.GetAsync($"{username}/{hashPassword}");

            return response;

            //if (response.IsSuccessStatusCode)1
            //{
            //    var content = await response.Content.ReadAsStringAsync();

            //    if(isDebug)
            //        Console.WriteLine("Response Sucsess full response: " + content);
            //    return true;
            //}
            //else
            //{
            //    if(isDebug)
            //        Console.WriteLine($"Response failed. Status code {response.StatusCode}");
            //    return false;
            //}

        }
        #endregion

    }
}
