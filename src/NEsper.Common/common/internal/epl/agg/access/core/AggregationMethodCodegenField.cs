///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // LocalMethod

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
	public class AggregationMethodCodegenField : CodegenFieldSharable
	{
		private readonly AggregationMethodForge _readerForge;
		private readonly CodegenClassScope _classScope;
		private readonly Type _generator;

		public AggregationMethodCodegenField(
			AggregationMethodForge readerForge,
			CodegenClassScope classScope,
			Type generator)
		{
			_readerForge = readerForge;
			_classScope = classScope;
			_generator = generator;
		}

		public Type Type()
		{
			return typeof(AggregationMultiFunctionAggregationMethod);
		}

		public CodegenExpression InitCtorScoped()
		{
			var symbols = new SAIFFInitializeSymbol();
			var init = _classScope.NamespaceScope.InitMethod
				.MakeChildWithScope(typeof(AggregationMultiFunctionAggregationMethod), _generator, symbols, _classScope)
				.AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
			init.Block.MethodReturn(_readerForge.CodegenCreateReader(init, symbols, _classScope));
			return LocalMethod(init, EPStatementInitServicesConstants.REF);
		}

		protected bool Equals(AggregationMethodCodegenField other)
		{
			return Equals(_readerForge, other._readerForge);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			if (ReferenceEquals(this, obj)) {
				return true;
			}

			if (obj.GetType() != GetType()) {
				return false;
			}

			return Equals((AggregationMethodCodegenField) obj);
		}

		public override int GetHashCode()
		{
			return (_readerForge != null ? _readerForge.GetHashCode() : 0);
		}
	}
} // end of namespace
