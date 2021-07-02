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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class DataInputOutputSerdeForgeParameterized : DataInputOutputSerdeForge
	{
		private readonly string _dioClassName;
		private readonly Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] _functions;

		public DataInputOutputSerdeForgeParameterized(
			string dioClassName,
			params Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions)
		{
			this._dioClassName = dioClassName;
			this._functions = functions;
		}

		public string ForgeClassName()
		{
			return _dioClassName;
		}

		public CodegenExpression Codegen(
			CodegenMethod method,
			CodegenClassScope classScope,
			CodegenExpression optionalEventTypeResolver)
		{
			CodegenExpression[] @params = new CodegenExpression[_functions.Length];
			DataInputOutputSerdeForgeParameterizedVars vars = new DataInputOutputSerdeForgeParameterizedVars(method, classScope, optionalEventTypeResolver);
			for (int i = 0; i < @params.Length; i++) {
				@params[i] = _functions[i].Invoke(vars);
			}

			return NewInstanceNamed(_dioClassName, @params);
		}
	}
} // end of namespace
