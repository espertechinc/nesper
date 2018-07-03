///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.index.service
{
    public class AdvancedIndexEvaluationHelper
    {
        public static double EvalDoubleColumn(
            ExprEvaluator col, string indexName, string colName,
            EventBean[] eventsPerStream, bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var number = col.Evaluate(new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext));
            if (number == null) {
                throw InvalidColumnValue(indexName, colName, null, "non-null");
            }

            return number.AsDouble();
        }

        public static double EvalDoubleParameter(
            ExprEvaluator param, string indexName, string parameterName,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var number = param.Evaluate(new EvaluateParams(null, true, exprEvaluatorContext));
            if (number == null) throw InvalidParameterValue(indexName, parameterName, null, "non-null");
            return number.AsDouble();
        }

        public static int EvalIntParameter(ExprEvaluator param, string indexName, string parameterName,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var number = param.Evaluate(new EvaluateParams(null, true, exprEvaluatorContext));
            if (number == null) throw InvalidParameterValue(indexName, parameterName, null, "non-null");
            return number.AsInt();
        }

        public static EPException InvalidParameterValue(string indexName, string parameterName, object value,
            string expected)
        {
            return new EPException("Invalid value for index '" + indexName + "' parameter '" + parameterName +
                                   "' received " + (value == null ? "null" : value.ToString()) + " and expected " +
                                   expected);
        }

        public static EPException InvalidColumnValue(string indexName, string parameterName, object value,
            string expected)
        {
            return new EPException("Invalid value for index '" + indexName + "' column '" + parameterName +
                                   "' received " + (value == null ? "null" : value.ToString()) + " and expected " +
                                   expected);
        }
    }
} // end of namespace