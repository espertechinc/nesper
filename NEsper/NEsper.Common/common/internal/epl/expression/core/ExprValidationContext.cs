///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.enummethod.compile;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprValidationContext
    {
        private readonly ContextCompileTimeDescriptor contextDescriptor;
        private readonly string intoTableName;

        public ExprValidationContext(
            StreamTypeService streamTypeService,
            ExprValidationContext ctx)
            : this(
                streamTypeService,
                ctx.ViewResourceDelegate,
                ctx.contextDescriptor,
                ctx.IsDisablePropertyExpressionEventCollCache,
                ctx.IsAllowRollupFunctions,
                ctx.IsAllowBindingConsumption,
                ctx.IsResettingAggregations,
                ctx.intoTableName,
                ctx.IsFilterExpression,
                ctx.MemberNames,
                ctx.IsAggregationFutureNameAlreadySet,
                ctx.StatementRawInfo,
                ctx.StatementCompileTimeService)
        {
        }

        internal ExprValidationContext(
            StreamTypeService streamTypeService,
            ViewResourceDelegateExpr viewResourceDelegate,
            ContextCompileTimeDescriptor contextDescriptor,
            bool disablePropertyExpressionEventCollCache,
            bool allowRollupFunctions,
            bool allowBindingConsumption,
            bool isUnidirectionalJoin,
            string intoTableName,
            bool isFilterExpression,
            ExprValidationMemberName memberName,
            bool aggregationFutureNameAlreadySet,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            StreamTypeService = streamTypeService;
            ViewResourceDelegate = viewResourceDelegate;
            this.contextDescriptor = contextDescriptor;
            IsDisablePropertyExpressionEventCollCache = disablePropertyExpressionEventCollCache;
            IsAllowRollupFunctions = allowRollupFunctions;
            IsAllowBindingConsumption = allowBindingConsumption;
            IsResettingAggregations = isUnidirectionalJoin;
            this.intoTableName = intoTableName;
            IsFilterExpression = isFilterExpression;
            MemberNames = memberName;
            IsAggregationFutureNameAlreadySet = aggregationFutureNameAlreadySet;
            StatementRawInfo = statementRawInfo;
            StatementCompileTimeService = compileTimeServices;

            IsExpressionAudit = AuditEnum.EXPRESSION.GetAudit(statementRawInfo.Annotations) != null;
            IsExpressionNestedAudit = AuditEnum.EXPRESSION_NESTED.GetAudit(statementRawInfo.Annotations) != null;
        }

        public IContainer Container => StatementCompileTimeService.Container;

        public Attribute[] Annotations => StatementRawInfo.Annotations;

        public StreamTypeService StreamTypeService { get; }

        public ContextCompileTimeDescriptor ContextDescriptor => StatementRawInfo.OptionalContextDescriptor;

        public bool IsFilterExpression { get; }

        public ScriptingService ScriptingService { get; }

        public ImportServiceCompileTime ImportService =>
            StatementCompileTimeService.ImportServiceCompileTime;

        public VariableCompileTimeResolver VariableCompileTimeResolver =>
            StatementCompileTimeService.VariableCompileTimeResolver;

        public TableCompileTimeResolver TableCompileTimeResolver =>
            StatementCompileTimeService.TableCompileTimeResolver;

        public string StatementName => StatementRawInfo.StatementName;

        public bool IsDisablePropertyExpressionEventCollCache { get; }

        public ViewResourceDelegateExpr ViewResourceDelegate { get; }

        public ExprValidationMemberName MemberNames { get; }

        public StatementType StatementType => StatementRawInfo.StatementType;

        public bool IsResettingAggregations { get; }

        public bool IsAllowRollupFunctions { get; }

        public StatementCompileTimeServices StatementCompileTimeService { get; }

        public StatementRawInfo StatementRawInfo { get; }

        public bool IsAllowBindingConsumption { get; }

        public EnumMethodCallStackHelperImpl EnumMethodCallStackHelper =>
            StatementCompileTimeService.EnumMethodCallStackHelper;

        public bool IsAggregationFutureNameAlreadySet { get; }

        public bool IsExpressionNestedAudit { get; }

        public bool IsExpressionAudit { get; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => EventBeanTypedEventFactoryCompileTime.INSTANCE;

        public string ModuleName => StatementRawInfo.ModuleName;
    }
} // end of namespace