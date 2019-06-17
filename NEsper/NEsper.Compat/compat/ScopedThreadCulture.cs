///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Globalization;
using System.Threading;

namespace com.espertech.esper.compat
{
    public class ScopedThreadCulture : IDisposable
    {
        private readonly CultureInfo previous;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedThreadCulture"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        public ScopedThreadCulture(CultureInfo culture)
        {
            previous = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = culture;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }

        #endregion
    }
}
