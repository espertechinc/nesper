///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleProviderCLPair
    {
        public ModuleProviderCLPair(
            TypeResolver typeResolver,
            ModuleProvider moduleProvider)
        {
            TypeResolver = typeResolver;
            ModuleProvider = moduleProvider;
        }

        public TypeResolver TypeResolver { get; }

        public ModuleProvider ModuleProvider { get; }
    }
} // end of namespace