///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.bean
{
    public class SupportBeanArrayCollMap
    {
        private Object anyObject;

        public SupportBeanArrayCollMap()
        {
            anyObject = null;
        }

        public SupportBeanArrayCollMap(Object anyObject)
        {
            this.anyObject = anyObject;
        }

        public SupportBeanArrayCollMap(object[] objectArr)
        {
            ObjectArr = objectArr;
        }

        public SupportBeanArrayCollMap(int[] intArr)
        {
            IntArr = intArr;
        }

        public SupportBeanArrayCollMap(int[] intArr, long?[] longArr)
        {
            IntArr = intArr;
            LongArr = longArr;
        }

        public SupportBeanArrayCollMap(bool makeCol, int[] intArr, long?[] longArr, long? longBoxed)
            : this(makeCol, intArr, longArr)
        {
            LongBoxed = longBoxed;
        }

        public SupportBeanArrayCollMap(bool makeCol, int[] intArr, long?[] longArr)
        {
            if (makeCol)
            {
                IntCol = ConvertCol(intArr);
                LongCol = ConvertCol(longArr);
            }
            else
            {
                IntMap = ConvertMap(intArr);
                LongMap = ConvertMap(longArr);
            }
        }

        public SupportBeanArrayCollMap(long? longBoxed, int[] intArr, long?[] longColl, int[] intMap)
        {
            LongBoxed = longBoxed;
            IntArr = intArr;
            LongMap = ConvertMap(longColl);
            IntCol = ConvertCol(intMap);
        }

        public long? LongBoxed { get; set; }

        public int[] IntArr { get; set; }

        public long?[] LongArr { get; set; }

        public ICollection<int> IntCol { get; set; }

        public IList<long?> LongCol { get; set; }

        public IDictionary<int, string> IntMap { get; set; }

        public IDictionary<long?, string> LongMap { get; set; }

        public object[] ObjectArr { get; set; }

        public object AnyObject
        {
            get { return anyObject; }
            set { anyObject = value; }
        }

        public IDictionary<string, object> OtherMap { get; set; }

        private static IDictionary<T, String> ConvertMap<T>(T[] anyArr)
        {
            if (anyArr == null) {
                return null;
            }

            var anyMap = new Dictionary<T, String>().WithSafeSupport();
            foreach (var anyValue in anyArr) {
                anyMap.Put(anyValue, "");
            }
            return anyMap;
        }

        private static IList<T> ConvertCol<T>(T[] anyArr)
        {
            if (anyArr == null) {
                return null;
            }

            IList<T> anyCol = new List<T>();
            foreach (var anyValue in anyArr) {
                anyCol.Add(anyValue);
            }
            return anyCol;
        }
    }
}
