using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_ClientService.Settings
{
    public class Screen
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public int? Height { get; set; } 
        public int? Width { get; set; }
        public string? ViewLayout{ get; set; }
        private List<ScreenViewData> views;
        public List<ScreenViewData>? Views
        {
            get
            {
                if (views == null)
                {
                    views = DecodeViewLayout();
                }
                return views;
            }
        }
        private int? screenHeight;
        public int? ScreenHeight { get 
            {
                if(screenHeight==null)
                {
                    if (Height == null)
                    {
                        var temp = Views; // Load In All Views And thus Find ScreenHeight
                    }
                    else
                        screenHeight =(int) Height;

                }
                return screenHeight;
            } }
        private List<ScreenViewData>? DecodeViewLayout()
        {
            if (ViewLayout == null)
                return null;
            //Console.WriteLine("Decoding this: " + ViewLayout);
            var ids=ViewLayout.Split(',');
            var list=new List<ScreenViewData>();
            int ViewHeight = 0;
            foreach (var id in ids)
            {
                //Console.WriteLine(id);
                if(int.TryParse(id,out int ID)){
                    var view = Settings.Views[ID];
                    var viewData=new ScreenViewData(this,view,ViewHeight,Width);
                    list.Add(viewData);
                    ViewHeight+=view.Height;
                }
                
            }

            if (Height == null)
                screenHeight = ViewHeight;

            return list;
        }
    }

    public class ScreenViewData
    {
        public View View { get; private set; }
        public Screen Screen { get; private set; }
        public int StartHeight { get; private set; }
        public int? ScreenWidth { get; private set; }

        public ScreenViewData(Screen screen,View view,int startHeight,int? screenWidth) 
        {
            Screen = screen;
            this.View = view;
            this.StartHeight = startHeight;
            this.ScreenWidth = screenWidth;

        }
    }
}
