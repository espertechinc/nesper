///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpUtil 
    {
        public static int? GetInt(ExprEvaluator expr, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) 
        {
            Object result = expr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            if (result == null) {
                return null;
            }
            return (int?) result;
        }
    
        public static CalendarFieldEnum GetEnum(String methodName, ExprNode exprNode)
        {
            var message = "Date-time enumeration method '" + methodName + "'";
            var validFieldNames = "valid field names are '" + Enum.GetNames(typeof(CalendarFieldEnum)) + "'";

            if (!ExprNodeUtility.IsConstantValueExpr(exprNode)) {
                throw new ExprValidationException(message + " requires a constant string-type parameter as its first parameter, " + validFieldNames);
            }

            var fieldname = (String) exprNode.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
            try
            {
                return CalendarFieldEnumExtensions.FromString(fieldname);
            }
            catch (ArgumentException)
            {
                throw new ExprValidationException(
                    message + " datetime-field name '" + fieldname + "' is not recognized, " + validFieldNames);
            }
        }
    }
}
