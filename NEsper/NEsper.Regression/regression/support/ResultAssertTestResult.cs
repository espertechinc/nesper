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

namespace com.espertech.esper.regression.support
{
    public class ResultAssertTestResult
    {
        private readonly String category;
        private readonly String title;
        private readonly String[] properties;

        private readonly SortedDictionary<long, IDictionary<int, StepDesc>> assertions;
    
        public ResultAssertTestResult(String category, String title, String[] properties) {
            this.category = category;
            this.title = title;
            this.properties = properties;

            assertions = new SortedDictionary<long, IDictionary<int, StepDesc>>();
        }
    
        public void AddResultInsert(long time, int step, object[][] newDataPerRow)
        {        
            AddResultInsRem(time, step, newDataPerRow, null);
        }
    
        public void AddResultRemove(long time, int step, object[][] oldDataPerRow)
        {
            AddResultInsRem(time, step, null, oldDataPerRow);
        }
    
        public void AddResultInsRem(long time, int step, object[][] newDataPerRow, object[][] oldDataPerRow)
        {
            if (step >= 10)
            {
                throw new ArgumentException("Step max value is 10 for any time slot");
            }
            IDictionary<int, StepDesc> stepMap = assertions.Get(time);
            if (stepMap == null)
            {
                stepMap = new Dictionary<int, StepDesc>();
                assertions.Put(time, stepMap);
            }
    
            if (stepMap.ContainsKey(step))
            {
                throw new ArgumentException("Step already in map for time slot");
            }
            stepMap.Put(step, new StepDesc(step, newDataPerRow, oldDataPerRow));
        }

        public string Category
        {
            get { return category; }
        }

        public string Title
        {
            get { return title; }
        }

        public string[] Properties
        {
            get { return properties; }
        }

        public SortedDictionary<long, IDictionary<int, StepDesc>> Assertions
        {
            get { return assertions; }
        }
    }
}
