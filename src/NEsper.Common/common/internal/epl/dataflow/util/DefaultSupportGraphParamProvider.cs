///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DefaultSupportGraphParamProvider : EPDataFlowOperatorParameterProvider
    {
        private readonly IDictionary<string, object> @params;

        public DefaultSupportGraphParamProvider(IDictionary<string, object> @params)
        {
            this.@params = @params;
        }

        public object Provide(EPDataFlowOperatorParameterProviderContext context)
        {
            return @params.Get(context.ParameterName);
        }
    }
} // end of namespace