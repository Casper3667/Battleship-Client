using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Settings
{
    public static class Settings
    {
        public static Dictionary<int, View> Views { get; private set; } = new Dictionary<int, View>();
        public static Dictionary<int, Screen> Screens { get; private set; }

        public static void LoadSettings()
        {

            //System.IO.Path.GetDirectoryName()


            LoadViews();
            LoadScreens();


        }

        private static List<T> LoadJSON<T>(string FileName)
        {


#if DEBUG
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Console.WriteLine("Path: " + System.IO.Path.GetDirectoryName(path));
            string Identifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService\\";
            var forginPath = path.Split(Identifier)[0];
            Console.WriteLine("forgin Path: " + forginPath);

            string newPath = forginPath + Identifier;
            Console.WriteLine("new Path: " + newPath);
            FileName= newPath + "Settings\\" + FileName;
            Console.WriteLine("views Path: " + FileName);

#endif
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
