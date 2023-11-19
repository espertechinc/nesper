///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class GroupByClauseElementGroupingSet : GroupByClauseElement
    {
        public GroupByClauseElementGroupingSet(IList<GroupByClauseElement> elements)
        {
            Elements = elements;
        }

        public IList<GroupByClauseElement> Elements { get; private set; }
    }
}