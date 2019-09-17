///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace com.espertech.esper.compiler.@internal.util
{
    public class ImportDecl : IComparable<ImportDecl>
    {
        public string Namespace { get; set; }
        public string TypeName { get; set; }

        public bool IsNamespaceImport => TypeName == null;

        // Converts the import into a using directive syntax expression
        public UsingDirectiveSyntax UsingDirective {
            get {
                UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(Namespace));
                //if (IsStatic) {
                //    usingDirectiveSyntax = usingDirectiveSyntax.WithStaticKeyword(
                //        SyntaxFactory.Token(SyntaxKind.StaticKeyword));
                //}

                return usingDirectiveSyntax;
            }
        }

        public ImportDecl(
            string @namespace,
            string typeName = null)
        {
            Namespace = @namespace
                .Replace(".internal.", ".@internal.")
                .Replace(".internal", ".@internal")
                .Replace(".base.", ".@base.")
                .Replace(".base", ".@base")
                .Replace(".lock.", ".@lock.")
                .Replace(".lock", ".@lock")
                .Replace(".event.", ".@event.")
                .Replace(".event", ".@event");
            TypeName = typeName;
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

            var nameComparison = String.Compare(
                Namespace, that.Namespace, StringComparison.Ordinal);

            if (this.Namespace == "System") {
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
                    // do nothing with the name comparison
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
                if (nameComparison != 0) {
                    nameComparison = String.Compare(
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