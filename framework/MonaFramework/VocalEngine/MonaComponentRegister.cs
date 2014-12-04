using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonaFramework.Interfaces;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace MonaFramework.VocalEngine
{
    public sealed class MonaComponentRegister
    {
        //TODO: voir si on permet plusieurs composants pour un meme alias
        //private Dictionary<string, IAliased> components = new Dictionary<string, IAliased>();
        private Dictionary<string, IAliasAnswerer> components = new Dictionary<string, IAliasAnswerer>();
        private TcpClient socket;
        private bool active = false;

        public MonaComponentRegister()
        {

        }

        public MonaComponentRegister(TcpClient s)
        {
            socket = s;
        }

        public void startNotify()
        {
            active = true;
        }

        public void stopNotify()
        {
            active = false;
        }

        //public void registerComponent(IAliased comp)
        public void registerComponent(IAliasAnswerer comp)
        {
            registerComponent(comp.getAlias(), comp);
        }

        //public void registerComponent(string alias, IAliased comp)
        public void registerComponent(string alias, IAliasAnswerer comp)
        {
            if (alias != null && alias.Length > 0)
            {
                if (components.ContainsKey(comp.getAlias()))
                {
                    components[comp.getAlias()] = comp;
                }
                else
                {
                    components.Add(alias, comp);
                }

                if (socket != null && active)
                {
                    notifyAliasAdd(alias);
                }
            }
        }

        //public void unregisterComponent(IAliased comp)
        public void unregisterComponent(IAliasAnswerer comp)
        {
            try
            {
                unregisterComponent(comp.getAlias());
            }
            catch (Exception e)
            {

            }
        }

        //public void unregisterComponent(string alias)
        public void unregisterComponent(string alias)
        {
            if (alias != null && alias.Length > 0)
            {
                components.Remove(alias);

                if (socket != null && active)
                {
                    notifyAliasRemove(alias);
                }
            }
        }

        private void notifyAliasAdd(string alias)
        {
            NetworkStream stream = socket.GetStream();
            byte[] aliasData = Encoding.UTF8.GetBytes(alias);

            stream.Write(Encoding.UTF8.GetBytes("ADD"), 0, 3);
            stream.WriteByte((byte)aliasData.Length);
            stream.Write(aliasData, 0, aliasData.Length);
            stream.Flush();
        }

        private void notifyAliasRemove(string alias)
        {
            NetworkStream stream = socket.GetStream();
            byte[] aliasData = Encoding.UTF8.GetBytes(alias);

            stream.Write(Encoding.UTF8.GetBytes("RMV"), 0, 3);
            stream.WriteByte((byte)aliasData.Length);
            stream.Write(aliasData, 0, aliasData.Length);
            stream.Flush();
        }

        public IAliasAnswerer getComponent(string alias)
        {
            return components[alias];
        }

        public List<IAliasAnswerer> getAllComponents()
        {
            return components.Values.ToList();
        }
    }
}
