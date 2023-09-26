///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Count-min sketch agent that handles String-type values and uses UTF-16 encoding
    /// to transform strings to byte-array and back.
    /// </summary>
    public class CountMinSketchAgentStringUTF16 : CountMinSketchAgent
    {
        public Type[] AcceptableValueTypes {
            get {
                return new Type[] {
                    typeof(string)
                };
            }
        }

        public void Add(CountMinSketchAgentContextAdd ctx)
        {
            var text = (string)ctx.Value;
            if (text == null) {
                return;
            }

            var bytes = ToBytesUTF16(text);
            ctx.State.Add(bytes, 1);
        }

        public long? Estimate(CountMinSketchAgentContextEstimate ctx)
        {
            var text = (string)ctx.Value;
            if (text == null) {
                return null;
            }

            var bytes = ToBytesUTF16(text);
            return ctx.State.Frequency(bytes);
        }

        public object FromBytes(CountMinSketchAgentContextFromBytes ctx)
        {
            return Encoding.Unicode.GetString(ctx.Bytes);
        }

        private byte[] ToBytesUTF16(string text)
        {
            return Encoding.Unicode.GetBytes(text);
        }
    }
}