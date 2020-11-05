namespace com.espertech.esper.regressionlib.support.multistmtassert
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class EPLWithInvokedFlags
    {
        private readonly string epl;

        public EPLWithInvokedFlags(
            string epl,
            bool[] received)
        {
            this.epl = epl;
            Received = received;
        }

        public bool[] Received { get; }

        public string Epl()
        {
            return epl;
        }
    }
} // end of namespace