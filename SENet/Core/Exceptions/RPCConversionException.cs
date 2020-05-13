﻿using System;

namespace DeeZ.Core.Exceptions
{
    public class RPCConversionException : Exception
    {
        public RPCConversionException(string message) : base(message) { }
        public RPCConversionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
