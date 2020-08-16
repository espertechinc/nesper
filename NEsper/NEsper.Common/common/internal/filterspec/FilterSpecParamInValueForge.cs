///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSpecParamInValueForge : FilterSpecParamFilterForEvalForge
    {
        Type ReturnType { get; }

        bool IsConstant { get; }
        
#if INHERITED
        void ValueToString(StringBuilder @out);
#endif
    }
} // end of namespace