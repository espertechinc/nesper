///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1
{
    public interface CompilerServices
    {
        StatementSpecRaw ParseWalk(
            string epl,
            StatementSpecMapEnv mapEnv);

        string LexSampleSQL(string querySQL);

        ExprNode CompileExpression(
            string expression,
            StatementCompileTimeServices services);
        
        Type CompileStandInClass(
            CodegenClassType classType,
            string classNameSimple,
            ModuleCompileTimeServices services);

        CompileResponse Compile(CompileRequest request);
    }
} // end of namespace