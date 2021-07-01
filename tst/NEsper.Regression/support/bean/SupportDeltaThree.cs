namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportDeltaThree
    {
        public SupportDeltaThree(
            string k0,
            string p0,
            string p4)
        {
            K0 = k0;
            P0 = p0;
            P4 = p4;
        }

        public string K0 { get; }

        public string P0 { get; }

        public string P4 { get; }
    }
} // end of namespace