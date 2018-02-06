///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

public class CodegenExpressionUtil
{
    public static void RenderConstant(StringBuilder builder, Object constant)
    {
        if (constant is string)
        {
            builder.Append('"');
            string seq = (string)constant;
            if (seq.IndexOf('\"') == -1)
            {
                builder.Append(constant);
            }
            else
            {
                AppendSequenceEscapeDQ(builder, seq);
            }
            builder.Append('"');
        }
        else if (constant is char[])
        {
            AppendSequenceEscapeDQ(builder, (char[])constant);
        }
        else if (constant == null)
        {
            builder.Append("null");
        }
        else if (constant is int[])
        {
            builder.Append("new int[] {");
            int[] nums = (int[])constant;
            string delimiter = "";
            foreach (int num in nums)
            {
                builder.Append(delimiter).Append(num);
                delimiter = ",";
            }
            builder.Append("}");
        }
        else
        {
            builder.Append(constant);
        }
    }

    private static void AppendSequenceEscapeDQ(StringBuilder builder, char[] seq)
    {
        for (int i = 0; i < seq.Length; i++)
        {
            char c = seq[i];
            if (c == '\"')
            {
                builder.Append('\\');
                builder.Append(c);
            }
            else
            {
                builder.Append(c);
            }
        }
    }

    private static void AppendSequenceEscapeDQ(StringBuilder builder, string seq)
    {
        for (int i = 0; i < seq.Length; i++)
        {
            char c = seq[i];
            if (c == '\"')
            {
                builder.Append('\\');
                builder.Append(c);
            }
            else
            {
                builder.Append(c);
            }
        }
    }
}