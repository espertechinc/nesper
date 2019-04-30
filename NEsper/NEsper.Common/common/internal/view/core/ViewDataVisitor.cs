///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.view.core
{
    public interface ViewDataVisitor
    {
        void VisitPrimary(
            EventBean @event,
            string viewName);

        void VisitPrimary(
            EventBean[] events,
            string viewName);

        void VisitPrimary<K>(
            ICollection<K> primary,
            bool countsEvents,
            string viewName,
            int? count);

        void VisitPrimary<K, V>(
            IDictionary<K, V> currentBatch,
            bool countsEvents,
            string viewName,
            int? count,
            int? keyCountWhenAvailable);

        void VisitPrimary(
            ViewUpdatedCollection buffer,
            string viewName);
    }
}