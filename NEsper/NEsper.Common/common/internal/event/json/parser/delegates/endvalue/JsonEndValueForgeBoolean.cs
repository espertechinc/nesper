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

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
	public class JsonEndValueForgeBoolean : JsonEndValueForge
	{
		public readonly static JsonEndValueForgeBoolean INSTANCE = new JsonEndValueForgeBoolean();

		private JsonEndValueForgeBoolean()
		{
		}

		public CodegenExpression CaptureValue(
			JsonEndValueRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			return StaticMethod(typeof(JsonEndValueForgeBoolean), "JsonToBoolean", refs.ValueObject, refs.ValueString, refs.Name);
		}

		public static bool? JsonToBoolean(
			object objectValue,
			string stringValue,
			string name)
		{
			if (objectValue != null) {
				return (Boolean) objectValue;
			}

			switch (stringValue) {
				case null:
					return null;

				case "true":
					return true;

				case "false":
					return false;

				default:
					throw JsonEndValueForgeUtil.HandleBooleanException(name, stringValue);
			}
		}
	}
} // end of namespace
