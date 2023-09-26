///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class CodegenRepetitiveLengthBuilder : CodegenRepetitiveBuilderBase
    {
        private readonly int length;
        private ConsumerByLength consumer;

        public CodegenRepetitiveLengthBuilder(
            int length,
            CodegenMethod methodNode,
            CodegenClassScope classScope,
            Type provider) : base(methodNode, classScope, provider)
        {
            this.length = length;
        }

        public CodegenRepetitiveLengthBuilder AddParam<T>(string name)
        {
            return AddParam(typeof(T), name);
        }

        public CodegenRepetitiveLengthBuilder AddParam(
            Type type,
            string name)
        {
            @params.Add(new CodegenNamedParam(type, name));
            return this;
        }

        public override void Build()
        {
            var complexity = TargetMethodComplexity(classScope);
            if (length < complexity) {
                for (var i = 0; i < length; i++) {
                    consumer.Invoke(i, methodNode);
                }

                return;
            }

            var count = 0;
            while (count < length) {
                var remaining = length - count;
                var target = Math.Min(remaining, complexity);
                var child = methodNode.MakeChild(typeof(void), provider, classScope).AddParam(@params);
                methodNode.Block.LocalMethod(child, ParamNames());
                for (var i = 0; i < target; i++) {
                    consumer.Invoke(count, child);
                    count++;
                }
            }
        }

        public delegate void ConsumerByLength(
            int index,
            CodegenMethod leafMethod);

        public CodegenRepetitiveLengthBuilder SetConsumer(ConsumerByLength value)
        {
            consumer = value;
            return this;
        }
    }
} // end of namespace