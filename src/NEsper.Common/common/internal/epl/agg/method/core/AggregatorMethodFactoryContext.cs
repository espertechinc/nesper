///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.method.plugin;


namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public class AggregatorMethodFactoryContext
    {
        private readonly AggregationForgeFactoryPlugin factory;

        public AggregatorMethodFactoryContext(AggregationForgeFactoryPlugin factory)
        {
            this.factory = factory;
        }

        public AggregationForgeFactoryPlugin Factory => factory;
    }
} // end of namespace