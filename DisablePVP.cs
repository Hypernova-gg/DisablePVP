using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Facepunch;
using Oxide.Core;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Oxide.Plugins
{
	[Info("DisablePVP", "Airathias", "1.0.0")]
	[Description("Disable damage done to players by players only")]

    public class DisablePVP : RustPlugin
	{

        #region Configuration
        private ConfigurationFile Conf;

        public class ConfigurationFile
        {
            [JsonProperty(PropertyName = "PVP blocked (true/false)")]
            public bool Enabled = true;     
            
            [JsonProperty(PropertyName = "Chat Prefix")]
            public string ChatPrefix = "PVP Block";    
            
            [JsonProperty(PropertyName = "Chat Prefix Color")]
            public string ChatPrefixColor = "#eb4034";      

            [JsonProperty(PropertyName = "Print to chat (true/false)")]
            public bool PrintToChat = false;      
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            Conf = new ConfigurationFile();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Conf = Config.ReadObject<ConfigurationFile>();
            SaveConfig();
        }
        
        protected override void SaveConfig() => Config.WriteObject(Conf);
        #endregion

        
        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>()
            {
                { "pvpblocked", "You cannot damage another player!" },
            }, this);
        }

        /// <summary>
        /// Returns a formatted lang entry based on key and userId
        /// </summary>
        /// <param name="key">Multilingual identifier key</param>
        /// <param name="id">Id of the user</param>
        /// <param name="args">List of arguments to format the string with; can exceed amount of variables in string, cannot be less</param>
        /// <returns>Formatted multilingual string</returns>
        string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        /// <summary>
        /// Sends a player an in-game chat message with configurable prefix, if player is online and message is not empty
        /// </summary>
        /// <param name="player">Recipient of the chatmessage</param>
        /// <param name="msg">The actual formatted message</param>
        private void MessagePlayer(BasePlayer player, string msg)
		{
			if (player?.net?.connection == null || String.IsNullOrWhiteSpace(msg))
				return;

			SendReply(player, $"<color={Conf.ChatPrefixColor}>{Conf.ChatPrefix}</color>: {msg}");
		}

        #endregion

        private void OnServerInitialized()
		{
            if(!Conf.Enabled) {
                Unsubscribe(nameof(OnEntityTakeDamage));
            } else {
                Puts("PVP is disabled!");
            }
        }


        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity.ToPlayer() == null || info.Initiator.ToPlayer() == null) return true; // If either the player or the victim is not a player, this ain't pvp

            if(Conf.PrintToChat) {
                BasePlayer player = info.Initiator.ToPlayer();
                MessagePlayer(player, Lang("pvpblocked", player.UserIDString));
            }

            return null;
        }
    }
}