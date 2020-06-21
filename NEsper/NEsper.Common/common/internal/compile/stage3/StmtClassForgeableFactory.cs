///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public interface StmtClassForgeableFactory
    {
        StmtClassForgeable Make(
            CodegenNamespaceScope namespaceScope,
            string classPostfix);
    }

    public class ProxyStmtClassForgeableFactory : StmtClassForgeableFactory
    {
        public Func<CodegenNamespaceScope, string, StmtClassForgeable> ProcMake { get; set; }

        public ProxyStmtClassForgeableFactory()
        {
        }

        public ProxyStmtClassForgeableFactory(Func<CodegenNamespaceScope, string, StmtClassForgeable> procMake)
        {
            ProcMake = procMake;
        }

        public StmtClassForgeable Make(
            CodegenNamespaceScope namespaceScope,
            string classPostfix)
        {
            return ProcMake.Invoke(namespaceScope, classPostfix);
        }
    }
} // end of namespace