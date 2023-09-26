///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportCollection
    {
        private static string sampleStaticCSV;

        public ICollection<string> Strvals { get; set; }

        public ICollection<string> Strvalstwo { get; set; }

        public ICollection<int?> Intvals { get; set; }

        public ICollection<decimal?> Bdvals { get; set; }

        public ICollection<bool?> Boolvals { get; set; }

        public int?[] Intarray { get; set; }

        public IList<int?> Intiterable { get; set; }

        public static string SampleCSV {
            set => sampleStaticCSV = value;
        }

        public static string SampleStaticCSV {
            set => sampleStaticCSV = value;
        }

        public static SupportCollection MakeString(string csvlist)
        {
            var bean = new SupportCollection();
            bean.Strvals = ToListString(csvlist);
            bean.Strvalstwo = ToListString(csvlist);
            return bean;
        }

        public static SupportCollection MakeString(
            string csvlist,
            string csvlisttwo)
        {
            var bean = new SupportCollection();
            bean.Strvals = ToListString(csvlist);
            bean.Strvalstwo = ToListString(csvlisttwo);
            return bean;
        }

        public static SupportCollection MakeNumeric(string csvlist)
        {
            var bean = new SupportCollection();
            ICollection<string> list = ToListString(csvlist);
            bean.Intvals = ToInt(list);
            bean.Bdvals = ToDecimal(list);

            if (bean.Intvals != null) {
                bean.Intarray = new int?[bean.Intvals.Count];
                var count = 0;
                foreach (var val in bean.Intvals) {
                    bean.Intarray[count++] = val ?? int.MinValue;
                }

                bean.Intiterable = bean.Intvals.ToList();
            }

            return bean;
        }

        public static SupportCollection MakeBoolean(string csvlist)
        {
            var bean = new SupportCollection();
            ICollection<string> list = ToListString(csvlist);
            bean.Boolvals = ToBoolean(list);
            return bean;
        }

        private static IList<string> ToListString(string csvlist)
        {
            if (csvlist == null) {
                return null;
            }

            if (string.IsNullOrEmpty(csvlist)) {
                return Collections.GetEmptyList<string>();
            }

            var items = csvlist.SplitCsv();
            IList<string> list = new List<string>();
            for (var i = 0; i < items.Length; i++) {
                if (items[i].Equals("null")) {
                    list.Add(null);
                }
                else {
                    list.Add(items[i]);
                }
            }

            return list;
        }

        private static ICollection<decimal?> ToDecimal(ICollection<string> one)
        {
            if (one == null) {
                return null;
            }

            IList<decimal?> result = new List<decimal?>();
            foreach (var element in one) {
                if (element == null) {
                    result.Add(null);
                    continue;
                }

                result.Add(decimal.Parse(element));
            }

            return result;
        }

        private static ICollection<int?> ToInt(ICollection<string> one)
        {
            if (one == null) {
                return null;
            }

            IList<int?> result = new List<int?>();
            foreach (var element in one) {
                if (element == null) {
                    result.Add(null);
                    continue;
                }

                result.Add(int.Parse(element));
            }

            return result;
        }

        private static ICollection<bool?> ToBoolean(ICollection<string> one)
        {
            if (one == null) {
                return null;
            }

            IList<bool?> result = new List<bool?>();
            foreach (var element in one) {
                if (element == null) {
                    result.Add(null);
                    continue;
                }

                result.Add(bool.Parse(element));
            }

            return result;
        }

        public static IList<string> MakeSampleListString()
        {
            return ToListString(sampleStaticCSV);
        }

        public static string[] MakeSampleArrayString()
        {
            var list = ToListString(sampleStaticCSV);
            if (list == null) {
                return null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return $"{nameof(Strvals)}: {Strvals.RenderAny()}";
        }
    }
} // end of namespace