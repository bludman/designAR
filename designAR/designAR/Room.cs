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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

//GoblinXNA dependencies
using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision; // Required for vision
using GoblinXNA.Device.Vision.Marker; // Required for vision
using GoblinXNA.Device.Generic; // Required for generic input devices (mouse, keyboard, etc)
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1; // Required for physics simulation
using GoblinXNA.Helpers;
using GoblinXNA.Sounds; // Required for sound effects
using GoblinXNA.Graphics.ParticleEffects; // required for particle effects
using GoblinXNA.UI;

namespace designAR  
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
     class Room : IBindable//: Microsoft.Xna.Framework.Game
    {

        
        private Scene scene;
        public MarkerNode groundMarkerNode, toolbarMarkerNode,worldInMiniatureMarkerNode;
        private GeometryNode floor, frontWall, rightWall;
        private TransformNode floorTransNode, frontWallTransNode, rightWallTransNode;
        private Material floorMaterial,wallsMaterial;
        private int gridCenterX = 0;
        private int gridCenterY = 0;
        private int gridOffsetX = 40;
        private int gridOffsetY = 40;
        private int floorLength, floorBreadth;
        public Dictionary<String, Item> objectsInRoom;
        public int roomGroupID = 1;
       

        Random random = new Random(); // Random number generator. Required for particle effects

        public Room(Scene sceneFromMain, int floorLength, int floorBreadth)
        {
            this.scene = sceneFromMain;
            this.floorBreadth = floorBreadth;
            this.floorLength = floorLength;
            this.objectsInRoom = new Dictionary<string, Item>();
            //Content.RootDirectory = "Content";
            //this.Initialize();
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public void Initialize()
        {
            // Initialize the GoblinXNA framework
            //State.InitGoblin(graphics, Content, "");

            //this.IsMouseVisible = true;

            //scene = new Scene(this);


            // Use the newton physics engine to perform collision detection
            // scene.PhysicsEngine = new NewtonPhysics();

            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically


            // Set up the lights used in the scene
            CreateLights();

            // Create 3D objects
            CreateObjects();

            // Create the ground that represents the physical ground marker array
            CreateGround();

        }

        public void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            this.scene.RootNode.AddChild(lightNode);
        }

        //Let's set up marker tracking

        public void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");

            groundNode.Model = new Box(95, 59, 0.1f);

            // Set this ground model to act as an occluder so that it appears transparent
            groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // CastShadows set to true
            groundNode.Model.ReceiveShadows = true;

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.Gray.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;

            groundMarkerNode.AddChild(groundNode);
        }


        public void CreateObjects()
        {
            
            floor = new GeometryNode("Floor");//floor
            floor.Model = new Box(floorBreadth, floorLength, 1);
            floorTransNode = new TransformNode();
            floorTransNode.Translation = new Vector3(floorBreadth / 3, -floorLength / 2, 1);      // 0 0
            floor.Physics.Collidable = true;
            floor.Physics.Pickable = true;
            floor.GroupID = roomGroupID;
            floor.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            floor.AddToPhysicsEngine = true;
            //grid1.IsOccluder = true;


            //Material for the room floor
            floorMaterial = new Material();
            Vector4 alpha = Color.Gray.ToVector4();
            Vector3 tempAlpha = new Vector3(alpha.X, alpha.Y, alpha.Z);
            floorMaterial.Diffuse = new Vector4(tempAlpha, 0.2f);
            floorMaterial.Specular = Color.White.ToVector4();
            floorMaterial.SpecularPower = 20;

            floor.Material = floorMaterial;

            //Material for the walls
            wallsMaterial = new Material();
            wallsMaterial.Diffuse = Color.Gray.ToVector4();
            wallsMaterial.Specular = Color.White.ToVector4();
            wallsMaterial.SpecularPower = 20;

            frontWall = new GeometryNode("Front wall");//front wall
            frontWall.Model = new Box(floorBreadth, floorLength, 1);
            frontWallTransNode = new TransformNode();
            frontWallTransNode.Translation = new Vector3(floorBreadth / 3, 0, floorLength/2);  // 1 0
            frontWallTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90));
            frontWall.Material = wallsMaterial;
            frontWall.GroupID = roomGroupID;
            frontWall.AddToPhysicsEngine = true;
            frontWall.Physics.Collidable = true;
            frontWall.Physics.Pickable = true;
            frontWall.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;



            rightWall = new GeometryNode("Right wall");//right side wall
            rightWall.Model = new Box(floorBreadth, floorLength, 1);
            rightWallTransNode = new TransformNode();
            rightWallTransNode.Translation = new Vector3(floorBreadth*4.2f/5, -floorLength / 2, floorLength / 2); // -1 0
            rightWallTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90));
            rightWall.Material = wallsMaterial;
            rightWall.AddToPhysicsEngine = true;
            rightWall.GroupID = roomGroupID;
            rightWall.Physics.Pickable = true;
            rightWall.Physics.Collidable = true;
            rightWall.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;

          


            // Create a marker node to track a ground marker array.
#if USE_ARTAG
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ground");

            // Since we expect that the ground marker array won't move very much, we use a 
            // small smoothing alpha.
            //groundMarkerNode.Smoother = new DESSmoother(0.2f, 0.1f, 1, 1);
#else
            //   groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");
            groundMarkerNode = new MarkerNode(this.scene.MarkerTracker, "ALVARConfig.txt");

#endif




            // Now add the above nodes to the scene graph in the appropriate order.
            // Note that only the nodes added below the marker node are affected by 
            // the marker transformation.
            scene.RootNode.AddChild(groundMarkerNode);



            //Change the toolbar.txt config file. As of now, toolbar has markers included in the long stretch-alvarConfig.txt
            worldInMiniatureMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARConfig32_33.txt");


            scene.RootNode.AddChild(worldInMiniatureMarkerNode);





            groundMarkerNode.AddChild(floorTransNode);
            floorTransNode.AddChild(floor);


            groundMarkerNode.AddChild(frontWallTransNode);
            frontWallTransNode.AddChild(frontWall);

            groundMarkerNode.AddChild(rightWallTransNode);
            rightWallTransNode.AddChild(rightWall);
            


        }
        public void addObject(Item item)
        {
           
            objectsInRoom.Add(item.Label, item);
            //TODO: add object name along with its corresponding TransformNode to a HashMap.
            Console.Write("object added: " + item.Label);
        }

        public Item getObject(String itemName)
        {
            if (objectsInRoom.ContainsKey(itemName))
                return objectsInRoom[itemName];
            else
                return null;
            //TODO: return transformNode corresponding to the object with label ObjectName.  
        }


        public void Update()
        {
            // Allows the game to exit

            if (groundMarkerNode.MarkerFound && worldInMiniatureMarkerNode.MarkerFound)
            {
                frontWall.IsOccluder = false;
                rightWall.IsOccluder = false;
                floor.Material = wallsMaterial;

            }
            else if (groundMarkerNode.MarkerFound)
            {
                frontWall.IsOccluder = true;
                rightWall.IsOccluder = true;
                floor.Material = floorMaterial;
            }
            // TODO: Change wall occlusion status depending on whether world in miniature is to be displayed or not. 

            // TODO: Add your update logic here

            //base.Update(gameTime);

        }

        public Matrix getMarkerTransform()
        {
            return this.groundMarkerNode.WorldTransformation;
        }

        public BranchNode getBindNode()
        {
            return this.groundMarkerNode;
        }


        internal bool isVisible()
        {
            return this.groundMarkerNode.MarkerFound;
        }

    }

}
