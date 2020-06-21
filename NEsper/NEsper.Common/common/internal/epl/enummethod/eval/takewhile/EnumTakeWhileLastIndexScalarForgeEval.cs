///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumTakeWhileLastIndexScalarForgeEval : EnumEval
    {
        public const string METHOD_TAKEWHILELASTSCALARTOARRAY = "TakeWhileLastScalarToArray";

        private readonly EnumTakeWhileLastIndexScalarForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumTakeWhileLastIndexScalarForgeEval(
            EnumTakeWhileLastIndexScalarForge forge,
            ExprEvaluator innerExpression)
        {
            _forge = forge;
            _innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return enumcoll;
            }

            var evalEvent = new ObjectArrayEventBean(new object[1], _forge.evalEventType);
            eventsLambda[_forge.streamNumLambda] = evalEvent;
            var evalProps = evalEvent.Properties;
            var indexEvent = new ObjectArrayEventBean(new object[1], _forge.indexEventType);
            eventsLambda[_forge.streamNumLambda + 1] = indexEvent;
            var indexProps = indexEvent.Properties;

            if (enumcoll.Count == 1) {
                var item = enumcoll.First();
                evalProps[0] = item;
                indexProps[0] = 0;

                var pass = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || !(bool) pass) {
                    return Collections.GetEmptyList<object>();
                }

                return Collections.SingletonList(item);
            }

            var all = TakeWhileLastScalarToArray(enumcoll);
            var result = new ArrayDeque<object>();
            var index = 0;

            for (var i = all.Length - 1; i >= 0; i--) {
                evalProps[0] = all[i];
                indexProps[0] = index++;

                var pass = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || !(bool) pass) {
                    break;
                }

                result.AddFirst(all[i]);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumTakeWhileLastIndexScalarForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var evalTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.evalEventType, EPStatementInitServicesConstants.REF)));
            var indexTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.indexEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumTakeWhileLastIndexScalarForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);
            var innerValue = forge.innerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);
            block.DeclareVar<ObjectArrayEventBean>(
                    "evalEvent",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(1)),
                        evalTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), Ref("evalEvent"))
                .DeclareVar<object[]>("evalProps", ExprDotName(Ref("evalEvent"), "Properties"))
                .DeclareVar<ObjectArrayEventBean>(
                    "indexEvent",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(1)),
                        indexTypeMember))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.streamNumLambda + 1),
                    Ref("indexEvent"))
                .DeclareVar<object[]>("indexProps", ExprDotName(Ref("indexEvent"), "Properties"));

            var blockSingle = block.IfCondition(
                    EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
                .DeclareVar<object>(
                    "item",
                    ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First"))
                .AssignArrayElement("evalProps", Constant(0), Ref("item"))
                .AssignArrayElement("indexProps", Constant(0), Constant(0));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                forge.innerExpression.EvaluationType,
                innerValue,
                FlexEmpty());
            blockSingle.BlockReturn(FlexValue(Ref("item")));

            block
                .DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)))
                .DeclareVar<object[]>(
                    "all",
                    StaticMethod(
                        typeof(EnumTakeWhileLastIndexScalarForgeEval),
                        METHOD_TAKEWHILELASTSCALARTOARRAY,
                        EnumForgeCodegenNames.REF_ENUMCOLL))
                .DeclareVar<int>("index", Constant(0));
            var forEach = block.ForLoop(
                    typeof(int),
                    "i",
                    Op(ArrayLength(Ref("all")), "-", Constant(1)),
                    Relational(Ref("i"), GE, Constant(0)),
                    DecrementRef("i"))
                .AssignArrayElement("evalProps", Constant(0), ArrayAtIndex(Ref("all"), Ref("i")))
                .AssignArrayElement("indexProps", Constant(0), IncrementRef("index"));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                forEach,
                forge.innerExpression.EvaluationType,
                innerValue);
            forEach.Expression(ExprDotMethod(Ref("result"), "AddFirst", ArrayAtIndex(Ref("all"), Ref("i"))));
            block.MethodReturn(FlexWrap(Ref("result")));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumcoll">coll</param>
        /// <returns>array</returns>
        public static object[] TakeWhileLastScalarToArray(ICollection<object> enumcoll)
        {
            var size = enumcoll.Count;
            var all = new object[size];
            var count = 0;
            foreach (var item in enumcoll) {
                all[count++] = item;
            }

            return all;
        }
    }
} // end of namespace