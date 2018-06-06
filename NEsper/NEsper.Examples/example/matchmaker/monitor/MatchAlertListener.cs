///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using com.espertech.esper.compat.logging;
using NEsper.Examples.MatchMaker.eventbean;

namespace NEsper.Examples.MatchMaker.monitor
{
    public class MatchAlertListener
    {
        private List<MatchAlertBean> emittedList = new List<MatchAlertBean>();

        public void Emitted(Object obj)
        {
            Log.Info(".emitted Emitted object=" + obj);
            emittedList.Add((MatchAlertBean)obj);
        }

        public int Size
        {
            get { return emittedList.Count; }
        }

        public List<MatchAlertBean> EmittedList
        {
            get { return emittedList; }
        }

        public int GetAndClearEmittedCount()
        {
            int count = emittedList.Count;
            emittedList.Clear();
            return count;
        }

        public void ClearEmitted()
        {
            emittedList.Clear();
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
