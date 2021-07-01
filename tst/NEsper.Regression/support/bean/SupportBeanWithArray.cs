///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanWithArray
    {
        public SupportBeanWithArray(
            int indexNumber,
            int[] intarr)
        {
            IndexNumber = indexNumber;
            Intarr = intarr;
        }

        public int IndexNumber { get; }

        public int[] Intarr { get; }

        public string Id { get; }
    }
} // end of namespace