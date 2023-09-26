///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.controller.initterm;
using com.espertech.esper.common.@internal.context.controller.keyed;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public interface ContextControllerFactoryForgeVisitor<T>
    {
        T Visit(ContextControllerCategoryFactoryForge forge);
        T Visit(ContextControllerInitTermFactoryForge forge);
        T Visit(ContextControllerHashFactoryForge forge);
        T Visit(ContextControllerKeyedFactoryForge forge);
    }
} // end of namespace