///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.mgr;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public class ContextControllerCategoryImpl : ContextControllerCategory
    {
        public ContextControllerCategoryImpl(
            ContextManagerRealization realization,
            ContextControllerCategoryFactory factory)
            : base(realization, factory)
        {
            if (factory.FactoryEnv.IsRoot) {
                CategorySvc = new ContextControllerCategorySvcLevelOne();
            }
            else {
                CategorySvc = new ContextControllerCategorySvcLevelAny();
            }
        }
    }
} // end of namespace