///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class ResultAssertTestResult
    {
        private readonly SortedDictionary<long, IDictionary<int, StepDesc>> assertions;

        public ResultAssertTestResult(
            string category,
            string title,
            string[] properties)
        {
            Category = category;
            Title = title;
            Properties = properties;

            assertions = new SortedDictionary<long, IDictionary<int, StepDesc>>();
        }

        public string Category { get; }

        public string Title { get; }

        public string[] Properties { get; }

        public void AddResultInsert(
            long time,
            int step,
            object[][] newDataPerRow)
        {
            AddResultInsRem(time, step, newDataPerRow, null);
        }

        public void AddResultRemove(
            long time,
            int step,
            object[][] oldDataPerRow)
        {
            AddResultInsRem(time, step, null, oldDataPerRow);
        }

        public void AddResultInsRem(
            long time,
            int step,
            object[][] newDataPerRow,
            object[][] oldDataPerRow)
        {
            if (step >= 10) {
                throw new ArgumentException("Step max value is 10 for any time slot");
            }

            var stepMap = assertions.Get(time);
            if (stepMap == null) {
                stepMap = new Dictionary<int, StepDesc>();
                assertions.Put(time, stepMap);
            }

            if (stepMap.ContainsKey(step)) {
                throw new ArgumentException("Step already in map for time slot");
            }

            stepMap.Put(step, new StepDesc(step, newDataPerRow, oldDataPerRow));
        }

        public SortedDictionary<long, IDictionary<int, StepDesc>> GetAssertions()
        {
            return assertions;
        }
    }
} // end of namespace