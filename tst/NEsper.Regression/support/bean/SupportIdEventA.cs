///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportIdEventA
    {
        public SupportIdEventA(
            string id,
            string pa,
            int? mysec)
        {
            Id = id;
            Pa = pa;
            Mysec = mysec;
        }

        public string Id { get; }

        public string Pa { get; }

        public int? Mysec { get; }
    }
} // end of namespace