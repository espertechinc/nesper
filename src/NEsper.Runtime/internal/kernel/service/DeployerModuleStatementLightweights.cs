///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.module;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerModuleStatementLightweights
    {
        public DeployerModuleStatementLightweights(
            int statementIdFirstStatement,
            IList<StatementLightweight> lightweights,
            IDictionary<int, IDictionary<int, object>> substitutionParameters)
        {
            StatementIdFirstStatement = statementIdFirstStatement;
            Lightweights = lightweights;
            SubstitutionParameters = substitutionParameters;
        }

        public IList<StatementLightweight> Lightweights { get; }

        public IDictionary<int, IDictionary<int, object>> SubstitutionParameters { get; }

        public int StatementIdFirstStatement { get; }
    }
} // end of namespace