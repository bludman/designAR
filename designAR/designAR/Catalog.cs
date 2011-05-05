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

        private const bool SPIN = false;
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
        Dictionary<String, Item> names2itemsInCatalog;
        Dictionary<String, Item> names2itemsInRoom;
        public Catalog(Scene s)
        {
            this.my_scene = s;
            
            this.marker = new MarkerNode(my_scene.MarkerTracker, "palette_marker.xml");
            this.changeMarker = new MarkerNode(my_scene.MarkerTracker, "palette_turn_marker.xml");
            my_scene.RootNode.AddChild(marker);
            my_scene.RootNode.AddChild(changeMarker);
            library = new ItemLibrary("models.txt");
            names2itemsInCatalog = new Dictionary<string, Item>();
            names2itemsInRoom = new Dictionary<string, Item>();
            item_list = library.getAllItems();
          //  this.objects = l;
            int grid_x = 0;
            int grid_y = 0;
            foreach (Item i in item_list)
            {
                names2itemsInCatalog.Add(i.Label, i);
            }
            for (int i = cur_start; i < cur_end && i < item_list.Count; i++)
            {

                if (grid_x > 15)
                {
                    grid_x = 0;
                    grid_y -= 15;
                }
                item_list[i].BindTo(marker);
                item_list[i].MoveTo(new Vector3(grid_x, grid_y, 0));
                grid_x += 15;
             
            }

            // Create a geometry node with a model of box
            GeometryNode boxNode = new GeometryNode("Box");
            boxNode.Model = new Box(30,30,0.1f);

            TransformNode boxTransNode = new TransformNode();
            boxTransNode.AddChild(boxNode);
            boxTransNode.Translation += new Vector3(6, -6, -0.5f);

            Material boxMat = new Material();
            boxMat.Diffuse = Color.DimGray.ToVector4();

            boxNode.Material = boxMat;

            marker.AddChild(boxTransNode);


        }

        //THIS IS JUST A TEST 
        public Item selectPlacedItem(String s)
        {
            Item i;
            bool success = names2itemsInRoom.TryGetValue(s, out i);
            if (success)
            {
                i.Selected = true;
                return i;
            }
            else return null;
        }

        public Item selectCatalogItem(String s)
        {
            Item newI = cloneCatalogItem(s);
            if (newI != null)
            {
                Item i;
                bool success = names2itemsInCatalog.TryGetValue(s, out i);
                if (success)
                {
                    i.Selected = true;
                }
            }
            return newI;
        }

        public Item cloneCatalogItem(String s)
        {
            Item i;
            bool success = names2itemsInCatalog.TryGetValue(s, out i);
            if (success)
            {
                i.Selected = true;
                Item newI = new Item(i);
                names2itemsInRoom.Add(newI.Label, newI);
                return newI;
            }
            else return null;
        }

        public bool catalogContains(String s)
        {
            return names2itemsInCatalog.ContainsKey(s);
        }

        public bool roomContains(String s)
        {
            return names2itemsInRoom.ContainsKey(s);
        }

        public void display(GameTime gameTime)
        {
            if (marker.MarkerFound && SPIN)
            {
                for (int i = cur_start; i < cur_end && i <item_list.Count; i++)
                {
                   // objects[i].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, cur_angle);
                    item_list[i].RotateBy(cur_angle);
                }
                if (cur_angle 
                    >= 360) cur_angle = 0;
                else cur_angle += 0.02f * (float)gameTime.ElapsedGameTime.Milliseconds;

            }
            if (changeMarker.MarkerFound && (gameTime.TotalGameTime.Seconds - changeTime) > 2)
            {

                for (int i = cur_start; i < cur_end && i < item_list.Count; i++)
                {
                    item_list[i].Unbind();
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


        internal bool isVisible()
        {
            return this.marker.MarkerFound;
        }
    }
}
