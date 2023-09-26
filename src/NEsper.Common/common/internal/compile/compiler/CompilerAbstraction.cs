///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.compile.compiler
{
    public interface CompilerAbstraction
    {
        CompilerAbstractionClassCollection NewClassCollection();

        void CompileClasses(
            IList<CodegenClass> classes,
            CompilerAbstractionCompilationContext context,
            CompilerAbstractionClassCollection state);

        CompilerAbstractionCompileSourcesResult CompileSources(
            IList<string> sources,
            CompilerAbstractionCompilationContext context,
            CompilerAbstractionClassCollection state);
    }
} // end of namespace