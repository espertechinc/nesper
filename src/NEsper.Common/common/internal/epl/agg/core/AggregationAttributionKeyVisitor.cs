///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationAttributionKeyVisitor<T>
    {
        T Visit(AggregationAttributionKeyStatement stmt);
        T Visit(AggregationAttributionKeySubselect subselect);
        T Visit(AggregationAttributionKeyView view);
        T Visit(AggregationAttributionKeyRowRecog rowrecog);
    }
} // end of namespace