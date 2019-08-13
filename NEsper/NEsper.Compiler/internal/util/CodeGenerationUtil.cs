///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using com.espertech.esper.common.@internal.util;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CodeGenerationUtil
    {
        public static void NamespaceDecl(
            StringBuilder builder,
            string @namespace)
        {
            builder.Append("namespace ");
            builder.Append(@namespace);
            builder.Append(" {\n");
        }

        public static SyntaxList<UsingDirectiveSyntax> Importsdecl(
            ICollection<ImportDecl> imports)
        {
            var usingsList = SyntaxFactory.List<UsingDirectiveSyntax>();

            foreach (var importDecl in imports) {
                if (importDecl.Namespace != null) {
                    usingsList.Add(importDecl.UsingDirective);
                }
            }

            return usingsList;
        }

        public static void Importsdecl(
            StringBuilder builder,
            ICollection<ImportDecl> imports)
        {
            foreach (var importDecl in imports) {
                if (importDecl.Namespace != null) {
                    Importdecl(builder, importDecl);
                }
            }

            builder.Append("\n");
        }

        public static void Classimplements(
            StringBuilder builder,
            string classname,
            Type implementedInterface,
            string implementedInterfaceGeneric,
            bool isPublic,
            bool isStatic)
        {
            builder.Append("  ");

            if (isPublic) {
                builder.Append("public ");
            }

            if (isStatic) {
                builder.Append("static ");
            }

            builder.Append("class ");
            builder.Append(classname);
            if (implementedInterface != null) {
                builder.Append(" : ");

                if (implementedInterface.IsGenericTypeDefinition) {
                    if (string.IsNullOrEmpty(implementedInterfaceGeneric)) {
                        throw new ArgumentException(
                            "implementedInterfaceGeneric must be provided when interface is generic type definition");
                    }

                    AppendClassName(builder, implementedInterface);
                    builder
                        .Append("<")
                        .Append(implementedInterfaceGeneric)
                        .Append(">");
                }
                else {
                    AppendClassName(builder, implementedInterface);
                }
            }

            builder.Append(" {\n");
        }

        public static string CodeWithLineNum(string code)
        {
            var lines = FileUtil.ReadFile(new StringReader(code));
            var builder = new StringBuilder();
            var linenum = 1;
            foreach (var line in lines) {
                PaddedNumber(builder, linenum++, 4);
                builder.Append("  ");
                builder.Append(line);
                builder.Append("\n");
            }

            return builder.ToString();
        }

        private static void PaddedNumber(
            StringBuilder builder,
            int num,
            int size)
        {
            var text = Convert.ToString(num);
            if (text.Length < size) {
                for (var i = 0; i < size - text.Length; i++) {
                    builder.Append(" ");
                }
            }

            builder.Append(text);
        }

        public static void Importdecl(
            StringBuilder builder,
            ImportDecl importDecl)
        {
            builder.Append("using ");

            if (importDecl.IsStatic) {
                builder.Append("static ");
            }

            builder.Append(importDecl.Namespace);
            builder.Append(";\n");
        }
    }
} // end of namespace