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
        public MarkerNode groundMarkerNode, toolbarMarkerNode;
        private GeometryNode grid1, grid2, grid3, grid4, grid5, grid6, grid7;
        private TransformNode grid1TransNode, grid2TransNode, grid3TransNode, grid4TransNode, grid5TransNode, grid6TransNode, grid7TransNode;
        private Material grid1Material;
        private int gridCenterX = 0;
        private int gridCenterY = 0;
        private int gridOffsetX = 40;
        private int gridOffsetY = 40;
        DirectShowCapture2 captureDevice;
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
            GeometryNode[] grid = new GeometryNode[10];
            int i = 0;
            grid[1] = new GeometryNode("GridBox" + i);


            grid1 = new GeometryNode("Floor");//floor
            grid1.Model = new Box(floorBreadth, floorLength, 1);
            grid1TransNode = new TransformNode();
            grid1TransNode.Translation = new Vector3(floorBreadth / 3, -floorLength / 2, 1);      // 0 0
            grid1.Physics.Collidable = true;
            grid1.Physics.Pickable = true;
            grid1.GroupID = roomGroupID;
            grid1.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            grid1.AddToPhysicsEngine = true;
            //grid1.IsOccluder = true;

            grid1Material = new Material();
            Vector4 alpha = Color.Gray.ToVector4();
            Vector3 tempAlpha = new Vector3(alpha.X, alpha.Y, alpha.Z);
            grid1Material.Diffuse = new Vector4(tempAlpha, 0.2f);
            grid1Material.Specular = Color.White.ToVector4();

            grid1Material.SpecularPower = 20;

            grid1.Material = grid1Material;



            grid2 = new GeometryNode("Front wall");//front wall
            grid2.Model = new Box(floorBreadth, floorLength, 1);
            grid2TransNode = new TransformNode();
            grid2TransNode.Translation = new Vector3(floorBreadth / 3, 0, floorLength/2);  // 1 0
            grid2TransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90));
            grid2.Material = grid1Material;
            grid2.GroupID = roomGroupID;
            grid2.AddToPhysicsEngine = true;
            grid2.Physics.Collidable = true;
            grid2.Physics.Pickable = true;
            grid2.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;



            grid3 = new GeometryNode("Right wall");//right side wall
            grid3.Model = new Box(floorBreadth, floorLength, 1);
            grid3TransNode = new TransformNode();
            grid3TransNode.Translation = new Vector3(floorBreadth*4.2f/5, -floorLength / 2, floorLength / 2); // -1 0
            grid3TransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90));
            grid3.Material = grid1Material;
            grid3.AddToPhysicsEngine = true;
            grid3.GroupID = roomGroupID;
            grid3.Physics.Pickable = true;
            grid3.Physics.Collidable = true;
            grid3.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;

            grid4 = new GeometryNode("Grid Box4");
            grid4.Model = new Box(30, 30, 1);
            grid4TransNode = new TransformNode();
            grid4TransNode.Translation = new Vector3(gridCenterX, gridCenterY + gridOffsetY, 1); //0 1
            grid4.Material = grid1Material;
            grid4.AddToPhysicsEngine = true;
            grid4.Physics.Pickable = true;
            grid4.Physics.Collidable = true;
            grid4.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;


            //change direction of cylinders to get grid
            grid5 = new GeometryNode("Grid Box5");
            grid5.Model = new Box(30, 30, 1);
            grid5TransNode = new TransformNode();
            //grid5TransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90));
            grid5TransNode.Translation = new Vector3(gridCenterX, gridCenterY - gridOffsetY, 1);                        // 0 -1
            grid5.Material = grid1Material;
            grid5.AddToPhysicsEngine = true;
            grid5.Physics.Pickable = true;
            grid5.Physics.Collidable = true;
            grid5.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;

            grid6 = new GeometryNode("Grid Box6");
            grid6.Model = new Box(30, 30, 1);
            grid6TransNode = new TransformNode();
            //grid6TransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90));
            grid6TransNode.Translation = new Vector3(gridCenterX + gridOffsetX, gridCenterY + gridOffsetY, 1);                                    //1 1
            grid6.Material = grid1Material;
            grid6.AddToPhysicsEngine = true;
            grid6.Physics.Pickable = true;
            grid6.Physics.Collidable = true;
            grid6.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;


            grid7 = new GeometryNode("Grid Box7");
            grid7.Model = new Box(30, 30, 1);
            grid7TransNode = new TransformNode();
            //grid7TransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90));
            grid7TransNode.Translation = new Vector3(gridCenterX - gridOffsetX, gridCenterY + gridOffsetY, 1);                                      // -1 1
            grid7.Material = grid1Material;
            grid7.AddToPhysicsEngine = true;
            grid7.Physics.Pickable = true;
            grid7.Physics.Collidable = true;
            grid7.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;




            // Create a geometry node with a model of a sphere that will be overlaid on


            // Create a marker node to track a ground marker array.
#if USE_ARTAG
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ground");

            // Since we expect that the ground marker array won't move very much, we use a 
            // small smoothing alpha.
            //groundMarkerNode.Smoother = new DESSmoother(0.2f, 0.1f, 1, 1);
#else
            //   groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");
            groundMarkerNode = new MarkerNode(this.scene.MarkerTracker, "ALVARConfig_14000.txt");

#endif




            // Now add the above nodes to the scene graph in the appropriate order.
            // Note that only the nodes added below the marker node are affected by 
            // the marker transformation.
            scene.RootNode.AddChild(groundMarkerNode);




            toolbarMarkerNode = new MarkerNode(scene.MarkerTracker, "Toolbar.txt");


            scene.RootNode.AddChild(toolbarMarkerNode);





            groundMarkerNode.AddChild(grid1TransNode);
            grid1TransNode.AddChild(grid1);


            groundMarkerNode.AddChild(grid2TransNode);
            grid2TransNode.AddChild(grid2);

            groundMarkerNode.AddChild(grid3TransNode);
            grid3TransNode.AddChild(grid3);
            /*
                        groundMarkerNode.AddChild(grid4TransNode);
                        grid4TransNode.AddChild(grid4);


                        groundMarkerNode.AddChild(grid5TransNode);
                        grid5TransNode.AddChild(grid5);


                        groundMarkerNode.AddChild(grid6TransNode);
                        grid6TransNode.AddChild(grid6);

                        groundMarkerNode.AddChild(grid7TransNode);
                        grid7TransNode.AddChild(grid7);
                */


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

            if (groundMarkerNode.MarkerFound)
            {
                Console.Write("Ground found");
            }
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

        internal bool isVisble()
        {
            return this.groundMarkerNode.MarkerFound;
        }
    }

}
