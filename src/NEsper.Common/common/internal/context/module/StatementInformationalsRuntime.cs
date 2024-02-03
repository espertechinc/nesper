///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.context.module
{
    public class StatementInformationalsRuntime
    {
        public string StatementNameCompileTime { get; set; }

        public bool IsAlwaysSynthesizeOutputEvents { get; set; }

        public string OptionalContextName { get; set; }

        public string OptionalContextModuleName { get; set; }

        public NameAccessModifier? OptionalContextVisibility { get; set; }

        public bool IsCanSelfJoin { get; set; }

        public bool HasSubquery { get; set; }

        public bool IsNeedDedup { get; set; }

        public Attribute[] Annotations { get; set; }

        public bool IsStateless { get; set; }

        public object UserObjectCompileTime { get; set; }

        public int NumFilterCallbacks { get; set; }

        public int NumScheduleCallbacks { get; set; }

        public int NumNamedWindowCallbacks { get; set; }

        public StatementType StatementType { get; set; }

        public int Priority { get; set; }

        public bool IsPreemptive { get; set; }

        public bool HasVariables { get; set; }

        public bool IsWritesToTables { get; set; }

        public bool HasTableAccess { get; set; }

        public Type[] SelectClauseTypes { get; set; }

        public string[] SelectClauseColumnNames { get; set; }

        public bool IsForClauseDelivery { get; set; }

        public ExprEvaluator GroupDeliveryEval { get; set; }

        public bool HasMatchRecognize { get; set; }

        public AuditProvider AuditProvider { get; set; } = AuditProviderDefault.INSTANCE;

        public bool IsInstrumented { get; set; }

        public InstrumentationCommon InstrumentationProvider { get; set; }

        public Type[] SubstitutionParamTypes { get; set; }

        public string InsertIntoLatchName { get; set; }

        public bool IsAllowSubscriber { get; set; }

        public IDictionary<StatementProperty, object> Properties { get; set; }

        public IDictionary<string, int> SubstitutionParamNames { get; set; }

        public ExpressionScriptProvided[] OnScripts { get; set; }
    }
} // end of namespace