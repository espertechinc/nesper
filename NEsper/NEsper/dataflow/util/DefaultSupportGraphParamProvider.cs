///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.dataflow.util
{
    public class DefaultSupportGraphParamProvider : EPDataFlowOperatorParameterProvider
    {
        private readonly IDictionary<String, Object> _params;

        public DefaultSupportGraphParamProvider(IDictionary<String, Object> @params)
        {
            _params = @params;
        }

        public Object Provide(EPDataFlowOperatorParameterProviderContext context)
        {
            return _params.Get(context.ParameterName);
        }
    }
}
