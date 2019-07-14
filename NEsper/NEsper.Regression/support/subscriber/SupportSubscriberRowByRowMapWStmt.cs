///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberRowByRowMapWStmt : SupportSubscriberRowByRowMapBase
    {
        public SupportSubscriberRowByRowMapWStmt() : base(true)
        {
        }

        public void Update(
            EPStatement stmt,
            IDictionary<string, object> row)
        {
            AddIndicationIStream(stmt, row);
        }

        public void UpdateRStream(
            EPStatement stmt,
            IDictionary<string, object> row)
        {
            AddIndicationRStream(stmt, row);
        }
    }
} // end of namespace