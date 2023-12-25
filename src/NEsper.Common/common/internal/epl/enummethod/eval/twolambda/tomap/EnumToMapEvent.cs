///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.tomap
{
    public class EnumToMapEvent : EnumForgeBasePlain
    {
        private readonly ExprForge _secondExpression;

        public ExprForge SecondExpression => _secondExpression;

        public EnumToMapEvent(
            ExprForge innerExpression,
            int streamCountIncoming,
            ExprForge secondExpression) : base(innerExpression, streamCountIncoming)
        {
            this._secondExpression = secondExpression;
        }

        public override EnumEval EnumEvaluator {
            get {
                var first = InnerExpression.ExprEvaluator;
                var second = _secondExpression.ExprEvaluator;
                return new ProxyEnumEval(
                    (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        if (enumcoll.IsEmpty()) {
                            return EmptyDictionary<object, object>.Instance;
                        }

                        IDictionary<object, object> map = new NullableDictionary<object, object>();

                        var beans = (ICollection<EventBean>)enumcoll;
                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var key = first.Evaluate(eventsLambda, isNewData, context);
                            var value = second.Evaluate(eventsLambda, isNewData, context);
                            map.Put(key, value);
                        }

                        return map;
                    });
            }
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var keyType = InnerExpression.EvaluationType ?? typeof(object);
            var valType = SecondExpression.EvaluationType ?? typeof(object);
            var dictionaryType = typeof(IDictionary<,>).MakeGenericType(keyType, valType);
            
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    dictionaryType,
                    typeof(EnumToMapEvent),
                    scope,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumValue(typeof(EmptyDictionary<,>).MakeGenericType(keyType, valType), "Instance"));

            block
                .DeclareVar(dictionaryType, "map", NewInstance(typeof(NullableDictionary<,>).MakeGenericType(keyType, valType)))
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .ForEach<EventBean>("next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("next"))
                .DeclareVar(keyType, "key", InnerExpression.EvaluateCodegen(keyType, methodNode, scope, codegenClassScope))
                .DeclareVar(valType, "value", _secondExpression.EvaluateCodegen(valType, methodNode, scope, codegenClassScope))
                .Expression(ExprDotMethod(Ref("map"), "Put", Ref("key"), Ref("value")));
            
            block.MethodReturn(Ref("map"));
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }
    }
} // end of namespace