using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;

namespace designAR
{
    class Wand
    {
        private const int IDLE_STATE = 0, PLACING_STATE = 1, MANIPULATING_STATE = 2;
        //private int state = 0;
        private SpriteBatch spriteBatch;
        //private Texture2D crosshairIdle;
        private GraphicsDevice graphicsDevice;
        private Scene scene;
        private Catalog catalog;

        public Wand(Scene theScene, GraphicsDevice gDevice, Catalog cat)
        {
            scene = theScene;
            graphicsDevice = gDevice;
            catalog = cat;
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Add a mouse click callback function to perform ray picking when mouse is clicked
            MouseInput.Instance.MouseClickEvent += new HandleMouseClick(MouseClickHandler);

            // Add a keyboard press handler for user input
            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressHandler);
        }

        private void Select()
        {
            // 0 means on the near clipping plane, and 1 means on the far clipping plane
            //Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
            //Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);
            Vector3 nearSource = new Vector3(graphicsDevice.Viewport.Width / 2.0f, graphicsDevice.Viewport.Height / 2.0f, 0);
            Vector3 farSource = new Vector3(graphicsDevice.Viewport.Width / 2.0f, graphicsDevice.Viewport.Height / 2.0f, 1);

            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, catalog.marker.WorldTransformation);
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, catalog.marker.WorldTransformation);

            // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
            // the physics objects in the scene (which we have set up to approximate the model geometry).
            List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(
                nearPoint, farPoint);

            // If one or more objects intersect with our ray vector
            if (pickedObjects.Count > 0)
            {
                // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                // the order of closest intersected object to farthest intersected object
                pickedObjects.Sort();

                // We only care about the closest picked object for now, so we'll simply display the name 
                // of the closest picked object whose container is a geometry node
                //label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";
                Console.WriteLine(((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name);
            }
            else
            {
                Console.WriteLine("NOTHING HERE BITCHES");
            }
        }

        private void Place()
        {

        }

        public void Draw()
        {

        }

        private void MouseClickHandler(int button, Point mouseLocation)
        {
            if (button == MouseInput.LeftButton)
            {
                Select();
            }
            else if (button == MouseInput.RightButton)
            {
                Place();
            }
        }

        private void KeyPressHandler(Keys keys, KeyModifier modifier)
        {
            // Detect key press "a"
            if (keys == Keys.A)
            {

            }
        }
    }
}
