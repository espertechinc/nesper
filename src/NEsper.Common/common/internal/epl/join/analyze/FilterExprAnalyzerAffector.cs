///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.analyze
{
    public interface FilterExprAnalyzerAffector
    {
        void Apply(QueryGraphForge queryGraph);
    }
} // end of namespace