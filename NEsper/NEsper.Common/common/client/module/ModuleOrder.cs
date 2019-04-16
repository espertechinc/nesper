///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Holds an ordered list of modules considering each module's uses-dependencies on other modules.
    /// </summary>
    [Serializable]
    public class ModuleOrder
    {
        private IList<Module> ordered;

        /// <summary>
        /// Cotr.
        /// </summary>
        /// <param name="ordered">list of modules</param>
        public ModuleOrder(IList<Module> ordered)
        {
            this.ordered = ordered;
        }

        /// <summary>
        /// Returns the list of modules.
        /// </summary>
        /// <returns>modules</returns>
        public IList<Module> GetOrdered()
        {
            return ordered;
        }
    }
} // end of namespace