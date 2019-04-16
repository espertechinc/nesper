///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly ImportServiceRuntime _importServiceRuntime;
        private readonly Attribute[] annotations;
        private readonly VariableManagementService variableManagementService;
        private readonly TableExprEvaluatorContext tableExprEvaluatorContext;

        public StatementContextFilterEvalEnv(
            ImportServiceRuntime importServiceRuntime,
            Attribute[] annotations,
            VariableManagementService variableManagementService,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            this._importServiceRuntime = importServiceRuntime;
            this.annotations = annotations;
            this.variableManagementService = variableManagementService;
            this.tableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public ImportServiceRuntime ImportServiceRuntime {
            get => _importServiceRuntime;
        }

        public Attribute[] GetAnnotations()
        {
            return annotations;
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext {
            get => tableExprEvaluatorContext;
        }

        public VariableManagementService VariableManagementService {
            get => variableManagementService;
        }
    }
} // end of namespace