///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.table
{
    [Serializable]
    public class ExprTableAccessNodeTopLevel
        : ExprTableAccessNode
        , ExprEvaluatorTypableReturn
    {
        [NonSerialized]
        private LinkedHashMap<string, object> _eventType;

        public ExprTableAccessNodeTopLevel(string tableName) : base(tableName)
        {
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        protected override void ValidateBindingInternal(ExprValidationContext validationContext, TableMetadata tableMetadata)
        {
            ValidateGroupKeys(tableMetadata);
            _eventType = new LinkedHashMap<string, object>();
            foreach (var entry in tableMetadata.TableColumns)
            {
                Type classResult;
                if (entry.Value is TableMetadataColumnPlain) {
                    classResult = tableMetadata.InternalEventType.GetPropertyType(entry.Key);
                }
                else
                {
                    TableMetadataColumnAggregation aggcol = (TableMetadataColumnAggregation) entry.Value;
                    classResult = TypeHelper.GetBoxedType(aggcol.Factory.ResultType);
                }
                _eventType.Put(entry.Key, classResult);
            }
        }

        public Type ReturnType
        {
            get { return typeof(IDictionary<string, object>); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QExprTableTop(this, TableName);
                object result = Strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                InstrumentationHelper.Get().AExprTableTop(result);
                return result;
            }
            return Strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public IDictionary<string, object> RowProperties
        {
            get { return _eventType; }
        }

        public bool? IsMultirow
        {
            get { return false; }
        }

        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return Strategy.EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }
    
        public object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw new UnsupportedOperationException();
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPLInternal(writer);
        }
    
        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            return true;
        }
    }
}
