///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public abstract class ContextControllerForgeBase : ContextControllerFactoryForge
    {
        public ContextControllerForgeBase(ContextControllerFactoryEnv ctx)
        {
            FactoryEnv = ctx;
        }

        public ContextControllerFactoryEnv FactoryEnv { get; }

        public abstract ContextControllerPortableInfo ValidationInfo { get; }

        public abstract void ValidateGetContextProps(
            LinkedHashMap<string, object> props, string contextName, StatementRawInfo statementRawInfo, StatementCompileTimeServices services);

        public abstract CodegenMethod MakeCodegen(CodegenClassScope classScope, CodegenMethodScope parent, SAIFFInitializeSymbol symbols);
    }
} // end of namespace