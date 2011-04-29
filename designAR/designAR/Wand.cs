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
using GoblinXNA.UI;

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
        private Room room;
        private Item itemToPlace;

        Vector3 nearSource;
        Vector3 farSource;
        Vector2 screenCenter;
        Vector2 cursorPosition;

        protected Texture2D selectSprite;
        protected Texture2D currentCursor;

        public Wand(Scene theScene, GraphicsDevice gDevice, Catalog cat, Room rm)
        {
            scene = theScene;
            graphicsDevice = gDevice;
            catalog = cat;

            room = rm;

            spriteBatch = new SpriteBatch(graphicsDevice);


            screenCenter = new Vector2(graphicsDevice.Viewport.Width / 2.0f, graphicsDevice.Viewport.Height / 2.0f);
            //nearSource = new Vector3(graphicsDevice.Viewport.Width / 2.0f, graphicsDevice.Viewport.Height / 2.0f, 0);
            nearSource = new Vector3(screenCenter, 0);
            //farSource = new Vector3(graphicsDevice.Viewport.Width / 2.0f, graphicsDevice.Viewport.Height / 2.0f, 1);
            farSource = new Vector3(screenCenter, 1);

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

            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, catalog.getMarkerTransform());
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, catalog.getMarkerTransform());

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
                // Getting an new instance of the item
                itemToPlace = catalog.selectItem(((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name);
            }
            else
            {
                Console.WriteLine("NOTHING HERE BITCHES");
            }
        }

        private void Place()
        {
            Console.WriteLine("Placing!");
            Notifier.AddMessage("Placing!");

            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());

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
                // Getting an new instance of the item
                //itemToPlace = catalog.selectItem(((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name);
                Notifier.AddMessage(pickedObjects[0].IntersectParam.ToString() + " " + ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name);

                if (itemToPlace != null)
                {
                    itemToPlace.BindTo(room);
                    Vector3 direction = farPoint - nearPoint;
                    direction.Normalize();
                    Vector3 placement = nearPoint + direction*float.Parse(pickedObjects[0].IntersectParam.ToString());
                    Notifier.AddMessage(placement.X + " " + placement.Y + " " + placement.Z);
                    Console.WriteLine(placement.X + " " + placement.Y + " " + placement.Z);
                    itemToPlace.MoveTo(placement);
                }
            }
        }

        public void Draw()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(currentCursor, cursorPosition, Color.White);
            spriteBatch.End();
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

        internal void setTexture(Texture2D sprite)
        {
            this.selectSprite = sprite;
            currentCursor = selectSprite;
            cursorPosition = new Vector2(screenCenter.X - currentCursor.Width / 2f, screenCenter.Y - currentCursor.Height / 2f);
        }
    }
}
