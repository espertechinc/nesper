///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public interface ExprDotEvalVisitor
    {
        void VisitPropertySource();
        void VisitEnumeration(string name);
        void VisitMethod(string methodName);
        void VisitDateTime();
        void VisitUnderlyingEvent();
        void VisitUnderlyingEventColl();
        void VisitArraySingleItemSource();
        void VisitArrayLength();
    }
}