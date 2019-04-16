///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.script.compiletime;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
    public class StatementSpecMapEnv
    {
        public StatementSpecMapEnv(
            ImportServiceCompileTime importService,
            VariableCompileTimeResolver variableCompileTimeResolver,
            Configuration configuration,
            ExprDeclaredCompileTimeResolver exprDeclaredCompileTimeResolver,
            ContextCompileTimeResolver contextCompileTimeResolver,
            TableCompileTimeResolver tableCompileTimeResolver,
            ScriptCompileTimeResolver scriptCompileTimeResolver,
            CompilerServices compilerServices)
        {
            ImportService = importService;
            VariableCompileTimeResolver = variableCompileTimeResolver;
            Configuration = configuration;
            ExprDeclaredCompileTimeResolver = exprDeclaredCompileTimeResolver;
            ContextCompileTimeResolver = contextCompileTimeResolver;
            TableCompileTimeResolver = tableCompileTimeResolver;
            ScriptCompileTimeResolver = scriptCompileTimeResolver;
            CompilerServices = compilerServices;
        }

        public ImportServiceCompileTime ImportService { get; }

        public VariableCompileTimeResolver VariableCompileTimeResolver { get; }

        public Configuration Configuration { get; }

        public ExprDeclaredCompileTimeResolver ExprDeclaredCompileTimeResolver { get; }

        public TableCompileTimeResolver TableCompileTimeResolver { get; }

        public ContextCompileTimeResolver ContextCompileTimeResolver { get; }

        public ScriptCompileTimeResolver ScriptCompileTimeResolver { get; }

        public CompilerServices CompilerServices { get; }
    }
} // end of namespace