using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace ZombleMode
{
    public class ZSupply
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Completed { get; set; }
        public List<NetItem> Items { get; set; }
        public ZSupply(int x,int y,bool state)
        {
            X = x;
            Y = y;
            Completed = state;
            Items = new List<NetItem>(40);
        }
        public void Generate() 
        {
            Place();
            PlaceItem();
            //TSPlayer.All.SendTileRect((short)X, (short)Y);
            Update();
            Completed = true;
        }
        public void Place() 
        {

            WorldGen.AddBuriedChest(X,Y);
            
            //TSPlayer.All.SendTileRect((short)X, (short)Y, 4,4);
        }
        public void Kill() 
        {
            var id = Chest.FindChest(X,Y);
            if (id!=-1) Chest.DestroyChestDirect(X, Y, id);
            Update(); //TSPlayer.All.SendTileRect((short)X, (short)Y, 4);
            Completed = false;
        }
        public void PlaceItem() 
        {
            var id = Chest.FindChest(X, Y);
            if (id != -1)
            {
                var chest = Terraria.Main.chest[id];
                for (int i = 0; i < Items.Count; i++)
                {
                    chest.item[i].netDefaults(Items[i].NetId);
                    chest.item[i].stack = Items[i].Stack;
                    chest.item[i].prefix = Items[i].PrefixId;
                    TSPlayer.All.SendData(PacketTypes.ChestItem, "", id, i);
                }  
            }
        }
        public void Update() 
        {
            int x = Netplay.GetSectionX(X-3);
            int y = Netplay.GetSectionY(Y-3);
            int x2 = Netplay.GetSectionX(X+3);
            int y2 = Netplay.GetSectionY(Y+3);
            for (int k = 0; k < TShock.Players.Length; k++)
            {
                if (TShock.Players[k] == null) { continue; };
                for (int i = x; i <= y; i++)
                {
                    for (int j = x2; j <= y2; j++)
                    {
                        Netplay.Clients[k].TileSections[i, j] = false;
                    }
                }
            }
        }
    }
}
