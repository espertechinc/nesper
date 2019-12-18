namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportChainChildOne
    {
        private readonly string _text;
        private readonly int _value;

        public SupportChainChildOne(
            string text,
            int value)
        {
            _text = text;
            _value = value;
        }

        public SupportChainChildTwo GetChildTwo(string append)
        {
            return new SupportChainChildTwo(_text + append, 1 + _value);
        }
    }
} // end of namespace