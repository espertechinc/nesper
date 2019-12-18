///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    public sealed class TrackedDisposable : IDisposable
    {
        private readonly Action _actionOnDispose;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedDisposable"/> class.
        /// </summary>
        /// <param name="actionOnDispose">The action on dispose.</param>
        public TrackedDisposable(Action actionOnDispose)
        {
            _actionOnDispose = actionOnDispose;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _actionOnDispose.Invoke();
        }
    }
}
