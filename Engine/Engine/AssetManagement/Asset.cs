﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.AssetManagement.Processors;
using SE.Common;
using SE.Core;
using SE.Rendering;

namespace SE.AssetManagement
{
    public class Asset<T> : SEObject, IAsset, IAssetConsumer
    {
        public ulong AssetID { get; internal set; }
        public string ID { get; internal set; }
        public AssetConsumer AssetConsumer { get; }
        public HashSet<AssetConsumer> References { get; set; } = new HashSet<AssetConsumer>();
        public ContentLoader ContentLoader { get; }
        public uint LoadOrder { get; set; }
        public bool Loaded { get; set; }

        private IAssetProcessor processor;

        /// <value>Returns the inner resource of the asset.</value>
        internal T Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (!Loaded)
                    Load();

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => this.value = value;
        }
        private T value;

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        /// <param name="processor">Function called whenever the asset is reloaded.</param>
        /// <param name="contentLoader">Content loader the asset will be added to.</param>
        /// <param name="referencedAssets">Asset dependencies.</param>
        internal Asset(string id, AssetProcessor processor, ContentLoader contentLoader)
        {
            ID = id;
            LoadOrder = AssetManager.CurrentAssetPriority++;

            AssetConsumer = new AssetConsumer();
            ContentLoader = contentLoader;
            this.processor = processor;
            HashSet<IAsset> refAssets = processor.GetReferencedAssets();
            if(refAssets == null)
                return;

            foreach (IAsset refAsset in processor.GetReferencedAssets()) {
                refAsset?.AddReference(AssetConsumer);
            }
        }

        public IAsset AsIAsset() => this;

        internal T Get(IAssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == this)
                throw new InvalidOperationException("Attempted to retrieve self.");

            AddReference(consumer.AssetConsumer);
            return GetNoRef();
        }

        private T GetNoRef()
        {
            Load();
            return Value;
        }

        public void AddReference(AssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == AssetConsumer)
                return;

            References.Add(consumer);
            consumer.AddReference(this);
            if (References.Count > 0) {
                Load();
            }
        }

        public void RemoveReference(AssetConsumer consumer)
        {
            if (consumer == AssetConsumer)
                throw new NullReferenceException("The IAssetConsumer instance was null.");

            References.Remove(consumer);
            consumer.RemoveReference(this);
            if (References.Count < 1) {
                Unload();
            }
        }

        public void Load()
        {
            if(Loaded)
                return;

            value = (T) processor.Construct();
            AssetConsumer.ReferenceAssets();
            Loaded = true;
            ContentLoader.AddReference(this);
        }

        public void Unload()
        {
            if(!Loaded)
                return;

            DrawCallDatabase.PruneAsset(Value);
            Loaded = false;
            AssetConsumer.DereferenceAssets();
            ContentLoader.RemoveReference(this);
            Value = default;
        }

        public void Purge()
        {
            if (References.Count < 1) {
                Unload();
            }
        }
    }

    public interface IAsset
    {
        uint LoadOrder { get; set; } 
        bool Loaded { get; set; }
        HashSet<AssetConsumer> References { get; set; }

        void RemoveReference(AssetConsumer reference);
        void AddReference(AssetConsumer reference);
        void Load();
        void Unload();
        void Purge();
    }
}
