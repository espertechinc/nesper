///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class ISupportBImpl : ISupportB
    {
        public ISupportBImpl(
            string valueB,
            string valueBaseAB)
        {
            B = valueB;
            BaseAB = valueBaseAB;
        }

        public string B { get; }

        public string BaseAB { get; }

        public override string ToString()
        {
            return "ISupportBImpl{" +
                   "valueB='" +
                   B +
                   '\'' +
                   ", valueBaseAB='" +
                   BaseAB +
                   '\'' +
                   '}';
        }
    }
} // end of namespace