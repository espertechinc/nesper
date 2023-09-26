///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
    public class DIOMultiKeyArrayDoubleSerde : DIOMultiKeyArraySerde<MultiKeyArrayDouble>
    {
        public static readonly DIOMultiKeyArrayDoubleSerde INSTANCE = new DIOMultiKeyArrayDoubleSerde();

        public Type ComponentType => typeof(double);

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((MultiKeyArrayDouble) @object, output, unitKey, writer);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadValue(input, unitKey);
        }
        
        public void Write(
            MultiKeyArrayDouble mk,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(mk.Keys, output);
        }

        public MultiKeyArrayDouble ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return new MultiKeyArrayDouble(ReadInternal(input));
        }

        private void WriteInternal(
            double[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                output.WriteDouble(i);
            }
        }

        private double[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new double[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadDouble();
            }

            return array;
        }
    }
} // end of namespace