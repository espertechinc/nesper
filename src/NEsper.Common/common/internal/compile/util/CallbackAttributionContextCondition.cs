///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.util
{
    public class CallbackAttributionContextCondition : CallbackAttribution
    {
        public CallbackAttributionContextCondition(
            int nestingLevel,
            bool startCondition)
        {
            NestingLevel = nestingLevel;
            IsStartCondition = startCondition;
        }

        public int NestingLevel { get; }

        public bool IsStartCondition { get; }

        public T Accept<T>(CallbackAttributionVisitor<T> visitor)
        {
            return visitor.Accept(this);
        }
    }
} // end of namespace