///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportTopGroupSubGroupEvent
    {
        public SupportTopGroupSubGroupEvent(
            int topgroup,
            int subgroup)
        {
            Topgroup = topgroup;
            Subgroup = subgroup;
            Op = null;
        }

        public SupportTopGroupSubGroupEvent(
            int topgroup,
            int subgroup,
            string op)
        {
            Topgroup = topgroup;
            Subgroup = subgroup;
            Op = op;
        }

        public int Topgroup { get; }

        public int Subgroup { get; }

        public string Op { get; }
    }
} // end of namespace