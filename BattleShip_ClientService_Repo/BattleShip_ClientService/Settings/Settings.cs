using BattleShip_ClientService.Interfaces;
using BattleShip_ClientService.Settings.LoginServiceInterfaceSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Settings
{
    public static class Settings
    {
        public static bool IsSettingsLoaded { get; private set; } = false;
        public static Dictionary<int, View> Views { get; private set; } = new Dictionary<int, View>();
        public static Dictionary<int, Screen> Screens { get; private set; }
        public static LoginServiceInterfaceSettings.LoginServiceInterfaceSettings LoginServiceSettings { get; private set; }

        public static void LoadSettings()
        {
            if(IsSettingsLoaded==false)
            {
                Testing.Print("Loading Settings");

                LoadViews();
                LoadScreens();
                GetLoginServiceSettings();
                IsSettingsLoaded = true;
            }
            else
            {
                Testing.Print("Settings Already Loaded");
            }
        }

        public static string GetLoginServiceSettings()
        {
           var settings =LoadJSON<LoginServiceInterfaceSettings.LoginServiceInterfaceSettings>("LoginServiceInterfaceSettings\\LoginServiceInterfaceSettings.JSON");
            if (settings != null && settings.Any())
            {
                string ip = settings[0].IP;
                LoginServiceSettings = settings[0];
                //LoginServiceIPAdress = ip;
                return ip;
            }
            else
            {
                Testing.Print("Couldnt Find IP So returned Local Host");
                Debug.Fail("Couldnt Find IP So returned Local Host");
                return "localHost";
            }
            //using (StreamReader r = new StreamReader(GetPathToSettingsFile("LoginServiceInterfaceSettings\\LoginServiceInterfaceSettings.JSON")))
            //{
            //    r.
            //    string json = r.ReadToEnd();
            //    items = JsonSerializer.Deserialize<List<T>>(json);

            //}

        }
        // For This to Work File Must be in the Settings Folder
        public static string GetPathToSettingsFile(string FileName)
        {
            FileName = "Settings\\" + FileName;

#if DEBUG
            Testing.Print("Is In Debug Mode");
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Testing.Print("Path: " + System.IO.Path.GetDirectoryName(path));

            string Identifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService\\";
            string TestIdentifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService_UnitTests\\"; // Indicates that it is a test and splits where the Test Begins to get the part of the Path that should vary form computer to Computer
            string forginPath = "";
            if (path.Contains(Identifier))
            {
                forginPath = path.Split(Identifier)[0];
            }
            else if (path.Contains(TestIdentifier))
            {
                forginPath = path.Split(TestIdentifier)[0];
            }

            Testing.Print("forgin Path: " + forginPath);

            string newPath = forginPath + Identifier;
            Testing.Print("new Path: " + newPath);
            FileName = newPath /*+ "Settings\\"*/ + FileName;
            Testing.Print("views Path: " + FileName);

#endif
            return FileName;
        }

        private static List<T> LoadJSON<T>(string FileName)
        {
            FileName = GetPathToSettingsFile(FileName);
                
//#if DEBUG
//            Testing.Print("Is In Debug Mode");
//            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
//            Testing.Print("Path: " + System.IO.Path.GetDirectoryName(path));
           
//            string Identifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService\\";
//            string TestIdentifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService_UnitTests\\"; // Indicates that it is a test and splits where the Test Begins to get the part of the Path that should vary form computer to Computer
//            string forginPath="";
//            if (path.Contains(Identifier))
//            {
//                forginPath= path.Split(Identifier)[0];
//            }
//            else if(path.Contains(TestIdentifier))
//            {
//                forginPath = path.Split(TestIdentifier)[0];
//            }
            
//            Testing.Print("forgin Path: " + forginPath);

//            string newPath = forginPath + Identifier;
//            Testing.Print("new Path: " + newPath);
//            FileName = newPath + "Settings\\" + FileName;
//            Testing.Print("views Path: " + FileName);

//#endif

            //FileName = "Settings\\" + FileName;

            List<T> items = new List<T>();
            using (StreamReader r = new StreamReader(FileName))
            {
                string json = r.ReadToEnd();
                items = JsonSerializer.Deserialize<List<T>>(json);

            }


            return items;
        }

        private static void LoadViews()
        {

            var views = LoadJSON<View>("Views.JSON");
//            string viewsAdress = "Views.JSON";
//#if DEBUG
//            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
//            Console.WriteLine("Path: " + System.IO.Path.GetDirectoryName(path));
//            string Identifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService\\";
//            var forginPath = path.Split(Identifier)[0];
//            Console.WriteLine("forgin Path: " + forginPath);

//            string newPath = forginPath + Identifier;
//            Console.WriteLine("new Path: " + newPath);
//            viewsAdress = newPath + "Settings\\" + viewsAdress;
//            Console.WriteLine("views Path: " + viewsAdress);

//#endif
//            List<View> views = new List<View>();
//            using (StreamReader r = new StreamReader(viewsAdress))
//            {
//                string viewsJson = r.ReadToEnd();
//                views = JsonSerializer.Deserialize<List<View>>(viewsJson);

//            }

            views.ForEach(delegate (View view) { Views.Add(view.ID, view); });
        }
        private static void LoadScreens()
        {
            Screens = new Dictionary<int, Screen>();
            var screens= LoadJSON<Screen>("Screens.JSON");
            screens.ForEach(delegate (Screen screen) { Screens.Add(screen.ID, screen); });
        }

    }
}
