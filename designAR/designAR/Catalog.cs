using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;

namespace designAR
{
    class Catalog
    {

        private MarkerNode marker, changeMarker;
       // List<TransformNode> objects;
        int changeTime;
        int num_displayed = 9;
        int cur_start = 0;
        int cur_end = 9;
        float cur_angle = 0;
        ItemLibrary library;
        List<Item> item_list;
        Scene my_scene;
        Dictionary<String, Item> names2items;
        public Catalog(Scene s)
        {
            this.my_scene = s;
            
            this.marker = new MarkerNode(my_scene.MarkerTracker, "palette_marker.xml");
            this.changeMarker = new MarkerNode(my_scene.MarkerTracker, "palette_turn_marker.xml");
            my_scene.RootNode.AddChild(marker);
            my_scene.RootNode.AddChild(changeMarker);
            library = new ItemLibrary("models.txt");
            names2items = new Dictionary<string, Item>();
            item_list = library.getAllItems();
          //  this.objects = l;
            int grid_x = 0;
            int grid_y = 0;
            foreach (Item i in item_list)
            {
                names2items.Add(i.Name, i);
            }
            for (int i = cur_start; i < cur_end && i < item_list.Count; i++)
            {
                grid_x += 10;

                if (grid_x > 46)
                {
                    grid_x = 0;
                    grid_y -= 10;
                }
                item_list[i].BindTo(marker);
               // objects[i].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2);
               item_list[i].MoveTo( new Vector3(grid_x, grid_y, 0));
             
            }
        }
        public Item selectItem(String s)
        {
            Item i;
            names2items.TryGetValue(s, out i);
            i.Selected = true;
            return new Item(i);
        }
        public void display(GameTime gameTime)
        {
            if (marker.MarkerFound)
            {
                for (int i = cur_start; i < cur_end && i <item_list.Count; i++)
                {
                   // objects[i].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, cur_angle);

                }
                if (cur_angle >= Math.PI * 2) cur_angle = 0;
                else cur_angle += 0.01f;

            }
            if (changeMarker.MarkerFound && (gameTime.TotalGameTime.Seconds - changeTime) > 2)
            {

                for (int i = cur_start; i < cur_end && i < item_list.Count; i++)
                {

                    item_list[i].UnbindFrom(marker);


                }
                changeTime = gameTime.TotalGameTime.Seconds;
                if (cur_end > item_list.Count)
                {
                    cur_end = num_displayed;
                    cur_start = 0;
                }
                else
                {
                    cur_start += num_displayed;
                    cur_end += num_displayed;
                }
                int grid_x = 0;
                int grid_y = 0;
                for (int i = cur_start; i < cur_end && i < item_list.Count; i++)
                {
                    grid_x += 10;

                    if (grid_x > 30)
                    {
                        grid_x = 0;
                        grid_y -= 10;
                    }
                    item_list[i].Selected = true;
                    item_list[i].BindTo(marker);
                    item_list[i].MoveTo( new Vector3(grid_x, grid_y, 0));  

                }


            }
        }


        public Matrix getMarkerTransform()
        {
            return this.marker.WorldTransformation;
        }

    }
}
