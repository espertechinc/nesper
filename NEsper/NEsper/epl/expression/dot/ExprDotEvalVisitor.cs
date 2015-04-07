///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.dot
{
    public interface ExprDotEvalVisitor
    {
        void VisitPropertySource();
        void VisitEnumeration(String name);
        void VisitMethod(String methodName);
        void VisitDateTime();
        void VisitUnderlyingEvent();
        void VisitUnderlyingEventColl();
        void VisitArraySingleItemSource();
        void VisitArrayLength();
    }
}
