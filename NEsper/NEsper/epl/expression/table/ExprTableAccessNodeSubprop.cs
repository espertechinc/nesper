///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public class ExprTableAccessNodeSubprop
        : ExprTableAccessNode
        , ExprEvaluator
        , ExprEvaluatorEnumeration
    {
        private readonly string _subpropName;
    
        private Type _bindingReturnType;
        [NonSerialized] private EPType _optionalEnumerationType;
        [NonSerialized] private ExprEvaluatorEnumerationGivenEvent _optionalPropertyEnumEvaluator;
    
        public ExprTableAccessNodeSubprop(string tableName, string subpropName)
            : base(tableName)
        {
            _subpropName = subpropName;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        protected override void ValidateBindingInternal(ExprValidationContext validationContext, TableMetadata tableMetadata)
                {
            ValidateGroupKeys(tableMetadata);
            var column = ValidateSubpropertyGetCol(tableMetadata, _subpropName);
            if (column is TableMetadataColumnPlain) {
                _bindingReturnType = tableMetadata.InternalEventType.GetPropertyType(_subpropName);
                var enumerationSource = ExprDotNodeUtility.GetPropertyEnumerationSource(_subpropName, 0, tableMetadata.InternalEventType, true, true);
                _optionalEnumerationType = enumerationSource.ReturnType;
                _optionalPropertyEnumEvaluator = enumerationSource.EnumerationGivenEvent;
            } else {
                var aggcol = (TableMetadataColumnAggregation) column;
                _optionalEnumerationType = aggcol.OptionalEnumerationType;
                _bindingReturnType = aggcol.Factory.ResultType;
            }
        }

        public Type ReturnType
        {
            get { return _bindingReturnType; }
        }

        public Object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QExprTableSubproperty(this, base.TableName, _subpropName);
                var result = base.Strategy.Evaluate(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
                InstrumentationHelper.Get().AExprTableSubproperty(result);
                return result;
            }

            return Strategy.Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            ToPrecedenceFreeEPLInternal(writer, _subpropName);
        }

        public string SubpropName
        {
            get { return _subpropName; }
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return EPTypeHelper.OptionalIsEventTypeColl(_optionalEnumerationType);
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams) 
        {
            return Strategy.EvaluateGetROCollectionEvents(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        public Type ComponentTypeCollection
        {
            get { return EPTypeHelper.OptionalIsComponentTypeColl(_optionalEnumerationType); }
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            return Strategy.EvaluateGetROCollectionScalar(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }
    
        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return EPTypeHelper.OptionalIsEventTypeSingle(_optionalEnumerationType);
        }
    
        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return Strategy.EvaluateGetEventBean(evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        public ExprEvaluatorEnumerationGivenEvent OptionalPropertyEnumEvaluator
        {
            get { return _optionalPropertyEnumEvaluator; }
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            var that = (ExprTableAccessNodeSubprop) other;
            return _subpropName.Equals(that._subpropName);
        }
    }
} // end of namespace
