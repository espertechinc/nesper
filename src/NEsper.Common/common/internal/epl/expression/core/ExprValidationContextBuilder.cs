///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.view.access;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public sealed class ExprValidationContextBuilder
    {
        private readonly StatementCompileTimeServices _compileTimeServices;
        private readonly StatementRawInfo _statementRawInfo;
        private readonly StreamTypeService _streamTypeService;
        private bool _aggregationFutureNameAlreadySet;
        private bool _allowRollupFunctions;
        private bool _allowBindingConsumption;
        private bool _allowTableAggReset;
        private ContextCompileTimeDescriptor _contextDescriptor;
        private bool _disablePropertyExpressionEventCollCache;
        private string _intoTableName;
        private bool _isFilterExpression;
        private bool _isResettingAggregations;
        private ExprValidationMemberName _memberName = ExprValidationMemberNameDefault.INSTANCE;

        private ViewResourceDelegateExpr _viewResourceDelegate;

        public ExprValidationContextBuilder(
            StreamTypeService streamTypeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            _streamTypeService = streamTypeService;
            _statementRawInfo = statementRawInfo;
            _compileTimeServices = compileTimeServices;
        }

        public ExprValidationContextBuilder WithViewResourceDelegate(ViewResourceDelegateExpr viewResourceDelegate)
        {
            _viewResourceDelegate = viewResourceDelegate;
            return this;
        }

        public ExprValidationContextBuilder WithContextDescriptor(ContextCompileTimeDescriptor contextDescriptor)
        {
            _contextDescriptor = contextDescriptor;
            return this;
        }

        public ExprValidationContextBuilder WithDisablePropertyExpressionEventCollCache(
            bool disablePropertyExpressionEventCollCache)
        {
            _disablePropertyExpressionEventCollCache = disablePropertyExpressionEventCollCache;
            return this;
        }

        public ExprValidationContextBuilder WithAllowRollupFunctions(bool allowRollupFunctions)
        {
            _allowRollupFunctions = allowRollupFunctions;
            return this;
        }

        public ExprValidationContextBuilder WithAllowBindingConsumption(bool allowBindingConsumption)
        {
            _allowBindingConsumption = allowBindingConsumption;
            return this;
        }

        public ExprValidationContextBuilder WithAllowTableAggReset(bool allowTableAggReset)
        {
            _allowTableAggReset = allowTableAggReset;
            return this;
        }

        public ExprValidationContextBuilder WithIntoTableName(string intoTableName)
        {
            _intoTableName = intoTableName;
            return this;
        }

        public ExprValidationContextBuilder WithIsFilterExpression(bool isFilterExpression)
        {
            _isFilterExpression = isFilterExpression;
            return this;
        }

        public ExprValidationContextBuilder WithMemberName(ExprValidationMemberName memberName)
        {
            _memberName = memberName;
            return this;
        }

        public ExprValidationContextBuilder WithIsResettingAggregations(bool isResettingAggregations)
        {
            _isResettingAggregations = isResettingAggregations;
            return this;
        }

        public ExprValidationContextBuilder WithAggregationFutureNameAlreadySet(bool aggregationFutureNameAlreadySet)
        {
            _aggregationFutureNameAlreadySet = aggregationFutureNameAlreadySet;
            return this;
        }

        public ExprValidationContext Build()
        {
            return new ExprValidationContext(
                _streamTypeService,
                _viewResourceDelegate,
                _contextDescriptor,
                _disablePropertyExpressionEventCollCache,
                _allowRollupFunctions,
                _allowBindingConsumption,
                _allowTableAggReset,
                _isResettingAggregations,
                _intoTableName,
                _isFilterExpression,
                _memberName,
                _aggregationFutureNameAlreadySet,
                _statementRawInfo,
                _compileTimeServices);
        }
    }
} // end of namespace