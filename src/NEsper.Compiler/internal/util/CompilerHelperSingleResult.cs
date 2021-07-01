///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperSingleResult
    {
        public CompilerHelperSingleResult(
            StatementSpecRaw statementSpecRaw,
            ClassProvidedPrecompileResult classesInlined)
        {
            StatementSpecRaw = statementSpecRaw;
            ClassesInlined = classesInlined;
        }

        public StatementSpecRaw StatementSpecRaw { get; }

        public ClassProvidedPrecompileResult ClassesInlined { get; }
    }
} // end of namespace