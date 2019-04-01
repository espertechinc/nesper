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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxByScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumMinMaxByScalarLambdaForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumMinMaxByScalarLambdaForgeEval(EnumMinMaxByScalarLambdaForge forge, ExprEvaluator innerExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> enumcoll, bool isNewData, ExprEvaluatorContext context)
        {
            IComparable minKey = null;
            object result = null;
            ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            object[] props = resultEvent.Properties;

            ICollection<object> values = (ICollection<object>)enumcoll;
            foreach (object next in values)
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
                    result = next;
                }
                else
                {
                    if (forge.max)
                    {
                        if (minKey.CompareTo(comparable) < 0)
                        {
                            minKey = (IComparable)comparable;
                            result = next;
                        }
                    }
                    else
                    {
                        if (minKey.CompareTo(comparable) > 0)
                        {
                            minKey = (IComparable)comparable;
                            result = next;
                        }
                    }
                }
            }

            return result;
        }

        public static CodegenExpression Codegen(EnumMinMaxByScalarLambdaForge forge, EnumForgeCodegenParams args, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            Type innerTypeBoxed = Boxing.GetBoxedType(forge.innerExpression.EvaluationType);
            Type resultTypeBoxed = Boxing.GetBoxedType(EPTypeHelper.GetCodegenReturnType(forge.resultType));
            CodegenExpressionField resultTypeMember = codegenClassScope.AddFieldUnshared(true, typeof(ObjectArrayEventType), Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(resultTypeBoxed, typeof(EnumMinMaxByScalarLambdaForgeEval), scope, codegenClassScope).AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                    .DeclareVar(innerTypeBoxed, "minKey", ConstantNull())
                    .DeclareVar(resultTypeBoxed, "result", ConstantNull())
                    .DeclareVar(typeof(ObjectArrayEventBean), "resultEvent", NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                    .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                    .DeclareVar(typeof(object[]), "props", ExprDotMethod(@Ref("resultEvent"), "getProperties"));

            CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                    .AssignArrayElement("props", Constant(0), @Ref("next"))
                    .DeclareVar(innerTypeBoxed, "value", forge.innerExpression.EvaluateCodegen(innerTypeBoxed, methodNode, scope, codegenClassScope))
                    .IfRefNull("value").BlockContinue();

            forEach.IfCondition(EqualsNull(@Ref("minKey")))
                    .AssignRef("minKey", @Ref("value"))
                    .AssignRef("result", Cast(resultTypeBoxed, @Ref("next")))
                    .IfElse()
                    .IfCondition(Relational(ExprDotMethod(@Ref("minKey"), "compareTo", @Ref("value")), forge.max ? LT : GT, Constant(0)))
                    .AssignRef("minKey", @Ref("value"))
                    .AssignRef("result", Cast(resultTypeBoxed, @Ref("next")));

            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace