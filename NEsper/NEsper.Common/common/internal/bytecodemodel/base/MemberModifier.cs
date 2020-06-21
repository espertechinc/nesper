using System;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    [Flags]
    public enum MemberModifier
    {
        NONE = 0,
        VIRTUAL = 1,
        OVERRIDE = 2,
        STATIC = 4
    }

    public static class MemberModifierExtensions
    {
        public static bool IsOverride(this MemberModifier modifier)
        {
            return (modifier & MemberModifier.OVERRIDE) == MemberModifier.OVERRIDE;
        }

        public static bool IsVirtual(this MemberModifier modifier)
        {
            return (modifier & MemberModifier.VIRTUAL) == MemberModifier.VIRTUAL;
        }
        
        public static bool IsStatic(this MemberModifier modifier)
        {
            return (modifier & MemberModifier.STATIC) == MemberModifier.STATIC;
        }

        public static MemberModifier Enable(
            this MemberModifier modifier,
            MemberModifier flag)
        {
            return modifier | flag;
        }

        public static MemberModifier Disable(
            this MemberModifier modifier,
            MemberModifier flag)
        {
            return modifier & ~flag;
        }

        public static MemberModifier EnableDisable(
            this MemberModifier modifier,
            MemberModifier flag,
            bool enable)
        {
            return enable ? Enable(modifier, flag) : Disable(modifier, flag);
        }
    }
}