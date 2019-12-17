///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberRowByRowMapNStmt : SupportSubscriberRowByRowMapBase
    {
        public SupportSubscriberRowByRowMapNStmt() : base(false)
        {
        }

        public void Update(IDictionary<string, object> row)
        {
            AddIndicationIStream(row);
        }

        public void UpdateRStream(IDictionary<string, object> row)
        {
            AddIndicationRStream(row);
        }
    }
} // end of namespace