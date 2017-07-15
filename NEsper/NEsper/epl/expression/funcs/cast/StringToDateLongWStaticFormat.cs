///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateLongWStaticFormat : CasterParserComputer
    {
        protected readonly string DateFormat;

        protected StringToDateLongWStaticFormat(string dateFormat)
        {
            DateFormat = dateFormat;
        }

        public bool IsConstantForConstInput
        {
            get { return true; }
        }

        public abstract object Compute(object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext);
    }
}
