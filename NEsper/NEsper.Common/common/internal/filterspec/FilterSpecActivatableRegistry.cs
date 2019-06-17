///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSpecActivatableRegistry
    {
        void Register(FilterSpecActivatable filterSpecActivatable);
    }

    public class ProxyFilterSpecActivatableRegistry : FilterSpecActivatableRegistry
    {
        public Action<FilterSpecActivatable> ProcRegister { get; set; }
        public void Register(FilterSpecActivatable filterSpecActivatable)
        {
            ProcRegister?.Invoke(filterSpecActivatable);
        }
    }
} // end of namespace