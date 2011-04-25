﻿using System;
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
        protected static int instance = 0;
        protected int instanceNumber;
        protected string name;
      
        public Item(IModel model,string label)
        {
            name = label;
            instanceNumber = instance;
            restrictedDimension = new Vector3(1);
            geo = new GeometryNode(label+instance);
            trans = new TransformNode(label+"Trans"+instance);
            instance++;
            trans.AddChild(geo);

            geo.Model = model;
            geo.Physics.Shape = GoblinXNA.Physics.ShapeType.TriangleMesh;
            geo.Physics.Pickable = true;
            geo.AddToPhysicsEngine = true;
            trans.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), 0, MathHelper.ToRadians(90));

        }

        public Item(IModel model, string label, Material material) : this(model,label)
        {
            geo.Material = material;
            ((Model)geo.Model).UseInternalMaterials = true;
        }


        public Item(Item other) : this(other.geo.Model, other.name)
        {
            this.Scale = new Vector3(other.Scale.X, other.Scale.Y, other.Scale.Z);
        }


        public virtual Vector3 Scale
        {
            get { return trans.Scale; }
            set { trans.Scale = value; }
        }
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
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
