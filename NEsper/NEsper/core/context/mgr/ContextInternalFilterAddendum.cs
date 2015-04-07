///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextInternalFilterAddendum
    {
        private readonly IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> _filterAddendum;

        public ContextInternalFilterAddendum()
        {
            _filterAddendum = new IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]>();
        }

        public FilterValueSetParam[][] GetFilterAddendum(FilterSpecCompiled filterSpecCompiled)
        {
            return _filterAddendum.Get(filterSpecCompiled);
        }

        public IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> FilterAddendum
        {
            get { return _filterAddendum; }
        }

        public ContextInternalFilterAddendum DeepCopy()
        {
            var copy = new ContextInternalFilterAddendum();
            foreach (var entry in _filterAddendum)
            {
                var copy2Dim = new FilterValueSetParam[entry.Value.Length][];
                copy.FilterAddendum[entry.Key] = copy2Dim;
                for (int ii = 0; ii < entry.Value.Length; ii++)
                {
                    var copyList = new FilterValueSetParam[entry.Value[ii].Length];
                    copy2Dim[ii] = copyList;
                    Array.Copy(entry.Value[ii], 0, copyList, 0, copyList.Length);
                }
                copy.FilterAddendum[entry.Key] = copy2Dim;
            }
            return copy;
        }
    }
}