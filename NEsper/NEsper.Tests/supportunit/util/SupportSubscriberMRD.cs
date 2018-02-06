///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.supportunit.util
{
    public class SupportSubscriberMRD
    {
        public SupportSubscriberMRD()
        {
            RemoveStreamList = new List<Object[][]>();
            InsertStreamList = new List<Object[][]>();
        }

        public void Update(Object[][] insertStream, Object[][] removeStream)
        {
            IsInvoked = true;
            InsertStreamList.Add(insertStream);
            RemoveStreamList.Add(insertStream);
        }

        public IList<object[][]> InsertStreamList { get; set; }

        public IList<object[][]> RemoveStreamList { get; set; }

        public void Reset()
        {
            IsInvoked = false;
            InsertStreamList.Clear();
            RemoveStreamList.Clear();
        }

        public bool IsInvoked { get; private set; }

        public bool GetAndClearIsInvoked()
        {
            bool invoked = IsInvoked;
            IsInvoked = false;
            return invoked;
        }
    }
}
