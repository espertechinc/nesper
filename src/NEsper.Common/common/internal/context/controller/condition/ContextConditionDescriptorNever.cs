///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextConditionDescriptorNever : ContextConditionDescriptor
    {
        public static readonly ContextConditionDescriptorNever INSTANCE = new ContextConditionDescriptorNever();

        private ContextConditionDescriptorNever()
        {
        }

        public void AddFilterSpecActivatable(IList<FilterSpecActivatable> activatables)
        {
            // none
        }
    }
} // end of namespace