using MiniGamesAPI.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace ZombleMode
{
    public static class ConfigUtils
    {
        public static readonly string configDir = TShock.SavePath + "/ZombleMode";
        public static readonly string playersPath = configDir + "/players.json";
        public static readonly string roomsPath = configDir + "/rooms.json";
        public static readonly string packsPath = configDir + "/packs.json";
        public static List<ZPlayer> players = new List<ZPlayer>();
        public static List<ZRoom> rooms = new List<ZRoom>();
        public static List<MiniPack> packs = new List<MiniPack>();
        public static void LoadConfig()
        {
            if (Directory.Exists(configDir))
            {
                if (File.Exists(playersPath))
                {
                    players = JsonConvert.DeserializeObject<List<ZPlayer>>(File.ReadAllText(playersPath));
                }
                else
                {
                    File.WriteAllText(playersPath, JsonConvert.SerializeObject(players, Formatting.Indented));
                }
                if (File.Exists(roomsPath))
                {
                    rooms = JsonConvert.DeserializeObject<List<ZRoom>>(File.ReadAllText(roomsPath));
                }
                else
                {
                    File.WriteAllText(roomsPath, JsonConvert.SerializeObject(rooms, Formatting.Indented));
                }
                if (File.Exists(packsPath))
                {
                    packs = JsonConvert.DeserializeObject<List<MiniPack>>(File.ReadAllText(packsPath));
                }
                else
                {
                    File.WriteAllText(packsPath, JsonConvert.SerializeObject(packs, Formatting.Indented));
                }
            }
            else
            {
                Directory.CreateDirectory(configDir);
                File.WriteAllText(playersPath, JsonConvert.SerializeObject(players, Formatting.Indented));
                File.WriteAllText(roomsPath, JsonConvert.SerializeObject(rooms, Formatting.Indented));
                File.WriteAllText(packsPath, JsonConvert.SerializeObject(packs, Formatting.Indented));
            }

        }
        public static ZPlayer GetPlayerByName(string name)
        {
            return players.Find(p => p.Name == name);
        }
        public static ZRoom GetRoomByID(int id)
        {
            return rooms.Find(p => p.ID == id);
        }
        public static ZRoom GetRoomFromLocal(int id)
        {
            var tempRooms = JsonConvert.DeserializeObject<List<ZRoom>>(File.ReadAllText(roomsPath));
            return tempRooms.Find(p => p.ID == id);
        }
        public static MiniPack GetPackByID(int id)
        {
            return packs.Find(p => p.ID == id);
        }
        public static void UpdateRooms()
        {
            File.WriteAllText(roomsPath, JsonConvert.SerializeObject(rooms, Formatting.Indented));
        }
        public static void UpdatePlayers()
        {
            File.WriteAllText(playersPath, JsonConvert.SerializeObject(players, Formatting.Indented));
        }
        public static void UpdatePacks()
        {
            File.WriteAllText(packsPath, JsonConvert.SerializeObject(packs, Formatting.Indented));
        }
        public static void UpdateSingleRoom(ZRoom room) 
        {
            var tempRooms = JsonConvert.DeserializeObject<List<ZRoom>>(File.ReadAllText(roomsPath));
            var tempRoom = tempRooms.Find(r=>r.ID==room.ID);
            tempRoom = room;
            File.WriteAllText(roomsPath, JsonConvert.SerializeObject(rooms, Formatting.Indented));
        }
        public static void AddSingleRoom(ZRoom room) 
        {
            var tempRooms= JsonConvert.DeserializeObject<List<ZRoom>>(File.ReadAllText(roomsPath));
            tempRooms.Add(room);
            File.WriteAllText(roomsPath, JsonConvert.SerializeObject(rooms, Formatting.Indented));
        }
        public static void RemoveSingleRoom(ZRoom room)
        {
            var tempRooms = JsonConvert.DeserializeObject<List<ZRoom>>(File.ReadAllText(roomsPath));
            tempRooms.Remove(room);
            File.WriteAllText(roomsPath, JsonConvert.SerializeObject(rooms, Formatting.Indented));
        }
        public static void ReloadPacks()
        {
            packs = JsonConvert.DeserializeObject<List<MiniPack>>(File.ReadAllText(packsPath));
        }
    }
}
