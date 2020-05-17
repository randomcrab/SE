﻿using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core.Exceptions;
using SE.Engine.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.Engine.Networking
{
    public static class NetData
    {
        /// <summary>Dictionary used to read an object from a NetIncomingMessage. The byte key identifies which Type the object is.</summary>
        internal static Dictionary<Type, Func<NetPacketReader, object>> DataReaders = new Dictionary<Type, Func<NetPacketReader, object>> {
            {typeof(long), message => message.GetLong()},
            {typeof(ulong), message => message.GetULong()},
            {typeof(int), message => message.GetInt()},
            {typeof(uint), message => message.GetUInt()},
            {typeof(short), message => message.GetShort()},
            {typeof(ushort), message => message.GetUShort()},
            {typeof(bool), message => message.GetBool()},
            {typeof(byte), message => message.GetByte()},
            {typeof(float), message => message.GetFloat()},
            {typeof(double), message => message.GetDouble()},
            {typeof(string), message => message.GetString()},
            {typeof(long[]), message => message.GetLongArray()},
            {typeof(ulong[]), message => message.GetULongArray()},
            {typeof(int[]), message => message.GetIntArray()},
            {typeof(uint[]), message => message.GetUIntArray()},
            {typeof(short[]), message => message.GetShortArray()},
            {typeof(ushort[]), message => message.GetUShortArray()},
            {typeof(bool[]), message => message.GetBoolArray()},
            {typeof(byte[]), message => message.GetBytesWithLength()},
            {typeof(float[]), message => message.GetFloatArray()},
            {typeof(double[]), message => message.GetDoubleArray()},
            {typeof(string[]), message => message.GetStringArray()},
            {typeof(Vector2), message => new Vector2(message.GetFloat(), message.GetFloat())}
        };

        /// <summary>Dictionary used to write an object to a NetIncomingMessage. The byte key identifies which Type the object is.</summary>
        internal static Dictionary<Type, Action<object, NetDataWriter>> DataWriters = new Dictionary<Type, Action<object, NetDataWriter>> {
            {typeof(long),  (obj, message) => message.Put((long)obj)},
            {typeof(ulong),  (obj, message) => message.Put((ulong)obj)},
            {typeof(int),  (obj, message) => message.Put((int)obj)},
            {typeof(uint),  (obj, message) => message.Put((uint)obj)},
            {typeof(short),  (obj, message) => message.Put((short)obj)},
            {typeof(ushort),  (obj, message) => message.Put((ushort)obj)},
            {typeof(bool),  (obj, message) => message.Put((bool)obj)},
            {typeof(byte),  (obj, message) => message.Put((byte)obj)},
            {typeof(float),  (obj, message) => message.Put((float)obj)},
            {typeof(double),  (obj, message) => message.Put((double)obj)},
            {typeof(string),  (obj, message) => message.Put(obj as string)},
            {typeof(long[]), (obj, message) => message.PutArray(obj as long[]) },
            {typeof(ulong[]), (obj, message) => message.PutArray(obj as ulong[]) },
            {typeof(int[]), (obj, message) => message.PutArray(obj as int[]) },
            {typeof(uint[]), (obj, message) => message.PutArray(obj as uint[]) },
            {typeof(short[]), (obj, message) => message.PutArray(obj as short[]) },
            {typeof(ushort[]), (obj, message) => message.PutArray(obj as ushort[]) },
            {typeof(bool[]), (obj, message) => message.PutArray(obj as bool[]) },
            {typeof(byte[]), (obj, message) => message.PutBytesWithLength(obj as byte[]) },
            {typeof(float[]), (obj, message) => message.PutArray(obj as float[]) },
            {typeof(double[]), (obj, message) => message.PutArray(obj as double[]) },
            {typeof(string[]), (obj, message) => message.PutArray(obj as string[]) },
            {typeof(Vector2), (obj, message) => {
                Vector2 vector = (Vector2) obj;
                message.Put(vector.X);
                message.Put(vector.Y);
            }}
        };

        public static void AddDataType(Type type, Func<NetPacketReader, object> readFunction, Action<object, NetDataWriter> writeFunction)
        {
            DataReaders.Add(type, readFunction);
            DataWriters.Add(type, writeFunction);
        }

        /// <summary>
        /// Reads serialized data from a NetPacketReader, and returns the result.
        /// </summary>
        /// <param name="type">Type of the object to be read.</param>
        /// <param name="reader">NetPacketReader the data will be read from.</param>
        /// <returns>A deserialized object obtained from the NetPacketReader.</returns>
        public static object Read(Type type, NetPacketReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (!DataReaders.TryGetValue(type, out var func))
                throw new NullReferenceException("No data reader found for type " + type + ".");
            
            return func.Invoke(reader);
        }

        /// <summary>
        /// Serializes an object and writes the result to a NetDataWriter.
        /// </summary>
        /// <param name="type">Type of the object.</param>
        /// <param name="obj">Object to be written.</param>
        /// <param name="writer">NetDataWriter the object will be serialized into.</param>
        public static void Write(Type type, object obj, NetDataWriter writer)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (!DataWriters.TryGetValue(type, out var func))
                throw new NullReferenceException("No data writer found for type " + type + ".");
            
            func.Invoke(obj, writer);
        }
    }
}
