///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportEventInnerTypeWGetIds
    {
        public SupportEventInnerTypeWGetIds(int[] ids)
        {
            Ids = ids;
        }

        public int[] Ids { get; }

        public int GetIds(int subkey)
        {
            return Ids[subkey];
        }

        public int GetIds(
            EventBean @event,
            string name)
        {
            return 999999;
        }
    }
} // end of namespace