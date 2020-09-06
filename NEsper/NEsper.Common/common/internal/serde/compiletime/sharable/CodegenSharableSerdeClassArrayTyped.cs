///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.compiletime.sharable
{
	public class CodegenSharableSerdeClassArrayTyped : CodegenFieldSharable
	{
		private readonly CodegenSharableSerdeName _name;
		private readonly Type[] _valueTypes;
		private readonly DataInputOutputSerdeForge[] _serdes;
		private readonly CodegenClassScope _classScope;

		public CodegenSharableSerdeClassArrayTyped(
			CodegenSharableSerdeName name,
			Type[] valueTypes,
			DataInputOutputSerdeForge[] serdes,
			CodegenClassScope classScope)
		{
			_name = name;
			_valueTypes = valueTypes;
			_serdes = serdes;
			_classScope = classScope;
		}

		public Type Type()
		{
			return typeof(DataInputOutputSerde);
		}

		public CodegenExpression InitCtorScoped()
		{
			return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
				.Get(EPStatementInitServicesConstants.EVENTTYPERESOLVER)
				.Add(EventTypeResolverConstants.GETEVENTSERDEFACTORY)
				.Add(
					_name.MethodName,
					DataInputOutputSerdeForgeExtensions.CodegenArray(
						_serdes,
						_classScope.NamespaceScope.InitMethod,
						_classScope,
						null));
		}

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (o == null || GetType() != o.GetType()) return false;

			CodegenSharableSerdeClassArrayTyped that = (CodegenSharableSerdeClassArrayTyped) o;

			if (_name != that._name) return false;
			// Probably incorrect - comparing Object[] arrays with Arrays.equals
			return Arrays.AreEqual(_valueTypes, that._valueTypes);
		}

		public override int GetHashCode()
		{
			int result = _name.GetHashCode();
			result = 31 * result + CompatExtensions.HashAll(_valueTypes);
			return result;
		}


		public class CodegenSharableSerdeName
		{
			public static readonly CodegenSharableSerdeName OBJECTARRAYMAYNULLNULL =
				new CodegenSharableSerdeName("ObjectArrayMayNullNull");

			public string MethodName { get; }
			CodegenSharableSerdeName(string methodName)
			{
				MethodName = methodName;
			}
		}
	}
} // end of namespace
