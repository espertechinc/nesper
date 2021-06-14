///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportSimpleWordCSVFunction : AggregationFunction
    {
        private readonly IDictionary<string, int?> countPerWord = new LinkedHashMap<string, int?>();

        public void Enter(object value)
        {
            var word = (string) value;
            var count = countPerWord.Get(word);
            if (count == null) {
                countPerWord.Put(word, 1);
            }
            else {
                countPerWord.Put(word, count + 1);
            }
        }

        public void Leave(object value)
        {
            var word = (string) value;
            var count = countPerWord.Get(word);
            if (count == null) {
                countPerWord.Put(word, 1);
            }
            else if (count == 1) {
                countPerWord.Remove(word);
            }
            else {
                countPerWord.Put(word, count - 1);
            }
        }

        public object Value {
            get {
                var writer = new StringWriter();
                var delimiter = "";
                foreach (var entry in countPerWord) {
                    writer.Write(delimiter);
                    delimiter = ",";
                    writer.Write(entry.Key);
                }

                return writer.ToString();
            }
        }

        public void Clear()
        {
            countPerWord.Clear();
        }
    }
} // end of namespace