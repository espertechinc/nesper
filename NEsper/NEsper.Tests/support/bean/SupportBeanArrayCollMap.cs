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

namespace com.espertech.esper.support.bean
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

        public SupportBeanArrayCollMap(Object[] objectArr)
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

        private static IDictionary<long?, String> ConvertMap(long?[] longArr)
        {
            if (longArr == null) {
                return null;
            }

            var longMap = new Dictionary<long?, String>().WithSafeSupport();
            foreach (long? along in longArr) {
                longMap.Put(along, "");
            }
            return longMap;
        }

        private static IDictionary<int, String> ConvertMap(int[] intArr)
        {
            if (intArr == null) {
                return null;
            }

            var intMap = new Dictionary<int, String>();
            foreach (int anIntArr in intArr) {
                intMap.Put(anIntArr, "");
            }
            return intMap;
        }

        private static IList<long?> ConvertCol(long?[] longArr)
        {
            if (longArr == null) {
                return null;
            }

            IList<long?> longCol = new List<long?>();
            foreach (long? along in longArr) {
                longCol.Add(along);
            }
            return longCol;
        }

        private static IList<int> ConvertCol(int[] intArr)
        {
            if (intArr == null) {
                return null;
            }

            var intCol = new List<int>();
            foreach (int anIntArr in intArr) {
                intCol.Add(anIntArr);
            }
            return intCol;
        }
    }
}
