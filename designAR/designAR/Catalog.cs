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

        public MarkerNode marker, changeMarker;
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

            // Create a geometry node with a model of box
            GeometryNode boxNode = new GeometryNode("Box");
            boxNode.Model = new Box(Vector3.One * 3);

            Material boxMat = new Material();
            boxMat.Diffuse = Color.Red.ToVector4();
            boxMat.Specular = Color.White.ToVector4();
            boxMat.SpecularPower = 5;

            boxNode.Material = boxMat;

            TransformNode boxTransNode = new TransformNode();
            boxTransNode.Translation = new Vector3(-5, 0, -6);

            // Define the most suitable shape type for this model
            // which is Box in this case so that the physics engine
            // will understand how to take care of the collision
            boxNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            // Set this box model to be pickable
            boxNode.Physics.Pickable = true;
            // Add this box model to the physics engine
            boxNode.AddToPhysicsEngine = true;

            marker.AddChild(boxTransNode);
            boxTransNode.AddChild(boxNode);

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
