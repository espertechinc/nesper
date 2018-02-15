///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportregression.epl
{
    public class SupportMethodInvocationJoinInvalid
    {
        public static IDictionary<string,object> ReadRowNoMetadata()
        {
            return null;
        }

        public static IDictionary<string, object> ReadRowWrongMetadata()
        {
            return null;
        }
    
        public static object ReadRowWrongMetadataMetadata()
        {
            return null;
        }
    }
}
