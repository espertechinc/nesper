///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.Numerics;

using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.compat.util
{
    public class SimpleTypeParserFunctions
    {
        public static char ParseChar(string value)
        {
            return value[0];
        }

        public static bool ParseBoolean(string value)
        {
            value = value.Trim();
            return bool.Parse(value);
        }

        public static Guid ParseGuid(string value)
        {
            return new Guid(value.Trim());
        }

        public static byte ParseByte(string value)
        {
            value = value.Trim();
            if (value.StartsWith("0x")) {
                return byte.Parse(value.Substring(2), NumberStyles.HexNumber);
            }

            return byte.Parse(value);
        }

        public static sbyte ParseSByte(string value)
        {
            value = value.TrimEnd(' ');
            return sbyte.Parse(value);
        }

        public static short ParseInt16(string value)
        {
            value = value.TrimEnd(' ');
            return short.Parse(value);
        }

        public static ushort ParseUInt16(string value)
        {
            value = value.TrimEnd(' ');
            return ushort.Parse(value);
        }

        public static int ParseInt32(string value)
        {
            value = value.TrimEnd(' ');
            return int.Parse(value);
        }

        public static uint ParseUInt32(string value)
        {
            value = value.TrimEnd(' ');
            return uint.Parse(value);
        }

        public static long ParseInt64(string value)
        {
            value = value.TrimEnd('l', 'L', ' ');
            return long.Parse(value);
        }

        public static ulong ParseUInt64(string value)
        {
            value = value.TrimEnd('l', 'L', ' ');
            return ulong.Parse(value);
        }

        public static float ParseFloat(string value)
        {
            value = value.TrimEnd('f', 'F', ' ');
            return float.Parse(value);
        }

        public static double ParseDouble(string value)
        {
            value = value.TrimEnd('f', 'F', 'd', 'D', ' ');
            return double.Parse(value);
        }

        public static decimal ParseDecimal(string value)
        {
            value = value.TrimEnd('f', 'F', 'd', 'D', 'M', 'm', ' ');
            return decimal.Parse(value);
        }

        public static object ParseEnum(
            string value,
            Type type)
        {
            return Enum.Parse(type, value, true);
        }

        public static BigInteger ParseBigInteger(string value)
        {
            value = value.TrimEnd();
            return BigInteger.Parse(value);
        }

        public static DateTime ParseDateTime(string value)
        {
            value = value.TrimEnd();
            return DateTimeParsingFunctions.ParseDefault(value).DateTime;
        }

        public static DateTimeOffset ParseDateTimeOffset(string value)
        {
            value = value.TrimEnd();
            return DateTimeParsingFunctions.ParseDefault(value);
        }
    }
}