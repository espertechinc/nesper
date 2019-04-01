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
    public interface ContextControllerFactoryForge
    {
        ContextControllerFactoryEnv FactoryEnv { get; }

        ContextControllerPortableInfo ValidationInfo { get; }

        void ValidateGetContextProps(
            LinkedHashMap<string, object> props, string contextName, StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services);

        CodegenMethod MakeCodegen(
            CodegenClassScope classScope, CodegenMethodScope parent, SAIFFInitializeSymbol symbols);
    }
} // end of namespace