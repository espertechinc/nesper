///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
    /// <summary>
    /// Result set processor prototype for the simplest case: no aggregation functions used in the select clause, and no group-by.
    /// </summary>
    public class ResultSetProcessorSimpleForge : ResultSetProcessorFactoryForgeBase
    {
        private readonly bool isSelectRStream;
        private readonly ExprForge optionalHavingNode;
        private readonly OutputLimitSpec outputLimitSpec;
        private readonly ResultSetProcessorOutputConditionType? outputConditionType;
        private readonly bool isSorting;
        private readonly EventType[] eventTypes;
        private StateMgmtSetting outputAllHelperSettings;
        private StateMgmtSetting outputLastHelperSettings;

        public ResultSetProcessorSimpleForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            OutputLimitSpec outputLimitSpec,
            ResultSetProcessorOutputConditionType? outputConditionType,
            bool isSorting,
            EventType[] eventTypes) : base(resultEventType, typesPerStream)
        {
            this.optionalHavingNode = optionalHavingNode;
            this.isSelectRStream = isSelectRStream;
            this.outputLimitSpec = outputLimitSpec;
            this.outputConditionType = outputConditionType;
            this.isSorting = isSorting;
            this.eventTypes = eventTypes;
        }

        public bool IsSelectRStream => isSelectRStream;

        public bool IsOutputLast => outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public bool IsSorting => isSorting;

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(bool),
                "HasHavingClause",
                typeof(ResultSetProcessorSimple),
                classScope,
                property => property.GetterBlock.BlockReturn(Constant(optionalHavingNode != null)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(optionalHavingNode, classScope, instance);
        
#if DEFINED_IN_BASECLASS
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                GetType(),
                classScope,
                property => property.GetterBlock.BlockReturn(MEMBER_EXPREVALCONTEXT));
#endif
        }

        public override void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedViewCodegen(this, method);
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedJoinCodegen(this, method);
        }

        public override void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            // no action
        }

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            // no action
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public override void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public override void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.StopMethodCodegen(method, instance);
        }

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            // no clearing aggregations
        }

        public void PlanStateSettings(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (IsOutputAll) {
                outputAllHelperSettings =
                    services.StateMgmtSettingsProvider.ResultSet.SimpleOutputAll(fabricCharge, statementRawInfo, this);
            }
            else if (IsOutputLast) {
                outputLastHelperSettings =
                    services.StateMgmtSettingsProvider.ResultSet.SimpleOutputLast(fabricCharge, statementRawInfo, this);
            }
        }

        public ExprForge OptionalHavingNode => optionalHavingNode;

        public ResultSetProcessorOutputConditionType? OutputConditionType => outputConditionType;

        public int NumStreams => eventTypes.Length;

        public EventType[] EventTypes => eventTypes;

        public override Type InterfaceClass => typeof(ResultSetProcessorSimple);

        public override string InstrumentedQName => "ResultSetProcessSimple";

        public StateMgmtSetting OutputAllHelperSettings => outputAllHelperSettings;

        public StateMgmtSetting OutputLastHelperSettings => outputLastHelperSettings;
    }
} // end of namespace