using Microsoft.Xna.Framework;
using MiniGamesAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ZombleMode
{
    [ApiVersion(2, 1)]
    public class MainPlugin : TerrariaPlugin
    {
        public MainPlugin(Main game) : base(game){}
        public override string Name => "ZombleMode";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Author => "bean paste and aarvndh";
        public override string Description => "biochemical pattern";
        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this,OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this,OnLeave);
            ServerApi.Hooks.ServerChat.Register(this,OnChat);
            ServerApi.Hooks.GamePostInitialize.Register(this,OnPostInitialize);
            GetDataHandlers.KillMe += OnKillMe;
            GetDataHandlers.ChestOpen += OnOpenChest;
            GetDataHandlers.PlayerSpawn += OnPlayerSpawn;
            GetDataHandlers.TogglePvp += OnChangePVP;
            GetDataHandlers.PlayerTeam += OnChangeTeam;
            GetDataHandlers.NewProjectile += OnNewProjectile;
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            ConfigUtils.LoadConfig();
        }
        private void OnPostInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("zm.user", ZM,"zm","biochemical pattern"));
            Commands.ChatCommands.Add(new Command("zm.admin", ZMA, "zma", "biochemical management"));
        }
        private void ZMA(CommandArgs args)
        {
            var plr = args.Player;
            ZRoom room = null;
            int id,count;
            StringBuilder board = new StringBuilder();
            if (args.Parameters.Count < 1)
            {
                args.Player.SendInfoMessage("please enter/zma help view help");
                return;
            }
            switch (args.Parameters[0])
            {
                case "list":
                    foreach (var tempRoom in ConfigUtils.rooms)
                    {
                        board.AppendLine($"[{tempRoom.ID}] [{tempRoom.Name}] [{tempRoom.GetPlayerCount()}/{tempRoom.MaxPlayer}] [{tempRoom.Status.ToString()}]");
                    }
                    plr.SendInfoMessage(board.ToString());
                    break;
                case "create":
                    if (args.Parameters.Count != 2)
                    {
                        plr.SendInfoMessage("correct instruction：/zma create [room name]");
                        return;
                    }
                    room = new ZRoom(ConfigUtils.rooms.Count + 1,args.Parameters[1]);
                    ConfigUtils.rooms.Add(room);
                    ConfigUtils.AddSingleRoom(room);
                    plr.SendInfoMessage("Room created successfully [{0}][{1}]",room.ID,room.Name);
                    break;
                case "newpack":
                    if (args.Parameters.Count!=3)
                    {
                        plr.SendInfoMessage("command error");
                        return;
                    }
                    string name, plrName;
                    name = args.Parameters[1];
                    plrName = args.Parameters[2];
                    if (TSPlayer.FindByNameOrID(plrName).Count!=0)
                    {
                        var target = TSPlayer.FindByNameOrID(plrName)[0];
                        var pack = new MiniPack(name, ConfigUtils.packs.Count + 1);
                        pack.CopyFromPlayer(target);
                        ConfigUtils.packs.Add(pack);
                        ConfigUtils.UpdatePacks();
                        plr.SendInfoMessage($"successfully created with player{target.Name}Backpack based backpack");
                    }
                    break;
                case "smp"://Set the maximum number of players
                    if (args.Parameters.Count!=3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1],out id)&&int.TryParse(args.Parameters[2],out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room!=null)
                        {
                            room.MaxPlayer = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The maximum number of players for{room.MaxPlayer}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "sdp"://Set minimum number of players
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.MinPlayer = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The minimum number of players for{room.MinPlayer}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "hp"://Setting Up the Ghost Hunter Backpack
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.HunterPackID = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The Ghost Hunter Backpack is{room.HunterPackID}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "sgt":
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.GamingTime = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})was played for{room.GamingTime}秒");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "sst":
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.SeletingTime = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The selection matrix duration of{room.SeletingTime}Second");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please enter the number");
                    }
                    break;
                case "swt":
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.WaitingTime = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The waiting time for{room.WaitingTime}Second");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "src"://Set the number of mothers
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.RootZombleAmount = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The number of mothers is{room.RootZombleAmount}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "snp"://Normal Zombie Backpack
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.NormalPackID = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The normal zombie backpack ID is{room.NormalPackID}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please enter the number");
                    }
                    break;
                case "srp"://Mother Zombie Backpack
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.RootPackID = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The parent backpack ID of is{room.RootPackID}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "shp"://human backpack
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.HumanPackID = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The human backpack ID of{room.HumanPackID}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "svp"://Spectator Backpack
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.ViewerPackID= count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The observer's backpack ID is{room.ViewerPackID}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "sp":
                    if (!plr.RealPlayer)
                    {
                        plr.SendInfoMessage("Please use this command in game");
                        return;
                    }
                    plr.AwaitingTempPoint = 1;
                    plr.SendInfoMessage("please select a point");
                    break;
                case "swp":
                    if (!plr.RealPlayer)
                    {
                        plr.SendInfoMessage("Please use this command in game");
                        return;
                    }
                    if (args.Parameters.Count != 2)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) )
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.LobbyPoint = plr.TempPoints[0];
                            plr.TempPoints[0] = Point.Zero;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})waiting hall");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "addp":
                    if (!plr.RealPlayer)
                    {
                        plr.SendInfoMessage("Please use this command in game");
                        return;
                    }
                    if (args.Parameters.Count != 2)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.SpawnPoints.Add(plr.TempPoints[0]);
                            plr.TempPoints[0] = Point.Zero;
                            plr.SendInfoMessage($"successfully added room(ID：{room.ID})a birth point of");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "respawntime":
                    if (args.Parameters.Count != 3)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id) && int.TryParse(args.Parameters[2], out count))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.RespawnTime = count;
                            plr.SendInfoMessage($"Successfully set up the room(ID：{room.ID})The respawn seconds for{room.RespawnTime}");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please enter the number");
                    }
                    break;
                case "remove":
                    if (args.Parameters.Count != 2)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.Stop();
                            room.Conclude();
                            room.Status = MiniGamesAPI.Enum.RoomStatus.Waiting;
                            for (int i = room.Players.Count; i>=0; i--)
                            {
                                var rplr = room.Players[i];
                                rplr.Leave();
                                rplr.SendInfoMessage("The room was forcibly deleted");
                            }
                            ConfigUtils.rooms.Remove(room);
                            ConfigUtils.RemoveSingleRoom(room);
                            plr.SendInfoMessage($"Successfully deleted room(ID：{room.ID})");
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please enter the number");
                    }
                    break;
                case "start":
                    if (args.Parameters.Count != 2)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.Start();
                            room.Status = MiniGamesAPI.Enum.RoomStatus.Waiting;
                            plr.SendInfoMessage($"Successfully opened the room(ID：{room.ID})");
                            ConfigUtils.UpdateSingleRoom(room);
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "stop":
                    if (args.Parameters.Count != 2)
                    {
                        plr.SendInfoMessage("Incorrect command");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out id))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room != null)
                        {
                            room.Stop();
                            room.Conclude();
                            room.Restore();
                            room.Status = MiniGamesAPI.Enum.RoomStatus.Waiting;
                            for (int i = room.Players.Count-1; i >= 0; i--)
                            {
                                var rplr = room.Players[i];
                                rplr.Leave();
                                rplr.SendInfoMessage("The room is forced to stop");
                            }
                            room.Status = MiniGamesAPI.Enum.RoomStatus.Stopped;
                            ConfigUtils.UpdateSingleRoom(room);
                            plr.SendInfoMessage($"Successfully stopped the room(ID：{room.ID})");
                        }
                        else
                        {
                            plr.SendInfoMessage("room does not exist");
                        }
                    }
                    else
                    {
                        plr.SendInfoMessage("Please key in numbers");
                    }
                    break;
                case "help":
                default:
                    int pageNumber;
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
                        return;
                    List<string> lines = new List<string>();
                    lines.Add("/zma create room name create room");
                    lines.Add("/zma newpack [backpack name] [player name] create backpack");
                    lines.Add("/zma remove [room ID] remove room");
                    lines.Add("/zma start [room ID] open room");
                    lines.Add("/zma stop [room ID] close the room");
                    lines.Add("/zma sgt [room ID] [time] Set the game time (unit: second)");
                    lines.Add("/zma swt [room ID] [time] Set the waiting time (unit: second)");
                    lines.Add("/zma sst [room ID] [time] Set selection time (unit: second)");
                    lines.Add("/zma srp [room ID] [Backpack ID] set parent backpack");
                    lines.Add("/zma snp [room ID] [Backpack ID] Set normal zombie backpack");
                    lines.Add("/zma shp [room ID] [Backpack ID] Set Human Backpack");
                    lines.Add("/zma hp [room ID] [Backpack ID] set hunter backpack");
                    lines.Add("/zma svp [room ID] [Backpack ID] Set spectator backpack");
                    lines.Add("/zma smp [room ID] [Number of Players] Set the maximum number of players");
                    lines.Add("/zma sdp [room ID] [number of players] set the minimum number of players");
                    lines.Add("/zma respawntime [Room ID] [Time] Set the respawn time (unit: second)");
                    lines.Add("/zma reloadpacks Reload backpack data");
                    lines.Add("/zma src [Room ID] [Number of mothers] Set the number of mothers");
                    lines.Add("/zma sp Pick a temporary point");
                    lines.Add("/zma swp [Room ID] Set the waiting point of the room");
                    lines.Add("/zma addp [Room ID] Add spawn point");
                    PaginationTools.Settings settings= new PaginationTools.Settings();
                    settings.FooterFormat = $"Enter /zma help {pageNumber+1} to see more commands";
                    settings.HeaderFormat = "biomode administrator command";
                    PaginationTools.SendPage(plr,pageNumber,lines,settings);
                    break;
                case "reloadpacks":
                    ConfigUtils.ReloadPacks();
                    args.Player.SendInfoMessage("Backpack reloaded successfully");
                    break;
            }
        }
        private void ZM(CommandArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            ZRoom room = null;
            int id;
            StringBuilder board = new StringBuilder();
            if (plr==null)
            {
                args.Player.SendInfoMessage("Data error, please try to re-enter the server");
                return;
            }
            if (args.Parameters.Count<1)
            {
                args.Player.SendInfoMessage("Please enter /zm help to view help");
                return;
            }
            switch (args.Parameters[0])
            {
                case "join":
                    if (args.Parameters.Count!=2)
                    {
                        plr.SendInfoMessage("Correct command: /zm join [room number]");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1],out id))
                    {
                        room = ConfigUtils.GetRoomByID(id);
                        if (room!=null)
                            plr.Join(room);
                        else
                            plr.SendInfoMessage("room does not exist");
                    }
                    else 
                    {
                        plr.SendInfoMessage("Please enter the number");
                    }
                    break;
                case "leave":
                    plr.Leave();
                    break;
                case "ready":
                    room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                    if (room!=null&&plr.Status==MiniGamesAPI.Enum.PlayerStatus.Waiting)
                    {
                        plr.Ready();
                        room.Broadcast($"Player[{plr.Name}]{(plr.IsReady?"已准备":"未准备")}",Color.Gold);
                    }
                    else
                    {
                        plr.SendInfoMessage("The room does not exist or the current state does not allow preparation");
                    }
                    break;
                case "list":
                    foreach (var tempRoom in ConfigUtils.rooms)
                    {
                        board.AppendLine($"[{tempRoom.ID}] [{tempRoom.Name}] [{tempRoom.GetPlayerCount()}/{tempRoom.MaxPlayer}] [{tempRoom.Status.ToString()}]");
                    }
                    plr.SendInfoMessage(board.ToString());
                    break;
                case "help":
                default:    
                    board.AppendLine("/zm join [room number] join a room");
                    board.AppendLine("/zm leave leaves the current room");
                    board.AppendLine("/zm ready ready/not ready");
                    board.AppendLine("/zm list view room list");
                    plr.SendInfoMessage(board.ToString());
                    break;
            }
        }
        private void OnChat(ServerChatEventArgs args)
        {
            if (args.Text.StartsWith(TShock.Config.Settings.CommandSilentSpecifier)||args.Text.StartsWith(TShock.Config.Settings.CommandSpecifier)) return;
            var tsplr = TShock.Players[args.Who];
            var plr = ConfigUtils.GetPlayerByName(tsplr.Name);
            if (plr!=null&&plr.CurrentRoomID!=0)
            {
                var room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                if (room != null) 
                {
                    room.Broadcast($"[Chat in the room][{(plr.Character==ZEnum.Zomble? "Zombie":"Human")}][{(plr.IsDead?""Dead":"Survival")}]{plr.Name}: {args. Text}",Color.LightPink);
                    args.Handled = true;
                }
            }

        }
        private void OnPlayerUpdate(object sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            ZRoom room = null;
            if (plr != null)
            {
                room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                bool flag = (room != null && room.Status == MiniGamesAPI.Enum.RoomStatus.Gaming && plr.Character==ZEnum.Human&&args.Control.IsUsingItem&&args.Player.TPlayer.HeldItem.netID==29&&!plr.isHunter);
                //bool flag_2 = (room!=null&&(room.Status==MiniGamesAPI.Enum.RoomStatus.Gaming|| room.Status == MiniGamesAPI.Enum.RoomStatus.Selecting)&&plr.Character==ZEnum.Human&&plr.BeLoaded&&args.Control.IsUsingItem&& args.Player.TPlayer.HeldItem.ranged);
                if (flag)
                {
                    plr.SelectPackID = room.HunterPackID;
                    plr.Godmode(true);
                    plr.Firework(1);
                    var pack = ConfigUtils.GetPackByID(room.HunterPackID);
                    if (pack != null) plr.RestorePlayerInv(pack);
                    plr.Godmode(false);
                    plr.isHunter = true;
                    room.Broadcast($"Player {plr.Name} has transformed into a ghost hunter",Color.Crimson);
                }
                /*if (flag_2)
                {
                    plr.SetBuff(23,60);
                    plr.SendInfoMessage("触发");
                }*/
            }
        }
        private void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            ZRoom room = null;
            if (plr != null)
            {
                room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                if (room != null && room.Status == MiniGamesAPI.Enum.RoomStatus.Gaming)
                {
                    if (args.Player.TPlayer.HeldItem.ranged && plr.Character == ZEnum.Human)
                    {
                        if (plr.BulletAmount==0)
                        {
                            plr.BeLoaded = true;
                            plr.SetBuff(23, 30);
                            var proj = Terraria.Main.projectile[args.Identity];
                            proj.active = false;
                            TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", args.Identity,proj.owner);
                            plr.SendCombatMessage("Beloading...",Color.Crimson);
                            //plr.SendErrorMessage("上膛中...");
                           /* plr.SendInfoMessage($"index:{args.Index}   id:{args.Identity}");*/
                            //args.Handled = true;

                        }
                        else
                        {
                            plr.BulletAmount -= 1;
                            plr.SendCombatMessage($"Left: {plr.BulletAmount}", Color.MediumAquamarine);
                        }
                    }
                }
            }
        }
        private void OnChangePVP(object sender, GetDataHandlers.TogglePvpEventArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            if (plr!=null&&plr.Status==MiniGamesAPI.Enum.PlayerStatus.Gaming)
            {
                if (plr.IsDead)
                    plr.SetPVP(false);
                else
                    plr.SetPVP(true) ;
                args.Handled = true;
            }
        }
        private void OnChangeTeam(object sender, GetDataHandlers.PlayerTeamEventArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            if (plr!=null&&plr.Status==MiniGamesAPI.Enum.PlayerStatus.Gaming)
            {
                if (plr.Character==ZEnum.Human)
                {
                    plr.SetTeam(3);
                }
                else
                {
                    if (plr.IsDead)
                        plr.SetTeam(0);
                    else 
                        plr.SetTeam(1);
                    
                }
                args.Handled = true;
            }
        }
        private void OnPlayerSpawn(object sender, GetDataHandlers.SpawnEventArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            ZRoom room = null;
            if (plr!=null)
            {
                //plr.Player.Spawn(PlayerSpawnContext.SpawningIntoWorld,1);
                room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                if (room!=null)
                {
                    var rand = new Terraria.Utilities.UnifiedRandom();
                    var pack = ConfigUtils.GetPackByID(plr.SelectPackID);
                    if (room.Status==MiniGamesAPI.Enum.RoomStatus.Gaming)
                    {
                        plr.Teleport(room.SpawnPoints[rand.Next(0,room.SpawnPoints.Count-1)]);
                        plr.SendInfoMessage("reborn！");
                        plr.SetTeam(1);
                        if (pack != null) pack.RestoreCharacter(plr);
                        if (plr.IsDead)
                        {
                            plr.SetPVP(false);
                            plr.SetTeam(0);
                            plr.BulletTimer.Stop();
                            plr.SendInfoMessage("Entered the spectator mode");
                        }
                        
                    }
                    if(room.Status==MiniGamesAPI.Enum.RoomStatus.Selecting)
                    {
                        plr.Teleport(room.LobbyPoint);
                        plr.SetTeam(0);
                        plr.SendInfoMessage("sent you back to the waiting room");
                        pack.RestoreCharacter(plr);
                    }
                    if(room.Status==MiniGamesAPI.Enum.RoomStatus.Waiting||room.Status==MiniGamesAPI.Enum.RoomStatus.Concluding||room.Status==MiniGamesAPI.Enum.RoomStatus.Restoring)
                    {
                        plr.Teleport(room.LobbyPoint);
                        plr.SetPVP(false);
                        plr.SetTeam(0);
                        plr.BackUp.RestoreCharacter(plr);
                        plr.SendInfoMessage("sent you back to the waiting room");
                    }
                    args.Handled = true;
                }
            }
        }
        private void OnOpenChest(object sender, GetDataHandlers.ChestOpenEventArgs args)
        {
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            if (plr!=null&&plr.CurrentRoomID!=0)
            {
                plr.SendInfoMessage("Boxes cannot be opened in game");
                args.Handled = true;
            }

        }
        private void OnKillMe(object sender,GetDataHandlers.KillMeEventArgs args)
        {
            TSPlayer other = null;
            ZPlayer zohter = null;
            ZPlayer victim = ConfigUtils.GetPlayerByName(args.Player.Name);
            ZRoom targetRoom = null;
            if (args.PlayerDeathReason._sourcePlayerIndex != -1) 
            { 
                other = TShock.Players[args.PlayerDeathReason._sourcePlayerIndex];
                zohter = ConfigUtils.GetPlayerByName(other.Name);//获取对方的实例化
            }
            if (victim!=null&&victim.CurrentRoomID!=0)
            {
                targetRoom = ConfigUtils.GetRoomByID(victim.CurrentRoomID);//获取受害者当前房间ID
            }
            if (targetRoom == null && victim == null) return;
            if (args.Pvp)
            {
                if (targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Waiting ||
                    targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Concluding || 
                    targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Restoring)
                {

                }
                if (targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Selecting)
                {
                    //死亡就跳转到Gaming时 变成普通僵尸
                    //先转移回出生点，等待Gaming
                    //这个一般不会被触发
                }
                if (targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Gaming)
                {
                    if (zohter == null) return;
                    if (victim.Character==ZEnum.Human&&zohter.Character==ZEnum.Zomble)
                    {
                        victim.Character = ZEnum.Zomble;//角色身份变化
                        victim.SetTeam(1);
                        victim.SelectPackID = targetRoom.NormalPackID;
                        targetRoom.Broadcast($"Player [{victim.Name}] was scratched by [{zohter.Name}]! Become an infected body!",Color.Crimson);
                    }
                    if (victim.Character==ZEnum.Zomble&&zohter.Character==ZEnum.Human)
                    {
                        if (zohter.Player.TPlayer.HeldItem.ranged)
                        {
                            targetRoom.Broadcast($"Infected {victim.Name} was shot by {zohter.Name}!",Color.DarkTurquoise);
                        }
                        else
                        {
                            victim.IsDead = true;
                            victim.SelectPackID = targetRoom.ViewerPackID;
                            targetRoom.Broadcast($"remarkably brave! Infected {victim.Name} was slashed by {zohter.Name}! Impossible to revive", Color.DarkTurquoise);
                        }
                    }
                }

            }
            else 
            {
                if (targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Waiting||
                    targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Concluding|| 
                    targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Restoring)
                {

                }
                if (targetRoom.Status==MiniGamesAPI.Enum.RoomStatus.Selecting|| targetRoom.Status == MiniGamesAPI.Enum.RoomStatus.Gaming)
                {
                    victim.IsDead = true;
                    victim.SelectPackID = targetRoom.ViewerPackID;
                    victim.SetTeam(0);
                    targetRoom.Broadcast($"The player {victim.Name} died due to unknown reasons, and will become an infected body after resurrection",Color.DarkTurquoise);
                    
                }
            }
            args.Player.Spawn(0,targetRoom.RespawnTime);
            args.Handled = true;
            /*TSPlayer other=null;
            if (args.PlayerDeathReason._sourcePlayerIndex!=-1) other = TShock.Players[args.PlayerDeathReason._sourcePlayerIndex];
            ZPlayer zother = null;
            var plr = ConfigUtils.GetPlayerByName(args.Player.Name);
            ZRoom room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
            if (room == null) return;
            if (args.Pvp&&room.Status==MiniGamesAPI.Enum.RoomStatus.Gaming)
            {
                zother = ConfigUtils.GetPlayerByName(other.Name);
                plr.Player.RespawnTimer = room.RespawnTime;
                if (zother.Character == ZEnum.Zomble && plr.Character == ZEnum.Human)//enemy's corpse
                {
                    plr.Character = ZEnum.Zomble;
                    plr.SelectPackID = room.NormalPackID;
                    room.Broadcast($"{plr.Name} got scratched by {zother.Name} and turned into a zombie!", Color.Crimson);
                }
                if (zother.Character == ZEnum.Human && plr.Character == ZEnum.Zomble)//dead enemy
                {
                    if (zother.Player.ItemInHand.ranged)
                    {
                        plr.IsDead = true;
                        plr.SelectPackID = room.ViewerPackID;
                        room.Broadcast($"{zother.Name} Give {plr.Name} to the knife! remarkably brave", Color.MediumAquamarine);
                    }
                    else
                    {
                        room.Broadcast($"{zother.Name} Shoot {plr.Name} dead!", Color.MediumAquamarine);
                    }
                }
                args.Handled = true;
            }else if (!args.Pvp && room.Status == MiniGamesAPI.Enum.RoomStatus.Waiting) 
            {
                plr.Player.RespawnTimer = room.RespawnTime;
                plr.Character = ZEnum.Human;
                plr.BackUp.RestoreCharacter(plr);
                args.Handled = true;
            }
            else if(!args.Pvp&&room.Status==MiniGamesAPI.Enum.RoomStatus.Selecting)
            {
                plr.Player.RespawnTimer = room.RespawnTime;
                plr.IsDead = true;
                plr.SelectPackID = room.ViewerPackID;
                args.Handled = true;
            }
            else if (!args.Pvp && room.Status == MiniGamesAPI.Enum.RoomStatus.Gaming)
            {
                plr.Player.RespawnTimer = room.RespawnTime;
                //plr.IsDead = true;
                plr.SelectPackID = room.NormalPackID;
                plr.Character = ZEnum.Zomble;
                plr.SendInfoMessage("Died for unknown reasons, will become a zombie after rebirth");
                args.Handled = true;
            }
            */
            /*if (plr!=null&&zother!=null&&plr.CurrentRoomID!=0&&plr.CurrentRoomID==zother.CurrentRoomID)
            {
                room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                if (room!=null&&room.Status==MiniGamesAPI.Enum.RoomStatus.Gaming)
                {
                    plr.Player.RespawnTimer = room.RespawnTime;
                    if (zother.Character==ZEnum.Zomble&&plr.Character==ZEnum.Human)//enemy's corpse
                    {
                        plr.Character = ZEnum.Zomble;
                        plr.SelectPackID = room.NormalPackID;
                        room.Broadcast($"{plr.Name} Got scratched by {zother.Name} and turned into a zombie!",Color.Crimson);
                    }
                    if (zother.Character == ZEnum.Human && plr.Character == ZEnum.Zomble)//dead enemy
                    {
                        if (!zother.Player.ItemInHand.ranged)
                        {
                            plr.IsDead = true;
                            plr.SelectPackID = room.ViewerPackID;
                            room.Broadcast($"{zother.Name} Bundle {plr.Name} Give the knife! remarkably brave",Color.MediumAquamarine);
                        }
                        else 
                        {
                            room.Broadcast($"{zother.Name} Bundle {plr.Name} shot!", Color.MediumAquamarine);
                        }
                    }
                    args.Handled = true;
                }
            } */
        }
        private void OnLeave(LeaveEventArgs args)
        {
            var tsplr = TShock.Players[args.Who];
            try
            {
                var plr = ConfigUtils.GetPlayerByName(tsplr.Name);
                if (plr != null)
                {
                    var room = ConfigUtils.GetRoomByID(plr.CurrentRoomID);
                    if (room != null)
                    {
                        room.Players.Remove(plr);
                        room.Broadcast($"Player [{tsplr.Name}] forcibly exited the room", Color.DarkTurquoise);
                    }
                    if (plr.BackUp!=null)
                    {
                        plr.BackUp.RestoreCharacter(plr);
                    }
                    
                    plr.BackUp = null;
                    plr.Player = null;
                }
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError(e.ToString());
                TShock.Log.ConsoleInfo($"Player [{tsplr.Name}] encountered an error logging out of the server");
            }
            

        }
        private void OnJoin(GreetPlayerEventArgs args)
        {
            var tsplr = TShock.Players[args.Who];
            var plr = ConfigUtils.GetPlayerByName(tsplr.Name);
            if (plr==null)
            {
                plr = new ZPlayer(ConfigUtils.players.Count+1,tsplr);
                ConfigUtils.players.Add(plr);
                ConfigUtils.UpdatePlayers();
            }
            plr.Player = tsplr;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
                GetDataHandlers.KillMe -= OnKillMe;
                GetDataHandlers.ChestOpen -= OnOpenChest;
                GetDataHandlers.NewProjectile -= OnNewProjectile;
                GetDataHandlers.PlayerSpawn -= OnPlayerSpawn;
                GetDataHandlers.TogglePvp -= OnChangePVP;
                GetDataHandlers.PlayerTeam -= OnChangeTeam;
            }
            base.Dispose(disposing);
        }
    }
}
