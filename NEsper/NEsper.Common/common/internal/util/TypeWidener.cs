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

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Interface for a type widener.
    /// </summary>
    public interface TypeWidener
    {
        /// <summary>
        /// Widen input value.
        /// </summary>
        /// <param name="input">the object to widen.</param>
        /// <returns>widened object.</returns>
        object Widen(object input);
    }

    public class ProxyTypeWidener : TypeWidener
    {
        public Func<object, object> ProcWiden;
        public object Widen(object input) => ProcWiden(input);

        public ProxyTypeWidener()
        {
        }

        public ProxyTypeWidener(Func<object, object> procWiden)
        {
            ProcWiden = procWiden;
        }
    }
} // end of namespace