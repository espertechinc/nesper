///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.datetimemethod;
using com.espertech.esper.common.@internal.epl.datetime.eval;

namespace com.espertech.esper.common.@internal.epl.datetime.plugin
{
    public class DTMPluginForgeFactory : DatetimeMethodProviderForgeFactory
    {
        private readonly DateTimeMethodForgeFactory _forgeFactory;

        public DTMPluginForgeFactory(DateTimeMethodForgeFactory forgeFactory)
        {
            _forgeFactory = forgeFactory;
        }

        public DateTimeMethodOps Validate(DateTimeMethodValidateContext usageDesc)
        {
            return _forgeFactory.Validate(usageDesc);
        }
    }
} // end of namespace