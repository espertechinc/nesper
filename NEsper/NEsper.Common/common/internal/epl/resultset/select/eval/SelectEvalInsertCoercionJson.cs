///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
	public class SelectEvalInsertCoercionJson : SelectExprProcessorForge
	{
		private readonly JsonEventType source;
		private readonly JsonEventType target;

		public SelectEvalInsertCoercionJson(
			JsonEventType source,
			JsonEventType target)
		{
			this.source = source;
			this.target = target;
		}

		public EventType ResultEventType => target;

		public CodegenMethod ProcessCodegen(
			CodegenExpression resultEventType,
			CodegenExpression eventBeanFactory,
			CodegenMethodScope codegenMethodScope,
			SelectExprProcessorCodegenSymbol selectSymbol,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope);
			CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
			methodNode.Block
				.DeclareVar(source.Detail.UnderlyingClassName, "src", CastUnderlying(source.Detail.UnderlyingClassName, ArrayAtIndex(refEPS, Constant(0))))
				.DeclareVar(target.Detail.UnderlyingClassName, "und", NewInstance(target.Detail.UnderlyingClassName));
			foreach (var entryTarget in target.Detail.FieldDescriptors) {
				var src = source.Detail.FieldDescriptors.Get(entryTarget.Key);
				if (src == null) {
					continue;
				}

				methodNode.Block.AssignRef("und." + entryTarget.Value.FieldName, Ref("src." + src.FieldName));
			}

			methodNode.Block.MethodReturn(ExprDotMethod(eventBeanFactory, "adapterForTypedJson", Ref("und"), resultEventType));
			return methodNode;
		}
	}
} // end of namespace
