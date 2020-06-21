///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod
using static com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue.JsonEndValueForgeUtil; // handleNumberException

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
	public class JsonEndValueForgeDecimal : JsonEndValueForge
	{
		public readonly static JsonEndValueForgeDecimal INSTANCE = new JsonEndValueForgeDecimal();

		private JsonEndValueForgeDecimal()
		{
		}

		public CodegenExpression CaptureValue(
			JsonEndValueRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			return StaticMethod(typeof(JsonEndValueForgeDecimal), "JsonToDecimal", refs.ValueString, refs.Name);
		}

		public static decimal? JsonToDecimal(
			string value,
			string name)
		{
			if (value == null)
				return null;
			return JsonToDecimalNonNull(value, name);
		}

		public static decimal JsonToDecimalNonNull(
			string stringValue,
			string name)
		{
			try {
				return Decimal.Parse(stringValue);
			}
			catch (FormatException ex) {
				throw HandleNumberException(name, typeof(decimal), stringValue, ex);
			}
		}
	}
} // end of namespace
