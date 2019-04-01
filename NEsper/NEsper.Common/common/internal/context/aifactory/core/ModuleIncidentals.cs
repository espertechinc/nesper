///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class ModuleIncidentals
    {
        public ModuleIncidentals(
            IDictionary<string, NamedWindowMetaData> namedWindows, IDictionary<string, ContextMetaData> contexts,
            IDictionary<string, VariableMetaData> variables, IDictionary<string, ExpressionDeclItem> expressions,
            IDictionary<string, TableMetaData> tables)
        {
            NamedWindows = namedWindows;
            Contexts = contexts;
            Variables = variables;
            Expressions = expressions;
            Tables = tables;
        }

        public IDictionary<string, NamedWindowMetaData> NamedWindows { get; }

        public IDictionary<string, ContextMetaData> Contexts { get; }

        public IDictionary<string, VariableMetaData> Variables { get; }

        public IDictionary<string, ExpressionDeclItem> Expressions { get; }

        public IDictionary<string, TableMetaData> Tables { get; }
    }
} // end of namespace