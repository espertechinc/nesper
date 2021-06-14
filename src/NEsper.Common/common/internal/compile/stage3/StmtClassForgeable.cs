///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public interface StmtClassForgeable
    {
        string ClassName { get; }

        StmtClassForgeableType ForgeableType { get; }

        CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget);
    }
} // end of namespace