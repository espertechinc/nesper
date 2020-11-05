///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public static partial class Comparers
    {
        public static IComparer<T> Default<T>()
        {
            return DefaultComparer<T>.Instance;
        }

        public class DefaultComparer<T> : IComparer<T>
        {
            public static readonly DefaultComparer<T> Instance = new DefaultComparer<T>();
            
            public static bool IsInteger(Type type)
            {
                return type == typeof(Int64) ||
                       type == typeof(Int32) ||
                       type == typeof(Int16) ||
                       type == typeof(SByte) ||
                       type == typeof(Byte);
            }
            
            public static bool IsFloatingPoint(Type type)
            {
                return type == typeof(Double) ||
                       type == typeof(Single);
            }

            public static bool IsDecimal(Type type)
            {
                return type == typeof(Decimal);
            }

            public int Compare(
                T x,
                T y)
            {
                if ((x == null) && (y == null)) {
                    return 0;
                } else if (x == null) {
                    return 1;
                } else if (y == null) {
                    return -1;
                }

                var xType = x.GetType();
                var yType = y.GetType();
                if (xType != yType) {
                    // We can handle arithmetic types that can be coerced into each other without loss.
                    if (IsInteger(xType) && IsInteger(yType)) {
                        return x.AsInt64().CompareTo(y.AsInt64());
                    } else if (IsFloatingPoint(xType) && IsFloatingPoint(yType)) {
                        return x.AsDouble().CompareTo(y.AsDouble());
                    }
                    
                    // Everything else is a fall-through
                }
                
                return ((IComparable) x).CompareTo(y);
            }
        }
    }
}