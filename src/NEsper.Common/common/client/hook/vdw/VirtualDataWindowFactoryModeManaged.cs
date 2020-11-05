///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.forgeinject;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    /// Use this class to provide a virtual data window factory wherein there is no need to write code that generates code.
    /// </summary>
    public class VirtualDataWindowFactoryModeManaged : VirtualDataWindowFactoryMode
    {
        private InjectionStrategy injectionStrategyFactoryFactory;

        /// <summary>
        /// Returns the injection strategy for the virtual data window factory
        /// </summary>
        /// <returns>strategy</returns>
        public InjectionStrategy InjectionStrategyFactoryFactory {
            get => injectionStrategyFactoryFactory;
        }

        /// <summary>
        /// Sets the injection strategy for the virtual data window factory
        /// </summary>
        /// <param name="strategy">strategy</param>
        /// <returns>itself</returns>
        public VirtualDataWindowFactoryModeManaged SetInjectionStrategyFactoryFactory(InjectionStrategy strategy)
        {
            this.injectionStrategyFactoryFactory = strategy;
            return this;
        }
    }
} // end of namespace