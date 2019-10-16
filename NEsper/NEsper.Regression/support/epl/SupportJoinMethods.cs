///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Castle.Facilities.TypedFactory.Internal;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.epl
{
    public class SupportJoinMethods
    {
        private static readonly IDictionary<string, object>[] Empty = 
            new IDictionary<string, object>[0];
        public static IDictionary<string, object>[] FetchVal(
            string prefix,
            int? number)
        {
            if (number == null || number.Value == 0) {
                return Empty;
            }

            var result = new IDictionary<string, object>[number.Value];
            for (var i = 0; i < number; i++) {
                result[i] = new Dictionary<string, object> {
                    ["val"] = prefix + Convert.ToString(i + 1),
                    ["index"] = i + 1
                };
            }

            return result;
        }

        public static IDictionary<string, object>[] FetchValMultiRow(
            string prefix,
            int? number,
            int? numRowsPerIndex)
        {
            if (number == null || number == 0) {
                return new IDictionary<string, object>[0];
            }

            var rows = number.Value;
            if (numRowsPerIndex > 1) {
                rows *= numRowsPerIndex.Value;
            }

            var result = new IDictionary<string, object>[rows];
            var count = 0;
            for (var i = 0; i < number; i++) {
                for (var j = 0; j < numRowsPerIndex; j++) {
                    result[count] = new Dictionary<string, object>();
                    result[count].Put("val", prefix + Convert.ToString(i + 1) + "_" + j);
                    result[count].Put("index", i + 1);
                    count++;
                }
            }

            return result;
        }

        public static IDictionary<string, object> FetchValMultiRowMetadata()
        {
            return FetchValMetadata();
        }

        public static IDictionary<string, object> FetchValMetadata()
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            values.Put("val", typeof(string));
            values.Put("index", typeof(int?));
            return values;
        }
    }
} // end of namespace