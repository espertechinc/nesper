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
        private readonly SelectClauseStreamSelectorEnum insertIntoStreamSelector;
        private readonly bool isRouted;
        private readonly bool routeToFront;
        private readonly SelectClauseStreamSelectorEnum selectStreamSelector;
        private readonly TableMetaData table;

        public OutputStrategyPostProcessForge(
            bool isRouted,
            SelectClauseStreamSelectorEnum insertIntoStreamSelector,
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
        public CodegenMethod PostProcessCodegenMayNullMayForce(CodegenClassScope classScope, CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(void), typeof(OutputStrategyPostProcessForge), classScope)
                .AddParam(typeof(bool), "forceUpdate")
                .AddParam(typeof(UniformPair<EventBean>), "result");

            var ifChild = method.Block.IfCondition(NotEqualsNull(REF_CHILD));

            // handle non-null
            var ifResultNotNull = ifChild.IfRefNotNull("result");
            if (isRouted) {
                if (insertIntoStreamSelector.IsSelectsIStream) {
                    ifResultNotNull.LocalMethod(
                        RouteCodegen(classScope, parent),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("result"), "getFirst")));
                }

                if (insertIntoStreamSelector.IsSelectsRStream) {
                    ifResultNotNull.LocalMethod(
                        RouteCodegen(classScope, parent),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("result"), "getSecond")));
                }
            }

            if (selectStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ONLY) {
                ifResultNotNull.IfCondition(NotEqualsNull(ExprDotMethod(Ref("result"), "getSecond")))
                    .ExprDotMethod(
                        REF_CHILD, "newResult",
                        NewInstance(
                            typeof(UniformPair<EventBean>), ExprDotMethod(Ref("result"), "getSecond"), ConstantNull()))
                    .IfElseIf(Ref("forceUpdate"))
                    .ExprDotMethod(
                        REF_CHILD, "newResult", PublicConstValue(typeof(UniformPair<EventBean>), "EMPTY_PAIR"));
            }
            else if (selectStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH) {
                ifResultNotNull.IfCondition(
                        Or(
                            NotEqualsNull(ExprDotMethod(Ref("result"), "getFirst")),
                            NotEqualsNull(ExprDotMethod(Ref("result"), "getSecond"))))
                    .ExprDotMethod(REF_CHILD, "newResult", Ref("result"))
                    .IfElseIf(Ref("forceUpdate"))
                    .ExprDotMethod(
                        REF_CHILD, "newResult", PublicConstValue(typeof(UniformPair<EventBean>), "EMPTY_PAIR"));
            }
            else {
                ifResultNotNull.IfCondition(NotEqualsNull(ExprDotMethod(Ref("result"), "getFirst")))
                    .ExprDotMethod(
                        REF_CHILD, "newResult",
                        NewInstance(
                            typeof(UniformPair<EventBean>), ExprDotMethod(Ref("result"), "getFirst"), ConstantNull()))
                    .IfElseIf(Ref("forceUpdate"))
                    .ExprDotMethod(
                        REF_CHILD, "newResult", PublicConstValue(typeof(UniformPair<EventBean>), "EMPTY_PAIR"));
            }

            // handle null-result (force-update)
            var ifResultNull = ifResultNotNull.IfElse();
            ifResultNull.IfCondition(Ref("forceUpdate"))
                .ExprDotMethod(REF_CHILD, "newResult", PublicConstValue(typeof(UniformPair<EventBean>), "EMPTY_PAIR"));

            return method;
        }

        private CodegenMethod RouteCodegen(CodegenClassScope classScope, CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(void), typeof(OutputStrategyPostProcessForge), classScope)
                .AddParam(typeof(EventBean[]), "events");
            var forEach = method.Block
                .IfRefNull("events").BlockReturnNoValue()
                .ForEach(typeof(EventBean), "routed", Ref("events"));

            if (audit) {
                forEach.Expression(
                    ExprDotMethodChain(Ref(NAME_AGENTINSTANCECONTEXT)).Add("getAuditProvider").Add(
                        "insert", Ref("routed"), Ref(NAME_AGENTINSTANCECONTEXT)));
            }

            forEach.Expression(
                ExprDotMethodChain(Ref(NAME_AGENTINSTANCECONTEXT)).Add("getInternalEventRouter").Add(
                    "route", Ref("routed"), Ref(NAME_AGENTINSTANCECONTEXT), Constant(routeToFront)));

            return method;
        }

        public CodegenExpression Make(CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var resolveTable = table == null
                ? ConstantNull()
                : TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method));
            return NewInstance(
                typeof(OutputStrategyPostProcessFactory), Constant(isRouted),
                EnumValue(typeof(SelectClauseStreamSelectorEnum), insertIntoStreamSelector.GetName()),
                EnumValue(typeof(SelectClauseStreamSelectorEnum), selectStreamSelector.GetName()),
                Constant(routeToFront), resolveTable);
        }
    }
} // end of namespace