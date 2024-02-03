///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectActivationPlan
    {
        public SubSelectActivationPlan(
            EventType viewableType,
            IList<ViewFactoryForge> viewForges,
            ViewableActivatorForge activator,
            StreamSpecCompiled streamSpecCompiled)
        {
            ViewableType = viewableType;
            ViewForges = viewForges;
            Activator = activator;
            StreamSpecCompiled = streamSpecCompiled;
        }

        public EventType ViewableType { get; }

        public IList<ViewFactoryForge> ViewForges { get; }

        public ViewableActivatorForge Activator { get; }

        public StreamSpecCompiled StreamSpecCompiled { get; }
    }
} // end of namespace