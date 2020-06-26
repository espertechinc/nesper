///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class VMExtendedBuiltinSerdeFactory {
	    public static DataInputOutputSerde<object> GetSerde(Type type) {
	        if (type == typeof(BigInteger)) {
	            return DIOBigIntegerSerde.INSTANCE;
	        }
	        if (type == typeof(decimal)) {
	            return DIODecimalSerde.INSTANCE;
	        }
	        if (type == typeof(DateTime)) {
	            return DIODateTimeSerde.INSTANCE;
	        }
	        if (type == typeof(DateTimeOffset)) {
	            return DIODateTimeOffsetSerde.INSTANCE;
	        }
	        if (type == typeof(DateTimeEx)) {
	            return DIODateTimeExSerde.INSTANCE;
	        }
	        if (type.IsArray) {
	            var componentType = type.GetElementType();
	            if (componentType == typeof(int)) {
	                return DIOPrimitiveIntArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(bool)) {
	                return DIOPrimitiveBooleanArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(char)) {
	                return DIOPrimitiveCharArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(byte)) {
	                return DIOPrimitiveByteArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(short)) {
	                return DIOPrimitiveShortArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(long)) {
	                return DIOPrimitiveLongArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(float)) {
	                return DIOPrimitiveFloatArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(double)) {
	                return DIOPrimitiveDoubleArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(decimal)) {
		            return DIOPrimitiveDecimalArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(string)) {
	                return DIOStringArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(char?)) {
	                return DIOBoxedCharacterArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(bool?)) {
	                return DIOBoxedBooleanArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(byte?)) {
	                return DIOBoxedByteArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(short?)) {
	                return DIOBoxedShortArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(int?)) {
	                return DIOBoxedIntegerArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(long?)) {
	                return DIOBoxedLongArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(float?)) {
	                return DIOBoxedFloatArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(double?)) {
	                return DIOBoxedDoubleArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(decimal?)) {
	                return DIOBoxedDecimalArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(BigInteger)) {
	                return DIOBigIntegerArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(DateTime)) {
	                return DIODateTimeArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(DateTimeOffset)) {
	                return DIODateTimeOffsetArrayNullableSerde.INSTANCE;
	            }
	            if (componentType == typeof(DateTimeEx)) {
	                return DIODateTimeExArrayNullableSerde.INSTANCE;
	            }
	        }
	        return null;
	    }
	}
} // end of namespace
