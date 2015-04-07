///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEnumerationSource
    {
        public ExprDotEnumerationSource(EPType returnType, int? streamOfProviderIfApplicable, ExprEvaluatorEnumeration enumeration)
        {
            ReturnType = returnType;
            StreamOfProviderIfApplicable = streamOfProviderIfApplicable;
            Enumeration = enumeration;
        }

        public ExprEvaluatorEnumeration Enumeration { get; private set; }

        public EPType ReturnType { get; private set; }

        public int? StreamOfProviderIfApplicable { get; private set; }
    }
}
