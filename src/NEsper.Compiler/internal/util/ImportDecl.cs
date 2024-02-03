///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace com.espertech.esper.compiler.@internal.util
{
    public class ImportDecl : IComparable<ImportDecl>
    {
        private string _namespace;

        public string Namespace {
            get => _namespace;
            set => _namespace = CleanNamespace(value);
        }

        public string TypeName { get; set; }

        public bool IsNamespaceImport => TypeName == null;
        
        public IEnumerable<UsingDirectiveSyntax> UsingDirectives { get; }

        private static string CleanNamespace(string value)
        {
            if (value == null)
                return null;
            
            return value
                .Replace(".internal.", ".@internal.")
                .Replace(".internal", ".@internal")
                .Replace(".base.", ".@base.")
                .Replace(".base", ".@base")
                .Replace(".lock.", ".@lock.")
                .Replace(".lock", ".@lock")
                .Replace(".event.", ".@event.")
                .Replace(".event", ".@event");
        }

        private static string GetNestedTypeName(Type type)
        {
            if (type.IsNested) {
                return GetNestedTypeName(type.DeclaringType) + "." + type.Name;
            }

            return type.Name;
        }
        
        private static IEnumerable<UsingDirectiveSyntax> ConvertToUsingDirectives(Type type, ISet<Type> visitorSet)
        {
            visitorSet ??= new HashSet<Type>();

            if (!visitorSet.Contains(type)) {
                visitorSet.Add(type);

                if (type.IsArray) {
                    foreach (var directive in ConvertToUsingDirectives(type.GetElementType(), visitorSet)) {
                        yield return directive;
                    }
                }
                else if (type.IsGenericType) {
                    // Generic types cannot be imported using an alias.  As such, their entire
                    // namespace must be imported or they must be aliased to a very specific
                    // type.  Both of these cause some issues.
                    yield return SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(CleanNamespace(type.Namespace)));
                    foreach (var argument in type.GetGenericArguments()) {
                        if (!argument.IsGenericParameter) {
                            foreach (var directive in ConvertToUsingDirectives(argument, visitorSet)) {
                                yield return directive;
                            }
                        }
                    }
                }
                else {
                    var typeAlias = type.Name;
                    var typeNamespace = CleanNamespace(type.Namespace);

                    SimpleNameSyntax typeNameSyntax = SyntaxFactory.IdentifierName(type.Name);
                    if (type.IsNested && type.DeclaringType != null) {
                        foreach (var directive in ConvertToUsingDirectives(type.DeclaringType, visitorSet)) {
                            yield return directive;
                        }

                        typeNameSyntax = SyntaxFactory.IdentifierName(GetNestedTypeName(type));
                    }

                    NameSyntax importName;
                    if (typeNamespace != null) {
                        importName = SyntaxFactory.QualifiedName(
                            SyntaxFactory.ParseName(typeNamespace),
                            typeNameSyntax);
                    }
                    else {
                        importName = typeNameSyntax;
                    }

                    if (type.IsDefined(typeof(ExtensionAttribute), false)) {
                        yield return SyntaxFactory.UsingDirective(
                            SyntaxFactory.Token(SyntaxKind.UsingKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                            null,
                            importName,
                            SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    }

                    yield return SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(typeAlias), importName);
                }
            }
        }

        public static UsingDirectiveSyntax ConvertToSimpleUsingDirective(string @namespace, string typeName)
        {
            UsingDirectiveSyntax usingDirectiveSyntax;
            if (typeName == null) {
                usingDirectiveSyntax = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace));
            }
            else {
                usingDirectiveSyntax = SyntaxFactory.UsingDirective(
                    SyntaxFactory.NameEquals(typeName),
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.ParseName(@namespace),
                        SyntaxFactory.IdentifierName(typeName)));
            }
            //if (IsStatic) {
            //    usingDirectiveSyntax = usingDirectiveSyntax.WithStaticKeyword(
            //        SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            //}

            return usingDirectiveSyntax;
        }
        
        public ImportDecl(Type type)
        {
            Namespace = type.Namespace;
            TypeName = type.Name;
            UsingDirectives = ConvertToUsingDirectives(type, null).ToList();
        }
        
        public ImportDecl(
            string @namespace,
            string typeName)
        {
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            TypeName = typeName;
            UsingDirectives = Collections.SingletonList(
                ConvertToSimpleUsingDirective(@namespace, typeName));
        }

        protected bool Equals(ImportDecl other)
        {
            return string.Equals(Namespace, other.Namespace)
                   && string.Equals(TypeName, other.TypeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((ImportDecl) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((Namespace != null ? Namespace.GetHashCode() : 0) * 397) 
                       ^ (TypeName != null ? TypeName.GetHashCode() : 0);
            }
        }

        public int CompareTo(ImportDecl that)
        {
            // Compare the namespaces - this forms the baseline for the order of items in
            // the set.  However, anything beginning with "System" holds a special place
            // at the front of the set.

            var nameComparison = string.Compare(
                Namespace, that.Namespace, StringComparison.Ordinal);

            if (this.Namespace == null) {
                if (that.Namespace == null) {
                    nameComparison = 0;
                }
                else {
                    nameComparison = -1;
                }
            }
            else if (this.Namespace == "System") {
                if (that.Namespace == "System") {
                    nameComparison = 0;
                }
                else {
                    nameComparison = -1;
                }
            }
            else if (that.Namespace == "System") {
                nameComparison = 1;
            }
            else if (this.Namespace.StartsWith("System.")) {
                if (this.Namespace == that.Namespace) {
                    nameComparison = 0;
                } else if (!that.Namespace.StartsWith("System.")) {
                    nameComparison = -1;
                }
            }
            else if (that.Namespace.StartsWith("System.")) {
                nameComparison = 1;
            }

            // Type specific imports are placed after namespace imports.  This simply assist with
            // rendering order.

            if (this.TypeName != null) {
                if (that.TypeName == null) {
                    return 1;
                }
                if (nameComparison == 0) {
                    nameComparison = string.Compare(
                        TypeName, that.TypeName, StringComparison.Ordinal);
                }
            }
            else if (that.TypeName != null) {
                return -1;
            }

            return nameComparison;
        }

        public override string ToString()
        {
            return $"ImportDecl: {nameof(Namespace)}: {Namespace}, {nameof(TypeName)}: {TypeName}";
        }
    }
}