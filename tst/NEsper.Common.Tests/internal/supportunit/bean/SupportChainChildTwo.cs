///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportChainChildTwo
    {
        public SupportChainChildTwo(
            string text,
            int value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; }

        public int Value { get; }
    }
} // end of namespace
