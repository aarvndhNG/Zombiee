using Microsoft.Xna.Framework;
using MiniGamesAPI;
using MiniGamesAPI.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TShockAPI;

namespace ZombleMode
{
    public class ZPlayer:MiniPlayer
    {
        [JsonIgnore]
        public ZEnum Character { get; set; }
        [JsonIgnore]
        public bool IsDead { get; set; }
        [JsonIgnore]
        public int CD { get; set; }
        [JsonIgnore]
        public int BulletAmount { get; set; }
        [JsonIgnore]
        public bool BeLoaded { get; set; }
        [JsonIgnore]
        public bool isHunter;
        [JsonIgnore]
        public bool crystalGiven;
        public List<string> KillNames { get; set; }
        [JsonIgnore]
        public Timer BulletTimer = new Timer(1000);
        public ZPlayer(int id,TSPlayer plr):base(id, plr)
        {
            Player = plr;
            BackUp = null;
            Character = ZEnum.Human;
            KillNames = new List<string>();
            CD = 5;
            BulletAmount = 30;
            BeLoaded = false;
            BulletTimer.Elapsed += OnTick;
            isHunter = false;
            crystalGiven = false;
        }
        public ZPlayer():base()
        {
            CD = 5;
            BulletAmount = 30;
            BeLoaded = false;
            BulletTimer.Elapsed += OnTick;
            IsDead = false;
            Character = ZEnum.Human;
            KillNames = new List<string>();
            isHunter = false;
            crystalGiven = false;
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            if (IsDead)
            {
                SetBuff(10,3600);
            }
            if (BeLoaded)
            {
                if (CD!=0)
                {
                    CD -= 1;
                }
                else
                {
                    BeLoaded = false;
                    CD = 5;
                    BulletAmount = 30;
                }
            }
            
        }

        public void Join(ZRoom room)
        {
            if (room.Status!=MiniGamesAPI.Enum.RoomStatus.Waiting)
            {
                SendInfoMessage("The current room status cannot join the game");
                return;
            }
            if (room.Players.Count>=room.MaxPlayer)
            {
                SendInfoMessage("the room is full");
                return;
            }
            if (CurrentRoomID!=0)
            {
                var originRoom = ConfigUtils.GetRoomByID(CurrentRoomID);
                if (originRoom != null) Leave();
            }
            if (!room.Players.Contains(this))
            {
                room.Broadcast($"Player [{Name}] has joined the room", Color.Orange);
                room.Players.Add(this);
                CurrentRoomID = room.ID;
                SelectPackID = room.HumanPackID;
                BackUp = new MiniPack(Name,ID);
                BackUp.CopyFromPlayer(Player);
                BulletTimer.Start();
                Teleport(room.LobbyPoint) ;
                SendSuccessMessage($"You have joined room [{room.ID}][{room.Name}]");
            }
            else 
            {
                SendInfoMessage("you are already in this room");
            }
        }
        public new void  Leave()
        {
            var room = ConfigUtils.GetRoomByID(CurrentRoomID);
            if (room==null)
            {
                SendInfoMessage("Room does not exist or you are not in any room");
                return;
            }
            if (room.Status!=MiniGamesAPI.Enum.RoomStatus.Waiting)
            {
                SendInfoMessage("The current room status does not allow leaving");
                return;
            }
            BulletTimer.Stop();
            room.Players.Remove(this);
            room.Broadcast($"Player {Name} has left the room", Color.Crimson);
            CurrentRoomID = 0;
            SelectPackID = 0;
            IsReady = false;
            Status = MiniGamesAPI.Enum.PlayerStatus.Waiting;
            Character = ZEnum.Human;
            if (BackUp != null) BackUp.RestoreCharacter(Player);
            Player.SaveServerCharacter();
            SendSuccessMessage($"You left room [{room.ID}][{room.Name}]");
            SendBoardMsg("");
            Teleport(new Point(Terraria.Main.spawnTileX,Terraria.Main.spawnTileY));
        }
        public void SendCombatMessage(string text,Color color)
        {
            TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended,text,(int)color.packedValue,Player.TPlayer.position.X, Player.TPlayer.position.Y);
        
        }
    }
}
