namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportChainTop
    {
        public static SupportChainTop Make()
        {
            return new SupportChainTop();
        }

        public SupportChainChildOne GetChildOne(
            string text,
            int value)
        {
            return new SupportChainChildOne(text, value);
        }
    }
} // end of namespace