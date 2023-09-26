///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportBeanArrayCollMap
    {
        public SupportBeanArrayCollMap()
        {
            AnyObject = AnyObject;
        }

        public SupportBeanArrayCollMap(object anyObject)
        {
            AnyObject = anyObject;
        }

        public SupportBeanArrayCollMap(object[] objectArr)
        {
            ObjectArr = objectArr;
        }

        public SupportBeanArrayCollMap(int[] intArr)
        {
            IntArr = intArr;
        }

        public SupportBeanArrayCollMap(
            int[] intArr,
            long?[] longArr)
        {
            IntArr = intArr;
            LongArr = longArr;
        }

        public SupportBeanArrayCollMap(
            bool makeCol,
            int[] intArr,
            long?[] longArr,
            long? longBoxed) : this(makeCol, intArr, longArr)
        {
            LongBoxed = longBoxed;
        }

        public SupportBeanArrayCollMap(
            bool makeCol,
            int[] intArr,
            long?[] longArr)
        {
            if (makeCol) {
                IntCol = intArr?.ToList();
                LongCol = longArr?.Where(v => v != null)
                    .Select(v => v.Value)
                    .ToList();
            }
            else {
                IntMap = ConvertMap(intArr);
                LongMap = ConvertMap(longArr);
            }
        }

        public SupportBeanArrayCollMap(
            long? longBoxed,
            int[] intArr,
            long?[] longColl,
            int[] intMap)
        {
            LongBoxed = longBoxed;
            IntArr = intArr;
            LongMap = ConvertMap(longColl);
            IntCol = intMap?.ToList();
        }

        public SupportBeanArrayCollMap(ISet<string> setOfString)
        {
            SetOfString = setOfString;
        }

        public long? LongBoxed { get; set; }

        public int[] IntArr { get; set; }

        public long?[] LongArr { get; set; }

        public ICollection<int> IntCol { get; set; }

        public ICollection<long> LongCol { get; set; }

        public IDictionary<int, string> IntMap { get; set; }

        public IDictionary<long?, string> LongMap { get; set; }

        public object[] ObjectArr { get; set; }

        public object AnyObject { get; set; }

        public ISet<string> SetOfString { get; }

        public IDictionary<string, object> OtherMap { get; set; }
        
        public string Id { get; set; }

        private static IDictionary<long?, string> ConvertMap(long?[] longArr)
        {
            if (longArr == null) {
                return null;
            }

            var longMap = new HashMap<long?, string>();
            foreach (var along in longArr) {
                longMap.Put(along, "");
            }

            return longMap;
        }

        private static IDictionary<int, string> ConvertMap(int[] intArr)
        {
            if (intArr == null) {
                return null;
            }

            var intMap = new Dictionary<int, string>();
            foreach (var anIntArr in intArr) {
                intMap.Put(anIntArr, "");
            }

            return intMap;
        }

        //private static IList<Tin> ConvertCol<Tin>(Tin[] inputArray)
        //{
        //    return inputArray == null ? null : inputArray.ToList();
        //}
    }
} // end of namespace