///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.client.serde
{
    public abstract class DataInputOutputSerdeBase<T> : DataInputOutputSerde<T>
    {
        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((T)@object, output, unitKey, writer);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadValue(input, unitKey);
        }

        public abstract void Write(
            T @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer);

        public abstract T ReadValue(
            DataInput input,
            byte[] unitKey);
    }
}