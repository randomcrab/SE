﻿using System;
using System.Collections.Generic;
using LiteNetLib;
using SE.Core.Extensions;
using SE.Engine.Networking.Attributes;
using SE.Utility;
using static SE.Core.Network;

namespace SE.Engine.Networking
{
    public class Instantiator : INetworkExtension
    {
        private RPCMethod instantiateMethod;
        private RPCMethod destroyMethod;

        private static readonly SelfReferenceDictionary<string, Type> spawnables = new SelfReferenceDictionary<string, Type>();
        private static readonly List<uint> clearBuffer = new List<uint>();

        public uint ID { get; private set; }
        public bool IsSetup { get; private set; }
        public bool IsOwner { get; private set; }

        // Cache...
        private List<string> netStates = new List<string>();
        private List<uint> netIDs = new List<uint>();

        public void Setup(uint id, bool isOwner)
        {
            ID = id;
            IsOwner = isOwner;
            IsSetup = true;
        }

        public void Update()
        {
            for (int i = 0; i < clearBuffer.Count; i++) {
                clearBuffer.RemoveAt(i);
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
            lock (SpawnedNetObjects) {
                foreach (uint i in SpawnedNetObjects.Keys) {
                    InstantiateFromBuffer(SpawnedNetObjects[i], peer, i);
                }
            }
        }

        internal bool CleanNetObject(SpawnedNetObject netObj)
        {
            lock (SpawnedNetObjects) {
                SpawnedNetObjects.Remove(netObj.NetworkID);
            }
            if (netObj.NetLogic is INetInstantiatable instantiatable)
                instantiatable.NetClean();

            return true;
        }

        internal void InstantiateFromBuffer(SpawnedNetObject netObj, NetPeer conn, uint netID, bool isOwner = false)
        {
            if (InstanceType != NetInstanceType.Server)
                return;

            byte[] data = null;
            byte[] netState = null;
            string instantiateParams = null;
            if (netObj.NetLogic is INetInstantiatable instantiatable) {
                data = instantiatable.GetBufferedData();
                instantiateParams = instantiatable.InstantiateParameters?.Serialize();
            }
            if (netObj.NetLogic is INetPersistable persist)
                netState = persist.SerializeNetworkState();
            
            SendRPC(instantiateMethod, conn, netObj.SpawnableID, conn.GetUniqueID() == netObj.Owner, netObj.NetworkID, netState ?? new byte[0], data ?? new byte[0], instantiateParams ?? "");
        }

        public void Instantiate(string type, string owner = "SERVER", object[] parameters = null)
        {
            try {
                if (InstanceType != NetInstanceType.Server)
                    throw new InvalidOperationException("Attempted to instantiate on a client. Instantiate can only be called on the server.");
                if (!spawnables.TryGetValue(type, out Type t))
                    throw new Exception("No Spawnable of Type " + type + " found in dictionary.");

                object obj = parameters != null
                    ? Activator.CreateInstance(t, parameters)
                    : Activator.CreateInstance(t);

                INetLogic logic = null;
                if (obj is INetLogicProxy proxy)
                    logic = proxy.NetLogic;
                else if (obj is INetLogic netLogic)
                    logic = netLogic;

                if (logic == null)
                    throw new Exception("NetLogic not found.");

                SetupNetLogic(logic, owner == "SERVER");
                lock (SpawnedNetObjects) {
                    SpawnedNetObjects.Add(logic.ID, new SpawnedNetObject(logic, logic.ID, type, owner));
                }

                byte[] returned = null;
                byte[] netState = null;
                string paramsData = parameters?.Serialize();
                if (logic is INetInstantiatable instantiatable) {
                    instantiatable.OnNetworkInstantiatedServer(type, owner);
                    returned = instantiatable.GetBufferedData();
                }
                if (logic is INetPersistable persist)
                    netState = persist.SerializeNetworkState();

                foreach (string playerID in Connections.Keys) {
                    SendRPC(instantiateMethod, Connections[playerID], type, playerID == owner, logic.ID, netState ?? new byte[0], returned ?? new byte[0], paramsData ?? "");
                }
            } catch (Exception e) {
                LogError(null, new Exception("OOF!" ,e));
            }
        }

        [ClientRPC(frequent: true)]
        public void Instantiate_CLIENT(string type, bool isOwner, uint netID, byte[] netState, byte[] data, string paramsData)
        {
            if (!spawnables.TryGetValue(type, out Type t))
                throw new Exception("No Spawnable of Type " + type + " found in dictionary.");

            object[] instantiateParameters = null;
            if (!string.IsNullOrEmpty(paramsData))
                instantiateParameters = paramsData.Deserialize<object[]>();

            // TODO: Temporary fix to prevent error where JSON deserializes floats to doubles.
            for (int i = 0; i < instantiateParameters.Length; i++) {
                if (instantiateParameters[i] is double) {
                    instantiateParameters[i] = Convert.ToSingle(instantiateParameters[i]);
                }
            }

            object obj = instantiateParameters != null
                ? Activator.CreateInstance(t, instantiateParameters)
                : Activator.CreateInstance(t);

            INetLogic logic = null;
            if (obj is INetLogicProxy proxy)
                logic = proxy.NetLogic;
            else if (obj is INetLogic netLogic)
                logic = netLogic;

            if (logic == null)
                throw new Exception("NetLogic not found.");

            SetupNetLogic(logic, netID, isOwner, netState);
            lock (SpawnedNetObjects) {
                SpawnedNetObjects.Add(logic.ID, new SpawnedNetObject(logic, logic.ID, type));
            }

            if (logic is INetInstantiatable instantiatable)
                instantiatable.OnNetworkInstantiatedClient(type, isOwner, data);
        }

        public void Destroy(uint netID)
        {
            try {
                // For security, only the server is authorized to destroy networked GameObjects.
                if (InstanceType != NetInstanceType.Server)
                    throw new InvalidOperationException("Destroying a networked GameObject is only authorized for the server.");

                Destroy_CLIENT(netID);
                SendRPC(destroyMethod, netID);
            } catch (Exception e) {
                LogError(exception: e);
            }
        }

        [ClientRPC(frequent: true)]
        public void Destroy_CLIENT(uint netID)
        {
            lock (SpawnedNetObjects) {
                if (SpawnedNetObjects.TryGetValue(netID, out SpawnedNetObject netObj)) {
                    CleanNetObject(netObj);
                } else {
                    clearBuffer.Add(netID);
                }
            }
        }

        public bool AddSpawnable(string key, Type spawn)
        {
            if (spawnables.ContainsKey(key))
                throw new Exception("Attemped to add Type '" + spawn + "' with already existing key '" + key + "' to spawnables.");

            spawnables.Add(key, spawn);
            return true;
        }

        public static bool RemoveSpawnable(string key)
            => spawnables.Remove(key);

        internal Instantiator()
        {
            instantiateMethod = new RPCMethod(this, "Instantiate", DeliveryMethod.ReliableOrdered, 1, Scope.Unicast);
            destroyMethod = new RPCMethod(this, "Destroy", DeliveryMethod.ReliableOrdered, 1, Scope.Broadcast);
        }
    }
}
