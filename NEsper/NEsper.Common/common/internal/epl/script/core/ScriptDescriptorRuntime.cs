///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptDescriptorRuntime
    {
        public string OptionalDialect { get; set; }

        public string ScriptName { get; set; }

        public string Expression { get; set; }

        public string[] ParameterNames { get; set; }

        public Type[] EvaluationTypes { get; set; }

        public string DefaultDialect { get; set; }

        public ImportService ImportService { get; set; }

        public ImportService ClasspathImportService {
            get => ImportService;
            set => ImportService = value;
        }

        public ExprEvaluator[] Parameters { get; set; }

        public Coercer Coercer { get; set; }

#if false
        public SimpleNumberCoercer Coercer {
            get => SimpleNumberCoercer;
            set => SimpleNumberCoercer = value;
        }
#endif
    }
} // end of namespace