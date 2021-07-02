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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; //.*

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class DataInputOutputSerdeForgeSingleton : DataInputOutputSerdeForge
	{
		private readonly Type _serdeClass;

		public DataInputOutputSerdeForgeSingleton(Type serdeClass)
		{
			this._serdeClass = serdeClass;
		}

		public CodegenExpression Codegen(
			CodegenMethod method,
			CodegenClassScope classScope,
			CodegenExpression optionalEventTypeResolver)
		{
			return PublicConstValue(_serdeClass, "INSTANCE");
		}

		public string ForgeClassName()
		{
			return _serdeClass.Name;
		}
	}
} // end of namespace
