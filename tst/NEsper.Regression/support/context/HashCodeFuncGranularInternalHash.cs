///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.context
{
    public class HashCodeFuncGranularInternalHash : HashCodeFunc
    {
        private readonly int granularity;

        public HashCodeFuncGranularInternalHash(int granularity)
        {
            this.granularity = granularity;
        }

        public int CodeFor(string key)
        {
            return key.GetHashCode() % granularity;
        }
    }
} // end of namespace