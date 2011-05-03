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
    class ItemLibrary
    {
        protected List<Item> items;
        protected Dictionary<string, Item> itemMap;

        public ItemLibrary(String filename)
        {
            int counter = 0;
            string line;
            items = new List<Item>();
            itemMap = new Dictionary<string, Item>();
            ModelLoader loader = new ModelLoader();

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader(filename);


          
            
            while ((line = file.ReadLine()) != null)
            {
                //Allow comments in config file
                if (line.StartsWith("#"))
                    continue;

                string[] tokens=line.Split(',');
                Console.WriteLine("Loading: "+tokens[Item.NAME]);

                /*
                Model m = (Model)loader.Load("", tokens[Item.NAME]);

                Material defaultMaterial = new Material();
                defaultMaterial.Diffuse = new Vector4(0, 0.5f, 0, 1);
                defaultMaterial.Specular = Color.White.ToVector4();
                defaultMaterial.SpecularPower = 10;

                Item item = new Item(m, tokens[Item.NAME], defaultMaterial);
                */

                Item item = new Item(tokens);
                //item.Scale = new Vector3(float.Parse(tokens[Item.SCALE]));
                items.Add(item);
                itemMap.Add(tokens[Item.NAME], item);


                counter++;
            }

            file.Close();
        }

        public Item getItem(int id)
        {
            return new Item(items[id]);
        }

        public Item getItem(string label)
        {
            return new Item(itemMap[label]);
        }

        /// <summary>
        /// Returns a copy of all the items in this library
        /// </summary>
        /// <returns></returns>
        public List<Item> getAllItems()
        {
            List<Item> copy = new List<Item>();
            foreach (Item i in items)
            {
                copy.Add(new Item(i));
            }

            return copy;
        }


    }
}
