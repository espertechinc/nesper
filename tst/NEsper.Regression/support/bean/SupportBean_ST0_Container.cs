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

using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportBean_ST0_Container
    {
        private static string[] _samples;

        public SupportBean_ST0_Container(IList<SupportBean_ST0> contained)
        {
            Contained = contained;
        }

        public SupportBean_ST0_Container(
            IList<SupportBean_ST0> contained,
            IList<SupportBean_ST0> containedTwo)
        {
            Contained = contained;
            ContainedTwo = containedTwo;
        }

        public IList<SupportBean_ST0> Contained { get; }

        public IList<SupportBean_ST0> ContainedTwo { get; }

        public static string[] Samples {
            set => _samples = value;
        }

        public static IList<SupportBean_ST0> MakeSampleList()
        {
            if (_samples == null) {
                return null;
            }

            return Make2Value(_samples).Contained;
        }

        public static SupportBean_ST0[] MakeSampleArray()
        {
            if (_samples == null) {
                return null;
            }

            var items = Make2Value(_samples).Contained;
            return items.ToArray();
        }

        public static SupportBean_ST0_Container Make3Value(params string[] values)
        {
            if (values == null) {
                return new SupportBean_ST0_Container(null);
            }

            IList<SupportBean_ST0> contained = new List<SupportBean_ST0>();
            for (var i = 0; i < values.Length; i++) {
                var triplet = values[i].SplitCsv();
                contained.Add(new SupportBean_ST0(triplet[0], triplet[1], int.Parse(triplet[2])));
            }

            return new SupportBean_ST0_Container(contained);
        }

        public static SupportBean_ST0_Container Make3ValueNull()
        {
            return new SupportBean_ST0_Container(null);
        }

        public static IList<SupportBean_ST0> Make2ValueList(params string[] values)
        {
            if (values == null) {
                return null;
            }

            IList<SupportBean_ST0> result = new List<SupportBean_ST0>();
            for (var i = 0; i < values.Length; i++) {
                var pair = values[i].SplitCsv();
                result.Add(new SupportBean_ST0(pair[0], int.Parse(pair[1])));
            }

            return result;
        }

        public static SupportBean_ST0_Container Make2Value(params string[] values)
        {
            return new SupportBean_ST0_Container(Make2ValueList(values));
        }

        public static SupportBean_ST0_Container Make2ValueNull()
        {
            return new SupportBean_ST0_Container(null);
        }

        public static SupportBean_ST0 MakeTest(string value)
        {
            return Make2Value(value).Contained[0];
        }

        public override string ToString()
        {
            return $"{nameof(Contained)}: {Contained}";
        }
    }
} // end of namespace