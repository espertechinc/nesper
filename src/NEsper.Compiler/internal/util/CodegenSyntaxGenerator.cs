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
using System.Reflection;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CodegenSyntaxGenerator
    {
        private static readonly CodegenIndent INDENT = new CodegenIndent(true);

        private class AssemblyIndexCache
        {
            private readonly Assembly[] indices;

            public AssemblyIndexCache(Assembly[] assemblies)
            {
                indices = assemblies;
            }

            private bool DoesImportResolveType(
                Type type,
                ImportDecl import)
            {
                if (import.IsNamespaceImport)
                {
                    var importName = $"{import.Namespace}.{type.Name}".Replace("@", "");
                    return indices.Any(assembly => assembly.GetType(importName, false) != null);
                }

                return import.TypeName == type.Name;
            }
            
            public bool IsAmbiguous(
                Type type,
                ISet<ImportDecl> imports)
            {
                var count = 0;
             
                foreach (var import in imports) {
                    if (DoesImportResolveType(type, import)) {
                        if (++count > 1) {
                            return true; // it is ambiguous
                        }
                    }
                }

                return false;
                //return imports.Count(import => DoesImportResolveType(type, import)) > 1;
            }
        }

        public static string Compile(CodegenClass clazz)
        {
            // build members and imports
            var classes = clazz.GetReferencedClasses();
            var imports = CompileImports(classes);

            // generate code
            return GenerateCode(imports, clazz);
        }

        private static ICollection<ImportDecl> CompileImports(IEnumerable<Type> types)
        {
            var typeList = types
                .Where(type => type != null && type != typeof(void))
                .ToList();

            ISet<ImportDecl> imports = new SortedSet<ImportDecl>();
            imports.Add(new ImportDecl(typeof(int).Namespace, null));
            imports.Add(new ImportDecl(typeof(CompatExtensions).Namespace, null));
            imports.Add(new ImportDecl(typeof(UnsupportedOperationException).Namespace, null));
            imports.Add(new ImportDecl(typeof(Enumerable).Namespace, null));

            var typeListSize = typeList.Count;
            for (var ii = 0; ii < typeListSize; ii++) {
                var type = typeList[ii];
                if (type.Namespace != null) {
                    imports.Add(new ImportDecl(type.Namespace, null));
                }
            }

            var assemblyIndices = new AssemblyIndexCache(AppDomain.CurrentDomain.GetAssemblies());

            // Ensure that all types can be imported without any ambiguity.

            foreach (var type in typeList) {
                if (assemblyIndices.IsAmbiguous(type, imports)) {
                    if (type.Namespace != null) {
                        imports.Add(
                            new ImportDecl(
                                type.Namespace,
                                type.CleanName(false)));
                    }
                }
            }

            return imports;
        }

        private static string GenerateCode(
            ICollection<ImportDecl> imports,
            CodegenClass clazz)
        {
            var builder = new StringBuilder();

            CodeGenerationUtil.Importsdecl(builder, imports);
            CodeGenerationUtil.NamespaceDecl(builder, clazz.Namespace);
            CodeGenerationUtil.Classimplements(builder, clazz.ClassName, clazz.BaseList, true, false);

            // members
            GenerateCodeMembers(builder, clazz.ExplicitMembers, clazz.OptionalCtor, 2);

            // ctor
            GenerateCodeCtor(builder, clazz.ClassName, false, clazz.OptionalCtor, 1);

            // properties
            GenerateCodeProperties(builder, false, clazz.PublicProperties, clazz.PrivateProperties, 1);

            // methods
            GenerateCodeMethods(builder, false, clazz.PublicMethods, clazz.PrivateMethods, 1);

            // inner classes
            foreach (var inner in clazz.InnerClasses)
            {
                builder.Append("\n");
                INDENT.Indent(builder, 2);
                CodeGenerationUtil.Classimplements(
                    builder,
                    inner.ClassName,
                    inner.BaseList,
                    false,
                    false);

                GenerateCodeMembers(
                    builder,
                    inner.ExplicitMembers,
                    inner.Ctor,
                    2);

                GenerateCodeCtor(
                    builder,
                    inner.ClassName,
                    true,
                    inner.Ctor,
                    1);

                GenerateCodeProperties(
                    builder,
                    true,
                    inner.Properties.PublicProperties,
                    inner.Properties.PrivateProperties,
                    1);

                GenerateCodeMethods(
                    builder,
                    true,
                    inner.Methods.PublicMethods,
                    inner.Methods.PrivateMethods,
                    1);

                INDENT.Indent(builder, 1);
                builder.Append("}\n");
            }

            // close
            builder.Append("  }\n"); // class
            builder.Append("}\n"); // namespace
            return builder.ToString();
        }

        private static string GenerateSyntax(
            ICollection<ImportDecl> imports,
            CodegenClass clazz)
        {
            var builder = new StringBuilder();

            var usingsList = CodeGenerationUtil.Importsdecl(imports);
            var namespaceUnit = NamespaceDeclaration(ParseName(clazz.Namespace));

            // Generate the class using descending construction of the class elements
            // relative to the syntax tree.  All items should be children of the namespaceUnit.

            namespaceUnit = namespaceUnit.AddMembers(clazz.CodegenSyntax());

            // inner classes
            foreach (var inner in clazz.InnerClasses) {
                builder.Append("\n");
                INDENT.Indent(builder, 2);
                CodeGenerationUtil.Classimplements(
                    builder,
                    inner.ClassName,
                    inner.BaseList,
                    false,
                    false);

                GenerateCodeMembers(
                    builder,
                    inner.ExplicitMembers,
                    inner.Ctor,
                    2);

                GenerateCodeCtor(
                    builder,
                    inner.ClassName,
                    true,
                    inner.Ctor,
                    1);

                GenerateCodeProperties(
                    builder,
                    true,
                    inner.Properties.PublicProperties,
                    inner.Properties.PrivateProperties,
                    1);

                GenerateCodeMethods(
                    builder,
                    true,
                    inner.Methods.PublicMethods,
                    inner.Methods.PrivateMethods,
                    1);

                INDENT.Indent(builder, 1);
                builder.Append("}\n");
            }

            var membersList = SingletonList<MemberDeclarationSyntax>(namespaceUnit);

            var compilationUnit = CompilationUnit()
                .WithUsings(usingsList)
                .WithMembers(membersList)
                .NormalizeWhitespace();

            return compilationUnit.ToFullString();
        }

        internal static void GenerateCodeProperties(
            StringBuilder builder,
            bool isInnerClass,
            IList<CodegenPropertyWGraph> publicProperties,
            IList<CodegenPropertyWGraph> privateProperties,
            int additionalIndent)
        {
            // public methods
            var delimiter = "";
            foreach (var property in publicProperties) {
                builder.Append(delimiter);
                property.Render(builder, true, isInnerClass, INDENT, additionalIndent);
                delimiter = "\n";
            }

            // private methods
            foreach (var property in privateProperties) {
                builder.Append(delimiter);
                property.Render(builder, false, isInnerClass, INDENT, additionalIndent);
                delimiter = "\n";
            }
        }

        internal static void GenerateCodeMethods(
            StringBuilder builder,
            bool isInnerClass,
            IList<CodegenMethodWGraph> publicMethods,
            IList<CodegenMethodWGraph> privateMethods,
            int additionalIndent)
        {
            // public methods
            var delimiter = "";
            foreach (var publicMethod in publicMethods) {
                builder.Append(delimiter);
                publicMethod.Render(builder, true, isInnerClass, INDENT, additionalIndent);
                delimiter = "\n";
            }

            // private methods
            foreach (var method in privateMethods) {
                builder.Append(delimiter);
                method.Render(builder, false, isInnerClass, INDENT, additionalIndent);
                delimiter = "\n";
            }
        }

        private static void GenerateCodeCtor(
            StringBuilder builder,
            string className,
            bool isInnerClass,
            CodegenCtor optionalCtor,
            int additionalIndent)
        {
            INDENT.Indent(builder, 1 + additionalIndent);
            builder.Append("public ").Append(className).Append("(");
            var delimiter = "";

            // parameters
            if (optionalCtor != null) {
                foreach (var param in optionalCtor.CtorParams) {
                    builder.Append(delimiter);
                    param.RenderAsParameter(builder);
                    delimiter = ",";
                }
            }

            builder.Append("){\n");

            // code assigning parameters
            if (optionalCtor != null) {
                foreach (var param in optionalCtor.CtorParams) {
                    if (param.IsMemberWhenCtorParam) {
                        INDENT.Indent(builder, 2 + additionalIndent);
                        builder
                            .Append("this.")
                            .Append(param.Name)
                            .Append("=")
                            .Append(param.Name)
                            .Append(";\n");
                    }
                }
            }

            optionalCtor?.Block.Render(builder, isInnerClass, 2 + additionalIndent, INDENT);

            INDENT.Indent(builder, 1 + additionalIndent);
            builder.Append("}\n");
            builder.Append("\n");
        }

        private static void GenerateCodeMembers(
            StringBuilder builder,
            IList<CodegenTypedParam> explicitMembers,
            CodegenCtor optionalCtor,
            int indent)
        {
            if (optionalCtor != null) {
                foreach (var param in optionalCtor.CtorParams) {
                    if (param.IsMemberWhenCtorParam) {
                        INDENT.Indent(builder, indent);
                        builder.Append("internal ");
                        param.RenderAsMember(builder);
                        builder.Append(";\n");
                    }
                }
            }

            foreach (var param in explicitMembers) {
                INDENT.Indent(builder, indent);

                if (param.IsPublic) {
                    builder.Append("public ");
                }
                else {
                    builder.Append("internal ");
                }

                //if (param.IsReadonly) {
                //    builder.Append("readonly ");
                //}

                if (param.IsStatic) {
                    builder.Append("static ");
                }

                param.RenderType(builder);
                builder
                    .Append(" ")
                    .Append(param.Name)
                    .Append(";\n");
            }

            builder.Append("\n");
        }
    }
} // end of namespace