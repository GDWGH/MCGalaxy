﻿/*
    Copyright 2011 MCForge
    
    Written by fenderrock87
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCGalaxy.Config;

namespace MCGalaxy.Games {
    
    public sealed class CTFConfig {
 
        [ConfigInt("base.red.x", null, 0)]
        public int RedFlagX;
        [ConfigInt("base.red.y", null, 0)]
        public int RedFlagY;
        [ConfigInt("base.red.z", null, 0)]
        public int RedFlagZ;
        [ConfigByte("base.red.block", null, 0)]
        public byte RedFlagBlock;
 
        [ConfigInt("base.blue.x", null, 0)]
        public int BlueFlagX;
        [ConfigInt("base.blue.y", null, 0)]
        public int BlueFlagY;
        [ConfigInt("base.blue.z", null, 0)]
        public int BlueFlagZ;
        [ConfigByte("base.blue.block", null, 0)]
        public byte BlueFlagBlock;
        
        [ConfigInt("base.red.spawnx", null, 0)]
        public int RedSpawnX;
        [ConfigInt("base.red.spawny", null, 0)]
        public int RedSpawnY;
        [ConfigInt("base.red.spawnz", null, 0)]
        public int RedSpawnZ;
 
        [ConfigInt("base.blue.spawnx", null, 0)]
        public int BlueSpawnX;
        [ConfigInt("base.blue.spawny", null, 0)]
        public int BlueSpawnY;
        [ConfigInt("base.blue.spawnz", null, 0)]
        public int BlueSpawnZ;
        
        [ConfigBool("auto.setup", null, false)]
        public bool NeedSetup;
        [ConfigInt("map.line.z", null, 0)]
        public int ZLine;
        
        [ConfigInt("game.maxpoints", null, 0)]
        public int MaxPoints;
        [ConfigInt("game.tag.points-gain", null, 0)]
        public int Tag_PointsGained;
        [ConfigInt("game.tag.points-lose", null, 0)]
        public int Tag_PointsLost;
        [ConfigInt("game.capture.points-gain", null, 0)]
        public int Capture_PointsGained;
        [ConfigInt("game.capture.points-lose", null, 0)]
        public int Capture_PointsLost;
    }
}