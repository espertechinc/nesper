///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Coordinates between view factories and requested resource (by expressions) the availability of view resources to expressions.
    /// </summary>
    public class ViewResourceDelegateVerified
    {
        public ViewResourceDelegateVerified(bool hasPrior, bool hasPrevious, ViewResourceDelegateVerifiedStream[] perStream) {
            HasPrior = hasPrior;
            HasPrevious = hasPrevious;
            PerStream = perStream;
        }

        public ViewResourceDelegateVerifiedStream[] PerStream { get; private set; }

        public bool HasPrior { get; private set; }

        public bool HasPrevious { get; private set; }
    }
}
