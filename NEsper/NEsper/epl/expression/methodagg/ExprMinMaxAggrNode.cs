///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.methodagg
{
    /// <summary>
    /// Represents the min/Max(distinct? ...) aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprMinMaxAggrNode : ExprAggregateNodeBase
    {
        private readonly MinMaxTypeEnum _minMaxTypeEnum;
        private readonly bool _hasFilter;
        private readonly bool _isEver;
    
        public ExprMinMaxAggrNode(bool distinct, MinMaxTypeEnum minMaxTypeEnum, bool hasFilter, bool isEver)
            : base(distinct)
        {
            _minMaxTypeEnum = minMaxTypeEnum;
            _hasFilter = hasFilter;
            _isEver = isEver;
        }
    
        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            var positionalParams = PositionalParams;
            if (positionalParams.Length == 0 || positionalParams.Length > 2) {
                throw new ExprValidationException(_minMaxTypeEnum.ToString() + " node must have either 1 or 2 parameters");
            }
    
            var child = positionalParams[0];
            bool hasDataWindows;
            if (_isEver) {
                hasDataWindows = false;
            } else {
                if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE) {
                    hasDataWindows = true;
                } else {
                    hasDataWindows = ExprNodeUtility.HasRemoveStreamForAggregations(child, validationContext.StreamTypeService, validationContext.IsResettingAggregations);
                }
            }
    
            if (_hasFilter) {
                if (positionalParams.Length < 2) {
                    throw new ExprValidationException(_minMaxTypeEnum.ToString() + "-filtered aggregation function must have a filter expression as a second parameter");
                }
                base.ValidateFilter(positionalParams[1].ExprEvaluator);
            }
            return validationContext.EngineImportService.AggregationFactoryFactory.MakeMinMax(validationContext.StatementExtensionSvcContext, this, child.ExprEvaluator.ReturnType, hasDataWindows);
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node) {
            var other = node as ExprMinMaxAggrNode;
            if (other == null)
            {
                return false;
            }
            return other._minMaxTypeEnum == this._minMaxTypeEnum && other._isEver == this._isEver;
        }

        /// <summary>
        /// Returns the indicator for minimum or maximum.
        /// </summary>
        /// <value>min/max indicator</value>
        public MinMaxTypeEnum MinMaxTypeEnum => _minMaxTypeEnum;

        public bool HasFilter => _hasFilter;

        public override string AggregationFunctionName => _minMaxTypeEnum.GetExpressionText();

        public bool IsEver => _isEver;

        protected override bool IsFilterExpressionAsLastParameter => true;
    }
} // end of namespace
