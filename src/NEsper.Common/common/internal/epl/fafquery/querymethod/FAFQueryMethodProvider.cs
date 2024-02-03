///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     FAF query execute.
    /// </summary>
    public interface FAFQueryMethodProvider
    {
        FAFQueryMethod QueryMethod { get; }
        FAFQueryInformationals QueryInformationals { get; }
        FAFQueryMethodAssignerSetter SubstitutionFieldSetter { get; }
    }
} // end of namespace