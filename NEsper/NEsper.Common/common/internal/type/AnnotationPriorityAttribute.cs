///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.type
{
    public class AnnotationPriorityAttribute : PriorityAttribute
    {
        private readonly int priority;

        public AnnotationPriorityAttribute(int priority)
        {
            this.priority = priority;
        }

        public override int Value => priority;

        public Type AnnotationType => typeof(PriorityAttribute);

        public override string ToString()
        {
            return "@Priority(\"" + priority + "\")";
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (AnnotationPriorityAttribute) o;

            return priority == that.priority;
        }

        public override int GetHashCode()
        {
            return priority;
        }
    }
} // end of namespace