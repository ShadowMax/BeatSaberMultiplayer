﻿using ServerHub.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ServerHub.Misc
{
    static class ClientHelper
    {
        public static event Action<Client> LostConnection;

        public static BasePacket ReceiveData(this Client client, bool waitForData = true)
        {

            if (client.socket == null || !client.socket.Connected)
                return null;

            if (!waitForData && client.socket.Available == 0)
                return null;

            byte[] dataBuffer = null;

            try
            {
                byte[] lengthBuffer = new byte[4];
                client.socket.Receive(lengthBuffer, 0, 4, SocketFlags.None);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                dataBuffer = new byte[length];

                int nDataRead = 0;
                int nStartIndex = 0;

                while (nDataRead < length)
                {

                    int nBytesRead = client.socket.Receive(dataBuffer, nStartIndex, length - nStartIndex, SocketFlags.None);

                    nDataRead += nBytesRead;
                    nStartIndex += nBytesRead;
                }
            }
            catch(IOException e)
            {
#if DEBUG
                if (client.playerInfo != null)
                {
                    Logger.Instance.Log($"Lost connection to the {client.playerInfo.playerName}: {e}");
                }
                else
                {
                    Logger.Instance.Log($"Lost connection to the client: {e}");
                }
#else
                if (client.playerInfo != null)
                {
                    Logger.Instance.Log($"Lost connection to the {client.playerInfo.playerName}.");
                }
                else
                {
                    Logger.Instance.Log($"Lost connection to the client.");
                }
#endif
                LostConnection?.Invoke(client);
            }
            catch (Exception)
            {

            }

            return new BasePacket(dataBuffer);            
        }

        public static void SendData(this Client client, BasePacket packet)
        {
            if (client.socket == null || !client.socket.Connected)
                return;

            byte[] buffer = packet.ToBytes();

            try
            {
                StateObject state = new StateObject(buffer.Length) { workSocket = client.socket};
                client.socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, state);
#if DEBUG
                if (packet.commandType != CommandType.UpdatePlayerInfo)
                    Logger.Instance.Log($"Sent {packet.commandType} to {client.playerInfo.playerName}");
#endif
            }catch(IOException e)
            {
#if DEBUG
                Logger.Instance.Log($"Lost connection to the {client.playerInfo.playerName}: {e}");
#else
                Logger.Instance.Log($"Lost connection to the {client.playerInfo.playerName}.");
#endif
                LostConnection?.Invoke(client);
            }
            
        }

        private static void SendCallback(IAsyncResult ar)
        {
            StateObject recState = (StateObject)ar.AsyncState;
            Socket client = recState.workSocket;

            try
            {
                SocketError error;
                client.EndSend(ar, out error);

                if (error != SocketError.Success)
                {
                    Logger.Instance.Warning("Socket error occurred! " + error);
                }
            }catch (Exception e)
            {
                Logger.Instance.Warning("Socket exception occurred! " + e);
            }
        }
    }
}
