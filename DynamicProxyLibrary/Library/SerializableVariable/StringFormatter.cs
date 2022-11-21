namespace Microsoft.Tools.TestClient
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.IO;
    using System.Text;

    internal class StringFormatter
    {
        internal static string FromEscapeCode(string input)
        {
            char[] chArray = input.ToCharArray();
            StringBuilder builder = new StringBuilder();
            int index = 0;
            int num2 = 0;
            while (index < chArray.Length)
            {
                char ch = chArray[index];
                switch (num2)
                {
                    case 0:
                        if (chArray[index] == '\\')
                        {
                            num2 = 1;
                        }
                        else
                        {
                            num2 = 0;
                            builder.Append(ch);
                        }
                        goto Label_0087;

                    case 1:
                        switch (ch)
                        {
                            case 'r':
                                num2 = 0;
                                builder.Append('\r');
                                goto Label_0087;

                            case 'n':
                                num2 = 0;
                                builder.Append('\n');
                                goto Label_0087;

                            case 't':
                                num2 = 0;
                                builder.Append('\t');
                                goto Label_0087;

                            case '\\':
                                num2 = 0;
                                builder.Append('\\');
                                goto Label_0087;
                        }
                        break;
                }
                return null;
            Label_0087:
                index++;
            }
            if (num2 == 0)
            {
                return builder.ToString();
            }
            return null;
        }

        internal static string ToEscapeCode(string input)
        {
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            new CSharpCodeProvider().GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, new CodeGeneratorOptions());
            return writer.ToString();
        }
    }
}

