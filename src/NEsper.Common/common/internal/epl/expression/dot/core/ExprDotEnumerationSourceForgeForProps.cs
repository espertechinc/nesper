///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotEnumerationSourceForgeForProps : ExprDotEnumerationSourceForge
    {
        public ExprDotEnumerationSourceForgeForProps(
            ExprEnumerationForge enumeration,
            EPChainableType returnType,
            int? streamOfProviderIfApplicable,
            ExprEnumerationGivenEventForge enumerationGivenEvent)
            : base(returnType, streamOfProviderIfApplicable, enumeration)
        {
            EnumerationGivenEvent = enumerationGivenEvent;
        }

        public ExprEnumerationGivenEventForge EnumerationGivenEvent { get; }
    }
} // end of namespace