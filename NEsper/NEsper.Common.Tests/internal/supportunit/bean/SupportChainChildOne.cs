///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportChainChildOne
    {
        private readonly string text;
        private readonly int value;

        public SupportChainChildOne(
            string text,
            int value)
        {
            this.text = text;
            this.value = value;
        }

        public SupportChainChildTwo GetChildTwo(string append)
        {
            return new SupportChainChildTwo(text + append, 1 + value);
        }
    }
} // end of namespace
