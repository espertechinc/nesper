///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Use this class to provide a table reader wherein there is no need to write code that generates code,
    /// </summary>
    public class AggregationMultiFunctionTableReaderModeManaged : AggregationMultiFunctionTableReaderMode
    {
        private InjectionStrategy injectionStrategyTableReaderFactory;

        /// <summary>
        /// Returns the injection strategy for the aggregation table reader factory
        /// </summary>
        /// <returns>strategy</returns>
        public InjectionStrategy InjectionStrategyTableReaderFactory {
            get => injectionStrategyTableReaderFactory;
        }

        /// <summary>
        /// Sets the injection strategy for the aggregation table reader factory
        /// </summary>
        /// <param name="strategy">strategy</param>
        /// <returns>itself</returns>
        public AggregationMultiFunctionTableReaderModeManaged SetInjectionStrategyTableReaderFactory(
            InjectionStrategy strategy)
        {
            this.injectionStrategyTableReaderFactory = strategy;
            return this;
        }
    }
} // end of namespace