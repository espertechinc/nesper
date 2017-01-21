///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.support.epl
{
    public class SupportChainFuncLib
    {
        public static SupportChainInner GetInner(int one, int two)
        {
            return new SupportChainInner(one, two);
        }

        public class SupportChainInner
        {
            private readonly int _sum;

            public SupportChainInner(int one, int two)
            {
                _sum = one + two;
            }

            public SupportChainInner Add(int one, int two)
            {
                return new SupportChainInner(_sum, one + two);
            }

            public int GetTotal()
            {
                return _sum;
            }
        }
    }
}
