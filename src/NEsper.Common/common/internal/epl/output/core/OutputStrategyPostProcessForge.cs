///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.output.core.OutputProcessViewCodegenNames;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputStrategyPostProcessForge
    {
        private readonly bool isRouted;
        private readonly SelectClauseStreamSelectorEnum? insertIntoStreamSelector;
        private readonly SelectClauseStreamSelectorEnum selectStreamSelector;
        private readonly bool routeToFront;
        private readonly TableMetaData table;
        private readonly bool audit;
        private readonly ExprNode eventPrecedence;

        public OutputStrategyPostProcessForge(
            bool isRouted,
            SelectClauseStreamSelectorEnum? insertIntoStreamSelector,
            SelectClauseStreamSelectorEnum selectStreamSelector,
            bool routeToFront,
            TableMetaData table,
            bool audit,
            ExprNode eventPrecedence)
        {
            this.isRouted = isRouted;
            this.insertIntoStreamSelector = insertIntoStreamSelector;
            this.selectStreamSelector = selectStreamSelector;
            this.routeToFront = routeToFront;
            this.table = table;
            this.audit = audit;
            this.eventPrecedence = eventPrecedence;
        }

        public bool HasTable()
        {
            return table != null;
        }

        /// <summary>
        /// Code for post-process, "result" can be null, "force-update" can be passed in
        /// </summary>
        /// <param name="classScope">class scope</param>
        /// <param name="parent">parent</param>
        /// <returns>method</returns>
        public CodegenMethod PostProcessCodegenMayNullMayForce(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(void), typeof(OutputStrategyPostProcessForge), classScope)
                .AddParam<bool>("forceUpdate")
                .AddParam<UniformPair<EventBean[]>>("result");

            var ifChild = method.Block.IfCondition(NotEqualsNull(MEMBER_CHILD));

            // handle non-null
            var ifResultNotNull = ifChild.IfRefNotNull("result");
            if (isRouted) {
                if (insertIntoStreamSelector != null) {
                    if (insertIntoStreamSelector.Value.IsSelectsIStream()) {
                        ifResultNotNull.LocalMethod(
                            RouteCodegen(classScope, parent),
                            Cast(typeof(EventBean[]), ExprDotName(Ref("result"), "First")));
                    }

                    if (insertIntoStreamSelector.Value.IsSelectsRStream()) {
                        ifResultNotNull.LocalMethod(
                            RouteCodegen(classScope, parent),
                            Cast(typeof(EventBean[]), ExprDotName(Ref("result"), "Second")));
                    }
                }
            }

            if (selectStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ONLY) {
                ifResultNotNull.IfCondition(NotEqualsNull(ExprDotName(Ref("result"), "Second")))
                    .ExprDotMethod(
                        MEMBER_CHILD,
                        "NewResult",
                        NewInstance(
                            typeof(UniformPair<EventBean[]>),
                            ExprDotName(Ref("result"), "Second"),
                            ConstantNull()))
                    .IfElseIf(Ref("forceUpdate"))
                    .ExprDotMethod(
                        MEMBER_CHILD,
                        "NewResult",
                        PublicConstValue(typeof(UniformPair<EventBean[]>), "EMPTY_PAIR"));
            }
            else if (selectStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH) {
                ifResultNotNull.IfCondition(
                        Or(
                            NotEqualsNull(ExprDotName(Ref("result"), "First")),
                            NotEqualsNull(ExprDotName(Ref("result"), "Second"))))
                    .ExprDotMethod(MEMBER_CHILD, "NewResult", Ref("result"))
                    .IfElseIf(Ref("forceUpdate"))
                    .ExprDotMethod(
                        MEMBER_CHILD,
                        "NewResult",
                        PublicConstValue(typeof(UniformPair<EventBean[]>), "EMPTY_PAIR"));
            }
            else {
                ifResultNotNull.IfCondition(NotEqualsNull(ExprDotName(Ref("result"), "First")))
                    .ExprDotMethod(
                        MEMBER_CHILD,
                        "NewResult",
                        NewInstance(
                            typeof(UniformPair<EventBean[]>),
                            ExprDotName(Ref("result"), "First"),
                            ConstantNull()))
                    .IfElseIf(Ref("forceUpdate"))
                    .ExprDotMethod(
                        MEMBER_CHILD,
                        "NewResult",
                        PublicConstValue(typeof(UniformPair<EventBean[]>), "EMPTY_PAIR"));
            }

            // handle null-result (force-update)
            var ifResultNull = ifResultNotNull.IfElse();
            ifResultNull.IfCondition(Ref("forceUpdate"))
                .ExprDotMethod(
                    MEMBER_CHILD,
                    "NewResult",
                    PublicConstValue(typeof(UniformPair<EventBean[]>), "EMPTY_PAIR"));

            return method;
        }

        private CodegenMethod RouteCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(void), typeof(OutputStrategyPostProcessForge), classScope)
                .AddParam<EventBean[]>("events");
            var forEach = method.Block
                .IfRefNull("events")
                .BlockReturnNoValue()
                .ForEach<EventBean>("routed", Ref("events"));

            if (audit) {
                forEach.Expression(
                    ExprDotMethodChain(MEMBER_AGENTINSTANCECONTEXT)
                        .Get("AuditProvider")
                        .Add("insert", Ref("routed"), MEMBER_AGENTINSTANCECONTEXT));
            }

            // Evaluate event precedence
            if (eventPrecedence != null) {
                if (eventPrecedence.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    forEach.DeclareVar<int>(
                        "precedence",
                        Constant(eventPrecedence.Forge.ExprEvaluator.Evaluate(null, true, null)));
                }
                else {
                    var methodPrecedence = CodegenLegoMethodExpression.CodegenExpression(
                        eventPrecedence.Forge,
                        method,
                        classScope);
                    CodegenExpression exprEventPrecedence = LocalMethod(
                        methodPrecedence,
                        NewArrayWithInit(typeof(EventBean), Ref("routed")),
                        Constant(true),
                        MEMBER_AGENTINSTANCECONTEXT);
                    forEach.DeclareVar<int>("precedence", Constant(0))
                        .DeclareVar(typeof(int?), "precedenceResult", exprEventPrecedence)
                        .IfRefNotNull("precedenceResult")
                        .AssignRef("precedence", Ref("precedenceResult"));
                }
            }
            else {
                forEach.DeclareVar<int>("precedence", Constant(0));
            }

            forEach.Expression(
                ExprDotMethodChain(MEMBER_AGENTINSTANCECONTEXT)
                    .Get("InternalEventRouter")
                    .Add(
                        "Route",
                        Ref("routed"),
                        MEMBER_AGENTINSTANCECONTEXT,
                        Constant(routeToFront),
                        Ref("precedence")));

            return method;
        }

        public CodegenExpression Make(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var resolveTable = table == null
                ? ConstantNull()
                : TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method));
            var eventPrecedenceEval = eventPrecedence == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(eventPrecedence.Forge, method, GetType(), classScope);
            return NewInstance(
                typeof(OutputStrategyPostProcessFactory),
                Constant(isRouted),
                insertIntoStreamSelector == null
                    ? ConstantNull()
                    : EnumValue(typeof(SelectClauseStreamSelectorEnum), insertIntoStreamSelector.Value.GetName()),
                EnumValue(typeof(SelectClauseStreamSelectorEnum), selectStreamSelector.GetName()),
                Constant(routeToFront),
                resolveTable,
                eventPrecedenceEval);
        }
    }
} // end of namespace