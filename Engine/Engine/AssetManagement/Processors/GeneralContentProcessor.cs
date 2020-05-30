﻿using System;
using System.Collections.Generic;
using System.Text;
using SE.AssetManagement;
using SE.Core;

namespace SE.Engine.AssetManagement.Processors
{
    public class GeneralContentProcessor<T> : IAssetProcessor<T>
    {
        private string contentLoaderID;
        private string assetID;

        public T Construct()
        {
            ContentLoader loader = AssetManager.GetLoader(contentLoaderID);
            if (loader == null)
                throw new NullReferenceException();

            return loader.Load<T>(assetID);
        }

        object IAssetProcessor.Construct()
        {
            return Construct();
        }

        public GeneralContentProcessor(string contentLoaderID, string assetID)
        {
            this.contentLoaderID = contentLoaderID;
            this.assetID = assetID;
        }

        public GeneralContentProcessor() { }
    }
}
