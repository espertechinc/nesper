///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class CodegenRepetitiveValueBuilder<V> : CodegenRepetitiveBuilderBase
    {
        private readonly ICollection<V> values;
        private ConsumerByValue<V> consumer;

        public CodegenRepetitiveValueBuilder(
            ICollection<V> values,
            CodegenMethod methodNode,
            CodegenClassScope classScope,
            Type provider) : base(methodNode, classScope, provider)
        {
            this.values = values;
        }

        public CodegenRepetitiveValueBuilder<V> AddParam<T>(string name)
        {
            return AddParam(typeof(T), name);
        }

        public CodegenRepetitiveValueBuilder<V> AddParam(
            Type type,
            string name)
        {
            @params.Add(new CodegenNamedParam(type, name));
            return this;
        }

        public override void Build()
        {
            var complexity = TargetMethodComplexity(classScope);
            if (values.Count < complexity) {
                var index = 0;
                foreach (var value in values) {
                    consumer.Invoke(value, index++, methodNode);
                }

                return;
            }

            var count = 0;
            IEnumerator<V> enumerator = values.GetEnumerator();
            while (count < values.Count) {
                var remaining = values.Count - count;
                var target = Math.Min(remaining, complexity);
                var child = methodNode.MakeChild(typeof(void), provider, classScope).AddParam(@params);
                methodNode.Block.LocalMethod(child, ParamNames());
                for (var i = 0; i < target; i++) {
                    enumerator.MoveNext();
                    V value = enumerator.Current;
                    consumer.Invoke(value, count, child);
                    count++;
                }
            }
        }

        public delegate void ConsumerByValue<V>(
            V value,
            int index,
            CodegenMethod leafMethod);

        public CodegenRepetitiveValueBuilder<V> SetConsumer(ConsumerByValue<V> value)
        {
            consumer = value;
            return this;
        }
    }
} // end of namespace