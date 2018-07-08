﻿/*
    Copyright 2015 MCGalaxy
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using System.Threading;
using MCGalaxy.Commands.World;
using MCGalaxy.Events.GameEvents;

namespace MCGalaxy.Games {
    
    public abstract partial class RoundsGame : IGame {
        public int RoundsLeft;
        public bool RoundInProgress;
        public DateTime RoundStart;
        public string LastMap = "";
        public LevelPicker Picker;
        
        public abstract void OutputStatus(Player p);
        public abstract RoundsGameConfig GetConfig();
        public abstract void UpdateMapConfig();
        
        protected abstract void DoRound();
        protected abstract List<Player> GetPlayers();
        protected virtual void SaveStats(Player pl) { }
        
        protected abstract void StartGame();
        public virtual void Start(Player p, string map, int rounds) {
            map = GetStartMap(this, map);
            if (map == null) {
                Player.Message(p, "No maps have been setup for {0} yet", GameName); return;
            }
            if (!SetMap(map)) {
                Player.Message(p, "Failed to load initial map!"); return;
            }
            
            Chat.MessageGlobal("A game of {0} is starting on {1}%S!", GameName, Map.ColoredName);
            Logger.Log(LogType.GameActivity, "[{0}] Game started", GameName);
            
            StartGame();
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) {
                if (pl.level == Map) PlayerJoinedGame(pl);
            }
            
            RoundsLeft = rounds;
            Running = true;            
            IGame.RunningGames.Add(this);
            OnStateChangedEvent.Call(this);
            HookEventHandlers();
            
            Thread t = new Thread(RunGame);
            t.Name = "MCG_" + GameName;
            t.Start();
        }
        
        void RunGame() {
            try {
                while (Running && RoundsLeft > 0) {
                    RoundInProgress = false;
                    if (RoundsLeft != int.MaxValue) RoundsLeft--;
                    
                    DoRound();
                    if (Running) EndRound();
                    if (Running) VoteAndMoveToNextMap();
                }
                End();
            } catch (Exception ex) {
                Logger.LogError("Error in game " + GameName, ex);
                Chat.MessageGlobal("%W" + GameName + " disabled due to an error.");
                
                try { End(); }
                catch (Exception ex2) { Logger.LogError(ex2); }
            }
            IGame.RunningGames.Remove(this);
        }
        
        protected virtual string GetStartMap(RoundsGame game, string forcedMap) {
            if (forcedMap.Length > 0) return forcedMap;
            List<string> maps = Picker.GetCandidateMaps(game);
            
            if (maps == null || maps.Count == 0) return null;
            return LevelPicker.GetRandomMap(new Random(), maps);
        }
        
        protected virtual bool SetMap(string map) {
            Picker.QueuedMap = null;
            Level next = LevelInfo.FindExact(map);
            if (next == null) next = CmdLoad.LoadLevel(null, map);
            if (next == null) return false;
            
            Map = next;
            Map.SaveChanges = false;
            
            if (GetConfig().SetMainLevel) Server.SetMainLevel(Map);
            UpdateMapConfig();
            return true;
        }
        
        protected void DoCountdown(string format, int delay, int minThreshold) {
            const CpeMessageType type = CpeMessageType.Announcement;
            for (int i = delay; i > 0 && Running; i--) {
                if (i == 1) {
                    MessageMap(type, String.Format(format, i)
                               .Replace("seconds", "second"));
                } else if (i < minThreshold || (i % 10) == 0) {
                    MessageMap(type, String.Format(format, i));
                }
                Thread.Sleep(1000);
            }
            MessageMap(type, "");
        }
        
        protected List<Player> DoRoundCountdown(int delay) {
            while (true) {
                RoundStart = DateTime.UtcNow.AddSeconds(delay);
                if (!Running) return null;

                DoCountdown("&4Starting in &f{0} &4seconds", delay, 10);
                if (!Running) return null;
                
                List<Player> players = GetPlayers();
                if (players.Count >= 2) return players;
                Map.Message("%WNeed 2 or more non-ref players to start a round.");
            }
        }
        
        protected virtual void VoteAndMoveToNextMap() {
            Picker.AddRecentMap(Map.MapName);
            if (RoundsLeft == 0) return;
            string map = Picker.ChooseNextLevel(this);
            if (map == null) return;
            
            Map.Message("The next map has been chosen - &c" + map.ToLower());
            Map.Message("Please wait while you are transfered.");
            LastMap = Map.MapName;
            
            if (!SetMap(map)) {
                Map.Message("%WFailed to change map to " + map);
                Map.Message("Continuing " + GameName + " on the same map");
            } else {
                TransferPlayers(LastMap);
                Command.Find("Unload").Use(null, LastMap);
            }
        }
        
        void TransferPlayers(string lastMap) {
            Random rnd = new Random();
            Player[] online = PlayerInfo.Online.Items;
            List<Player> transfers = new List<Player>(online.Length);
            
            foreach (Player pl in online) {
                pl.Game.RatedMap = false;
                pl.Game.PledgeSurvive = false;
                if (pl.level != Map && pl.level.name.CaselessEq(lastMap)) { transfers.Add(pl); }
            }
            
            while (transfers.Count > 0) {
                int i = rnd.Next(0, transfers.Count);
                Player pl = transfers[i];
                
                pl.SendMessage("Going to the next map - &a" + Map.MapName);
                PlayerActions.ChangeMap(pl, Map);
                transfers.RemoveAt(i);
            }
        }
        
        protected abstract void EndGame();
        public override void End() {
            if (!Running) return;
            Running = false;
            IGame.RunningGames.Remove(this);
            UnhookEventHandlers();
            
            if (RoundInProgress) {
                EndRound();
                RoundInProgress = false;
            }
            
            EndGame();
            OnStateChangedEvent.Call(this);
            
            RoundStart = DateTime.MinValue;
            RoundsLeft = 0;
            RoundInProgress = false;
            
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) {
                if (pl.level != Map) continue;
                pl.Game.RatedMap = false;
                pl.Game.PledgeSurvive = false;
                PlayerLeftGame(pl);
                
                TabList.Update(pl, true);
                ResetStatus(pl);
                pl.SetPrefix();
            }
            
            // in case players left game partway through
            foreach (Player pl in players) { SaveStats(pl); }
            
            if (Map != null) Map.Message(GameName + " %Sgame ended");
            Logger.Log(LogType.GameActivity, "[{0}] Game ended", GameName);
            if (Picker != null) Picker.Clear();
            
            LastMap = "";
            if (Map != null) Map.AutoUnload();
            Map = null;
        } 
    }
}