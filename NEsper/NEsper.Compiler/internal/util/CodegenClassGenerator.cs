///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenClassUtil;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CodegenClassGenerator
    {
        private static readonly CodegenIndent INDENT = new CodegenIndent(true);

        public static string Compile(CodegenClass clazz)
        {
            // build members and imports
            var classes = clazz.GetReferencedClasses();
            var imports = CompileImports(classes);

            // generate code
            return GenerateCode(imports, clazz);
        }

        private static IDictionary<Type, string> CompileImports(ISet<Type> classes)
        {
            IDictionary<Type, string> imports = new Dictionary<Type, string>();
            IDictionary<string, Type> assignments = new Dictionary<string, Type>();
            foreach (var clazz in classes)
            {
                if (clazz == null || clazz.IsNested) // clazz.EnclosingClass != null
                {
                    continue;
                }

                if (clazz.IsArray)
                {
                    CompileImports(GetComponentTypeOutermost(clazz), imports, assignments);
                }
                else
                {
                    CompileImports(clazz, imports, assignments);
                }
            }

            return imports;
        }

        private static void CompileImports(
            Type clazz,
            IDictionary<Type, string> imports,
            IDictionary<string, Type> assignments)
        {
            if (clazz == null || clazz.IsPrimitive)
            {
                return;
            }

            try
            {
                if (clazz.Namespace != null &&
                    clazz.Namespace.Equals("System"))
                {
                    imports.Put(clazz, clazz.Name);
                    return;
                }
            }
            catch (Exception r)
            {
                Console.Out.WriteLine(r);
            }

            if (assignments.ContainsKey(clazz.Name))
            {
                return;
            }

            imports.Put(clazz, clazz.Name);
            assignments.Put(clazz.Name, clazz);
        }

        private static string GenerateCode(
            IDictionary<Type, string> imports,
            CodegenClass clazz)
        {
            var builder = new StringBuilder();

            CodeGenerationUtil.NamespaceDecl(builder, clazz.PackageName);
            CodeGenerationUtil.Importsdecl(builder, imports.Keys);
            CodeGenerationUtil.Classimplements(builder, clazz.ClassName, clazz.InterfaceImplemented, null, true, false, imports);

            // members
            GenerateCodeMembers(builder, clazz.ExplicitMembers, clazz.OptionalCtor, imports, 1);

            // ctor
            GenerateCodeCtor(builder, clazz.ClassName, false, clazz.OptionalCtor, imports, 0);

            // methods
            GenerateCodeMethods(builder, false, clazz.PublicMethods, clazz.PrivateMethods, imports, 0);

            // inner classes
            foreach (var inner in clazz.InnerClasses)
            {
                builder.Append("\n");
                INDENT.Indent(builder, 1);
                CodeGenerationUtil.Classimplements(
                    builder, inner.ClassName, inner.InterfaceImplemented, inner.InterfaceGenericClass, false, true, imports);

                GenerateCodeMembers(builder, inner.ExplicitMembers, inner.Ctor, imports, 2);

                GenerateCodeCtor(builder, inner.ClassName, true, inner.Ctor, imports, 1);

                GenerateCodeMethods(builder, true, inner.Methods.PublicMethods, inner.Methods.PrivateMethods, imports, 1);
                INDENT.Indent(builder, 1);
                builder.Append("}\n");
            }

            // close
            builder.Append("}\n");
            return builder.ToString();
        }

        protected static void GenerateCodeMethods(
            StringBuilder builder,
            bool isInnerClass,
            IList<CodegenMethodWGraph> publicMethods,
            IList<CodegenMethodWGraph> privateMethods,
            IDictionary<Type, string> imports,
            int additionalIndent)
        {
            // public methods
            var delimiter = "";
            foreach (var publicMethod in publicMethods)
            {
                builder.Append(delimiter);
                publicMethod.Render(builder, imports, true, isInnerClass, INDENT, additionalIndent);
                delimiter = "\n";
            }

            // private methods
            foreach (var method in privateMethods)
            {
                builder.Append(delimiter);
                method.Render(builder, imports, false, isInnerClass, INDENT, additionalIndent);
                delimiter = "\n";
            }
        }

        private static void GenerateCodeCtor(
            StringBuilder builder,
            string className,
            bool isInnerClass,
            CodegenCtor optionalCtor,
            IDictionary<Type, string> imports,
            int additionalIndent)
        {
            INDENT.Indent(builder, 1 + additionalIndent);
            builder.Append("public ").Append(className).Append("(");
            var delimiter = "";

            // parameters
            if (optionalCtor != null)
            {
                foreach (var param in optionalCtor.CtorParams)
                {
                    builder.Append(delimiter);
                    param.RenderAsParameter(builder, imports);
                    delimiter = ",";
                }
            }

            builder.Append("){\n");

            // code assigning parameters
            if (optionalCtor != null)
            {
                foreach (var param in optionalCtor.CtorParams)
                {
                    if (param.IsMemberWhenCtorParam)
                    {
                        INDENT.Indent(builder, 2 + additionalIndent);
                        builder.Append("this.").Append(param.Name).Append("=").Append(param.Name).Append(";\n");
                    }
                }
            }

            if (optionalCtor != null)
            {
                optionalCtor.Block.Render(builder, imports, isInnerClass, 2 + additionalIndent, INDENT);
            }

            INDENT.Indent(builder, 1 + additionalIndent);
            builder.Append("}\n");
            builder.Append("\n");
        }

        private static void GenerateCodeMembers(
            StringBuilder builder,
            IList<CodegenTypedParam> explicitMembers,
            CodegenCtor optionalCtor,
            IDictionary<Type, string> imports,
            int indent)
        {
            if (optionalCtor != null)
            {
                foreach (var param in optionalCtor.CtorParams)
                {
                    if (param.IsMemberWhenCtorParam)
                    {
                        INDENT.Indent(builder, indent);
                        builder.Append("final ");
                        param.RenderAsMember(builder, imports);
                        builder.Append(";\n");
                    }
                }
            }

            foreach (var param in explicitMembers)
            {
                INDENT.Indent(builder, indent);
                if (!param.IsPublic && param.IsFinal)
                {
                    builder.Append("final ");
                }

                if (param.IsStatic)
                {
                    builder.Append("static ");
                }

                param.RenderType(builder, imports);
                builder.Append(" ").Append(param.Name);
                builder.Append(";\n");
            }

            builder.Append("\n");
        }
    }
} // end of namespace