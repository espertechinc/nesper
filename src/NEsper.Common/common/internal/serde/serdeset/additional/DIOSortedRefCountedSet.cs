///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.additional
{
    public class DIOSortedRefCountedSet : DataInputOutputSerdeBase<SortedRefCountedSet<object>>
    {
        private readonly DataInputOutputSerde _inner;

        public DIOSortedRefCountedSet(DataInputOutputSerde inner)
        {
            _inner = inner;
        }

        public override void Write(
            SortedRefCountedSet<object> valueSet,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            output.WriteInt(valueSet.RefSet.Count);
            foreach (var entry in valueSet.RefSet) {
                _inner.Write(entry.Key, output, unitKey, writer);
                output.WriteLong(entry.Value.Get());
            }

            output.WriteLong(valueSet.CountPoints);
        }

        public override SortedRefCountedSet<object> ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            var valueSet = new SortedRefCountedSet<object>();
            var refSet = valueSet.RefSet;
            var size = input.ReadInt();
            for (var i = 0; i < size; i++) {
                var key = _inner.Read(input, unitKey);
                var cnt = input.ReadLong();
                refSet[key] = new AtomicLong(cnt);
            }

            valueSet.CountPoints = input.ReadLong();
            return valueSet;
        }
    }
} // end of namespace