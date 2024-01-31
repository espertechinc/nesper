///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
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
            return $"ISupportBImpl{{ValueB='{B}', ValueBaseAB='{BaseAB}'}}";
        }
    }
} // end of namespace