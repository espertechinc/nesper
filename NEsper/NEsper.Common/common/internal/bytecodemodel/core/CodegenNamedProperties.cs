///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenNamedProperties
    {
        private IDictionary<string, CodegenProperty> _properties;

        public IDictionary<string, CodegenProperty> Properties =>
            _properties ?? Collections.GetEmptyMap<string, CodegenProperty>();

        public CodegenProperty AddProperty(
            Type returnType,
            string propertyName,
            Type generator,
            CodegenClassScope classScope,
            Consumer<CodegenProperty> code)
        {
            return AddPropertyWithSymbols(
                returnType,
                propertyName,
                generator,
                classScope,
                code,
                CodegenSymbolProviderEmpty.INSTANCE);
        }

        public CodegenProperty AddPropertyWithSymbols(
            Type returnType,
            string propertyName,
            Type generator,
            CodegenClassScope classScope,
            Consumer<CodegenProperty> code,
            CodegenSymbolProvider symbolProvider)
        {
            if (_properties == null) {
                _properties = new Dictionary<string, CodegenProperty>();
            }

            Console.WriteLine("AddPropertyWithSymbols: {0}", propertyName);

            var existing = _properties.Get(propertyName);
            if (existing != null) {
                if (existing.ReturnType == returnType) {
                    return existing;
                }

                throw new IllegalStateException("Property by name '" + propertyName + "' already registered");
            }

            var property = CodegenProperty.MakePropertyNode(
                returnType, 
                generator,
                symbolProvider,
                classScope);

            _properties.Put(propertyName, property);
            code.Invoke(property);
            return property;
        }

        public CodegenProperty GetProperty(string propertyName)
        {
            var property = _properties.Get(propertyName);
            if (property == null) {
                throw new IllegalStateException("Property by name '" + propertyName + "' not found");
            }

            return property;
        }
    }
} // end of namespace