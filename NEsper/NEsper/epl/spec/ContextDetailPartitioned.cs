///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailPartitioned : ContextDetail
    {
        public IList<ContextDetailPartitionItem> Items { get; private set; }
        public ContextDetailPartitioned(IList<ContextDetailPartitionItem> items)
        {
            Items = items;
        }

        public IList<FilterSpecCompiled> ContextDetailFilterSpecs
        {
            get
            {
                return Items.Select(item => item.FilterSpecCompiled).ToList();
            }
        }
    }
}
