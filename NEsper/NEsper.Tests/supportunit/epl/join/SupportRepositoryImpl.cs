///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.join.rep;


namespace com.espertech.esper.supportunit.epl.join
{
    public class SupportRepositoryImpl : Repository
    {
        private IList<Cursor> cursorList = new List<Cursor>();
        private IList<ICollection<EventBean>> lookupResultsList = new List<ICollection<EventBean>>();
        private IList<int?> resultStreamList = new List<int?>();
    
        public IEnumerator<Cursor> GetCursors(int lookupStream)
        {
            yield return new Cursor(SupportJoinResultNodeFactory.MakeEvent(), 0, null);
        }
    
        public void AddResult(Cursor cursor, ICollection<EventBean> lookupResults, int resultStream)
        {
            cursorList.Add(cursor);
            lookupResultsList.Add(lookupResults);
            resultStreamList.Add(resultStream);
        }

        public IList<Cursor> CursorList
        {
            get { return cursorList; }
        }

        public IList<ICollection<EventBean>> LookupResultsList
        {
            get { return lookupResultsList; }
        }

        public IList<int?> ResultStreamList
        {
            get { return resultStreamList; }
        }
    }
}
