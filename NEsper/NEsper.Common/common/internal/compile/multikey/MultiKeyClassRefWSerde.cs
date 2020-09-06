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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.compile.multikey
{
	public class MultiKeyClassRefWSerde : MultiKeyClassRef
	{
		private readonly DataInputOutputSerdeForge forge;
		private readonly Type[] types;

		public MultiKeyClassRefWSerde(
			DataInputOutputSerdeForge forge,
			Type[] types)
		{
			this.forge = forge;
			this.types = types;
		}

		public NameOrType ClassNameMK => null;

		public Type[] MKTypes => types;

		public CodegenExpression GetExprMKSerde(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			return forge.Codegen(method, classScope, null);
		}
	}
} // end of namespace
