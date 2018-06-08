///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Denotes a value for use by the in-keyword within a list of values
    /// </summary>
    public interface FilterSpecParamInValue : FilterSpecParamFilterForEval
    {
        Type ReturnType { get; }
        bool IsConstant { get; }
    }
} // end of namespace
