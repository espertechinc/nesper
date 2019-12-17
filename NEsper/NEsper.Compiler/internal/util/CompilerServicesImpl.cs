///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerServicesImpl : CompilerServices
    {
        public StatementSpecRaw ParseWalk(
            string epl,
            StatementSpecMapEnv mapEnv)
        {
            return CompilerHelperSingleEPL.ParseWalk(epl, mapEnv);
        }

        public string LexSampleSQL(string querySQL)
        {
            return SQLLexer.LexSampleSQL(querySQL);
        }

        public ExprNode CompileExpression(
            string expression,
            StatementCompileTimeServices services)
        {
            var toCompile = "select * from System.Object#time(" + expression + ")";

            StatementSpecRaw raw;
            try {
                raw = services.CompilerServices.ParseWalk(toCompile, services.StatementSpecMapEnv);
            }
            catch (StatementSpecCompileException e) {
                throw new ExprValidationException(
                    "Failed to compile expression '" + expression + "': " + e.Expression,
                    e);
            }

            return raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
        }
    }
} // end of namespace