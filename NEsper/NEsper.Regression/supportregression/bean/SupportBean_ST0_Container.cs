///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.supportregression.bean
{
    public class SupportBean_ST0_Container
    {
        private static String[] _samples;

        public SupportBean_ST0_Container(IList<SupportBean_ST0> contained)
        {
            Contained = contained;
        }

        public SupportBean_ST0_Container(IList<SupportBean_ST0> contained,
                                         IList<SupportBean_ST0> containedTwo)
        {
            Contained = contained;
            ContainedTwo = containedTwo;
        }

        public IList<SupportBean_ST0> Contained { get; private set; }

        public IList<SupportBean_ST0> ContainedTwo { get; private set; }

        public static string[] Samples
        {
            set { SupportBean_ST0_Container._samples = value; }
        }

        public static IList<SupportBean_ST0> MakeSampleList()
        {
            if (_samples == null)
            {
                return null;
            }
            return Make2Value(_samples).Contained;
        }

        public static SupportBean_ST0[] MakeSampleArray()
        {
            if (_samples == null)
            {
                return null;
            }
            IList<SupportBean_ST0> items = Make2Value(_samples).Contained;
            return items.ToArray();
        }

        public static SupportBean_ST0_Container Make3Value(params String[] values)
        {
            if (values == null)
            {
                return new SupportBean_ST0_Container(null);
            }
            var contained = new List<SupportBean_ST0>();
            for (int i = 0; i < values.Length; i++)
            {
                String[] triplet = values[i].Split(',');
                contained.Add(new SupportBean_ST0(triplet[0], triplet[1], int.Parse(triplet[2])));
            }
            return new SupportBean_ST0_Container(contained);
        }

        public static List<SupportBean_ST0> Make2ValueList(params String[] values)
        {
            if (values == null)
            {
                return null;
            }
            var result = new List<SupportBean_ST0>();
            for (int i = 0; i < values.Length; i++)
            {
                String[] pair = values[i].Split(',');
                result.Add(new SupportBean_ST0(pair[0], int.Parse(pair[1])));
            }
            return result;
        }

        public static SupportBean_ST0_Container Make2Value(params String[] values)
        {
            return new SupportBean_ST0_Container(Make2ValueList(values));
        }

        public static SupportBean_ST0 MakeTest(String value)
        {
            return Make2Value(value).Contained[0];
        }
    }
}