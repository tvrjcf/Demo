namespace Py.Algorithm.Numerics
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class BigNumber
    {
        internal static string FormatBigInteger(BigInteger value, string format, NumberFormatInfo info)
        {
            int digits = 0;
            char ch = ParseFormatSpecifier(format, out digits);
            switch (ch)
            {
                case 'X':
                case 'x':
                    return FormatBigIntegerToHexString(value, ch, digits, info);

                default:
                {
                    bool flag = ((((ch == 'g') || (ch == 'G')) || ((ch == 'd') || (ch == 'D'))) || (ch == 'r')) || (ch == 'R');
                    if (!flag)
                    {
                        throw new FormatException("Format specifier was invalid.");
                    }
                    if (value._bits != null)
                    {
                        int num;
                        int num2;
                        int num4 = BigInteger.Length(value._bits);
                        try
                        {
                            num = ((num4 * 10) / 9) + 2;
                        }
                        catch (OverflowException exception)
                        {
                            throw new FormatException("The value is too large to be represented by this format specifier.", exception);
                        }
                        uint[] numArray = new uint[num];
                        int num5 = 0;
                        int index = num4;
                        while (--index >= 0)
                        {
                            uint uLo = value._bits[index];
                            for (int k = 0; k < num5; k++)
                            {
                                ulong num9 = NumericsHelpers.MakeUlong(numArray[k], uLo);
                                numArray[k] = (uint) (num9 % ((ulong) 0x3b9aca00L));
                                uLo = (uint) (num9 / ((ulong) 0x3b9aca00L));
                            }
                            if (uLo != 0)
                            {
                                numArray[num5++] = uLo % 0x3b9aca00;
                                uLo /= 0x3b9aca00;
                                if (uLo != 0)
                                {
                                    numArray[num5++] = uLo;
                                }
                            }
                        }
                        try
                        {
                            num2 = num5 * 9;
                        }
                        catch (OverflowException exception2)
                        {
                            throw new FormatException("The value is too large to be represented by this format specifier.", exception2);
                        }
                        if (flag)
                        {
                            if ((digits > 0) && (digits > num2))
                            {
                                num2 = digits;
                            }
                            if (value._sign < 0)
                            {
                                try
                                {
                                    num2 += info.NegativeSign.Length;
                                }
                                catch (OverflowException exception3)
                                {
                                    throw new FormatException("The value is too large to be represented by this format specifier.", exception3);
                                }
                            }
                        }
                        char[] chArray = new char[num2];
                        int startIndex = num2;
                        for (int i = 0; i < (num5 - 1); i++)
                        {
                            uint num12 = numArray[i];
                            int num13 = 9;
                            while (--num13 >= 0)
                            {
                                chArray[--startIndex] = (char) (0x30 + (num12 % 10));
                                num12 /= 10;
                            }
                        }
                        for (uint j = numArray[num5 - 1]; j != 0; j /= 10)
                        {
                            chArray[--startIndex] = (char) (0x30 + (j % 10));
                        }
                        int num15 = num2 - startIndex;
                        while ((digits > 0) && (digits > num15))
                        {
                            chArray[--startIndex] = '0';
                            digits--;
                        }
                        if (value._sign < 0)
                        {
                            string negativeSign = info.NegativeSign;
                            for (int m = negativeSign.Length - 1; m > -1; m--)
                            {
                                chArray[--startIndex] = negativeSign[m];
                            }
                        }
                        return new string(chArray, startIndex, num2 - startIndex);
                    }
                    switch (ch)
                    {
                        case 'g':
                        case 'r':
                        case 'G':
                        case 'R':
                            if (digits > 0)
                            {
                                format = string.Format(CultureInfo.InvariantCulture, "D{0}", digits.ToString(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                format = "D";
                            }
                            break;
                    }
                    break;
                }
            }
            return value._sign.ToString(format, info);
        }

        private static string FormatBigIntegerToHexString(BigInteger value, char format, int digits, NumberFormatInfo info)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = value.ToByteArray();
            string str = null;
            int index = buffer.Length - 1;
            if (index > -1)
            {
                bool flag = false;
                byte num2 = buffer[index];
                if (num2 > 0xf7)
                {
                    num2 = (byte) (num2 - 240);
                    flag = true;
                }
                if ((num2 < 8) || flag)
                {
                    str = string.Format(CultureInfo.InvariantCulture, "{0}1", new object[] { format });
                    builder.Append(num2.ToString(str, info));
                    index--;
                }
            }
            if (index > -1)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}2", new object[] { format });
                while (index > -1)
                {
                    builder.Append(buffer[index--].ToString(str, info));
                }
            }
            if ((digits > 0) && (digits > builder.Length))
            {
                builder.Insert(0, (value._sign >= 0) ? "0" : ((format == 'x') ? "f" : "F"), digits - builder.Length);
            }
            return builder.ToString();
        }

        internal static char ParseFormatSpecifier(string format, out int digits)
        {
            digits = -1;
            if ((format == null) || (format.Length == 0))
            {
                return 'R';
            }
            int num = 0;
            char ch = format[num];
            if (((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z')))
            {
                num++;
                int num2 = -1;
                if (((num < format.Length) && (format[num] >= '0')) && (format[num] <= '9'))
                {
                    num2 = format[num++] - '0';
                    while (((num < format.Length) && (format[num] >= '0')) && (format[num] <= '9'))
                    {
                        num2 = (num2 * 10) + (format[num++] - '0');
                        if (num2 >= 10)
                        {
                            break;
                        }
                    }
                }
                if ((num >= format.Length) || (format[num] == '\0'))
                {
                    digits = num2;
                    return ch;
                }
            }
            return '\0';
        }
    }
}

