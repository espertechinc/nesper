///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.view
{
    public class ViewServiceCreateResult
    {
        public ViewServiceCreateResult(Viewable finalViewable, Viewable topViewable, IList<View> newViews)
        {
            FinalViewable = finalViewable;
            TopViewable = topViewable;
            NewViews = newViews;
        }

        public Viewable FinalViewable { get; private set; }

        public Viewable TopViewable { get; private set; }

        public IList<View> NewViews { get; private set; }
    }
}