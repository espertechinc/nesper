///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ContextDetailConditionFilter : ContextDetailCondition
    {
        [NonSerialized]
        private FilterSpecCompiled _filterSpecCompiled;

        public ContextDetailConditionFilter(FilterSpecRaw filterSpecRaw, String optionalFilterAsName)
        {
            FilterSpecRaw = filterSpecRaw;
            OptionalFilterAsName = optionalFilterAsName;
        }

        public FilterSpecRaw FilterSpecRaw { get; private set; }

        public string OptionalFilterAsName { get; private set; }

        public FilterSpecCompiled FilterSpecCompiled
        {
            get { return _filterSpecCompiled; }
            set { _filterSpecCompiled = value; }
        }

        public IList<FilterSpecCompiled> FilterSpecIfAny
        {
            get
            {
                var list = new List<FilterSpecCompiled>(1);
                list.Add(_filterSpecCompiled);
                return list;
            }
        }
    }
}
