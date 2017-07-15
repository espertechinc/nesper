///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs
{
    public class ExprCastNodeDateDesc
    {
        public ExprCastNodeDateDesc(string staticDateFormat, ExprEvaluator dynamicDateFormat, bool iso8601Format)
        {
            StaticDateFormat = staticDateFormat;
            DynamicDateFormat = dynamicDateFormat;
            Iso8601Format = iso8601Format;
        }

        public string StaticDateFormat { get; private set; }

        public ExprEvaluator DynamicDateFormat { get; private set; }

        public bool Iso8601Format { get; private set; }
    }
} // end of namespace
