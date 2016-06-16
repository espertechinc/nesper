///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ContextDetailHash : ContextDetail
    {
        public ContextDetailHash(IList<ContextDetailHashItem> items, int granularity, bool preallocate)
        {
            Items = items;
            IsPreallocate = preallocate;
            Granularity = granularity;
        }

        public IList<ContextDetailHashItem> Items { get; private set; }

        public bool IsPreallocate { get; private set; }

        public int Granularity { get; private set; }

        public IList<FilterSpecCompiled> ContextDetailFilterSpecs
        {
            get { return Items.Select(item => item.FilterSpecCompiled).ToList(); }
        }
    }
}