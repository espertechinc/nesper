///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodHelper
    {
        protected internal static void ValidateFAFQuery(StatementSpecCompiled statementSpec)
        {
            for (int i = 0; i < statementSpec.StreamSpecs.Length; i++) {
                StreamSpecCompiled streamSpec = statementSpec.StreamSpecs[i];
                if (!(streamSpec is NamedWindowConsumerStreamSpec || streamSpec is TableQueryStreamSpec)) {
                    throw new ExprValidationException("On-demand queries require tables or named windows and do not allow event streams or patterns");
                }

                if (streamSpec.ViewSpecs.Length != 0) {
                    throw new ExprValidationException("Views are not a supported feature of on-demand queries");
                }
            }

            if (statementSpec.Raw.OutputLimitSpec != null) {
                throw new ExprValidationException("Output rate limiting is not a supported feature of on-demand queries");
            }
        }
    }
} // end of namespace