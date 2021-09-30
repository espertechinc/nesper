///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.arrayOf
{
	public class EnumArrayOfScalarNoParams : EnumForge
	{
		private readonly Type _arrayComponentType;

		public EnumArrayOfScalarNoParams(Type arrayComponentType)
		{
			_arrayComponentType = arrayComponentType;
		}

		public Type ArrayComponentType => _arrayComponentType;

		public EnumEval EnumEvaluator {
			get {
				return new ProxyEnumEval(
					(
						eventsLambda,
						enumcoll,
						isNewData,
						context) => {
						var array = Arrays.CreateInstanceChecked(_arrayComponentType, enumcoll.Count);
						if (enumcoll.IsEmpty()) {
							return array;
						}

						var count = 0;
						foreach (var next in enumcoll) {
							array.SetValue(next, count);
							count++;
						}

						return array;
					});
			}
		}

		public CodegenExpression Codegen(
			EnumForgeCodegenParams premade,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var arrayType = TypeHelper.GetArrayType(_arrayComponentType);
			var scope = new ExprForgeCodegenSymbol(false, null);
			var methodNode = codegenMethodScope
				.MakeChildWithScope(arrayType, typeof(EnumArrayOfScalarNoParams), scope, codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS);

			var block = methodNode.Block
				.IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
				.BlockReturn(NewArrayByLength(_arrayComponentType, Constant(0)))
				.DeclareVar(arrayType, "result", NewArrayByLength(_arrayComponentType, ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count")))
				.DeclareVar<int>("count", Constant(0));
			block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
				.AssignArrayElement(Ref("result"), Ref("count"), Cast(_arrayComponentType, Ref("next")))
				.IncrementRef("count");
			block.MethodReturn(Ref("result"));
			return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
		}

		public int StreamNumSize => 0;
	}
} // end of namespace
