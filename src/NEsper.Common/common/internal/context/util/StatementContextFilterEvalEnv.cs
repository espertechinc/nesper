///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementContextFilterEvalEnv
    {
        public StatementContextFilterEvalEnv(
            ImportServiceRuntime importServiceRuntime,
            Attribute[] annotations,
            VariableManagementService variableManagementService,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            ImportServiceRuntime = importServiceRuntime;
            Annotations = annotations;
            VariableManagementService = variableManagementService;
            TableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public ImportServiceRuntime ImportServiceRuntime { get; }

        public Attribute[] Annotations { get; }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        public VariableManagementService VariableManagementService { get; }
    }
} // end of namespace