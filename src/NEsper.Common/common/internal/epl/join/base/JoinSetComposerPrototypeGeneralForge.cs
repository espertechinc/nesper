///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.queryplan;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerPrototypeGeneralForge : JoinSetComposerPrototypeForge
    {
        private readonly bool hasHistorical;
        private readonly bool joinRemoveStream;
        private readonly QueryPlanForge queryPlan;
        private readonly StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult;
        private readonly string[] streamNames;

        public JoinSetComposerPrototypeGeneralForge(
            EventType[] streamTypes,
            ExprNode postJoinEvaluator,
            bool outerJoins,
            QueryPlanForge queryPlan,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            string[] streamNames,
            bool joinRemoveStream,
            bool hasHistorical)
            : base(streamTypes, postJoinEvaluator, outerJoins)
        {
            this.queryPlan = queryPlan;
            this.streamJoinAnalysisResult = streamJoinAnalysisResult;
            this.streamNames = streamNames;
            this.joinRemoveStream = joinRemoveStream;
            this.hasHistorical = hasHistorical;
        }

        public override QueryPlanForge OptionalQueryPlan => queryPlan;

        protected override Type Implementation()
        {
            return typeof(JoinSetComposerPrototypeGeneral);
        }

        protected override void PopulateInline(
            CodegenExpression impl,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(Ref("impl"), "QueryPlan", queryPlan.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("impl"),
                    "StreamJoinAnalysisResult",
                    streamJoinAnalysisResult.Make(method, symbols, classScope))
                .SetProperty(Ref("impl"), "StreamNames", Constant(streamNames))
                .SetProperty(Ref("impl"), "JoinRemoveStream", Constant(joinRemoveStream))
                .SetProperty(
                    Ref("impl"),
                    "EventTableIndexService",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.EVENTTABLEINDEXSERVICE))
                .SetProperty(Ref("impl"), "HasHistorical", Constant(hasHistorical));
        }
    }
} // end of namespace