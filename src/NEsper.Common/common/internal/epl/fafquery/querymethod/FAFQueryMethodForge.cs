///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public interface FAFQueryMethodForge
    {
        IList<StmtClassForgeable> MakeForgeables(
            string queryMethodProviderClassName,
            string classPostfix,
            CodegenNamespaceScope namespaceScope);

        void MakeMethod(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);
    }
} // end of namespace