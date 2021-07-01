///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.@join.querygraph;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportQueryGraphTestUtil
    {
        public static object[] GetStrictKeyProperties(
            QueryGraphForge graph,
            int lookup,
            int indexed)
        {
            var val = graph.GetGraphValue(lookup, indexed);
            var pair = val.HashKeyProps;
            return pair.StrictKeys;
        }

        public static object[] GetIndexProperties(
            QueryGraphForge graph,
            int lookup,
            int indexed)
        {
            var val = graph.GetGraphValue(lookup, indexed);
            var pair = val.HashKeyProps;
            return pair.Indexed;
        }
    }
} // end of namespace
