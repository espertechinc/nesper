///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerDetailHashItem
    {
        private FilterSpecActivatable filterSpecActivatable;
        private ExprFilterSpecLookupable lookupable;

        public FilterSpecActivatable FilterSpecActivatable {
            get => filterSpecActivatable;
        }

        public void SetFilterSpecActivatable(FilterSpecActivatable filterSpecActivatable)
        {
            this.filterSpecActivatable = filterSpecActivatable;
        }

        public ExprFilterSpecLookupable Lookupable {
            get => lookupable;
        }

        public void SetLookupable(ExprFilterSpecLookupable lookupable)
        {
            this.lookupable = lookupable;
        }
    }
} // end of namespace