///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailCategory : ContextDetail
    {
        private readonly IList<ContextDetailCategoryItem> _items;
        private readonly FilterSpecRaw _filterSpecRaw;
    
        [NonSerialized] private FilterSpecCompiled _filterSpecCompiled;
        [NonSerialized] private FilterValueSetParam[][] _filterParamsCompiled;

        public ContextDetailCategory(IList<ContextDetailCategoryItem> items, FilterSpecRaw filterSpecRaw)
        {
            _items = items;
            _filterSpecRaw = filterSpecRaw;
        }

        public IList<FilterSpecCompiled> ContextDetailFilterSpecs
        {
            get
            {
                IList<FilterSpecCompiled> filters = new List<FilterSpecCompiled>(1);
                filters.Add(_filterSpecCompiled);
                return filters;
            }
        }

        public FilterSpecRaw FilterSpecRaw
        {
            get { return _filterSpecRaw; }
        }

        public IList<ContextDetailCategoryItem> Items
        {
            get { return _items; }
        }

        public FilterSpecCompiled FilterSpecCompiled
        {
            get { return _filterSpecCompiled; }
            set
            {
                _filterSpecCompiled = value;
                _filterParamsCompiled = _filterSpecCompiled.GetValueSet(null, null, null).Parameters;
            }
        }

        public FilterValueSetParam[][] FilterParamsCompiled
        {
            get { return _filterParamsCompiled; }
        }
    }
}
