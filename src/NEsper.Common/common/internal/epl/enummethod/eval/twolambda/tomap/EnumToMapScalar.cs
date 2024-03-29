///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.codegen.EnumForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.tomap
{
    public class EnumToMapScalar : EnumForgeBasePlain
    {
        private readonly ExprForge _secondExpression;
        private readonly ObjectArrayEventType _resultEventType;
        private readonly int _numParameters;

        public EnumToMapScalar(
            ExprForge innerExpression,
            int streamCountIncoming,
            ExprForge secondExpression,
            ObjectArrayEventType resultEventType,
            int numParameters) : base(innerExpression, streamCountIncoming)
        {
            _secondExpression = secondExpression;
            _resultEventType = resultEventType;
            _numParameters = numParameters;
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
                        var resultEvent = new ObjectArrayEventBean(new object[3], _resultEventType);
                        eventsLambda[StreamNumLambda] = resultEvent;
                        var props = resultEvent.Properties;
                        props[2] = enumcoll.Count;
                        var values = (ICollection<object>)enumcoll;
                        var count = -1;
                        foreach (var next in values) {
                            count++;
                            props[1] = count;
                            props[0] = next;
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
			var resultTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
				typeof(ObjectArrayEventType),
				Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(_resultEventType, EPStatementInitServicesConstants.REF)));

            var keyType = InnerExpression.EvaluationType ?? typeof(object);
            var valType = _secondExpression.EvaluationType ?? typeof(object);
            var dictionaryType = typeof(IDictionary<,>).MakeGenericType(keyType, valType);
            
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(dictionaryType, typeof(EnumToMapScalar), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);
            
            var hasIndex = _numParameters >= 2;
            var hasSize = _numParameters >= 3;
            
            var block = methodNode.Block
				.IfCondition(ExprDotMethod(REF_ENUMCOLL, "IsEmpty"))
				.BlockReturn(EnumValue(typeof(EmptyDictionary<,>).MakeGenericType(keyType, valType), "Instance"));
            
            block
                .DeclareVar(dictionaryType, "map", NewInstance(typeof(NullableDictionary<,>).MakeGenericType(keyType, valType)))
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar<ObjectArrayEventBean>(
					"resultEvent",
                    NewInstance(
                        typeof(ObjectArrayEventBean),
                        NewArrayByLength(typeof(object), Constant(_numParameters)),
                        resultTypeMember))
                .AssignArrayElement(REF_EPS, Constant(StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));
            
            if (hasIndex) {
				block.DeclareVar<int>("count", Constant(-1));
            }

            if (hasSize) {
				block.AssignArrayElement(Ref("props"), Constant(2), ExprDotName(REF_ENUMCOLL, "Count"));
            }

            var forEach = block
                .ForEachVar("next", REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("next"));
            if (hasIndex) {
                forEach.IncrementRef("count").AssignArrayElement("props", Constant(1), Ref("count"));
            }

            forEach
                .DeclareVar(
                    keyType,
                    "key",
                    
                    InnerExpression.EvaluateCodegen(keyType, methodNode, scope, codegenClassScope))
                .DeclareVar(
                    valType,
                    "value",
                    _secondExpression.EvaluateCodegen(valType, methodNode, scope, codegenClassScope))
                .Expression(ExprDotMethod(Ref("map"), "Put", Ref("key"), Ref("value")));
            block.MethodReturn(Ref("map"));
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }

    }
} // end of namespace