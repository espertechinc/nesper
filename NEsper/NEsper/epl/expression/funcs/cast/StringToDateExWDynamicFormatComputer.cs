///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateExWDynamicFormatComputer : CasterParserComputer
    {
        private ExprEvaluator dateFormatEval;

        public StringToDateExWDynamicFormatComputer(ExprEvaluator dateFormatEval)
        {
        }

        public bool IsConstantForConstInput()
        {
            return true;
        }

        public abstract Object Parse(string input);

        public Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var format = dateFormatEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (format == null)
            {
                throw new EPException("Null date format returned by 'dateformat' expression");
            }
            DateTimeFormatter dateFormat;
            try
            {
                dateFormat = DateTimeFormatter.OfPattern(format.ToString());
            }
            catch (Exception ex)
            {
                throw new EPException("Invalid date format '" + format.ToString() + "': " + ex.Message, ex);
            }
            try
            {
                return Parse(input.ToString(), dateFormat);
            }
            catch (DateTimeParseException e)
            {
                throw HandleParseException(dateFormat.ToString(), input.ToString(), e);
            }
        }
    }
   
}
