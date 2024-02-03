///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    ///     Options class passed to #GetModuleOrder(ICollection, ModuleOrderOptions)} for controlling the behavior of
    ///     ordering and dependency checking logic.
    /// </summary>
    public class ModuleOrderOptions
    {
        private bool checkCircularDependency = true;
        private bool checkUses = true;

        /// <summary>
        ///     Returns true (the default) to indicate that the algorithm checks for circular dependencies among the
        ///     uses-dependency graph,
        ///     or false to not perform this check.
        /// </summary>
        /// <value>indicator.</value>
        public bool IsCheckCircularDependency {
            get => checkCircularDependency;
            set => checkCircularDependency = value;
        }

        /// <summary>
        ///     Returns true (the default) to cause the algorithm to check uses-dependencies ensuring all dependencies are
        ///     satisfied i.e.
        ///     all dependent modules are either deployed or are part of the modules passed in, or false to not perform the
        ///     checking.
        /// </summary>
        /// <value>indicator</value>
        public bool IsCheckUses {
            get => checkUses;
            set => checkUses = value;
        }
    }
} // end of namespace