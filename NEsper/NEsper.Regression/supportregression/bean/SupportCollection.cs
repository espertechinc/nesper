///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.supportregression.bean
{
    public class SupportCollection
    {
        private static String _sampleStaticCsv;

        private ICollection<decimal?> _bdvals;
        private ICollection<bool?> _boolvals;
        private int?[] _intarray;
        private IEnumerable<int?> _intiterable;
        private ICollection<int?> _intvals;
        private ICollection<String> _strvals;
        private ICollection<String> _strvalstwo;

        public ICollection<string> Strvals
        {
            get { return _strvals; }
            set { _strvals = value; }
        }

        public ICollection<string> Strvalstwo
        {
            get { return _strvalstwo; }
        }

        public ICollection<int?> Intvals
        {
            get { return _intvals; }
            set { _intvals = value; }
        }

        public ICollection<decimal?> Bdvals
        {
            get { return _bdvals; }
            set { _bdvals = value; }
        }

        public ICollection<bool?> Boolvals
        {
            get { return _boolvals; }
            set { _boolvals = value; }
        }

        public int?[] Intarray
        {
            get { return _intarray; }
            set { _intarray = value; }
        }

        public static SupportCollection MakeString(String csvlist)
        {
            var bean = new SupportCollection();
            bean._strvals = ToListString(csvlist);
            bean._strvalstwo = ToListString(csvlist);
            return bean;
        }

        public static SupportCollection MakeString(String csvlist,
                                                   String csvlisttwo)
        {
            var bean = new SupportCollection();
            bean._strvals = ToListString(csvlist);
            bean._strvalstwo = ToListString(csvlisttwo);
            return bean;
        }

        public static SupportCollection MakeNumeric(String csvlist)
        {
            var bean = new SupportCollection();
            ICollection<String> list = ToListString(csvlist);
            bean._intvals = ToInt(list);
            bean._bdvals = ToDecimal(list);

            if (bean._intvals != null)
            {
                bean._intarray = new int?[bean._intvals.Count];
                int count = 0;
                foreach (var val in bean._intvals)
                {
                    bean._intarray[count++] = val ?? int.MinValue;
                }

                ICollection<int?> iteratable = bean._intvals;
                bean._intiterable = iteratable;
            }

            return bean;
        }

        public static SupportCollection MakeBoolean(String csvlist)
        {
            var bean = new SupportCollection();
            ICollection<String> list = ToListString(csvlist);
            bean._boolvals = ToBoolean(list);
            return bean;
        }

        public IEnumerable<int?> Intiterable
        {
            get { return _intiterable; }
            set { _intiterable = value; }
        }

        private static List<String> ToListString(String csvlist)
        {
            if (csvlist == null)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(csvlist))
            {
                return new List<string>();
            }
            else
            {
                String[] items = csvlist.Split(',');
                var list = new List<String>();
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].Equals("null"))
                    {
                        list.Add(null);
                    }
                    else
                    {
                        list.Add(items[i]);
                    }
                }
                return list;
            }
        }

        private static ICollection<decimal?> ToDecimal(ICollection<String> one)
        {
            if (one == null)
            {
                return null;
            }
            var result = new List<decimal?>();
            foreach (String element in one)
            {
                if (element == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(Decimal.Parse(element));
            }
            return result;
        }

        private static ICollection<int?> ToInt(ICollection<String> one)
        {
            if (one == null)
            {
                return null;
            }
            var result = new List<int?>();
            foreach (String element in one)
            {
                if (element == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(int.Parse(element));
            }
            return result;
        }

        private static ICollection<bool?> ToBoolean(ICollection<String> one)
        {
            if (one == null)
            {
                return null;
            }
            var result = new List<bool?>();
            foreach (String element in one)
            {
                if (element == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(Boolean.Parse(element));
            }
            return result;
        }

        public static string SampleCSV
        {
            set { _sampleStaticCsv = value; }
            get { return _sampleStaticCsv; }
        }

        public static List<String> MakeSampleListString()
        {
            return ToListString(_sampleStaticCsv);
        }

        public static String[] MakeSampleArrayString()
        {
            List<String> list = ToListString(_sampleStaticCsv);
            if (list == null)
            {
                return null;
            }
            return list.ToArray();
        }
    }
}