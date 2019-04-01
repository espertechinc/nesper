///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class SAIFFInitializeBuilder
    {
        private readonly CodegenClassScope classScope;
        private readonly Type originator;
        private readonly string refName;
        private readonly SAIFFInitializeSymbol symbols;
        private bool closed;

        private readonly CodegenMethod method;

        public SAIFFInitializeBuilder(
            Type returnType, Type originator, string refName, CodegenMethodScope parent, SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            this.originator = originator;
            this.refName = refName;
            this.symbols = symbols;
            this.classScope = classScope;

            method = parent.MakeChild(returnType, originator, classScope);
            method.Block.DeclareVar(returnType, refName, NewInstance(returnType));
        }

        public SAIFFInitializeBuilder(
            string returnType, Type originator, string refName, CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            this.originator = originator;
            this.refName = refName;
            this.symbols = symbols;
            this.classScope = classScope;

            method = parent.MakeChild(returnType, originator, classScope);
            method.Block.DeclareVar(returnType, refName, NewInstance(returnType));
        }

        public SAIFFInitializeBuilder EventtypesMayNull(string name, EventType[] eventTypes)
        {
            return SetValue(
                name,
                eventTypes == null
                    ? ConstantNull()
                    : EventTypeUtility.ResolveTypeArrayCodegenMayNull(eventTypes, symbols.GetAddInitSvc(method)));
        }

        public SAIFFInitializeBuilder Eventtype(string name, EventType eventType)
        {
            return SetValue(
                name,
                eventType == null
                    ? ConstantNull()
                    : EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)));
        }

        public SAIFFInitializeBuilder Eventtypes(string name, EventType[] types)
        {
            return SetValue(
                name,
                types == null
                    ? ConstantNull()
                    : EventTypeUtility.ResolveTypeArrayCodegen(types, symbols.GetAddInitSvc(method)));
        }

        public SAIFFInitializeBuilder Exprnode(string name, ExprNode value)
        {
            return SetValue(
                name,
                value == null
                    ? ConstantNull()
                    : ExprNodeUtilityCodegen.CodegenEvaluator(value.Forge, method, GetType(), classScope));
        }

        public SAIFFInitializeBuilder Constant(string name, object value)
        {
            if (value is CodegenExpression) {
                throw new ArgumentException("Expected a non-expression value, received " + value);
            }

            return SetValue(name, value == null ? ConstantNull() : CodegenExpressionBuilder.Constant(value));
        }

        public CodegenMethod GetMethod()
        {
            return method;
        }

        public SAIFFInitializeBuilder Method(string name, Func<CodegenMethod, CodegenExpression> expressionFunc)
        {
            CodegenExpression expression = expressionFunc.Invoke(method);
            return SetValue(name, expression ?? ConstantNull());
        }

        public SAIFFInitializeBuilder Expression(string name, CodegenExpression expression)
        {
            return SetValue(name, expression == null ? ConstantNull() : expression);
        }

        public SAIFFInitializeBuilder Forges(string name, ExprForge[] evaluatorForges)
        {
            return SetValue(
                name,
                evaluatorForges == null
                    ? ConstantNull()
                    : ExprNodeUtilityCodegen.CodegenEvaluators(evaluatorForges, method, originator, classScope));
        }

        public SAIFFInitializeBuilder Manufacturer(string name, EventBeanManufacturerForge forge)
        {
            if (forge == null) {
                return SetValue(name, ConstantNull());
            }

            var manufacturer = classScope.AddFieldUnshared<EventBeanManufacturer>(
                true, forge.Make(method, classScope));
            return SetValue(name, manufacturer);
        }

        public SAIFFInitializeBuilder Map<T>(string name, IDictionary<string, T> values)
        {
            return SetValue(name, BuildMap(values));
        }

        private CodegenExpression BuildMap<T>(IDictionary<string, T> map)
        {
            if (map == null) {
                return ConstantNull();
            }

            if (map.IsEmpty()) {
                return StaticMethod(typeof(Collections), "emptyMap");
            }

            if (map.Count == 1) {
                var single = map.First();
                return StaticMethod(
                    typeof(Collections), "singletonMap", CodegenExpressionBuilder.Constant(single.Key),
                    BuildMapValue(single.Value));
            }

            var child = method.MakeChild(typeof(IDictionary<string, T>), originator, classScope);
            child.Block.DeclareVar(
                typeof(IDictionary<string, T>), "map",
                NewInstance(
                    typeof(LinkedHashMap<string, T>),
                    CodegenExpressionBuilder.Constant(CollectionUtil.CapacityHashMap(map.Count))));
            foreach (var entry in map) {
                child.Block.ExprDotMethod(
                    Ref("map"), "put", CodegenExpressionBuilder.Constant(entry.Key), BuildMapValue(entry.Value));
            }

            return LocalMethod(child);
        }

        private CodegenExpression BuildMapValue(object value)
        {
            if (value is IDictionary<string, object>) {
                return BuildMap((IDictionary<string, object>) value);
            }

            return CodegenExpressionBuilder.Constant(value);
        }

        private SAIFFInitializeBuilder SetValue(string name, CodegenExpression expression)
        {
            method.Block.ExprDotMethod(Ref(refName), "set" + GetBeanCap(name), expression);
            return this;
        }

        private string GetBeanCap(string name)
        {
            return name.Substring(0, 1).ToUpperInvariant() + name.Substring(1);
        }

        public CodegenExpression Build()
        {
            if (closed) {
                throw new IllegalStateException("Builder already completed build");
            }

            closed = true;
            method.Block.MethodReturn(Ref(refName));
            return LocalMethod(method);
        }
    }
} // end of namespace