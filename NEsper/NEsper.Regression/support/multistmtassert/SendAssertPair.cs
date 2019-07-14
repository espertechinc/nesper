namespace com.espertech.esper.regressionlib.support.multistmtassert
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SendAssertPair
    {
        public SendAssertPair(
            EventSender sender,
            Asserter<object> asserter)
        {
            Sender = sender;
            Asserter = asserter;
        }

        public EventSender Sender { get; }

        public Asserter<object> Asserter { get; }
    }
} // end of namespace