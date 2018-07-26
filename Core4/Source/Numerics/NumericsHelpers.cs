using System;
using System.Runtime.InteropServices;

namespace Py.Algorithm.Numerics {

    static class NumericsHelpers {
        private const int kcbitUint = 0x20;

        public static int CbitHighZero(uint u) {
            if (u == 0) {
                return kcbitUint;
            }
            int num = 0;
            if ((u & 0xffff0000) == 0) {
                num += 0x10;
                u = u << 0x10;
            }
            if ((u & 0xff000000) == 0) {
                num += 8;
                u = u << 8;
            }
            if ((u & 0xf0000000) == 0) {
                num += 4;
                u = u << 4;
            }
            if ((u & 0xc0000000) == 0) {
                num += 2;
                u = u << 2;
            }
            if ((u & 0x80000000) == 0) {
                num++;
            }
            return num;
        }

        public static int CbitHighZero(ulong uu) {
            if ((uu & 18446744069414584320L) == 0L) {
                return (kcbitUint + CbitHighZero((uint)uu));
            }
            return CbitHighZero((uint)(uu >> kcbitUint));
        }

        public static int CbitLowZero(uint u) {
            if (u == 0) {
                return kcbitUint;
            }
            int num = 0;
            if ((u & 0xffff) == 0) {
                num += 0x10;
                u = u >> 0x10;
            }
            if ((u & 0xff) == 0) {
                num += 8;
                u = u >> 8;
            }
            if ((u & 15) == 0) {
                num += 4;
                u = u >> 4;
            }
            if ((u & 3) == 0) {
                num += 2;
                u = u >> 2;
            }
            if ((u & 1) == 0) {
                num++;
            }
            return num;
        }

        public static uint[] DangerousMakeTwosComplement(uint[] d) {
            int index = 0;
            uint num2 = 0;
            while (index < d.Length) {
                num2 = ~d[index] + 1;
                d[index] = num2;
                if (num2 != 0) {
                    index++;
                    break;
                }
                index++;
            }
            if (num2 != 0) {
                while (index < d.Length) {
                    d[index] = ~d[index];
                    index++;
                }
                return d;
            }
            d = Resize(d, d.Length + 1);
            d[d.Length - 1] = 1;
            return d;
        }

        public static uint GCD(uint u1, uint u2) {
            if (u1 < u2) {
                goto Label_0021;
            }
        Label_0004:
            if (u2 == 0) {
                return u1;
            }
            int num = kcbitUint;
            do {
                u1 -= u2;
                if (u1 < u2) {
                    goto Label_0021;
                }
            }
            while (--num != 0);
            u1 = u1 % u2;
        Label_0021:
            if (u1 == 0) {
                return u2;
            }
            int num2 = kcbitUint;
            do {
                u2 -= u1;
                if (u2 < u1) {
                    goto Label_0004;
                }
            }
            while (--num2 != 0);
            u2 = u2 % u1;
            goto Label_0004;
        }

        public static ulong GCD(ulong uu1, ulong uu2) {
            uint num;
            if (uu1 < uu2) {
                goto Label_0028;
            }
        Label_0004:
            if (uu1 <= 0xffffffffL) {
                goto Label_004E;
            }
            if (uu2 == 0L) {
                return uu1;
            }
            int num2 = kcbitUint;
            do {
                uu1 -= uu2;
                if (uu1 < uu2) {
                    goto Label_0028;
                }
            }
            while (--num2 != 0);
            uu1 = uu1 % uu2;
        Label_0028:
            if (uu2 > 0xffffffffL) {
                if (uu1 == 0L) {
                    return uu2;
                }
                int num3 = kcbitUint;
                do {
                    uu2 -= uu1;
                    if (uu2 < uu1) {
                        goto Label_0004;
                    }
                }
                while (--num3 != 0);
                uu2 = uu2 % uu1;
                goto Label_0004;
            }
        Label_004E:
            num = (uint)uu1;
            uint num4 = (uint)uu2;
            if (num < num4) {
                goto Label_0077;
            }
        Label_0058:
            if (num4 == 0) {
                return (ulong)num;
            }
            int num5 = kcbitUint;
            do {
                num -= num4;
                if (num < num4) {
                    goto Label_0077;
                }
            }
            while (--num5 != 0);
            num = num % num4;
        Label_0077:
            if (num == 0) {
                return (ulong)num4;
            }
            int num6 = kcbitUint;
            do {
                num4 -= num;
                if (num4 < num) {
                    goto Label_0058;
                }
            }
            while (--num6 != 0);
            num4 = num4 % num;
            goto Label_0058;
        }

        public static double GetDoubleFromParts(int sign, int exp, ulong man) {
            DoubleUlong @ulong;
            @ulong.dbl = 0.0;
            if (man == 0L) {
                @ulong.uu = 0L;
            } else {
                int num = CbitHighZero(man) - 11;
                if (num < 0) {
                    man = man >> -num;
                } else {
                    man = man << num;
                }
                exp -= num;
                exp += 0x433;
                if (exp >= 0x7ff) {
                    @ulong.uu = 0x7ff0000000000000L;
                } else if (exp <= 0) {
                    exp--;
                    if (exp < -52) {
                        @ulong.uu = 0L;
                    } else {
                        @ulong.uu = man >> -exp;
                    }
                } else {
                    @ulong.uu = (man & ((ulong)0xfffffffffffffL)) | (((ulong)exp) << 0x34);
                }
            }
            if (sign < 0) {
                @ulong.uu |= 9223372036854775808L;
            }
            return @ulong.dbl;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct DoubleUlong {
            [FieldOffset(0)]
            public double dbl;
            [FieldOffset(0)]
            public ulong uu;
        }

        public static void GetDoubleParts(double dbl, out int sign, out int exp, out ulong man, out bool fFinite) {
            DoubleUlong @ulong;
            @ulong.uu = 0L;
            @ulong.dbl = dbl;
            sign = 1 - (((int)(@ulong.uu >> 0x3e)) & 2);
            man = @ulong.uu & ((ulong)0xfffffffffffffL);
            exp = ((int)(@ulong.uu >> 0x34)) & 0x7ff;
            if (exp == 0) {
                fFinite = true;
                if (man != 0L) {
                    exp = -1074;
                }
            } else if (exp == 0x7ff) {
                fFinite = false;
                exp = 0x7fffffff;
            } else {
                fFinite = true;
                man = (ulong)(man | 0x10000000000000L);
                exp -= 0x433;
            }
        }

        public static uint GetHi(ulong uu) {
            return (uint)(uu >> kcbitUint);
        }

        public static ulong MakeUlong(uint uHi, uint uLo) {
            return ((uHi << kcbitUint) | uLo);
        }

        public static uint[] Resize(uint[] v, int len) {
            if (v.Length == len) {
                return v;
            }
            uint[] numArray = new uint[len];
            int num = Math.Min(v.Length, len);
            for (int i = 0; i < num; i++) {
                numArray[i] = v[i];
            }
            return numArray;
        }

    }
}

