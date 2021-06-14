///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class ISupportABCImpl : ISupportA,
        ISupportB,
        ISupportC
    {
        public ISupportABCImpl(
            string valueA,
            string valueB,
            string valueBaseAB,
            string valueC)
        {
            A = valueA;
            B = valueB;
            BaseAB = valueBaseAB;
            C = valueC;
        }

        public string A { get; }

        public string BaseAB { get; }

        public string B { get; }

        public string C { get; }
    }
} // end of namespace
