///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprFilterSpecLookupable
    {
        private readonly String _expression;
        [NonSerialized] private readonly ExprEventEvaluator _eval;
        private readonly Type _returnType;
        private readonly bool _isNonPropertyEval;
        private readonly DataInputOutputSerde _valueSerde;
        [NonSerialized] private readonly ExprEvaluator _expr;

        public ExprFilterSpecLookupable(
            String expression,
            ExprEventEvaluator eval,
            ExprEvaluator expr,
            Type returnType,
            bool isNonPropertyEval,
            DataInputOutputSerde valueSerde)
        {
            _expression = expression;
            _eval = eval;
            _expr = expr;
            _returnType = Boxing.GetBoxedType(returnType); // For type consistency for recovery and serde define as boxed type
            _isNonPropertyEval = isNonPropertyEval;
            _valueSerde = valueSerde;
        }

        public string Expression => _expression;

        public ExprEventEvaluator Eval => _eval;

        public Type ReturnType => _returnType;
                
        public bool IsNonPropertyEval => _isNonPropertyEval;

        public DataInputOutputSerde ValueSerde => ValueSerde;

        public ExprEvaluator Expr => _expr;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprFilterSpecLookupable) o;
            return _expression.Equals(that._expression);
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }

        public void AppendTo(TextWriter writer)
        {
            writer.Write(Expression);
        }

        public override string ToString()
        {
            return $"ExprFilterSpecLookupable{{expression='{Expression}'}}";
        }

        public ExprFilterSpecLookupable Make(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext) {
            // this lookupable does not depend on matched-events or evaluation-context
            // we allow it to be a factory of itself
            return this;
        }
    }
} // end of namespace