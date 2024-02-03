///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.additional
{
    public class DIOMapPropertySerde : DataInputOutputSerdeBase<IDictionary<string, object>>
    {
        private readonly string[] _keys;
        private readonly DataInputOutputSerde[] _serdes;

        public DIOMapPropertySerde(
            string[] keys,
            DataInputOutputSerde[] serdes)
        {
            _keys = keys;
            _serdes = serdes;
        }

        public override IDictionary<string, object> ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            var map = new Dictionary<string, object>();
            for (var i = 0; i < _keys.Length; i++) {
                var value = _serdes[i].Read(input, unitKey);
                map.Put(_keys[i], value);
            }

            return map;
        }

        public override void Write(
            IDictionary<string, object> @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            for (var i = 0; i < _keys.Length; i++) {
                var value = _keys[i];
                _serdes[i].Write(value, output, unitKey, writer);
            }
        }
    }
} // end of namespace