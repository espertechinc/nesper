///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Factory for casters, which take an object and safely cast to a given type, 
    /// performing coercion or dropping precision if required. 
    /// </summary>
    public class SimpleTypeCasterFactory
    {
        /// <summary>
        /// Returns a caster that casts to a target type.
        /// </summary>
        /// <param name="fromType">can be null, if not known</param>
        /// <param name="targetType">to cast to</param>
        /// <returns>
        /// caster for casting objects to the required type
        /// </returns>
        public static SimpleTypeCaster GetCaster(Type fromType, Type targetType)
        {
            if (fromType == targetType)
            {
                return x => x; // null cast
            }

            bool isUnused;
            return GetCaster(targetType, out isUnused);
        }

        /// <summary>
        /// Returns a caster that casts to a target type.
        /// </summary>
        /// <param name="targetType">to cast to</param>
        /// <returns>
        /// caster for casting objects to the required type
        /// </returns>
        public static SimpleTypeCaster GetCaster(Type targetType)
        {
            bool isUnused;
            return GetCaster(targetType, out isUnused);    
        }

        /// <summary>
        /// Returns a caster that casts to a target type.
        /// </summary>
        /// <param name="targetType">to cast to</param>
        /// <param name="isNumeric">if set to <c>true</c> [is numeric].</param>
        /// <returns>
        /// caster for casting objects to the required type
        /// </returns>
        public static SimpleTypeCaster GetCaster(Type targetType, out bool isNumeric)
        {
            isNumeric = true;

            Type baseType = Nullable.GetUnderlyingType(targetType);
            if (baseType != null)
            {
                targetType = baseType;
            }

            if (targetType == typeof(Int32))
            {
                return CastHelper.PrimitiveCastInt32;
            }
            if (targetType == typeof(Int64))
            {
                return CastHelper.PrimitiveCastInt64;
            }
            if (targetType == typeof(Int16))
            {
                return CastHelper.PrimitiveCastInt16;
            }
            if (targetType == typeof(SByte))
            {
                return CastHelper.PrimitiveCastSByte;
            }
            if (targetType == typeof(Single))
            {
                return CastHelper.PrimitiveCastSingle;
            }
            if (targetType == typeof(Double))
            {
                return CastHelper.PrimitiveCastDouble;
            }
            if (targetType == typeof(Decimal))
            {
                return CastHelper.PrimitiveCastDecimal;
            }
            if (targetType == typeof(UInt32))
            {
                return CastHelper.PrimitiveCastUInt32;
            }
            if (targetType == typeof(UInt64))
            {
                return CastHelper.PrimitiveCastUInt64;
            }
            if (targetType == typeof(UInt16))
            {
                return CastHelper.PrimitiveCastUInt16;
            }
            if (targetType == typeof(Char))
            {
                return CastHelper.PrimitiveCastChar;
            }
            if (targetType == typeof(Byte))
            {
                return CastHelper.PrimitiveCastByte;
            }

            isNumeric = false;

            if (targetType == typeof(bool?))
            {
                return sourceObj => sourceObj == null ? (object) null : Convert.ToBoolean(sourceObj);
            }

            if ( targetType == typeof(string) ) {
                return delegate(Object sourceObj)
                {
                    if (sourceObj == null)
                    {
                        return null;
                    }
                    else if (sourceObj is string)
                    {
                        return (string)sourceObj;
                    }
                    else
                    {
                        return sourceObj.ToString();
                    }
                };
            }

            return delegate(Object sourceObj) {
                       if (sourceObj == null) {
                           return null;
                       }
                       var sourceObjType = sourceObj.GetType();
                       return targetType.IsAssignableFrom(sourceObjType) ? sourceObj : null;
                   };
        }
    }
}
