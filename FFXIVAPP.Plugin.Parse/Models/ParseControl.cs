﻿// FFXIVAPP.Plugin.Parse
// ParseControl.cs
// 
// Copyright © 2007 - 2015 Ryan Wilson - All Rights Reserved
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are met: 
// 
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer. 
//  * Redistributions in binary form must reproduce the above copyright 
//    notice, this list of conditions and the following disclaimer in the 
//    documentation and/or other materials provided with the distribution. 
//  * Neither the name of SyndicatedLife nor the names of its contributors may 
//    be used to endorse or promote products derived from this software 
//    without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using FFXIVAPP.Common.Core.Memory.Enums;
using FFXIVAPP.Common.RegularExpressions;
using FFXIVAPP.Plugin.Parse.Enums;
using FFXIVAPP.Plugin.Parse.Helpers;
using FFXIVAPP.Plugin.Parse.Models.StatGroups;
using FFXIVAPP.Plugin.Parse.Models.Timelines;
using FFXIVAPP.Plugin.Parse.Monitors;
using FFXIVAPP.Plugin.Parse.Properties;
using Newtonsoft.Json;
using NLog;

namespace FFXIVAPP.Plugin.Parse.Models
{
    public class ParseControl : IParsingControl, INotifyPropertyChanged
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Auto Properties

        public bool FirstActionFound { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        #endregion

        #region Declarations

        private readonly Timer _parseEntityTimer = new Timer(100);
        private ParseEntity LastParseEntity { get; set; }

        #endregion

        public ParseControl()
        {
            Timeline = new Timeline(this);
            TimelineMonitor = new TimelineMonitor(this);
            StatMonitor = new StatMonitor(this);
            StartTime = DateTime.Now;
            _parseEntityTimer.Elapsed += ParseEntityTimerOnElapsed;
            _parseEntityTimer.Start();
        }

        private bool ParseEntityTimerProcessing { get; set; }

        private void ParseEntityTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            EndTime = DateTime.Now;
            if (ParseEntityTimerProcessing)
            {
                return;
            }
            ParseEntityTimerProcessing = true;
            Func<bool> parseEntityProcessor = delegate
            {
                try
                {
                    var parseEntity = new ParseEntity
                    {
                        Players = new List<PlayerEntity>()
                    };
                    foreach (Player player in Timeline.Party)
                    {
                        try
                        {
                            var type = Regex.Match(player.Name, @"\[(?<type>.+)\]", SharedRegEx.DefaultOptions)
                                            .Groups["type"].Value;
                            var playerEntity = new PlayerEntity
                            {
                                Name = player.Name,
                                Job = Actor.Job.Unknown,
                                CombinedDPS = (decimal) player.GetStatValue("CombinedDPS"),
                                DPS = (decimal) player.GetStatValue("DPS"),
                                DOTPS = (decimal) player.GetStatValue("DOTPS"),
                                CombinedHPS = (decimal) player.GetStatValue("CombinedHPS"),
                                HPS = (decimal) player.GetStatValue("HPS"),
                                HOTPS = (decimal) player.GetStatValue("HOTPS"),
                                HOHPS = (decimal) player.GetStatValue("HOHPS"),
                                HMPS = (decimal) player.GetStatValue("HMPS"),
                                CombinedDTPS = (decimal) player.GetStatValue("CombinedDTPS"),
                                DTPS = (decimal) player.GetStatValue("DTPS"),
                                DTOTPS = (decimal) player.GetStatValue("DTOTPS"),
                                CombinedTotalOverallDamage = (decimal) player.GetStatValue("CombinedTotalOverallDamage"),
                                TotalOverallDamage = (decimal) player.GetStatValue("TotalOverallDamage"),
                                TotalOverallDamageOverTime = (decimal) player.GetStatValue("TotalOverallDamageOverTime"),
                                CombinedTotalOverallHealing = (decimal) player.GetStatValue("CombinedTotalOverallHealing"),
                                TotalOverallHealing = (decimal) player.GetStatValue("TotalOverallHealing"),
                                TotalOverallHealingOverTime = (decimal) player.GetStatValue("TotalOverallHealingOverTime"),
                                TotalOverallHealingOverHealing = (decimal) player.GetStatValue("TotalOverallHealingOverHealing"),
                                TotalOverallHealingMitigated = (decimal) player.GetStatValue("TotalOverallHealingMitigated"),
                                CombinedTotalOverallDamageTaken = (decimal) player.GetStatValue("CombinedTotalOverallDamageTaken"),
                                TotalOverallDamageTaken = (decimal) player.GetStatValue("TotalOverallDamageTaken"),
                                TotalOverallDamageTakenOverTime = (decimal) player.GetStatValue("TotalOverallDamageTakenOverTime"),
                                PercentOfTotalOverallDamage = (decimal) player.GetStatValue("PercentOfTotalOverallDamage"),
                                PercentOfTotalOverallDamageOverTime = (decimal) player.GetStatValue("PercentOfTotalOverallDamageOverTime"),
                                PercentOfTotalOverallHealing = (decimal) player.GetStatValue("PercentOfTotalOverallHealing"),
                                PercentOfTotalOverallHealingOverTime = (decimal) player.GetStatValue("PercentOfTotalOverallHealingOverTime"),
                                PercentOfTotalOverallHealingOverHealing = (decimal) player.GetStatValue("PercentOfTotalOverallHealingOverHealing"),
                                PercentOfTotalOverallHealingMitigated = (decimal) player.GetStatValue("PercentOfTotalOverallHealingMitigated"),
                                PercentOfTotalOverallDamageTaken = (decimal) player.GetStatValue("PercentOfTotalOverallDamageTaken"),
                                PercentOfTotalOverallDamageTakenOverTime = (decimal) player.GetStatValue("PercentOfTotalOverallDamageTakenOverTime")
                            };
                            switch (type)
                            {
                                case "P":
                                    playerEntity.Type = PlayerType.Party;
                                    break;
                                case "O":
                                    playerEntity.Type = PlayerType.Other;
                                    break;
                                case "A":
                                    playerEntity.Type = PlayerType.Alliance;
                                    break;
                                case "???":
                                    playerEntity.Type = PlayerType.Unknown;
                                    break;
                                default:
                                    playerEntity.Type = PlayerType.You;
                                    break;
                            }
                            if (player.NPCEntry != null)
                            {
                                playerEntity.Job = player.NPCEntry.Job;
                            }
                            parseEntity.Players.Add(playerEntity);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    parseEntity.CombinedDPS = (decimal) Timeline.Overall.GetStatValue("CombinedDPS");
                    parseEntity.DPS = (decimal) Timeline.Overall.GetStatValue("DPS");
                    parseEntity.DOTPS = (decimal) Timeline.Overall.GetStatValue("DOTPS");
                    parseEntity.CombinedHPS = (decimal) Timeline.Overall.GetStatValue("CombinedHPS");
                    parseEntity.HPS = (decimal) Timeline.Overall.GetStatValue("HPS");
                    parseEntity.HOTPS = (decimal) Timeline.Overall.GetStatValue("HOTPS");
                    parseEntity.HOHPS = (decimal) Timeline.Overall.GetStatValue("HOHPS");
                    parseEntity.HMPS = (decimal) Timeline.Overall.GetStatValue("HMPS");
                    parseEntity.CombinedDTPS = (decimal) Timeline.Overall.GetStatValue("CombinedDTPS");
                    parseEntity.DTPS = (decimal) Timeline.Overall.GetStatValue("DTPS");
                    parseEntity.DTOTPS = (decimal) Timeline.Overall.GetStatValue("DTOTPS");
                    parseEntity.CombinedTotalOverallDamage = (decimal) Timeline.Overall.GetStatValue("CombinedTotalOverallDamage");
                    parseEntity.TotalOverallDamage = (decimal) Timeline.Overall.GetStatValue("TotalOverallDamage");
                    parseEntity.TotalOverallDamageOverTime = (decimal) Timeline.Overall.GetStatValue("TotalOverallDamageOverTime");
                    parseEntity.CombinedTotalOverallHealing = (decimal) Timeline.Overall.GetStatValue("CombinedTotalOverallHealing");
                    parseEntity.TotalOverallHealing = (decimal) Timeline.Overall.GetStatValue("TotalOverallHealing");
                    parseEntity.TotalOverallHealingOverTime = (decimal) Timeline.Overall.GetStatValue("TotalOverallHealingOverTime");
                    parseEntity.TotalOverallHealingOverHealing = (decimal) Timeline.Overall.GetStatValue("TotalOverallHealingOverHealing");
                    parseEntity.TotalOverallHealingMitigated = (decimal) Timeline.Overall.GetStatValue("TotalOverallHealingMitigated");
                    parseEntity.CombinedTotalOverallDamageTaken = (decimal) Timeline.Overall.GetStatValue("CombinedTotalOverallDamageTaken");
                    parseEntity.TotalOverallDamageTaken = (decimal) Timeline.Overall.GetStatValue("TotalOverallDamageTaken");
                    parseEntity.TotalOverallDamageTakenOverTime = (decimal) Timeline.Overall.GetStatValue("TotalOverallDamageTakenOverTime");
                    parseEntity.PercentOfTotalOverallDamage = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallDamage");
                    parseEntity.PercentOfTotalOverallDamageOverTime = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallDamageOverTime");
                    parseEntity.PercentOfTotalOverallHealing = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallHealing");
                    parseEntity.PercentOfTotalOverallHealingOverTime = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallHealingOverTime");
                    parseEntity.PercentOfTotalOverallHealingOverHealing = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallHealingOverHealing");
                    parseEntity.PercentOfTotalOverallHealingMitigated = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallHealingMitigated");
                    parseEntity.PercentOfTotalOverallDamageTaken = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallDamageTaken");
                    parseEntity.PercentOfTotalOverallDamageTakenOverTime = (decimal) Timeline.Overall.GetStatValue("PercentOfTotalOverallDamageTakenOverTime");
                    var notify = false;
                    if (LastParseEntity == null)
                    {
                        LastParseEntity = parseEntity;
                        notify = true;
                    }
                    else
                    {
                        var hash1 = JsonConvert.SerializeObject(LastParseEntity)
                                               .GetHashCode();
                        var hash2 = JsonConvert.SerializeObject(parseEntity)
                                               .GetHashCode();
                        if (!hash1.Equals(hash2))
                        {
                            LastParseEntity = parseEntity;
                            notify = true;
                        }
                    }
                    if (notify)
                    {
                        if (Settings.Default.ShowDPSWidgetOnLoad)
                        {
                            EntityHelper.Parse.CleanAndCopy(parseEntity, EntityHelper.Parse.ParseType.DPS);
                        }
                        if (Settings.Default.ShowDTPSWidgetOnLoad)
                        {
                            EntityHelper.Parse.CleanAndCopy(parseEntity, EntityHelper.Parse.ParseType.DTPS);
                        }
                        if (Settings.Default.ShowHPSWidgetOnLoad)
                        {
                            EntityHelper.Parse.CleanAndCopy(parseEntity, EntityHelper.Parse.ParseType.HPS);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                ParseEntityTimerProcessing = false;
                return true;
            };
            parseEntityProcessor.BeginInvoke(delegate { }, parseEntityProcessor);
        }

        #region Implementation of IParsingControl

        private static ParseControl _instance;
        private StatMonitor _statMonitor;
        private Timeline _timeline;
        private TimelineMonitor _timelineMonitor;

        public static ParseControl Instance
        {
            get { return _instance ?? (_instance = new ParseControl()); }
            set { _instance = value; }
        }

        IParsingControl IParsingControl.Instance
        {
            get { return Instance; }
        }

        public Timeline Timeline
        {
            get { return _timeline ?? (_timeline = new Timeline(this)); }
            set
            {
                _timeline = value;
                RaisePropertyChanged();
            }
        }

        public StatMonitor StatMonitor
        {
            get { return _statMonitor ?? (_statMonitor = new StatMonitor(this)); }
            set
            {
                _statMonitor = value;
                RaisePropertyChanged();
            }
        }

        public TimelineMonitor TimelineMonitor
        {
            get { return _timelineMonitor ?? (_timelineMonitor = new TimelineMonitor(this)); }
            set
            {
                _timelineMonitor = value;
                RaisePropertyChanged();
            }
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
            _parseEntityTimer.Stop();
            _parseEntityTimer.Elapsed -= ParseEntityTimerOnElapsed;
            FirstActionFound = !FirstActionFound;
            StatMonitor.Clear();
            Timeline.Clear();
            TimelineMonitor.Clear();
            var parseEntity = new ParseEntity
            {
                Players = new List<PlayerEntity>()
            };
            StartTime = DateTime.Now;
            EntityHelper.Parse.CleanAndCopy(parseEntity, EntityHelper.Parse.ParseType.DPS);
            EntityHelper.Parse.CleanAndCopy(parseEntity, EntityHelper.Parse.ParseType.DTPS);
            EntityHelper.Parse.CleanAndCopy(parseEntity, EntityHelper.Parse.ParseType.HPS);
            _parseEntityTimer.Elapsed += ParseEntityTimerOnElapsed;
            _parseEntityTimer.Start();
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        #endregion
    }
}
