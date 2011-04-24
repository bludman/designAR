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
        const int IDLE_STATE = 0, PLACING_STATE = 1, MANIPULATING_STATE = 2;

        public Wand()
        {
            // Add a mouse click callback function to perform ray picking when mouse is clicked
            MouseInput.Instance.MouseClickEvent += new HandleMouseClick(MouseClickHandler);
        }

        private void MouseClickHandler(int button, Point mouseLocation)
        {
            if (button == MouseInput.LeftButton)
            {

            }
        }
    }
}
