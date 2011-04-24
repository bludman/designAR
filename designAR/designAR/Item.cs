using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Device.Generic;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.UI.UI2D;
using GoblinXNA.UI;
using GoblinXNA.Helpers;
using GoblinXNA.Sounds;

namespace designAR
{
    class Item
    {

        protected GeometryNode geo;
        protected TransformNode trans;
        protected bool selected;
        protected Vector3 restrictedDimension;
        protected int instance;

      

        public Item(BranchNode parentNode)
        {
            Build(parentNode);
        }

        public Item(IModel model)
        {
            instance = 0;
            restrictedDimension = new Vector3(1);
            geo = new GeometryNode();
            trans = new TransformNode();
            trans.AddChild(geo);

            geo.Model = model;
            trans.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), 0, MathHelper.ToRadians(90));

        }

        public Item(IModel model, Material material) : this(model)
        {
            geo.Material = material;
            ((Model)geo.Model).UseInternalMaterials = true;
        }


        public Item(Item other) : this(other.geo.Model)
        {
            this.Scale = new Vector3(other.Scale.X, other.Scale.Y, other.Scale.Z);
            this.instance = other.instance + 1;
        }

        private void Build(BranchNode parentNode)
        {
            restrictedDimension = new Vector3(1);
            geo = new GeometryNode("Box");
            geo.Model = new Box(10,50,5);
            
            // Add this box model to the physics engine for collision detection
            geo.AddToPhysicsEngine = true;
            geo.Physics.Shape = ShapeType.Box;
            // Make this box model cast and receive shadows
            geo.Model.CastShadows = true;
            geo.Model.ReceiveShadows = true;
            geo.Model.ShowBoundingBox = true;

            // Create a material to apply to the box model
            Material mat = new Material();
            mat.Diffuse = Color.Red.ToVector4();
            mat.Specular = Color.White.ToVector4();
            mat.SpecularPower = 10;

            geo.Material = mat;

            // Add this box model node to the ground marker node

            trans = new TransformNode(new Vector3(0, 10, 0));
            trans.AddChild(geo);
            BindTo(parentNode);//parentNode.AddChild(trans);
            createSelectionNodes();


        }

        private void createSelectionNodes()
        {
            SwitchNode selectionNode = new SwitchNode();
            GeometryNode selectionIndicator = new GeometryNode("Box");
            selectionIndicator.Model = new Box(20, 55, 0.5f);

        }


        public virtual Vector3 Scale
        {
            get { return trans.Scale; }
            set { trans.Scale = value; }
        }

         public virtual bool Selected
        {

            get { return selected; }
            set { selected = value; geo.Model.ShowBoundingBox = value; }
        }

        
        public virtual Model Model
        {
            get { return (Model)geo.Model; }
            set { geo.Model = value; }
        }

        public virtual Vector3 RestrictedDimension
        {
            get { return restrictedDimension; }
            set { restrictedDimension = value; }
        }

        public void MoveBy(Vector3 moveBy)
        {
            trans.Translation += moveBy*restrictedDimension;
        }

        public NewtonPhysics.CollisionPair getCollisionPair(IPhysicsObject otherObject)
        {
           return new NewtonPhysics.CollisionPair(otherObject, geo.Physics);
            //((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, callback);
        }

        public void BindTo(BranchNode parentNode)
        {
            if(trans!= null &&  trans.Parent!=null )
                ((BranchNode)trans.Parent).RemoveChild(trans);

            if(parentNode!=null)
                parentNode.AddChild(trans);
        }
        public void UnbindFrom(BranchNode parentNode)
        {
            if (trans != null && trans.Parent != null)
                ((BranchNode)trans.Parent).RemoveChild(trans);

            if (parentNode != null)
                parentNode.RemoveChild(trans);
        }

        public void MoveTo(Vector3 position)
        {
            trans.Translation = position*restrictedDimension;
        }

        


        internal Item clone()
        {
            throw new NotImplementedException();
        }
    }
}
