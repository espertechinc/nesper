///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    /// A callback interface for indicating a change in variable value.
    /// </summary>
    public interface VariableChangeCallback
    {
        /// <summary>
        /// Indicate a change in variable value.
        /// </summary>
        /// <param name="newValue">new value</param>
        /// <param name="oldValue">old value</param>
        void Update(
            object newValue,
            object oldValue);
    }

    public class ProxyVariableChangeCallback : VariableChangeCallback
    {
        public Action<object, object> ProcUpdate;

        public ProxyVariableChangeCallback(Action<object, object> procUpdate)
        {
            ProcUpdate = procUpdate;
        }

        public ProxyVariableChangeCallback()
        {
        }

        public void Update(
            object newValue,
            object oldValue)
        {
            ProcUpdate?.Invoke(newValue, oldValue);
        }
    }
} // end of namespace