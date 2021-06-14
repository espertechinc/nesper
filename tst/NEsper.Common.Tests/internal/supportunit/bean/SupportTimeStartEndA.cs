///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportTimeStartEndA : SupportTimeStartBase
    {
        public SupportTimeStartEndA(
            string key,
            string datestr,
            long duration)
            : base(key, datestr, duration)
        {
        }

        public static SupportTimeStartEndA Make(
            string key,
            string datestr,
            long duration)
        {
            return new SupportTimeStartEndA(key, datestr, duration);
        }
    }
} // end of namespace
