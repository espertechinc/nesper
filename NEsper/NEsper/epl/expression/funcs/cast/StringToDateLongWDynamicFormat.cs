///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateLongWDynamicFormat : CasterParserComputer
    {
        private readonly ExprEvaluator _dateFormatEval;

        protected StringToDateLongWDynamicFormat(ExprEvaluator dateFormatEval)
        {
            _dateFormatEval = dateFormatEval;
        }

        protected abstract Object ComputeFromFormat(string dateFormat, string format, Object input);

        public bool IsConstantForConstInput()
        {
            return false;
        }

        public Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var format = _dateFormatEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (format == null)
            {
                throw new EPException("Null date format returned by 'dateformat' expression");
            }
            string dateFormat;
            try
            {
                dateFormat = format.ToString();
            }
            catch (Exception ex)
            {
                throw new EPException("Invalid date format '" + format + "': " + ex.Message, ex);
            }

            try
            {
                return ComputeFromFormat(format.ToString(), dateFormat, input);
            }
            catch (ParseException ex)
            {
                throw HandleParseException(format.ToString(), input.ToString(), ex);
            }
        }
    }
}
