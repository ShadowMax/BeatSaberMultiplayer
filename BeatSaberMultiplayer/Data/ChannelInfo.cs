﻿using Lidgren.Network;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberMultiplayer.Data
{
    public enum ChannelState : byte { InGame, Voting, NextSong, Results };

    public class ChannelInfo
    {
        public int channelId;
        public string name;
        public string iconUrl;
        public ChannelState state;
        public SongInfo currentSong;
        public BeatmapDifficulty preferredDifficulty;
        public int playerCount;
        public string ip;
        public int port;

        public ChannelInfo(JSONNode jsonNode)
        {
            channelId = jsonNode["channelId"];
            ip = jsonNode["ip"];
            port = jsonNode["port"];
        }

        public ChannelInfo(NetIncomingMessage msg)
        {
            channelId = msg.ReadInt32();
            name = msg.ReadString();
            iconUrl = msg.ReadString();
            state = (ChannelState)msg.ReadByte();
            if (state != ChannelState.Voting)
            {
                currentSong = new SongInfo(msg);
            }
            else
                currentSong = null;
            preferredDifficulty = (BeatmapDifficulty)msg.ReadByte();
            playerCount = msg.ReadInt32();
        }

        public void AddToMessage(NetOutgoingMessage msg)
        {
            msg.Write(channelId);
            msg.Write(name);
            msg.Write(iconUrl);
            msg.Write((byte)state);
            if (state != ChannelState.Voting)
            {
                currentSong.AddToMessage(msg);
            }
            msg.Write((byte)preferredDifficulty);
            msg.Write(playerCount);
        }
    }
}
