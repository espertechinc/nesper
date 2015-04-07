///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.support.epl
{
    using DataMap = IDictionary<string, object>;
    
    public class SupportJoinMethods
    {
        public static DataMap[] FetchVal(String prefix, int? number)
        {
            if ((number == null) || (number == 0))
            {
                return new DataMap[0];
            }

            var result = new DataMap[number.Value];
            for (int i = 0; i < number; i++)
            {
                result[i] = new Dictionary<string, object>();
                result[i]["val"] = prefix + Convert.ToString(i + 1);
                result[i]["index"] = i + 1;
            }
    
            return result;
        }

        public static DataMap[] FetchValMultiRow(String prefix, int? number, int? numRowsPerIndex)
        {
            if ((number == null) || (number == 0))
            {
                return new DataMap[0];
            }
    
            int rows = number.Value;
            if (numRowsPerIndex > 1)
            {
                rows *= numRowsPerIndex.Value;
            }

            var result = new DataMap[rows];
            int count = 0;
            for (int i = 0; i < number; i++)
            {
                for (int j = 0; j < numRowsPerIndex; j++)
                {
                    result[count] = new Dictionary<string, object>();
                    result[count]["val"] = prefix + Convert.ToString(i + 1) + "_" + j;
                    result[count]["index"] = i + 1;
                    count++;
                }
            }
    
            return result;
        }

        public static IDictionary<String, Object> FetchValMultiRowMetadata()
        {
            return FetchValMetadata();
        }

        public static IDictionary<String, Object> FetchValMetadata()
        {
            IDictionary<String, Object> values = new Dictionary<String, Object>();
            values["val"] = typeof(string);
            values["index"] = typeof(int?);
            return values;
        }
    }
}
