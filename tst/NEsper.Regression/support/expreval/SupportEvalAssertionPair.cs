///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.expreval
{
    public class SupportEvalAssertionPair
    {
        public SupportEvalAssertionPair(
            object underlying,
            SupportEvalAssertionBuilder builder)
        {
            Underlying = underlying;
            Builder = builder;
        }

        public object Underlying { get; }

        public SupportEvalAssertionBuilder Builder { get; }
    }
} // end of namespace