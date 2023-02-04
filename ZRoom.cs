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
    public class ZRoom : MiniRoom, IRoom
    {
        [JsonIgnore]
        public bool hunterAppeared;
        [JsonIgnore]
        public int leftToHunter; 
        public int RespawnTime { get; set; }
        public int RootZombleAmount { get; set; }
        public int HumanPackID { get; set; }
        public int NormalPackID { get; set; }
        public int ViewerPackID { get; set; }
        public int RootPackID { get; set; }
        public int HunterPackID { get; set; }
        public bool HumanWin { get; set; }
        public Point LobbyPoint { get; set; }
        public List<Point> SpawnPoints { get; set; }
        [JsonIgnore]
        public List<ZPlayer> Players { get; set; }
        [JsonIgnore]
        public Timer waitingTimer = new Timer(1000);
        [JsonIgnore]
        public Timer gamingTimer = new Timer(1000);
        [JsonIgnore]
        public Timer selectTimer = new Timer(1000);
        public ZRoom(int id,string name) {
            ID = id;
            Name = name;
            Status = MiniGamesAPI.Enum.RoomStatus.Waiting;
            SpawnPoints = new List<Point>();
            Players = new List<ZPlayer>();
            leftToHunter = 0;
            hunterAppeared = false;
            Initialize();
            Start();
        }
        public ZRoom() 
        {
            leftToHunter = 0;
            hunterAppeared = false;
            SpawnPoints = new List<Point>();
            Players = new List<ZPlayer>();
            Initialize();
            Start();
        }
        public void Broadcast(string msg, Color color)
        {
            for (int i = Players.Count-1; i>=0; i--)
            {
                var plr = Players[i];
                plr.SendMessage(msg,color);
            }
        }
        public void Broadcast(string msg, Color color,string name)
        {
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                var plr = Players[i];
                if (plr.Name==name) continue;
                plr.SendMessage(msg, color);
            }
        }
        public void Conclude()
        {
            ShowVictory();
            for (int i = Players.Count-1; i>=0; i--)
            {
                var plr = Players[i];
                plr.IsReady = false;
                plr.IsDead = false;
                if (!plr.Player.Dead) plr.Teleport(LobbyPoint);
                plr.SetTeam(0);
                plr.SetPVP(false);
                plr.CD = 5;
                plr.BulletAmount = 30;
                plr.BeLoaded = false;
                plr.BackUp.RestoreCharacter(plr);
                plr.Player.SaveServerCharacter();
                plr.Character = ZEnum.Human;
                plr.Status = MiniGamesAPI.Enum.PlayerStatus.Waiting;
                plr.isHunter = false;
                plr.crystalGiven = false;
            }
            Status = MiniGamesAPI.Enum.RoomStatus.Restoring;
        }

        public void Dispose()
        {
            for (int i = Players.Count-1; i>=0; i--)
            {
                var plr = Players[i];
                plr.Teleport(Terraria.Main.spawnTileX,Terraria.Main.spawnTileY);
                plr.ClearRecord();
                plr.SendInfoMessage("The room is forced to suspend");
                plr.SetPVP(false);
                plr.SetTeam(0);
                plr.Character = ZEnum.Human;
                plr.Status = MiniGamesAPI.Enum.PlayerStatus.Waiting;
                plr.CurrentRoomID = 0;
                plr.SelectPackID = 0;
                plr.BackUp.RestoreCharacter(plr);
                plr.Player.SaveServerCharacter();
                plr.IsDead = false;
                plr.IsReady = false;
                Players.Clear();
            }
        }

        public int GetPlayerCount()
        {
            return Players.Count;
        }

        public void Initialize()
        {
            waitingTimer.Elapsed += OnWaiting;
            gamingTimer.Elapsed += OnGaming;
            selectTimer.Elapsed += OnSelecting;
            
        }

        private void OnSelecting(object sender, ElapsedEventArgs e)
        {
            ShowRoomMemberInfo();
            if (Status != MiniGamesAPI.Enum.RoomStatus.Gaming) return;
            if (SeletingTime==0)
            {
                selectTimer.Stop();
                hunterAppeared = false;
                leftToHunter = RootZombleAmount;
                SelectZomble();
                for (int i = Players.Count-1; i>=0; i--)
                {
                    var plr = Players[i];
                    if (plr.IsDead)
                    {
                        plr.IsDead = false;
                        plr.Character = ZEnum.Zomble;
                        plr.SelectPackID = NormalPackID;
                        var rand = new Random();
                        var pack = ConfigUtils.GetPackByID(plr.SelectPackID);
                        pack.RestoreCharacter(plr);
                        plr.Teleport(SpawnPoints[rand.Next(0,SpawnPoints.Count - 1)]);
                        plr.SendInfoMessage("reborn！");
                    }
                    plr.SetPVP(true);
                    plr.Godmode(false);
                    plr.SendInfoMessage($"The number of ghost hunters in this round is: {leftToHunter}");
                    if (plr.Character==ZEnum.Zomble)
                    {
                        plr.SetTeam(1);//set up red team
                    }
                    else { plr.SetTeam(3); }//set blue team
                }
                
                Broadcast("mother appeared！",Color.Crimson);
                gamingTimer.Start();
            }
            else
            {
                Broadcast($"The matrix will appear in {SeletingTime} seconds, please pay attention to the people around you..",Color.DarkTurquoise);
                SeletingTime--;
            }
        }

        private void OnGaming(object sender, ElapsedEventArgs e)
        {
            ShowRoomMemberInfo();
            if (Status != MiniGamesAPI.Enum.RoomStatus.Gaming) return;
            var zombles = Players.Where(p => p.Character == ZEnum.Zomble).ToList();
            var humen = Players.Where(p => p.Character == ZEnum.Human).ToList();
            if (GamingTime==0)
            {
                gamingTimer.Stop();
                if (humen.Count!=0) HumanWin = true;
                Status = MiniGamesAPI.Enum.RoomStatus.Concluding;
                Conclude();
                Restore();
            }
            else 
            {
                if (Players.Count==0)
                {
                    gamingTimer.Stop();
                    Status = MiniGamesAPI.Enum.RoomStatus.Concluding;
                    Conclude();
                    Restore();
                }
                if (humen.Count==0)
                {
                    gamingTimer.Stop();
                    Status = MiniGamesAPI.Enum.RoomStatus.Concluding;
                    Conclude();
                    Restore();
                }
                if (Players.Where(p=>p.Character==ZEnum.Zomble&&p.IsDead).Count()==zombles.Count)
                {
                    HumanWin = true;
                    gamingTimer.Stop();
                    Status = MiniGamesAPI.Enum.RoomStatus.Concluding;
                    Conclude();
                    Restore();
                }
                if (humen.Count==leftToHunter&&!hunterAppeared)
                {
                    for (int i = 0; i < humen.Count; i++)
                    {
                        var plr = humen[i];
                        if (!plr.crystalGiven)
                        {
                            plr.Player.GiveItem(29, 1);
                            plr.crystalGiven = true ;
                        }
                        plr.SendInfoMessage("You can transform into a ghost hunter, use the given life crystal to transform");
                    }
                    hunterAppeared = true;
                }
                if (GamingTime==60) Broadcast("1 minute left in the game..",Color.DarkTurquoise);
                GamingTime--;
            }
        }

        private void OnWaiting(object sender, ElapsedEventArgs e)
        {
            if (Players.Count<=0) return;
            ShowRoomMemberInfo();
            if (Status == MiniGamesAPI.Enum.RoomStatus.Waiting && Players.Where(p => p.IsReady).ToList().Count < MinPlayer) return;
            if (WaitingTime==0)
            {
                waitingTimer.Stop();
                for (int i = Players.Count-1; i>=0; i--)
                {
                    var plr = Players[i];
                    plr.SelectPackID = HumanPackID;
                    plr.IsReady = true;
                    plr.Status = MiniGamesAPI.Enum.PlayerStatus.Gaming;
                    plr.Godmode(true);
                    var pack = ConfigUtils.GetPackByID(plr.SelectPackID);
                    if (pack != null) pack.RestoreCharacter(plr);
                }
                TeleportRandomly();
                selectTimer.Start();
                Status = MiniGamesAPI.Enum.RoomStatus.Gaming;
            }
            else
            {
                Broadcast($"{WaitingTime} seconds until the game starts....",Color.MediumAquamarine);
                WaitingTime -= 1;
            }
        }

        public void Restore()
        {
            var room = ConfigUtils.GetRoomFromLocal(ID);
            WaitingTime = room.WaitingTime;
            GamingTime = room.GamingTime;
            LobbyPoint = room.LobbyPoint;
            SpawnPoints = room.SpawnPoints;
            RespawnTime = room.RespawnTime;
            MaxPlayer = room.MaxPlayer;
            MinPlayer = room.MinPlayer;
            RootPackID = room.RootPackID;
            RootZombleAmount = room.RootZombleAmount;
            HumanPackID = room.HumanPackID;
            NormalPackID = room.NormalPackID;
            SeletingTime = room.SeletingTime;
            HumanWin = false;
            hunterAppeared = false;
            leftToHunter = 0;
            Status = MiniGamesAPI.Enum.RoomStatus.Waiting;
            Start();
            TShock.Utils.Broadcast($"Biochemical mode room [{ID}][{Name}] has been reset, you can join the game！",Color.DarkTurquoise);
        }

        public void ShowRoomMemberInfo()
        {
            StringBuilder roomInfo = new StringBuilder();
            roomInfo.AppendLine(MiniGamesAPI.Utils.EndLine_10);
            if (Status==MiniGamesAPI.Enum.RoomStatus.Gaming)
            {
                var minutes = GamingTime / 60;
                var seconds = GamingTime % 60;
                roomInfo.AppendLine("———Room information———");
                roomInfo.AppendLine($"Game remaining time [{minutes}:{seconds}]");
                for (int i = Players.Count - 1; i >= 0; i--)
                {
                    var plr = Players[i];
                    roomInfo.AppendLine($"[{plr.Name}] [{(plr.Character==ZEnum.Zomble?"Infected":(plr.isHunter?"Ghost Hunter":"Terra"))}]");
                }

            }
            if (Status==MiniGamesAPI.Enum.RoomStatus.Waiting)
            {
                var minutes = WaitingTime / 60;
                var seconds = WaitingTime % 60;
                roomInfo.AppendLine("———Room information———");
                roomInfo.AppendLine($"wait for countdown[{minutes}:{seconds}]");
                roomInfo.AppendLine($"Number of people in the room[{Players.Count}/{MaxPlayer}]");
                for (int i = Players.Count - 1; i >= 0; i--)
                {
                    var plr = Players[i];
                    roomInfo.AppendLine($"[{plr.Name}] [{(plr.IsReady ? "已准备" : "未准备")}]");
                }
                roomInfo.AppendLine("Type /zm ready to prepare");
                roomInfo.AppendLine("Type /zm leave to leave the room");
            }
            roomInfo.AppendLine(MiniGamesAPI.Utils.EndLine_15);
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                var plr = Players[i];
                plr.SendBoardMsg(roomInfo.ToString());
            }
        }

        public void ShowVictory()
        {
            StringBuilder victoryInfo = new StringBuilder();
            if (HumanWin)
            {
                victoryInfo.AppendLine("Congratulations to the Terrans for their victory！");
            }
            else
            {
                victoryInfo.AppendLine("Mutant mothers triumph");
            }
            Broadcast(victoryInfo.ToString(),Color.MediumAquamarine);
        }

        public void Start()
        {
            waitingTimer.Start();
        }

        public void Stop()
        {
            gamingTimer.Stop();
            waitingTimer.Stop();
            selectTimer.Stop();
        }
        public void SelectZomble() 
        {
            var zombles = Players.Where(p=>p.Character==ZEnum.Zomble).ToList();
            var rand = new Terraria.Utilities.UnifiedRandom();
            var seed = rand.Next(0,Players.Where(p=>p.Character==ZEnum.Human).Count()-1);
            var plr = Players[seed];
            if (plr.Character == ZEnum.Zomble) { 
                SelectZomble();
                return;
            }
            plr.Character = ZEnum.Zomble;
            plr.SelectPackID = RootPackID;
            var pack = ConfigUtils.GetPackByID(plr.SelectPackID);
            pack.RestoreCharacter(plr);
            plr.SendErrorMessage("You have been chosen to be the Mother!");
            var secondZombles = Players.Where(p => p.Character == ZEnum.Zomble).ToList();
            if (secondZombles.Count<RootZombleAmount) SelectZomble();
        }
        public void TeleportRandomly() 
        {
            for (int i = Players.Count-1; i>=0; i--)
            {
                var plr = Players[i];
                var rand = new Terraria.Utilities.UnifiedRandom();
                var seed = rand.Next(0,SpawnPoints.Count-1);
                plr.Teleport(SpawnPoints[seed]);
                plr.SendInfoMessage("You have been teleported to a random spawn point");
            }
        }
    }
}
