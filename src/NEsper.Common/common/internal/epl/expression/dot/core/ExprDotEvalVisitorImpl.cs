///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotEvalVisitorImpl : ExprDotEvalVisitor
    {
        public string MethodType { get; private set; }

        public string MethodName { get; private set; }

        public void VisitPropertySource()
        {
            Set("property value", null);
        }

        public void VisitEnumeration(string name)
        {
            Set("enumeration method", name);
        }

        public void VisitMethod(string methodName)
        {
            Set("jvm method", methodName);
        }

        public void VisitDateTime()
        {
            Set("datetime method", null);
        }

        public void VisitUnderlyingEvent()
        {
            Set("underlying event", null);
        }

        public void VisitUnderlyingEventColl()
        {
            Set("underlying event collection", null);
        }

        public void VisitArraySingleItemSource()
        {
            Set("array item", null);
        }

        public void VisitArrayLength()
        {
            Set("array length", null);
        }

        private void Set(
            string methodType,
            string methodName)
        {
            MethodType = methodType;
            MethodName = methodName;
        }
    }
} // end of namespace