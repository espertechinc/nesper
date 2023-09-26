///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public interface ContextSpecConditionVisitor<T>
    {
        T Visit(ContextSpecConditionCrontab crontab);
        T Visit(ContextSpecConditionFilter filter);
        T Visit(ContextSpecConditionPattern pattern);
        T Visit(ContextSpecConditionNever never);
        T Visit(ContextSpecConditionTimePeriod timePeriod);
        T Visit(ContextSpecConditionImmediate immediate);
    }
} // end of namespace