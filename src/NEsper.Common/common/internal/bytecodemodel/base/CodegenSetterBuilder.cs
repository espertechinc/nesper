///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenSetterBuilder
    {
        private readonly Type _originator;
        private readonly string _refName;
        private readonly CodegenClassScope _classScope;
        private readonly bool _methodProvided;
        private readonly CodegenMethod _method;
        private readonly CodegenProperty _property;
        private bool _closed;

        public CodegenExpressionRef RefName => Ref(_refName);

        public CodegenSetterBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;

            _methodProvided = false;
            _method = parent.MakeChild(returnType, originator, classScope);
            _method.Block.DeclareVarNewInstance(returnType, refName);
        }

        public CodegenSetterBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;

            _methodProvided = true;
            _method = method;
            _method.Block.DeclareVarNewInstance(returnType, refName);
        }

        public CodegenSetterBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenClassScope classScope,
            CodegenProperty property)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;

            _methodProvided = false;
            _method = null;

            _property = property;
            _property.GetterBlock.DeclareVarNewInstance(returnType, refName);
        }
        
        public CodegenSetterBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenExpression initializer)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;
            _method = method;
            _methodProvided = true;
            method.Block.DeclareVar(returnType, refName, initializer);
        }

        public CodegenSetterBuilder ConstantExplicit(
            string name,
            object value)
        {
            if (value is CodegenExpression) {
                throw new ArgumentException("Expected a non-expression value, received " + value);
            }

            return SetValue(name, value == null ? ConstantNull() : Constant(value));
        }

        public CodegenSetterBuilder ConstantDefaultChecked(
            string name,
            bool value)
        {
            if (!value) {
                return this;
            }

            return SetValue(name, Constant(value));
        }

        public CodegenSetterBuilder ConstantDefaultChecked(
            string name,
            int value)
        {
            if (value == 0) {
                return this;
            }

            return SetValue(name, Constant(value));
        }

        public CodegenSetterBuilder ConstantDefaultChecked(
            string name,
            bool? value)
        {
            return ConstantDefaultCheckedObj(name, value);
        }

        public CodegenSetterBuilder ConstantDefaultChecked(
            string name,
            int? value)
        {
            return ConstantDefaultCheckedObj(name, value);
        }

        public CodegenSetterBuilder ConstantDefaultCheckedObj(
            string name,
            object value)
        {
            if (value == null) {
                return this;
            }

            return SetValue(name, Constant(value));
        }

        public CodegenSetterBuilder ExpressionDefaultChecked(
            string name,
            CodegenExpression expression)
        {
            if (expression.Equals(ConstantNull())) {
                return this;
            }

            return SetValue(name, expression);
        }

        public CodegenSetterBuilder Expression(
            string name,
            CodegenExpression expression)
        {
            return SetValue(name, expression);
        }

        public CodegenSetterBuilder Method(
            string name,
            Func<CodegenMethod, CodegenExpression> expressionFunc)
        {
            var expression = expressionFunc.Invoke(_method);
            return SetValue(name, expression ?? ConstantNull());
        }

        public CodegenSetterBuilder MapOfConstants<T>(
            string name,
            IDictionary<string, T> values)
        {
            CodegenSetterBuilderItemConsumer<T> consumer = (
                o,
                parent,
                scope) => Constant(o);
            return SetValue(name, BuildMap(values, consumer, _originator, _method, _classScope));
        }

        public CodegenSetterBuilder Map<TI>(
            string name,
            IDictionary<string, TI> values,
            CodegenSetterBuilderItemConsumer<TI> consumer)
        {
            return SetValue(name, BuildMap(values, consumer, _originator, _method, _classScope));
        }

        public CodegenExpression Build()
        {
            if (_methodProvided) {
                throw new IllegalStateException(
                    "Builder build is reserved for the case when the method is not already provided");
            }

            if (_closed) {
                throw new IllegalStateException("Builder already completed build");
            }

            _closed = true;

            if (_method != null) {
                _method.Block.MethodReturn(Ref(_refName));
            } else if (_property != null) {
                _property.GetterBlock.BlockReturn(Ref(_refName));
            }

            return LocalMethod(_method);
        }

        private static CodegenExpression BuildMap<TV>(
            IDictionary<string, TV> map,
            CodegenSetterBuilderItemConsumer<TV> valueConsumer,
            Type originator,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (map == null) {
                return ConstantNull();
            }

            if (map.IsEmpty()) {
                return EnumValue(typeof(EmptyDictionary<string, TV>), "Instance");
            }

            var child = method.MakeChild(typeof(IDictionary<string, TV>), originator, classScope);
            if (map.Count == 1) {
                var single = map.First();
                var value = BuildMapValue(single.Value, valueConsumer, originator, child, classScope);
                child.Block.MethodReturn(
                    StaticMethod(
                        typeof(Collections),
                        "SingletonMap",
                        new[] { typeof(string), typeof(TV) },
                        Constant(single.Key),
                        value));
            }
            else {
                child.Block.DeclareVar(
                    typeof(IDictionary<string, TV>),
                    "map",
                    NewInstance(typeof(LinkedHashMap<string, TV>)));
                foreach (var entry in map) {
                    var value = BuildMapValue(entry.Value, valueConsumer, originator, child, classScope);
                    child.Block.ExprDotMethod(Ref("map"), "Put", Constant(entry.Key), value);
                }

                child.Block.MethodReturn(Ref("map"));
            }

            return LocalMethod(child);
        }

        private static CodegenExpression BuildMapValue<TV>(
            TV value,
            CodegenSetterBuilderItemConsumer<TV> valueConsumer,
            Type originator,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (value is IDictionary<string, TV> valueMap) {
                return BuildMap(valueMap, valueConsumer, originator, method, classScope);
            }

            return valueConsumer.Invoke(value, method, classScope);
        }

        private CodegenSetterBuilder SetValue(
            string name,
            CodegenExpression expression)
        {
            if (_method != null) {
                _method.Block.SetProperty(Ref(_refName), GetBeanCap(name), expression);
            } else if (_property != null) {
                _property.GetterBlock.SetProperty(Ref(_refName), GetBeanCap(name), expression);
            }

            return this;
        }

        private string GetBeanCap(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }
    }
} // end of namespace