///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.output.core.OutputProcessViewCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputStrategyPostProcessForge
    {
        private readonly bool audit;
        private readonly SelectClauseStreamSelectorEnum? insertIntoStreamSelector;
        private readonly bool isRouted;
        private readonly bool routeToFront;
        private readonly SelectClauseStreamSelectorEnum selectStreamSelector;
        private readonly TableMetaData table;

        public OutputStrategyPostProcessForge(
            bool isRouted,
            SelectClauseStreamSelectorEnum? insertIntoStreamSelector,
            SelectClauseStreamSelectorEnum selectStreamSelector,
            bool routeToFront,
            TableMetaData table,
            bool audit)
        {
            this.isRouted = isRouted;
            this.insertIntoStreamSelector = insertIntoStreamSelector;
            this.selectStreamSelector = selectStreamSelector;
            this.routeToFront = routeToFront;
            this.table = table;
            this.audit = audit;
        }

        public bool HasTable => table != null;

        /// <summary>
        ///     Code for post-process, "result" can be null, "force-update" can be passed in
        /// </summary>
        /// <param name="classScope">class scope</param>
        /// <param name="parent">parent</param>
        /// <returns>method</returns>
        public CodegenMethod PostProcessCodegenMayNullMayForce(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(void), typeof(OutputStrategyPostProcessForge), classScope)
                .AddParam(typeof(bool), "forceUpdate")
                .AddParam(typeof(UniformPair<EventBean[]>), "result");

            var ifChild = method.Block.IfCondition(NotEqualsNull(MEMBER_CHILD));

            // handle non-null
            var ifResultNotNull = ifChild.IfRefNotNull("result");
            if (isRouted) {
                if (insertIntoStreamSelector != null) {
                    if (insertIntoStreamSelector.Value.IsSelectsIStream()) {
                        ifResultNotNull.InstanceMethod(
                            RouteCodegen(classScope, parent),
                            Cast(typeof(EventBean[]), ExprDotName(Ref("result"), "First")));
                    }

                    if (insertIntoStreamSelector.Value.IsSelectsRStream()) {
                        ifResultNotNull.InstanceMethod(
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
                        NewInstance<UniformPair<EventBean[]>>(
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
                        NewInstance<UniformPair<EventBean[]>>(
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
                .ExprDotMethod(MEMBER_CHILD, "NewResult", PublicConstValue(typeof(UniformPair<EventBean[]>), "EMPTY_PAIR"));

            return method;
        }

        private CodegenMethod RouteCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(void), typeof(OutputStrategyPostProcessForge), classScope)
                .AddParam(typeof(EventBean[]), "events");
            var forEach = method.Block
                .IfRefNull("events")
                .BlockReturnNoValue()
                .ForEach(typeof(EventBean), "routed", Ref("events"));

            if (audit) {
                forEach.Expression(
                    ExprDotMethodChain(MEMBER_AGENTINSTANCECONTEXT)
                        .Get("AuditProvider")
                        .Add(
                            "Insert",
                            Ref("routed"),
                            MEMBER_AGENTINSTANCECONTEXT));
            }

            forEach.Expression(
                ExprDotMethodChain(MEMBER_AGENTINSTANCECONTEXT)
                    .Get("InternalEventRouter")
                    .Add(
                        "Route",
                        Ref("routed"),
                        MEMBER_AGENTINSTANCECONTEXT,
                        Constant(routeToFront)));

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
            var insertIntoStreamSelectorExpr = insertIntoStreamSelector == null
                ? ConstantNull()
                : EnumValue(typeof(SelectClauseStreamSelectorEnum), insertIntoStreamSelector.Value.GetName());
            
            return NewInstance<OutputStrategyPostProcessFactory>(
                Constant(isRouted),
                insertIntoStreamSelectorExpr,
                EnumValue(typeof(SelectClauseStreamSelectorEnum), selectStreamSelector.GetName()),
                Constant(routeToFront),
                resolveTable);
        }
    }
} // end of namespace