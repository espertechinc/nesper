///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.util
{
    public class CallbackAttributionMatchRecognize : CallbackAttribution
    {
        public static readonly CallbackAttributionMatchRecognize INSTANCE = new CallbackAttributionMatchRecognize();

        private CallbackAttributionMatchRecognize()
        {
        }

        public T Accept<T>(CallbackAttributionVisitor<T> visitor)
        {
            return visitor.Accept(this);
        }
    }
} // end of namespace