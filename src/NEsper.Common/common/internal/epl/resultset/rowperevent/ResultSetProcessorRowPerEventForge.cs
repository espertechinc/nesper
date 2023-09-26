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
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    /// <summary>
    /// Result set processor prototype for the case: aggregation functions used in the select clause, and no group-by,
    /// and not all of the properties in the select clause are under an aggregation function.
    /// </summary>
    public class ResultSetProcessorRowPerEventForge : ResultSetProcessorFactoryForgeBase
    {
        private readonly ExprForge optionalHavingNode;
        private readonly bool isSelectRStream;
        private readonly bool isUnidirectional;
        private readonly bool isHistoricalOnly;
        private readonly OutputLimitSpec outputLimitSpec;
        private readonly bool hasOrderBy;
        private StateMgmtSetting outputAllHelperSettings;
        private StateMgmtSetting outputLastHelperSettings;

        public ResultSetProcessorRowPerEventForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            bool isHistoricalOnly,
            OutputLimitSpec outputLimitSpec,
            bool hasOrderBy) : base(resultEventType, typesPerStream)
        {
            this.optionalHavingNode = optionalHavingNode;
            this.isSelectRStream = isSelectRStream;
            this.isUnidirectional = isUnidirectional;
            this.isHistoricalOnly = isHistoricalOnly;
            this.outputLimitSpec = outputLimitSpec;
            this.hasOrderBy = hasOrderBy;
        }

        public bool IsSelectRStream => isSelectRStream;

        public bool IsUnidirectional => isUnidirectional;

        public bool IsHistoricalOnly => isHistoricalOnly;

        public bool IsOutputLast => outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public bool IsSorting => hasOrderBy;

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Methods.AddMethod(
                typeof(SelectExprProcessor),
                "getSelectExprProcessor",
                EmptyList<CodegenNamedParam>.Instance, 
                typeof(ResultSetProcessorRowPerEvent),
                classScope,
                methodNode => methodNode.Block.MethodReturn(MEMBER_SELECTEXPRPROCESSOR));
            instance.Methods.AddMethod(
                typeof(bool),
                "hasHavingClause",
                EmptyList<CodegenNamedParam>.Instance, 
                typeof(ResultSetProcessorRowPerEvent),
                classScope,
                methodNode => methodNode.Block.MethodReturn(Constant(optionalHavingNode != null)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(optionalHavingNode, classScope, instance);
        }

        public override void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public override void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.GetEnumeratorViewCodegen(this, classScope, method);
        }

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public override void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ApplyViewResultCodegen(method);
        }

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ApplyJoinResultCodegen(method);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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
            ResultSetProcessorRowPerEventImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public override void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.StopCodegen(method, instance);
        }

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowPerEventImpl.ClearMethodCodegen(method);
        }

        public void PlanStateSettings(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (IsOutputAll) {
                outputAllHelperSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RowPerEventOutputAll(
                        fabricCharge,
                        statementRawInfo,
                        this);
            }
            else if (IsOutputLast) {
                outputLastHelperSettings =
                    services.StateMgmtSettingsProvider.ResultSet.RowPerEventOutputLast(
                        fabricCharge,
                        statementRawInfo,
                        this);
            }
        }

        public ExprForge OptionalHavingNode => optionalHavingNode;

        public override Type InterfaceClass => typeof(ResultSetProcessorRowPerEvent);

        public override string InstrumentedQName => "ResultSetProcessUngroupedNonfullyAgg";

        public StateMgmtSetting OutputAllHelperSettings => outputAllHelperSettings;

        public StateMgmtSetting OutputLastHelperSettings => outputLastHelperSettings;
    }
} // end of namespace