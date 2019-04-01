///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public class AdvancedIndexEvaluationHelper
    {
        public static double EvalDoubleColumn(ExprEvaluator col, string indexName, string colName, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            Number number = col.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (number == null)
            {
                throw InvalidColumnValue(indexName, colName, null, "non-null");
            }
            return number.DoubleValue();
        }

        public static double EvalDoubleParameter(ExprEvaluator param, string indexName, string parameterName, ExprEvaluatorContext exprEvaluatorContext)
        {
            Number number = param.Evaluate(null, true, exprEvaluatorContext);
            if (number == null)
            {
                throw InvalidParameterValue(indexName, parameterName, null, "non-null");
            }
            return number.DoubleValue();
        }

        public static int EvalIntParameter(ExprEvaluator param, string indexName, string parameterName, ExprEvaluatorContext exprEvaluatorContext)
        {
            int? number = (int?)param.Evaluate(null, true, exprEvaluatorContext);
            if (number == null)
            {
                throw InvalidParameterValue(indexName, parameterName, null, "non-null");
            }
            return number;
        }

        public static EPException InvalidParameterValue(string indexName, string parameterName, object value, string expected)
        {
            return new EPException("Invalid value for index '" + indexName + "' parameter '" + parameterName + "' received " + (value == null ? "null" : value.ToString()) + " and expected " + expected);
        }

        public static EPException InvalidColumnValue(string indexName, string parameterName, object value, string expected)
        {
            return new EPException("Invalid value for index '" + indexName + "' column '" + parameterName + "' received " + (value == null ? "null" : value.ToString()) + " and expected " + expected);
        }
    }
} // end of namespace