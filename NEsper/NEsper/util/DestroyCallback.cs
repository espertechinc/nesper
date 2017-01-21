///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    /// <summary>
    /// General pupose callback to destroy a resource and free it's underlying resources.
    /// </summary>
    public interface DestroyCallback
    {
        /// <summary>
        /// Destroys the underlying resources.
        /// </summary>
        void Destroy();
    }

    public sealed class ProxyDestroyCallback : DestroyCallback
    {
        public Action ProcDestroy;

        public ProxyDestroyCallback() { }
        public ProxyDestroyCallback(Action procDestroy)
        {
            ProcDestroy = procDestroy;
        }

        public void Destroy()
        {
            ProcDestroy.Invoke();
        }
    }
}
