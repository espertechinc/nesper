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
using com.espertech.esper.common.@internal.@event.json.parser.forge;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
	public class JsonDelegateForgeByClass : JsonDelegateForge
	{
		private readonly Type _clazz;
		private readonly CodegenExpression[] _parameters;

		public JsonDelegateForgeByClass(Type clazz)
		{
			this._clazz = clazz;
			this._parameters = new CodegenExpression[0];
		}

		public JsonDelegateForgeByClass(
			Type clazz,
			params CodegenExpression[] @params)
		{
			this._clazz = clazz;
			this._parameters = @params;
		}

		public CodegenExpression NewDelegate(
			JsonDelegateRefs fields,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			CodegenExpression[] allParams = new CodegenExpression[2 + _parameters.Length];
			allParams[0] = fields.BaseHandler;
			allParams[1] = fields.This;
			Array.Copy(_parameters, 0, allParams, 2, _parameters.Length);
			return NewInstance(_clazz, allParams);
		}
	}
} // end of namespace
