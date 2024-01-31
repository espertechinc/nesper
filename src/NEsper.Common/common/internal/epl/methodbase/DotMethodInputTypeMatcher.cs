///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    public interface DotMethodInputTypeMatcher
    {
        public static readonly DotMethodInputTypeMatcher DEFAULT_ALL = new DotMethodInputTypeMatcherImpl();

        bool Matches(DotMethodFP footprint);
    }

    public sealed class DotMethodInputTypeMatcherImpl : DotMethodInputTypeMatcher
    {
        #region DotMethodInputTypeMatcher Members

        public bool Matches(DotMethodFP footprint)
        {
            return true;
        }

        #endregion
    }

    public sealed class ProxyDotMethodInputTypeMatcher : DotMethodInputTypeMatcher
    {
        public Func<DotMethodFP, bool> ProcMatches { get; set; }

        public bool Matches(DotMethodFP footprint)
        {
            return ProcMatches.Invoke(footprint);
        }
    }
}