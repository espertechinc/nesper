///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.settings
{
    public abstract class ImportServiceBase : ImportService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISet<string> _eventTypeAutoNames;
        private readonly IList<Import> _annotationImports = new List<Import>();
        private readonly IList<Import> _imports = new List<Import>();

        private readonly IDictionary<string, object> _transientConfiguration;

        public IDictionary<string, object> TransientConfiguration => _transientConfiguration;

        protected ImportServiceBase(
            IContainer container,
            IDictionary<string, object> transientConfiguration,
            TimeAbacus timeAbacus,
            ISet<string> eventTypeAutoNames)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            _transientConfiguration = transientConfiguration;
            TimeAbacus = timeAbacus;
            _eventTypeAutoNames = eventTypeAutoNames;
        }

        public IContainer Container { get; }

        public TimeAbacus TimeAbacus { get; }

        public Type ResolveType(
            string className,
            bool forAnnotation,
            ExtensionClass extension)
        {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, false, forAnnotation, extension);
            }
            catch (TypeLoadException e) {
                throw MakeClassNotFoundEx(className, e);
            }

            return clazz;
        }

        public virtual TypeResolverProvider TypeResolverProvider => TransientConfigurationResolver
            .ResolveTypeResolverProvider(Container, _transientConfiguration);

        public virtual TypeResolver TypeResolver => TransientConfigurationResolver
            .ResolveTypeResolver(Container, _transientConfiguration);

        public Type ResolveClassForBeanEventType(string fullyQualClassName)
        {
            try {
                var clazz = TypeResolver.ResolveType(fullyQualClassName, true);
                if (clazz != null) {
                    return clazz;
                }

                return TypeHelper.ResolveType(fullyQualClassName, true);
            }
            catch (TypeLoadException ex) {
                // Attempt to resolve from auto-name packages
                Type clazz = null;
                foreach (var @namespace in _eventTypeAutoNames) {
                    var generatedClassName = @namespace + "." + fullyQualClassName;
                    try {
                        var resolvedClass = TypeResolver.ResolveType(generatedClassName);
                        if (clazz != null) {
                            throw new ImportException(
                                "Failed to resolve name '" +
                                fullyQualClassName +
                                "', the class was ambiguously found both in " +
                                "namespace '" +
                                clazz.Namespace +
                                "' and in " +
                                "namespace '" +
                                resolvedClass.Namespace +
                                "'",
                                ex);
                        }

                        clazz = resolvedClass;
                    }
                    catch (TypeLoadException) {
                        // expected, class may not exists in all packages
                    }
                }

                if (clazz != null) {
                    return clazz;
                }

                return ResolveType(fullyQualClassName, false, ExtensionClassEmpty.INSTANCE);
            }
        }

        public MethodInfo ResolveMethodOverloadChecked(
            string className,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType,
            ExtensionClass extension)
        {
            Type clazz;
            try {
                clazz = ResolveClassInternal(className, false, false, extension);
            }
            catch (TypeLoadException e) {
                throw new ImportException(
                    "Could not load class by name '" + className + "', please check imports",
                    e);
            }

            try {
                return MethodResolver.ResolveMethod(
                    clazz,
                    methodName,
                    paramTypes,
                    false,
                    allowEventBeanType,
                    allowEventBeanCollType);
            }
            catch (MethodResolverNoSuchMethodException e) {
                var method = MethodResolver.ResolveExtensionMethod(
                    clazz,
                    methodName,
                    paramTypes,
                    true,
                    allowEventBeanType,
                    allowEventBeanType);
                if (method == null) {
                    throw Convert(clazz, methodName, paramTypes, e, false);
                }

                return method;
            }
        }

        public MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType)
        {
            try {
                return MethodResolver.ResolveMethod(
                    clazz,
                    methodName,
                    paramTypes,
                    true,
                    allowEventBeanType,
                    allowEventBeanCollType);
            }
            catch (MethodResolverNoSuchMethodException e) {
                var method = MethodResolver.ResolveExtensionMethod(
                    clazz,
                    methodName,
                    paramTypes,
                    true,
                    allowEventBeanType,
                    allowEventBeanType);
                if (method == null) {
                    throw Convert(clazz, methodName, paramTypes, e, true);
                }

                return method;
            }
        }

        public ConstructorInfo ResolveCtor(
            Type clazz,
            Type[] paramTypes)
        {
            try {
                return MethodResolver.ResolveCtor(clazz, paramTypes);
            }
            catch (MethodResolverNoSuchCtorException e) {
                throw Convert(clazz, paramTypes, e);
            }
        }

        public void AddImport(Import import)
        {
            ValidateImportAndAdd(import, _imports);
        }

        public void AddAnnotationImport(Import import)
        {
            ValidateImportAndAdd(import, _annotationImports);
        }

        /// <summary>
        ///     Finds a class by class name using the auto-import information provided.
        /// </summary>
        /// <param name="className">is the class name to find</param>
        /// <param name="requireAnnotation">whether the class must be an annotation</param>
        /// <param name="forAnnotationUse">whether resolving class for use with annotations</param>
        /// <param name="extension">for additional classes</param>
        /// <returns>class</returns>
        /// <throws>TypeLoadException if the class cannot be loaded</throws>
        protected Type ResolveClassInternal(
            string className,
            bool requireAnnotation,
            bool forAnnotationUse,
            ExtensionClass extension)
        {
            if (forAnnotationUse) {
                switch (className.ToLowerInvariant()) {
                    case "private":
                    case "privateattribute":
                        return typeof(PrivateAttribute);

                    case "protected":
                    case "protectedattribute":
                        return typeof(ProtectedAttribute);

                    case "public":
                    case "publicattribute":
                        return typeof(PublicAttribute);

                    case "buseventtype":
                    case "buseventtypeattribute":
                        return typeof(BusEventTypeAttribute);
                }
            }

            // attempt extension classes i.e. classes part of epl or otherwise not in classpath
            var clazzExtension = extension.FindClassByName(className);
            if (clazzExtension != null) {
                return clazzExtension;
            }

            // Attempt to retrieve the class with the name as-is
            var clazzForName = TypeResolver.ResolveType(className);
            if (clazzForName != null) {
                return clazzForName;
            }

            // check annotation-specific imports first
            if (forAnnotationUse) {
                var clazzInner = CheckImports(_annotationImports, requireAnnotation, className);
                if (clazzInner != null) {
                    return clazzInner;
                }
            }

            // check all imports
            var clazz = CheckImports(_imports, requireAnnotation, className);
            if (clazz != null) {
                return clazz;
            }

            // No import worked, the class isn't resolved
            throw new TypeLoadException("Unknown class " + className);
        }

        private Type CheckImports(
            IList<Import> imports,
            bool requireAnnotation,
            string className)
        {
            // Try all the imports
            foreach (var import in imports) {
                var clazz = import.Resolve(className, TypeResolver);
                if (clazz != null) {
                    if (requireAnnotation) {
                        if (clazz.IsAttribute()) {
                            return clazz;
                        }

                        Log.Warn(
                            "Resolved class {0}, but class was not an attribute as required",
                            className);
                    }
                    else {
                        return clazz;
                    }
                } // class not found with this import

                // clazz = ClassForNameProvider.ClassForName();
                // if ((clazz != null) && (!requireAnnotation || clazz.IsAttribute())) {
                //     return clazz;
                // }
            }

#if false
            // Try all the imports
            foreach (var import in imports) {
                var isClassName = IsClassName(import);
                var containsPackage = import.IndexOf('.') != -1;
                var classNameWithDot = "." + className;
                var classNameWithDollar = "$" + className;

                // Import is a class name
                if (isClassName) {
                    if (containsPackage && import.EndsWith(classNameWithDot) ||
                        containsPackage && import.EndsWith(classNameWithDollar) ||
                        !containsPackage && import.Equals(className) ||
                        !containsPackage && import.EndsWith(classNameWithDollar)) {
                        return ClassForNameProvider.ClassForName(import);
                    }

                    var prefixedClassName = import + '$' + className;
                    try {
                        var clazz = ClassForNameProvider.ClassForName(prefixedClassName);
                        if (!requireAnnotation || clazz.IsAttribute()) {
                            return clazz;
                        }
                    }
                    catch (TypeLoadException) {
                        if (Log.IsDebugEnabled) {
                            Log.Debug("Class not found for resolving from name '" + prefixedClassName + "'");
                        }
                    }
                }
                else {
                    if (requireAnnotation && import.Equals(ConfigurationCommon.ANNOTATION_NAMESPACE)) {
                        var clazz = BuiltinAnnotation.BUILTIN.Get(className.ToLowerInvariant());
                        if (clazz != null) {
                            return clazz;
                        }
                    }

                    // Import is a package name
                    var prefixedClassName = GetPackageName(import) + '.' + className;
                    try {
                        var clazz = ClassForNameProvider.ClassForName(prefixedClassName);
                        if (!requireAnnotation || clazz.IsAttribute()) {
                            return clazz;
                        }
                    }
                    catch (TypeLoadException) {
                        if (Log.IsDebugEnabled) {
                            Log.Debug("Class not found for resolving from name '" + prefixedClassName + "'");
                        }
                    }
                }
            }
#endif

            return null;
        }

        protected void ValidateImportAndAdd(
            Import import,
            IList<Import> imports)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Adding import " + import);
            }

            imports.Add(import);
        }

        protected ImportException MakeClassNotFoundEx(
            string className,
            Exception e)
        {
            return new ImportException(
                "Could not load class by name '" + className + "', please check imports",
                e);
        }

        protected ImportException Convert(
            Type clazz,
            Type[] paramTypes,
            MethodResolverNoSuchCtorException e)
        {
            var expected = TypeHelper.GetParameterAsString(paramTypes);
            var message = "Could not find constructor ";
            if (paramTypes.Length > 0) {
                message += "in class '" +
                           clazz.CleanName() +
                           "' with matching parameter number and expected parameter type(s) '" +
                           expected +
                           "'";
            }
            else {
                message += "in class '" + clazz.CleanName() + "' taking no parameters";
            }

            if (e.NearestMissCtor != null) {
                message += " (nearest matching constructor ";
                if (e.NearestMissCtor.GetParameters().Length == 0) {
                    message += "taking no parameters";
                }
                else {
                    message += "taking type(s) '" +
                               TypeHelper.GetParameterAsString(e.NearestMissCtor.GetParameterTypes()) +
                               "'";
                }

                message += ")";
            }

            return new ImportException(message, e);
        }

        protected ImportException Convert(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            MethodResolverNoSuchMethodException e,
            bool isInstance)
        {
            var expected = TypeHelper.GetParameterAsString(paramTypes);
            var message = "Could not find ";
            if (!isInstance) {
                message += "static method ";
            }
            else {
                message += "enumeration method, date-time method, instance method or property ";
            }

            if (paramTypes.Length > 0) {
                message +=
                    $"named '{methodName}' in class '{clazz.CleanName()}' with matching parameter number and expected parameter type(s) '{expected}'";
            }
            else {
                message += $"named '{methodName}' in class '{clazz.CleanName()}' taking no parameters";
            }

            if (e.NearestMissMethod != null) {
                message += " (nearest match found was '" + e.NearestMissMethod.Name;
                if (e.NearestMissMethod.GetParameters().Length == 0) {
                    message += "' taking no parameters";
                }
                else {
                    message += "' taking type(s) '" +
                               TypeHelper.GetParameterAsString(e.NearestMissMethod.GetParameterTypes()) +
                               "'";
                }

                message += ")";
            }

            return new ImportException(message, e);
        }


        public static bool IsClassName(string importName)
        {
            var classNameRegEx = "(\\w+\\.)*\\w+(\\+\\w+)?";
            return importName.Matches(classNameRegEx);
        }
    }
} // end of namespace