///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumMinMaxScalarLambdaForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumMinMaxScalarLambdaForgeEval(EnumMinMaxScalarLambdaForge forge, ExprEvaluator innerExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> enumcoll, bool isNewData, ExprEvaluatorContext context)
        {
            IComparable minKey = null;

            ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            object[] props = resultEvent.Properties;

            ICollection<object> coll = (ICollection<object>)enumcoll;
            foreach (object next in coll)
            {
                props[0] = next;

                object comparable = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (comparable == null)
                {
                    continue;
                }

                if (minKey == null)
                {
                    minKey = (IComparable)comparable;
                }
                else
                {
                    if (forge.max)
                    {
                        if (minKey.CompareTo(comparable) < 0)
                        {
                            minKey = (IComparable)comparable;
                        }
                    }
                    else
                    {
                        if (minKey.CompareTo(comparable) > 0)
                        {
                            minKey = (IComparable)comparable;
                        }
                    }
                }
            }

            return minKey;
        }

        public static CodegenExpression Codegen(EnumMinMaxScalarLambdaForge forge, EnumForgeCodegenParams args, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField resultTypeMember = codegenClassScope.AddFieldUnshared(true, typeof(ObjectArrayEventType), Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));
            Type innerType = forge.innerExpression.EvaluationType;
            Type innerTypeBoxed = Boxing.GetBoxedType(innerType);

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(innerTypeBoxed, typeof(EnumMinMaxScalarLambdaForgeEval), scope, codegenClassScope).AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                    .DeclareVar(innerTypeBoxed, "minKey", ConstantNull())
                    .DeclareVar(typeof(ObjectArrayEventBean), "resultEvent", NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                    .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                    .DeclareVar(typeof(object[]), "props", ExprDotMethod(@Ref("resultEvent"), "getProperties"));

            CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                    .AssignArrayElement("props", Constant(0), @Ref("next"))
                    .DeclareVar(innerTypeBoxed, "value", forge.innerExpression.EvaluateCodegen(innerTypeBoxed, methodNode, scope, codegenClassScope));
            if (!innerType.IsPrimitive)
            {
                forEach.IfRefNull("value").BlockContinue();
            }

            forEach.IfCondition(EqualsNull(@Ref("minKey")))
                    .AssignRef("minKey", @Ref("value"))
                    .IfElse()
                    .IfCondition(Relational(ExprDotMethod(@Ref("minKey"), "compareTo", @Ref("value")), forge.max ? LT : GT, Constant(0)))
                    .AssignRef("minKey", @Ref("value"));

            block.MethodReturn(@Ref("minKey"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace