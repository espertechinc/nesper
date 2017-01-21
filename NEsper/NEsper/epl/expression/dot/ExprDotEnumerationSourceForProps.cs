///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEnumerationSourceForProps : ExprDotEnumerationSource
    {
        public ExprDotEnumerationSourceForProps(ExprEvaluatorEnumeration enumeration, EPType returnType, int? streamOfProviderIfApplicable, ExprEvaluatorEnumerationGivenEvent enumerationGivenEvent)
            : base(returnType, streamOfProviderIfApplicable, enumeration)
        {
            EnumerationGivenEvent = enumerationGivenEvent;
        }

        public ExprEvaluatorEnumerationGivenEvent EnumerationGivenEvent { get; private set; }
    }
}
