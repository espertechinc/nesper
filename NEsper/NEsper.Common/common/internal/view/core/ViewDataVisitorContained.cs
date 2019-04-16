///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.view.core
{
    public interface ViewDataVisitorContained
    {
        void VisitPrimary(
            String viewName,
            int numContained);

        void VisitContained(
            Object containedKey,
            View containedView);
    }
}