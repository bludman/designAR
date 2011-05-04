using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics.Geometry;
using Microsoft.Xna.Framework.Content;
using GoblinXNA.Graphics;


namespace designAR
{
    class HUD
    {
        SpriteFont textFont;
        GeometryNode cornerPanelNode;
        TransformNode cornerPanelTransformNode;
        Scene scene;
        ContentManager Content;


        public HUD(Scene scene, Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            this.scene = scene;
            this.Content = Content;

            textFont = Content.Load<SpriteFont>("DebugFont");


            G2DPanel statusBar = new G2DPanel();
            statusBar.Bounds = new Rectangle(-10, 570, 642, 170);
            statusBar.Border = GoblinEnums.BorderFactory.LineBorder;
            statusBar.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)
            scene.UIRenderer.Add2DComponent(statusBar);

            G2DPanel topBar = new G2DPanel();
            topBar.Bounds = new Rectangle(-10, -3, 850, 35);
            topBar.Border = GoblinEnums.BorderFactory.LineBorder;
            topBar.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)
            scene.UIRenderer.Add2DComponent(topBar);

            createCorderBackground();
        }

        private void createCorderBackground()
        {
            cornerPanelTransformNode = new TransformNode("Corner Panel Trans");
            scene.RootNode.AddChild(cornerPanelTransformNode);

            cornerPanelNode = new GeometryNode("Corner Panel");
            cornerPanelNode.Model = new TexturedLayer(new Vector2(250, 250));//new Box(300, 250, 1);
            cornerPanelTransformNode.AddChild(cornerPanelNode);


            cornerPanelTransformNode.Translation = new Vector3(2.3f, -1.58f, -5);
            cornerPanelTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90));
            //cornerPanelTransformNode.Translation = new Vector3(0, .06f, -1);

            cornerPanelTransformNode.Scale = new Vector3(0.005f, 0.005f, 0.005f);

            Material pointerLabelMaterial = new Material();
            pointerLabelMaterial.Diffuse = Color.Gray.ToVector4(); ;// new Vector4(0, 0.5f, 0, 1);
           // pointerLabelMaterial.Specular = Color.White.ToVector4();
           // pointerLabelMaterial.SpecularPower = 50;
            pointerLabelMaterial.Texture = Content.Load<Texture2D>("hud/cornerPanel");

            cornerPanelNode.Material = pointerLabelMaterial;

            //cornerPanelTransformNode.SetAlpha(0.55f);
            Vector4 tempColor = cornerPanelNode.Material.Diffuse;
            Vector3 tempVec = new Vector3(tempColor.X, tempColor.Y, tempColor.Z);
            cornerPanelNode.Material.Diffuse = new Vector4(tempVec, 0.7f);
        }



        internal void Update(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }
    }
}
