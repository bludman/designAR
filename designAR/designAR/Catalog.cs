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

        MarkerNode marker, changeMarker;
       // List<TransformNode> objects;
        int changeTime;
        int num_displayed = 9;
        int cur_start = 0;
        int cur_end = 9;
        float cur_angle = 0;
        ItemLibrary library;
        Scene my_scene;
        public Catalog(Scene s)
        {
            this.my_scene = s;
            
            this.marker = new MarkerNode(my_scene.MarkerTracker, "palette_marker.xml");
            this.changeMarker = new MarkerNode(my_scene.MarkerTracker, "palette_turn_marker.xml");
            my_scene.RootNode.AddChild(marker);
            my_scene.RootNode.AddChild(changeMarker);
            library = new ItemLibrary("models.txt");
          //  this.objects = l;
            int grid_x = 0;
            int grid_y = 0;
            for (int i = cur_start; i < cur_end && i < library.getAllItems().Count; i++)
            {
                grid_x += 10;

                if (grid_x > 46)
                {
                    grid_x = 0;
                    grid_y -= 10;
                }
                library.getAllItems()[i].BindTo(marker);
               // objects[i].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2);
                library.getAllItems()[i].MoveTo( new Vector3(grid_x, grid_y, 0));

            }

        }
        public void display(GameTime gameTime)
        {
            if (marker.MarkerFound)
            {
                for (int i = cur_start; i < cur_end && i < library.getAllItems().Count; i++)
                {
                   // objects[i].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, cur_angle);

                }
                if (cur_angle >= Math.PI * 2) cur_angle = 0;
                else cur_angle += 0.01f;

            }
            if (changeMarker.MarkerFound && (gameTime.TotalGameTime.Seconds - changeTime) > 2)
            {

                for (int i = cur_start; i < cur_end && i < library.getAllItems().Count; i++)
                {

                    library.getAllItems()[i].UnbindFrom(marker);


                }
                changeTime = gameTime.TotalGameTime.Seconds;
                if (cur_end > library.getAllItems().Count)
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
                for (int i = cur_start; i < cur_end && i < library.getAllItems().Count; i++)
                {
                    grid_x += 10;

                    if (grid_x > 30)
                    {
                        grid_x = 0;
                        grid_y -= 10;
                    }
                    library.getAllItems()[i].BindTo(marker);
                    library.getAllItems()[i].MoveTo( new Vector3(grid_x, grid_y, 0));
                    //objects[i].Translation = new Vector3(0, 0, 0);
                    //objects[i].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2);
                   // objects[i].Translation = new Vector3(grid_x, grid_y, 0);

                }


            }
        }


    }
}
