///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.view.core
{
    public interface ViewForgeVisitor
    {
        void Visit(ViewFactoryForge forge);
    }

    public class ProxyViewForgeVisitor : ViewForgeVisitor
    {
        public Action<ViewFactoryForge> ProcVisit;

        public ProxyViewForgeVisitor()
        {
        }

        public ProxyViewForgeVisitor(Action<ViewFactoryForge> procVisit)
        {
            ProcVisit = procVisit;
        }

        public void Visit(ViewFactoryForge forge)
        {
            ProcVisit.Invoke(forge);
        }
    }
} // end of namespace