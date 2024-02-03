///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.fabric;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    /// <summary>
    ///     Result set processor prototype for the case: aggregation functions used in the select clause, and no group-by,
    ///     and all properties in the select clause are under an aggregation function.
    /// </summary>
    public class ResultSetProcessorRowForAllForge : ResultSetProcessorFactoryForgeBase
    {
        public ResultSetProcessorRowForAllForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            bool isHistoricalOnly,
            OutputLimitSpec outputLimitSpec,
            bool hasOrderBy) : base(resultEventType, typesPerStream)
        {
            OptionalHavingNode = optionalHavingNode;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            IsHistoricalOnly = isHistoricalOnly;
            OutputLimitSpec = outputLimitSpec;
            IsSorting = hasOrderBy;
        }

        public bool IsSelectRStream { get; }

        public bool IsUnidirectional { get; }

        public ExprForge OptionalHavingNode { get; }

        public bool IsHistoricalOnly { get; }

        public bool IsSorting { get; }

        public OutputLimitSpec OutputLimitSpec { get; }

        public override Type InterfaceClass => typeof(ResultSetProcessorRowForAll);

        public override string InstrumentedQName => "ResultSetProcessUngroupedFullyAgg";

        public StateMgmtSetting OutputAllHelperSettings { get; private set; }

        public StateMgmtSetting OutputLastHelperSettings { get; private set; }

        public bool IsOutputAll =>
            OutputLimitSpec != null &&
            OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public bool IsOutputLast =>
            OutputLimitSpec != null &&
            OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(AggregationService),
                "AggregationService",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(MEMBER_AGGREGATIONSVC));
            
#if DEFINED_IN_BASECLASS
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(MEMBER_EXPREVALCONTEXT));
#endif
            
            instance.Properties.AddProperty(
                typeof(bool),
                "IsSelectRStream",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(Constant(IsSelectRStream)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(OptionalHavingNode, classScope, instance);
            ResultSetProcessorRowForAllImpl.GetSelectListEventsAsArrayCodegen(this, classScope, instance);
        }

        public override void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public override void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ApplyViewResultCodegen(method);
        }

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ApplyJoinResultCodegen(method);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public override void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public override void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.StopCodegen(method, instance);
        }

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowForAllImpl.ClearCodegen(method);
        }

        public void PlanStateSettings(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (IsOutputAll) {
                OutputAllHelperSettings = services.StateMgmtSettingsProvider.ResultSet
                    .RowForAllOutputAll(fabricCharge, statementRawInfo, this);
            }
            else if (IsOutputLast) {
                OutputLastHelperSettings = services.StateMgmtSettingsProvider.ResultSet
                    .RowForAllOutputLast(fabricCharge, statementRawInfo, this);
            }
        }
    }
} // end of namespace