﻿using System;
using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core;
using SE.Engine.Networking;
using SE.Engine.Networking.Attributes;
using SE.Networking.Types;
using SE.Core.Extensions;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components.Network
{

    /// <summary>
    /// Networked transform component. Syncs an objects position over the network via sending velocity and position data.
    /// </summary>
    public class NetTransform : NetComponent, INetPersistable
    {
        private PhysicsObject physObj;

        private Vector2 curVelocity;
        private Vector2 oldVelocity;
        private Vector2 curPosition;
        private Vector2? snapPosition;

        private float curRotation;
        private float oldRotation;

        private float snapTime;
        private float snapMaxTime = 0.1f;

        private RPCMethod updateVelocityMethod;
        private RPCMethod updateVelocityMethodReliable;

        /// <summary>Latency compensation quality. Higher quality means higher network bandwidth.</summary>
        public CompensationQuality Quality = CompensationQuality.Low;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            base.OnInitialize();
            updateVelocityMethod = new RPCMethod(this, "UpdateVelocity");
            updateVelocityMethodReliable = new RPCMethod(this, "UpdateVelocity", DeliveryMethod.ReliableSequenced);
        }

        /// <inheritdoc />
        public override void OnFixedUpdate()
        {
            base.OnUpdate();
            if (!IsSetup)
                return;

            if (!IsOwner) {
                TransformUpdate();
                return;
            }

            if(physObj == null)
                physObj = Owner.GetComponent<PhysicsObject>();

            curPosition = Owner.Transform.GlobalPositionInternal;
            curRotation = Owner.Transform.GlobalRotationInternal;
            curVelocity = physObj.Body.LinearVelocity;

            RPCMethod velocityMethod = Quality == CompensationQuality.High
                ? updateVelocityMethodReliable : updateVelocityMethod;

            bool changedRotation = MathF.Abs(curRotation - oldRotation) > 0.01f;
            bool changedVelocity = MathF.Abs(curVelocity.X - oldVelocity.X) > 1.0f || MathF.Abs(curVelocity.Y - oldVelocity.Y) > 1.0f;
            if (changedVelocity || changedRotation) {
                SE.Core.Network.SendRPC(velocityMethod, curPosition, curVelocity, curRotation);
            }
            oldVelocity = curVelocity;
            oldRotation = curRotation;
        }

        private void TransformUpdate()
        {
            if (snapPosition.HasValue) {
                Vector2 smoothing = (snapPosition.Value - Owner.Transform.GlobalPositionInternal) * (snapTime / snapMaxTime);
                Owner.Transform.Position = Owner.Transform.GlobalPositionInternal + curVelocity * Time.FixedTimestep + smoothing;

                snapTime += Time.FixedTimestep;
                if (snapTime > snapMaxTime) {
                    snapTime = 0;
                    snapPosition = null;
                }
            } else {
                Owner.Transform.Position = Owner.Transform.GlobalPositionInternal + curVelocity * Time.FixedTimestep;
            }
            Owner.Transform.GlobalRotationInternal = curRotation;
        }

        private void SendUpdateVelocityRPC(DeliveryMethod deliveryMethod)
        {
            Core.Network.SendRPC(updateVelocityMethod, curPosition, curVelocity, curRotation);
        }

        public byte[] SerializeNetworkState(NetDataWriter writer)
        {
            writer.Put(Owner.Transform.Position.X);
            writer.Put(Owner.Transform.Position.Y);
            writer.Put(curVelocity.X);
            writer.Put(curVelocity.Y);
            writer.Put(Owner.Transform.Scale.X);
            writer.Put(Owner.Transform.Scale.Y);
            writer.Put(Owner.Transform.Rotation);
            return writer.CopyData();
        }

        public void RestoreNetworkState(NetDataReader reader)
        {
            Owner.Transform.Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            curVelocity = new Vector2(reader.GetFloat(), reader.GetFloat());
            Owner.Transform.Scale = new Vector2(reader.GetFloat(), reader.GetFloat());
            Owner.Transform.Rotation = reader.GetFloat();
        }

        [ServerRPC]
        public void UpdateVelocitySnap(Vector2 position, Vector2 velocity)
        {
            Owner.Transform.Position = position;
            curVelocity = velocity;
        }

        [ClientRPC]
        public void UpdateVelocitySnap_CLIENT(Vector2 position, Vector2 velocity)
        {
            Owner.Transform.Position = position;
            curVelocity = velocity;
        }

        [ServerRPC]
        public void UpdateVelocity(Vector2 position, Vector2 velocity, float rotation)
        {
            Owner.Transform.Position = position;
            Owner.Transform.GlobalRotationInternal = rotation;
            curVelocity = velocity;
            curRotation = rotation;
        }

        [ClientRPC]
        public void UpdateVelocity_CLIENT(Vector2 position, Vector2 velocity, float rotation)
        {
            curVelocity = velocity;
            curRotation = rotation;
            float distance = Vector2.Distance(position, Owner.Transform.GlobalPositionInternal);
            if (distance >= 32f) {
                Owner.Transform.Position = position;
            } else if (distance >= 6f) {
                snapPosition = position;
                snapMaxTime = 0.05f;
                snapTime = 0f;
            }
        }

        public NetTransform(CompensationQuality quality = CompensationQuality.Low)
        {
            Quality = quality;
        }

        public NetTransform() { }


        public enum CompensationQuality
        {
            Low,
            High
        }
    }
}