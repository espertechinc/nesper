///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compiler.@internal.util
{
    public class ImportDecl : IComparable<ImportDecl>
    {
        public bool IsStatic { get; set; }
        public string Namespace { get; set; }

        public ImportDecl(bool isStatic,
            string ns)
        {
            IsStatic = isStatic;
            Namespace = ns
                .Replace(".internal.", ".@internal.")
                .Replace(".base.", ".@base.")
                .Replace(".lock.", ".@lock.")
                .Replace(".event.", ".@event.");
        }

        public ImportDecl()
        {
        }

        protected bool Equals(ImportDecl other)
        {
            return IsStatic == other.IsStatic && string.Equals(Namespace, other.Namespace);
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
                return (IsStatic.GetHashCode() * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
            }
        }

        public int CompareTo(ImportDecl that)
        {
            if (this.Namespace == that.Namespace) {
                if (this.IsStatic == that.IsStatic) {
                    return 0;
                }
                else if (this.IsStatic) {
                    return 1;
                }
                else {
                    return -1;
                }
            }

            // Compare the namespaces - this forms the baseline for the order of items in
            // the set.  However, anything beginning with "System" holds a special place
            // at the front of the set.

            var nameComparison = Namespace.CompareTo(that.Namespace);

            if (this.Namespace == "System") {
                nameComparison = -1;
            } else if (that.Namespace == "System") {
                nameComparison = 1;
            } else if (this.Namespace.StartsWith("System.")) {
                if (!that.Namespace.StartsWith("System.")) {
                    nameComparison = -1;
                }
            } else if (that.Namespace.StartsWith("System.")) {
                nameComparison = 1;
            }

            // Static imports are placed after non-static imports.  This simply assist with
            // rendering order.

            if (this.IsStatic) {
                if (!that.IsStatic) {
                    return 1;
                }
            } else if (that.IsStatic) {
                return -1;
            }

            return nameComparison;
        }
    }
}