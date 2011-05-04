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
        protected enum STATES { SELECTING, PLACING, MANIPULATING };
        private STATES actionState;
        private bool isFineRotation = false;
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private Scene scene;
        private Catalog catalog;
        private Room room;
        private Item selectedItem;
        private Item selectedItemDisplay;
        private float selectedItemRotation = 0;

        protected bool actionDisabled = true;

        protected Vector3 nearSource;
        protected Vector3 farSource;
        protected Vector2 screenCenter;
        protected Vector2 cursorPosition;

        private Texture2D selectSprite;
        private Texture2D placeSprite;
        private Texture2D manipulateSprite;
        private Texture2D currentCursor;
        private Texture2D disabledActionSprite;

        protected HUD hud;

        public Wand(Scene theScene, GraphicsDevice gDevice, Catalog cat, Room rm)
        {
            scene = theScene;
            graphicsDevice = gDevice;
            catalog = cat;
            room = rm;

            actionState = STATES.SELECTING;

            spriteBatch = new SpriteBatch(graphicsDevice);

            screenCenter = new Vector2(graphicsDevice.Viewport.Width / 2.0f, graphicsDevice.Viewport.Height / 2.0f);
            nearSource = new Vector3(screenCenter, 0);
            farSource = new Vector3(screenCenter, 1);

            // Add a mouse click callback function to perform ray picking when mouse is clicked
            MouseInput.Instance.MouseClickEvent += new HandleMouseClick(MouseClickHandler);
            MouseInput.Instance.MouseWheelMoveEvent += new HandleMouseWheelMove(MouseWheelHandler);

            // Add a keyboard press handler for user input
            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressHandler);
        }

        private void MouseWheelHandler(int delta, int value)
        {
            if (actionState == STATES.MANIPULATING)
            {
                if (isFineRotation)
                {
                    selectedItem.RotateBy(value / 100f);
                }
                else
                {
                    float degrees = value / 10f;
                    float rem = degrees % 15;
                    degrees -= rem;
                    selectedItem.RotateBy(degrees);
                }
                // Notifier.AddMessage("Scrolling delta: "+ delta + "   Value: " + value);
            }
        }
        
        private void MouseClickHandler(int button, Point mouseLocation)
        {
            switch (actionState)
            {
                case STATES.SELECTING:
                    if (button == MouseInput.LeftButton)
                    {
                        Select();
                    }
                    break;
                case STATES.PLACING:
                    if (actionDisabled)
                        break;

                    if (button == MouseInput.RightButton)
                    {
                        if (!isOverCatalogItem() && !isOverRoomItem())
                        {
                            Place();
                        }
                    }
                    else if (button == MouseInput.LeftButton)
                    {
                        if (isOverCatalogItem() || isOverRoomItem()) 
                        {
                            Select();
                        }
                        
                    }
                    break;
                case STATES.MANIPULATING:
                    
                    if (button == MouseInput.RightButton)
                    {
                        if (!isOverCatalogItem() && !isOverRoomItem())
                        {
                            Manipulate();
                        }
                    }
                    else if (button == MouseInput.LeftButton)
                    {

                        selectedItem.Selected = false;
                        selectedItem = null;
                        setState(STATES.SELECTING);
                        if (isOverCatalogItem() || isOverRoomItem())
                        {
                            Select();
                        }
                    }
                    break;
            }

        }

        private void Select()
        {
           if(catalog.isVisible() && !room.isVisible())
                SelectFromCatalog();

           if (room.isVisible() && !catalog.isVisible())
                SelectFromRoom();
        }

        private void SelectFromCatalog()
        {
            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, catalog.getMarkerTransform());
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, catalog.getMarkerTransform());

            // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
            // the physics objects in the scene (which we have set up to approximate the model geometry).
            List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

            // If one or more objects intersect with our ray vector
            if (pickedObjects.Count > 0)
            {
                // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                // the order of closest intersected object to farthest intersected object
                pickedObjects.Sort();

                // We only care about the closest picked object for now, so we'll simply display the name 
                // of the closest picked object whose container is a geometry node
                //label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";
                GeometryNode tempNode = new GeometryNode();
                int i = 0;
                tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                while (tempNode.GroupID == room.roomGroupID && i+1 <pickedObjects.Count)
                {
                    i++;
                    tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                }

                Console.WriteLine("Duplicating item from " + (tempNode.Name));
                selectedItem = catalog.selectCatalogItem(tempNode.Name);

                if (tempNode.GroupID != room.roomGroupID)
                    DrawSelectedItem(catalog.cloneCatalogItem(tempNode.Name));

                if (selectedItem != null)
                {
                    Console.WriteLine("New item is " + selectedItem.Label);
                    setState(STATES.PLACING);
                }

            }
            else
            {
                Console.WriteLine("NOTHING HERE BITCHES");
            }
        }

        private void DrawSelectedItem(Item i)
        {
            if(selectedItemDisplay != null)
                selectedItemDisplay.Unbind();
            selectedItemDisplay = i;
            selectedItemDisplay.BindTo(scene.RootNode);
            selectedItemDisplay.Translation = new Vector3(.475f, -.4f, -1);
            //selectedItemDisplay.Translation = new Vector3(0, .06f, -1);
            selectedItemDisplay.Scale = new Vector3(0.005f, 0.005f, 0.005f);
            selectedItemDisplay.SetAlpha(0.55f);
        }

        private void SelectFromRoom()
        {
            // Now convert the near and far source to actual near and far 3D points based on our eye location
            // and view frustum
            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());

            // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
            // the physics objects in the scene (which we have set up to approximate the model geometry).
            List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

            // If one or more objects intersect with our ray vector
            if (pickedObjects.Count > 0)
            {
                // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                // the order of closest intersected object to farthest intersected object
                pickedObjects.Sort();

                // We only care about the closest picked object for now, so we'll simply display the name 
                // of the closest picked object whose container is a geometry node
                //label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";

                GeometryNode tempNode = new GeometryNode();
                int i = 0;
                tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                while (tempNode.GroupID == room.roomGroupID && i + 1 < pickedObjects.Count)
                {
                    i++;
                    tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                }


                Console.WriteLine("Duplicating item from " + tempNode.Name);
                selectedItem = catalog.selectPlacedItem(tempNode.Name);

                if (selectedItem != null)
                {
                    Console.WriteLine("New item is " + selectedItem.Label);
                    setState(STATES.MANIPULATING);
                }
                else
                {
                    Console.WriteLine("No item received");
                }
            }
            else
            {
                Console.WriteLine("Nothing to select");
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

                if (selectedItem != null)
                {
                    selectedItem.BindTo(room);
                    Vector3 direction = farPoint - nearPoint;
                    Vector3 placement = nearPoint + direction*pickedObjects[0].IntersectParam;
                    Notifier.AddMessage(placement.X + " " + placement.Y + " " + placement.Z);
                    Console.WriteLine(placement.X + " " + placement.Y + " " + placement.Z);
                    //placement.Z = 0f;
                    selectedItem.MoveTo(placement);
                    selectedItem.Selected = true;
                    selectedItemDisplay.Unbind();
                    setState(STATES.MANIPULATING);
                }
            }
        }


        private void Manipulate()
        {
            Console.WriteLine("Manipulate!");
            Notifier.AddMessage("Manipulate!");

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

                if (selectedItem != null)
                {
                    //itemToPlace.BindTo(room);
                    Vector3 direction = farPoint - nearPoint;
                    //direction.Normalize();
                    Vector3 placement = nearPoint + direction * pickedObjects[0].IntersectParam;
                    Notifier.AddMessage(placement.X + " " + placement.Y + " " + placement.Z);
                    Console.WriteLine(placement.X + " " + placement.Y + " " + placement.Z);
                    //placement.Z = 0f;
                    selectedItem.MoveTo(placement);
                    //setState(STATES.MANIPULATING);
                }
            }
        }

        private void KeyPressHandler(Keys keys, KeyModifier modifier)
        {
            // Detect key press "a"
            if (keys == Keys.A)
            {

            }
            if (keys == Keys.R)
            {
                isFineRotation = !isFineRotation;
            }
            if (keys == Keys.Delete || keys == Keys.Back)
            {
                if (actionState == STATES.MANIPULATING)
                {
                    selectedItem.Unbind();
                    setState(STATES.SELECTING);
                }
            }
        }

        public void Draw()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(currentCursor, cursorPosition, Color.White);
            if(actionDisabled)
                spriteBatch.Draw(disabledActionSprite, cursorPosition, Color.White);

            spriteBatch.End();
        }

        public void setSelectCrosshair(Texture2D sprite)
        {
            this.selectSprite = sprite;
            setTexture(selectSprite);
        }

        public void setPlaceCrosshair(Texture2D sprite)
        {
            this.placeSprite = sprite;
        }

        public void setManipulateCrosshair(Texture2D sprite)
        {
            this.manipulateSprite = sprite;
        }

        internal void setDisabledCrosshair(Texture2D sprite)
        {
            this.disabledActionSprite = sprite;
        }

        internal void setTexture(Texture2D sprite)
        {
            currentCursor = sprite;
            cursorPosition = new Vector2(screenCenter.X - currentCursor.Width / 2f, screenCenter.Y - currentCursor.Height / 2f);
        }

        private void setState(STATES s)
        {
            this.actionState = s;
            hud.StatusMessage = s.ToString();

            switch (actionState)
            {
                case STATES.SELECTING:
                    setTexture(selectSprite);
                    break;
                case STATES.PLACING:
                    setTexture(placeSprite);
                    break;
                case STATES.MANIPULATING:
                    setTexture(manipulateSprite);
                    break;
            }
        }

        private bool isOverCatalogItem()
        {

            if (catalog.isVisible())
            {
                // Now convert the near and far source to actual near and far 3D points based on our eye location
                // and view frustum
                Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                    State.ProjectionMatrix, State.ViewMatrix, catalog.getMarkerTransform());
                Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, catalog.getMarkerTransform());

                // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
                // the physics objects in the scene (which we have set up to approximate the model geometry).
                List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                // If one or more objects intersect with our ray vector
                if (pickedObjects.Count > 0)
                {
                    // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                    // the order of closest intersected object to farthest intersected object
                    pickedObjects.Sort();

                    // We only care about the closest picked object for now, so we'll simply display the name 
                    // of the closest picked object whose container is a geometry node
                    //label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";
                    GeometryNode tempNode = new GeometryNode();
                    int i = 0;
                    tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                    while (tempNode.GroupID == room.roomGroupID && i + 1 < pickedObjects.Count)
                    {
                        i++;
                        tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                    }


                    Console.WriteLine("Over item from " + (tempNode.Name));
                    return catalog.catalogContains(tempNode.Name);

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false; //catalog not visible
            }

        }

        private bool isOverRoomItem()
        {
            if (room.isVisible())
            {
                // Now convert the near and far source to actual near and far 3D points based on our eye location
                // and view frustum
                Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                    State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());
                Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());

                // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
                // the physics objects in the scene (which we have set up to approximate the model geometry).
                List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                // If one or more objects intersect with our ray vector
                if (pickedObjects.Count > 0)
                {
                    // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                    // the order of closest intersected object to farthest intersected object
                    pickedObjects.Sort();

                    // We only care about the closest picked object for now, so we'll simply display the name 
                    // of the closest picked object whose container is a geometry node
                    //label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";
                    GeometryNode tempNode = new GeometryNode();
                    int i = 0;
                    tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                    while (tempNode.GroupID == room.roomGroupID && i + 1 < pickedObjects.Count)
                    {
                        i++;
                        tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                    }


                    Console.WriteLine("Over item from " + (tempNode.Name));
                    return catalog.roomContains(tempNode.Name);

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false; //room not visible
            }
        }

        private bool isOverFloor()
        {
            if (room.isVisible())
            {
                // Now convert the near and far source to actual near and far 3D points based on our eye location
                // and view frustum
                Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearSource,
                    State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());
                Vector3 farPoint = graphicsDevice.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, room.getMarkerTransform());

                // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
                // the physics objects in the scene (which we have set up to approximate the model geometry).
                List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                // If one or more objects intersect with our ray vector
                if (pickedObjects.Count > 0)
                {
                    // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                    // the order of closest intersected object to farthest intersected object
                    pickedObjects.Sort();

                    Console.WriteLine("Over " + ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name);
                    return ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name.Equals("Floor");


                    /*
                    // We only care about the closest picked object for now, so we'll simply display the name 
                    // of the closest picked object whose container is a geometry node
                    //label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";
                     GeometryNode  tempNode = new GeometryNode();
                    int i = 0;
                    tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                    while (tempNode.GroupID == room.roomGroupID && i + 1 < pickedObjects.Count)
                    {
                        i++;
                        tempNode = (GeometryNode)pickedObjects[i].PickedPhysicsObject.Container;
                    }


                    Console.WriteLine("Over " + (tempNode.Name));
                    return catalog.roomContains(tempNode.Name);
                     * */


                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false; //room not visible
            }
        }



        internal void Update(GameTime gameTime)
        {
            actionDisabled = !isValidAction();
            if (actionState == STATES.PLACING)
            {
                if (isOverCatalogItem())
                {
                    setTexture(selectSprite);
                    actionDisabled = false;
                }
                else if (isOverRoomItem()) //could be snapping icon in future
                {
                    setTexture(selectSprite);
                    actionDisabled = false;
                }
                else
                {
                    setTexture(placeSprite);
                }
            }

            if (actionState == STATES.MANIPULATING)
            {
                if (isOverCatalogItem())
                {
                    setTexture(selectSprite);
                    actionDisabled = false;
                }
                else if (isOverRoomItem()) //could be snapping icon in future
                {
                    setTexture(selectSprite);
                    actionDisabled = false;
                }
                else
                {
                    setTexture(manipulateSprite);
                }
            }

            if (selectedItemDisplay != null)
                selectedItemDisplay.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(selectedItemRotation));

            selectedItemRotation += gameTime.ElapsedGameTime.Milliseconds/50f;
            if (selectedItemRotation > 360)
                selectedItemRotation -= 360;


            hud.StatusMessage = getStatusMessage();
        }

        private string getStatusMessage()
        {
            
            if (currentCursor == manipulateSprite)
                if (actionDisabled)
                    return "Move cursor to room to manipulate";
                else
                    return "Left Click: Confirm | Right Click: Move | Scroll Wheel: Rotate";
            else if (currentCursor == placeSprite)
                if (actionDisabled)
                    return "Move cursor to room to place the object";
                else
                    return "Right Click to place at target location";
            else if (currentCursor == selectSprite)
                if (actionDisabled)
                    return "Click on an object to select it";
                else
                    return "Left Click to select this item";
            else if (actionDisabled)
                return "Invalid position for this action";
            else
                return "";
        }

        private bool isValidAction()
        {
            switch (actionState)
            {
                case STATES.SELECTING:
                    return isOverCatalogItem() || isOverRoomItem();
                case STATES.PLACING:
                    return isOverFloor();
                case STATES.MANIPULATING:
                    return isOverFloor();
            }

            return false;
        }

        public virtual HUD Hud
        {
            //get { return restrictedDimension; }
            set { hud = value; }
        }
    }
}
