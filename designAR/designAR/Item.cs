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
        protected float rotationAngle = 0;
        protected static int instance = 0;
       

        protected int instanceNumber;
        protected string name;
        protected string[] savedTokens;
      
        protected static Item selectedItem;
        protected static ModelLoader loader = new ModelLoader();


        public static int NAME = 0;
        public static int SCALE = 1;



        public Item(IModel model,string name)
        {
            build(model, name);
        }

        private void build(IModel model, string name)
        {
            this.name = name;
            this.instanceNumber = instance;

            restrictedDimension = new Vector3(1,1,0);
            geo = new GeometryNode(this.Label);
            trans = new TransformNode(this.Label + "_Trans");

            instance++;
            trans.AddChild(geo);

            geo.Model = model;
            geo.Physics.Shape = GoblinXNA.Physics.ShapeType.ConvexHull;
            geo.Physics.Pickable = true;
            geo.AddToPhysicsEngine = true;
            trans.Rotation = Quaternion.CreateFromYawPitchRoll((float)Math.PI / 2, 0, (float)Math.PI / 2);
        }
        public void setGroupID(int id)
        {
            this.geo.GroupID = id;
        }


        public Item(IModel model, string name, Material material) 
        {
            build(model, name, material);
        }

        private void build(IModel model, string name, Material material)
        {
            build(model, name);
            //geo.Material = material;
            ((Model)geo.Model).UseInternalMaterials = true;
        }


        
        /*
        public Item(Item other) : this(other.geo.Model, other.name)
        {
            //this.geo.Model = new Model(null,other.geo.Model.
            //this.geo.Model.CopyGeometry(other.geo.Model);
            this.Scale = new Vector3(other.Scale.X, other.Scale.Y, other.Scale.Z);
        }
         * */

        public Item(Item other) : this(other.savedTokens)
        {}

        public Item(string[] tokens)
        {
            Model m = (Model)loader.Load("", tokens[NAME]);

            Material defaultMaterial = new Material();
            defaultMaterial.Diffuse = Color.White.ToVector4(); //new Vector4(0, 0.5f, 0, 1);
            defaultMaterial.Specular = Color.White.ToVector4();
            defaultMaterial.SpecularPower = 10;

            build(m, tokens[NAME], defaultMaterial);
            this.Scale = new Vector3(float.Parse(tokens[Item.SCALE]));
            this.savedTokens = tokens;

        }

        public void SetAlpha(float a)
        {
            Vector4 tempColor=geo.Material.Diffuse;
            Vector3 tempVec = new Vector3(tempColor.X, tempColor.Y, tempColor.Z);
            geo.Material.Diffuse = new Vector4(tempVec, a);
        }


        public virtual Vector3 Scale
        {
            get { return trans.Scale; }
            set { trans.Scale = value; }
        }


        public virtual Vector3 Translation
        {
            get { return trans.Translation; }
            set { trans.Translation = value; }
        }


        public virtual Quaternion Rotation
        {
            get { return trans.Rotation; }
            set { trans.Rotation = value; }
        }


        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual int InstanceNumber
        {
            get { return instanceNumber; }
            //set { instanceNumber = value; }
        }

        public virtual string Label
        {
            get { return name+"_"+instanceNumber; }
            //set { name = value; }
        }


         public virtual bool Selected
        {

            get { return selected; }
            set { 
                
                if (value)
                {
                    if (selectedItem != null)
                        selectedItem.Selected = false;

                    selectedItem = this;
                }
                else
                {
                    selectedItem = null;
                }

                selected = value;
                geo.Model.ShowBoundingBox = value;
                
            }
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

        public void BindTo(IBindable b)
        {

            BindTo(b.getBindNode());
        }

        public void BindTo(BranchNode parentNode)
        {
            if (trans != null && ((BranchNode)trans.Parent) != null)
            {
                ((BranchNode)trans.Parent).RemoveChild(trans);

                Console.WriteLine("Contained?" + ((BranchNode)trans.Parent).Children.Contains(trans));
            }

            if(parentNode!=null)
                parentNode.AddChild(trans);
        }
        public void Unbind()
        {
            if (trans != null && trans.Parent != null)
                ((BranchNode)trans.Parent).RemoveChild(trans);
        }

        public void MoveTo(Vector3 position)
        {
            trans.Translation = position*restrictedDimension;

            
           /* Matrix translate= Matrix.CreateTranslation(position*restrictedDimension);
            Matrix scale= Matrix.CreateScale(new Vector3(2));
            Matrix rotate= Matrix.CreateFromQuaternion(trans.Rotation);


            trans.WorldTransformation = scale * rotate * translate;
           */



        }

        public void RotateBy(float degrees)
        {
            Vector3 rotationAxis = GetRotationAxis();
            rotationAngle = degrees;
            if (rotationAngle > 360)
                rotationAngle -= 360;
            if (rotationAngle < 360)
                rotationAngle += 360;
            trans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) * Quaternion.CreateFromAxisAngle(rotationAxis, MathHelper.ToRadians(rotationAngle));
            //trans.Scale = new Vector3(f);
        }

        public void RotateCoarse(int times)
        {
            rotationAngle += times * 15;
            rotationAngle /= 15;
            rotationAngle = (float)Math.Round(rotationAngle, 0, MidpointRounding.AwayFromZero);
            rotationAngle *= 15;
            RotateBy(rotationAngle);
        }

        public void RotateFine(int times)
        {
            rotationAngle += times;
            RotateBy(rotationAngle);
        }

        public Vector3  GetRotationAxis()
        {
            if (restrictedDimension.X == 0)
            {
                return Vector3.UnitX;
            }
            else if (restrictedDimension.Z == 0)
            {
                return Vector3.UnitY;
            }
            else
            {
                return Vector3.UnitZ;
            }

        }

    }
}
