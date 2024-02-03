///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;

namespace com.espertech.esper.compiler.@internal.util
{
    public interface EPCompilerSPI : EPCompiler
    {
        EPCompilerSPIExpression ExpressionCompiler(Configuration configuration);
    }
} // end of namespace