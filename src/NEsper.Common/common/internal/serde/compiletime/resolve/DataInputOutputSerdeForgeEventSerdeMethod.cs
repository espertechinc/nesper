///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public enum DataInputOutputSerdeForgeEventSerdeMethod
    {
        NULLABLEEVENT,
        NULLABLEEVENTARRAY,
        NULLABLEEVENTORUNDERLYING,
        NULLABLEEVENTARRAYORUNDERLYING
    }

    public static class DataInputOutputSerdeForgeEventSerdeMethodExtensions
    {
        public static string GetMethodName(this DataInputOutputSerdeForgeEventSerdeMethod value)
        {
            return value switch {
                DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENT => "NullableEvent",
                DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENTARRAY => "NullableEventArray",
                DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENTORUNDERLYING => "NullableEventOrUnderlying",
                DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENTARRAYORUNDERLYING => "NullableEventArrayOrUnderlying",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
} // end of namespace