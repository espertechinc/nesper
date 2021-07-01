namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class ISupportBCImpl : ISupportB,
        ISupportC
    {
        public ISupportBCImpl(
            string valueB,
            string valueBaseAB,
            string valueC)
        {
            B = valueB;
            BaseAB = valueBaseAB;
            C = valueC;
        }

        public string B { get; }

        public string BaseAB { get; }

        public string C { get; }
    }
} // end of namespace