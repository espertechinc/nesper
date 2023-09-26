///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterAddendumUtil
    {
        public static FilterValueSetParam[][] AddAddendum(
            FilterValueSetParam[][] filters,
            FilterValueSetParam toAdd)
        {
            return AddAddendum(filters, new[] { toAdd });
        }

        public static FilterValueSetParam[][] AddAddendum(
            FilterValueSetParam[][] filters,
            FilterValueSetParam[] toAdd)
        {
            if (filters.Length == 0) {
                filters = new FilterValueSetParam[1][];
                filters[0] = Array.Empty<FilterValueSetParam>();
            }

            var @params = new FilterValueSetParam[filters.Length][];
            for (var i = 0; i < @params.Length; i++) {
                @params[i] = Append(filters[i], toAdd);
            }

            return @params;
        }

        public static FilterValueSetParam[][] MultiplyAddendum(
            FilterValueSetParam[][] filtersFirst,
            FilterValueSetParam[][] filtersSecond)
        {
            if (filtersFirst == null || filtersFirst.Length == 0) {
                return filtersSecond;
            }

            if (filtersSecond == null || filtersSecond.Length == 0) {
                return filtersFirst;
            }

            var size = filtersFirst.Length * filtersSecond.Length;
            var result = new FilterValueSetParam[size][];

            var count = 0;
            foreach (var lineFirst in filtersFirst) {
                foreach (var lineSecond in filtersSecond) {
                    result[count] = Append(lineFirst, lineSecond);
                    count++;
                }
            }

            return result;
        }

        private static FilterValueSetParam[] Append(
            FilterValueSetParam[] first,
            FilterValueSetParam[] second)
        {
            var appended = new FilterValueSetParam[first.Length + second.Length];
            Array.Copy(first, 0, appended, 0, first.Length);
            Array.Copy(second, 0, appended, first.Length, second.Length);
            return appended;
        }
    }
} // end of namespace