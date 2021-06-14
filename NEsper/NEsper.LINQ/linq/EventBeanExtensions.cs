///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client.linq
{
    public static class EventBeanExtensions
    {
        public static sbyte? GetSByte(this EventBean eventBean, string name)
        {
            return (sbyte?) eventBean.Get(name);
        }

        public static sbyte GetSByte(this EventBean eventBean, string name, sbyte defaultValue)
        {
            return GetSByte(eventBean, name) ?? defaultValue;
        }

        public static byte? GetByte(this EventBean eventBean, string name)
        {
            return (byte?) eventBean.Get(name);
        }

        public static byte GetByte(this EventBean eventBean, string name, byte defaultValue)
        {
            return GetByte(eventBean, name) ?? defaultValue;
        }

        public static char? GetChar(this EventBean eventBean, string name)
        {
            return (char?) eventBean.Get(name);
        }

        public static char GetChar(this EventBean eventBean, string name, char defaultValue)
        {
            return GetChar(eventBean, name) ?? defaultValue;
        }

        public static short? GetInt16(this EventBean eventBean, string name)
        {
            return (short?) eventBean.Get(name);
        }

        public static short GetInt16(this EventBean eventBean, string name, short defaultValue)
        {
            return GetInt16(eventBean, name) ?? defaultValue;
        }

        public static int? GetInt32(this EventBean eventBean, string name)
        {
            return (int?) eventBean.Get(name);
        }

        public static int GetInt32(this EventBean eventBean, string name, int defaultValue)
        {
            return GetInt32(eventBean, name) ?? defaultValue;
        }

        public static long? GetInt64(this EventBean eventBean, string name)
        {
            return (long?) eventBean.Get(name);
        }

        public static long GetInt64(this EventBean eventBean, string name, long defaultValue)
        {
            return GetInt64(eventBean, name) ?? defaultValue;
        }

        public static ushort? GetUInt16(this EventBean eventBean, string name)
        {
            return (ushort?) eventBean.Get(name);
        }

        public static ushort GetUInt16(this EventBean eventBean, string name, ushort defaultValue)
        {
            return GetUInt16(eventBean, name) ?? defaultValue;
        }

        public static uint? GetUInt32(this EventBean eventBean, string name)
        {
            return (uint?) eventBean.Get(name);
        }

        public static uint GetUInt32(this EventBean eventBean, string name, uint defaultValue)
        {
            return GetUInt32(eventBean, name) ?? defaultValue;
        }

        public static ulong? GetUInt64(this EventBean eventBean, string name)
        {
            return (ulong?) eventBean.Get(name);
        }

        public static ulong GetUInt64(this EventBean eventBean, string name, ulong defaultValue)
        {
            return GetUInt64(eventBean, name) ?? defaultValue;
        }

        public static float? GetFloat(this EventBean eventBean, string name)
        {
            return (float?) eventBean.Get(name);
        }

        public static float GetFloat(this EventBean eventBean, string name, float defaultValue)
        {
            return GetFloat(eventBean, name) ?? defaultValue;
        }

        public static double? GetDouble(this EventBean eventBean, string name)
        {
            return (double?) eventBean.Get(name);
        }

        public static double GetDouble(this EventBean eventBean, string name, double defaultValue)
        {
            return GetDouble(eventBean, name) ?? defaultValue;
        }

        public static decimal? GetDecimal(this EventBean eventBean, string name)
        {
            return (decimal?) eventBean.Get(name);
        }

        public static decimal GetDecimal(this EventBean eventBean, string name, decimal defaultValue)
        {
            return GetDecimal(eventBean, name) ?? defaultValue;
        }

        public static String GetString(this EventBean eventBean, string name)
        {
            return (String) eventBean.Get(name);
        }

        public static DateTime? GetDateTime(this EventBean eventBean, string name)
        {
            return (DateTime?) eventBean.Get(name);
        }

        public static DateTime GetDateTime(this EventBean eventBean, string name, DateTime defaultValue)
        {
            return GetDateTime(eventBean, name) ?? defaultValue;
        }

        public static Guid? GetGuid(this EventBean eventBean, string name)
        {
            return (Guid?) eventBean.Get(name);
        }

        public static Guid GetGuid(this EventBean eventBean, string name, Guid defaultValue)
        {
            return GetGuid(eventBean, name) ?? defaultValue;
        }
    }
}