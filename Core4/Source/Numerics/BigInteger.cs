
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Py.RunTime;

namespace Py.Algorithm.Numerics {
    
	/// <summary>
	/// 表示任意大的带符号整数。
	/// </summary>
	/// <remarks>
	/// 见 <see href="http://msdn.microsoft.com/zh-cn/library/system.numerics.biginteger.aspx"/> 。
	/// </remarks>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct BigInteger :IFormattable, IComparable, IComparable<BigInteger>, IEquatable<BigInteger> {
        private const int MaskHighBit = -2147483648;
        private const uint UmaskHighBit = 0x80000000;
        private const int Buint = 0x20;
        private const int BuLong = 0x40;
        private const int DecimalScaleFactorMask = 0xff0000;
        private const int DecimalSignMask = -2147483648;
        internal int _sign;
        internal uint[] _bits;
        private static readonly BigInteger _minInt = new BigInteger(-1, new uint[] { UmaskHighBit });
        private static readonly BigInteger _oneInt = new BigInteger(1);
        private static readonly BigInteger _zeroInt = new BigInteger(0);
        private static readonly BigInteger _minusOneInt = new BigInteger(-1);

        /// <summary>
        /// 获取一个表示数字 0（零）的值。
        /// </summary>
        /// <value>其值为 0（零）的整数。</value>
        /// <remarks>
        /// 由此属性返回的 BigInteger 对象提供一个方便的零值源，供在赋值和比较中使用。
        /// </remarks>
        public static BigInteger Zero {
            get {
                return _zeroInt;
            }
        }

        /// <summary>
        /// 获取一个表示数字一 (1) 的值。
        /// </summary>
        /// <value>其值为一 (1) 的对象。</value>
        /// <remarks>
        /// One 属性通常用于将 BigInteger 值与 1 比较以及将 1 赋值给 BigInteger 对象。
        /// </remarks>
        public static BigInteger One {
            get {
                return _oneInt;
            }
        }

        /// <summary>
        /// 获取一个表示数字负一 (-1) 的值。
        /// </summary>
        /// <value>其值为负一 (-1) 的整数。</value>
        /// <remarks>
        /// MinusOne 属性用于将 BigInteger 值与 -1 比较以及将 -1 赋值给 BigInteger 对象
        /// </remarks>
        public static BigInteger MinusOne {
            get {
                return _minusOneInt;
            }
        }

        /// <summary>
        /// 指示当前 BigInteger 对象的值是否是 2 的幂。
        /// </summary>
        /// <value>如果 BigInteger 对象的值是 2 的幂，则为 true；否则为 false。</value>
        /// <remarks>
        /// 此属性确定 BigInteger 值是否具有一个非零位集。这意味着如果当前 BigInteger 对象的值为 1（即 20）或 2 的任意较大幂，则它返回 true。如果当前 BigInteger 对象的值为 0，则返回 false。
        /// </remarks>
        public static bool IsPowerOfTwo(BigInteger value) {
            if (value._bits == null) {
                return (((value._sign & (value._sign - 1)) == 0) && (value._sign != 0));
                }
            if (value._sign == 1) {
                int index = Length(value._bits) - 1;
                if ((value._bits[index] & (value._bits[index] - 1)) == 0) {
                        while (--index >= 0) {
                            if (value._bits[index] != 0) {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
        }
        
        /// <summary>
        /// 指示当前 BigInteger 对象的值是否是 BigInteger.Zero。
        /// </summary>
        /// <value>BigInteger 对象的值是 BigInteger.Zero，则为 true；否则为 false。</value>
        /// <remarks>
        /// 此属性提供的性能明显优于 BigInteger.Equals(BigInteger.Zero)。
        /// </remarks>
        public static bool IsZero(BigInteger value) {
            return (value._sign == 0);
        }
        
        /// <summary>
        /// 指示当前 BigInteger 对象的值是否是 BigInteger.One。
        /// </summary>
        /// <value>如果 BigInteger 对象的值是 BigInteger.One，则为 true；否则为 false。</value>
        /// <remarks>
        /// 此属性提供的性能明显优于其他比较方法，例如 thisBigInteger.Equals(BigInteger.One)。
        /// </remarks>
        public static bool IsOne(BigInteger value) {
            return ((value._sign == 1) && (value._bits == null));
        }

        /// <summary>
        /// 指示当前 BigInteger 对象的值是否是偶数。
        /// </summary>
        /// <value>如果 BigInteger 对象的值是偶数，则为 true；否则为 false。</value>
        /// <remarks>
        /// 此属性是一个便利功能，指示 BigInteger 值是否能被 2 整除。它等效于以下表达式：[c#]value % 2 == 0;
        /// </remarks>
        public static bool IsEven(BigInteger value) {
            if (value._bits != null) {
                return ((value._bits[0] & 1) == 0);
            }
            return ((value._sign & 1) == 0);
        }

        /// <summary>
        /// 获取一个数字，该数字指示当前 BigInteger 对象的符号（负、正或零）。
        /// </summary>
        /// <value>一个指示 BigInteger 对象的符号的数字，如下表所示。</value>
        /// <remarks>
        /// Sign 属性与用于基元数值类型的 Math.Sign 方法等效。
        /// </remarks>
        public int Sign {
            get {
                return ((_sign >> 0x1f) - (-_sign >> 0x1f));
            }
        }

		/// <summary>
		/// 指示此实例与指定对象是否相等。
		/// </summary>
		/// <param name="obj">要比较的另一个对象。</param>
		/// <returns>
		/// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 true；否则为 false。
		/// </returns>
        public override bool Equals(object obj) {
            return ((obj is BigInteger) && Equals((BigInteger)obj));
        }

		/// <summary>
		/// 返回此实例的哈希代码。
		/// </summary>
		/// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public override int GetHashCode() {
            if (_bits == null) {
                return _sign;
            }
            int num = _sign;
            int index = Length(_bits);
            while (--index >= 0) {
                num = (int)((((uint)num << 7) | ((uint)num >> 0x19)) ^ (uint)_bits[index]);
            }
            return num;
        }

        /// <summary>
        /// 返回一个值，该值指示当前实例与 64 位带符号整数是否具有相同的值。
        /// </summary>
        /// <param name="other">要比较的 64 位带符号整数值。</param>
        /// <returns>如果 64 位带符号整数与当前实例具有相同的值，则为 true；否则为 false。</returns>
        /// <remarks>
        /// 如果 other 是 Byte、Int16、Int32、SByte、UInt16 或 UInt32 值，则在调用方法时，它将被隐式转换为 Int64 值。若要确定两个对象之间的关系，而不是测试其相等性，请调用 BigInteger.CompareTo(Int64) 方法。
        /// </remarks>
        public bool Equals(long other) {
            int num;
            if (_bits == null) {
                return (_sign == other);
            }
            if (((_sign ^ other) < 0L) || ((num = Length(_bits)) > 2)) {
                return false;
            }
            ulong num2 = (other < 0L) ? ((ulong)-other) : ((ulong)other);
            if (num == 1) {
                return (_bits[0] == num2);
            }
            return (NumericsHelpers.MakeUlong(_bits[1], _bits[0]) == num2);
        }

		/// <summary>
		/// 将此实例与 64 位无符号整数进行比较，并返回一个整数，该整数指示此实例的值是小于、等于还是大于 64 位无符号整数的值。
		/// </summary>
		/// <param name="other">要比较的 64 位无符号整数。</param>
		/// <returns>一个带符号整数，指示此实例和 other 的相对值。返回值小于零说明当前实例小于 other。返回值零说明当前实例等于 other。返回值大于零说明当前实例大于 other。</returns>
        [CLSCompliant(false)]    
        public bool Equals(ulong other) {
            if (_sign < 0) {
                return false;
            }
            if (_bits == null) {
                return ((ulong)_sign == other);
            }
            int num = Length(_bits);
            if (num > 2) {
                return false;
            }
            if (num == 1) {
                return (_bits[0] == other);
            }
            return (NumericsHelpers.MakeUlong(_bits[1], _bits[0]) == other);
        }

        /// <summary>
        /// 返回一个值，该值指示当前实例与指定的 BigInteger 对象是否具有相同的值。
        /// </summary>
        /// <param name="other">要比较的对象。</param>
        /// <returns>如果此 BigInteger 对象与 other 具有相同的值，则为 true；否则为 false。</returns>
        /// <remarks>
        /// 此方法实现 IEquatable&lt;T&gt; 接口，并且执行效果略好于 Equals(Object)，这是因为它不必将 other 参数转换为 BigInteger 对象。若要确定两个 BigInteger 对象之间的关系，而不是测试其相等性，请调用 BigInteger.CompareTo(BigInteger) 方法。
        /// </remarks>
        public bool Equals(BigInteger other) {
            if (_sign != other._sign) {
                return false;
            }
            if (_bits == other._bits) {
                return true;
            }
            if ((_bits == null) || (other._bits == null)) {
                return false;
            }
            int cu = Length(_bits);
            if (cu != Length(other._bits)) {
                return false;
            }
            return (GetDiffLength(_bits, other._bits, cu) == 0);
        }

        /// <summary>
        /// 将此实例与另一个 BigInteger 进行比较，并返回一个整数，该整数指示此实例的值是小于、等于还是大于指定对象的值。
        /// </summary>
        /// <param>要比较的对象。</param>
        /// <returns>一个带符号整数值，指示此实例与 other 的关系，如下表所示。返回值小于零说明当前实例小于 other。返回值零说明当前实例等于 other。返回值大于零说明当前实例大于 other。</returns>
        /// <remarks>
        /// CompareTo 方法的此重载将实现 IComparable.CompareTo 方法。它被泛型集合对象使用以整理集合中的项。
        /// </remarks>
        public int CompareTo(int other) {
            int num;
            if (_bits == null) {
                return _sign.CompareTo(other);
            }
            if (((_sign ^ other) < 0L) || ((num = Length(_bits)) > 2)) {
                return _sign;
            }
            int num3 = (other < 0L) ? ((int)-other) : ((int)other);
            int num4 = (num == 2) ? (int)NumericsHelpers.MakeUlong(_bits[1], _bits[0]) : ((int)_bits[0]);
            return (_sign * num4.CompareTo(num3));
        }

        /// <summary>
        /// 将此实例与 64 位带符号整数进行比较，并返回一个整数，该整数指示此实例的值是小于、等于还是大于 64 位带符号整数的值。
        /// </summary>
        /// <param name="other">要比较的 64 位带符号整数。返回值</param>
        /// <returns>
        /// 一个带符号整数值，指示此实例与 other 的关系。返回值小于零说明当前实例小于 other。返回值零说明当前实例等于 other。返回值大于零说明当前实例大于 other。
        /// </returns>
        /// <remarks>
        /// 如果 other 是 Byte、Int16、Int32、SByte、UInt16 或 UInt32 值，则在调用 CompareTo(Int64) 方法时，它将被隐式转换为 Int64 值。
        /// </remarks>
        public int CompareTo(long other) {
            int num;
            if (_bits == null) {
                long num2 = _sign;
                return num2.CompareTo(other);
            }
            if (((_sign ^ other) < 0L) || ((num = Length(_bits)) > 2)) {
                return _sign;
            }
            ulong num3 = (other < 0L) ? ((ulong)-other) : ((ulong)other);
            ulong num4 = (num == 2) ? NumericsHelpers.MakeUlong(_bits[1], _bits[0]) : ((ulong)_bits[0]);
            return (_sign * num4.CompareTo(num3));
        }

        /// <summary>
        /// 将此实例与 64 位无符号整数进行比较，并返回一个整数，该整数指示此实例的值是小于、等于还是大于 64 位无符号整数的值。
        /// </summary>
        /// <param name="other">要比较的 64 位无符号整数。</param>
        /// <returns>一个带符号整数，指示此实例和 other 的相对值。返回值小于零说明当前实例小于 other。返回值零说明当前实例等于 other。返回值大于零说明当前实例大于 other。</returns>
        [CLSCompliant(false)]
        public int CompareTo(ulong other) {
            if (_sign < 0) {
                return -1;
            }
            if (_bits == null) {
                ulong num = (ulong)_sign;
                return num.CompareTo(other);
            }
            int num2 = Length(_bits);
            if (num2 > 2) {
                return 1;
            }
            ulong num3 = (num2 == 2) ? NumericsHelpers.MakeUlong(_bits[1], _bits[0]) : ((ulong)_bits[0]);
            return num3.CompareTo(other);
        }

        /// <summary>
        /// 将此实例与另一个 BigInteger 进行比较，并返回一个整数，该整数指示此实例的值是小于、等于还是大于指定对象的值。
        /// </summary>
        /// <param name="other">System.Numerics.BigInteger要比较的对象。</param>
        /// <returns>一个带符号整数值，指示此实例与 other 的关系。返回值小于零说明当前实例小于 other。 返回值零说明当前实例等于 other。返回值大于零说明当前实例大于 other。
		/// </returns>
        /// <remarks>
        /// CompareTo 方法的此重载将实现 IComparable&lt;T&gt;.CompareTo 方法。它被泛型集合对象使用以整理集合中的项。
        /// </remarks>
        public int CompareTo(BigInteger other) {
            int num;
            int num2;
            if ((_sign ^ other._sign) < 0) {
                if (_sign >= 0) {
                    return 1;
                }
                return -1;
            }
            if (_bits == null) {
                if (other._bits != null) {
                    return -other._sign;
                }
                if (_sign < other._sign) {
                    return -1;
                }
                if (_sign <= other._sign) {
                    return 0;
                }
                return 1;
            }
            if ((other._bits == null) || ((num = Length(_bits)) > (num2 = Length(other._bits)))) {
                return _sign;
            }
            if (num >= num2) {
                int num3 = GetDiffLength(_bits, other._bits, num);
                if (num3 == 0) {
                    return 0;
                }
                if (_bits[num3 - 1] >= other._bits[num3 - 1]) {
                    return _sign;
                }
            }
            return -_sign;
        }

        /// <summary>
        /// 将此实例与指定对象进行比较，并返回一个整数，该整数指示此实例的值是小于、等于还是大于指定对象的值。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>一个带符号整数，指示当前实例与 obj 参数的关系，如下表所示。返回值小于零说明当前实例小于 obj。返回值零说明当前实例等于 obj。 返回值大于零说明当前实例大于 obj，或者 obj 参数为 null。</returns>
		/// <exception cref="ArgumentException"><paramref name="obj"/> 不是 BigInteger。</exception>
        /// <remarks>CompareTo 方法的此重载将实现 IComparable.CompareTo 方法。它被非泛型集合对象使用以整理集合中的项。obj 参数必须是以下之一：运行时类型为 BigInteger 的对象。值为 null 的 Object 变量。如果 obj 参数的值是 null，则该方法返回 1，这说明当前实例大于 obj。
        /// </remarks>
        public int CompareTo(object obj) {
            if (obj == null) {
                return 1;
            }
            Thrower.ThrowArgumentExceptionIf(!(obj is BigInteger),"obj", Properties.Messages.BigIntegerParam);
            return CompareTo((BigInteger)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray() {
            uint[] numArray;
            byte num;
            if ((_bits == null) && (_sign == 0)) {
                return new byte[1];
            }
            if (_bits == null) {
                numArray = new uint[] { (uint)_sign };
                num = (_sign < 0) ? ((byte)0xff) : ((byte)0);
            } else if (_sign == -1) {
                numArray = (uint[])_bits.Clone();
                NumericsHelpers.DangerousMakeTwosComplement(numArray);
                num = 0xff;
            } else {
                numArray = _bits;
                num = 0;
            }
            byte[] sourceArray = new byte[4 * numArray.Length];
            int num2 = 0;
            for (int i = 0; i < numArray.Length; i++) {
                uint num4 = numArray[i];
                for (int j = 0; j < 4; j++) {
                    sourceArray[num2++] = (byte)(num4 & 0xff);
                    num4 = num4 >> 8;
                }
            }
            int index = sourceArray.Length - 1;
            while (index > 0) {
                if (sourceArray[index] != num) {
                    break;
                }
                index--;
            }
            bool flag = (sourceArray[index] & 0x80) != (num & 0x80);
            byte[] destinationArray = new byte[(index + 1) + (flag ? 1 : 0)];
            Array.Copy(sourceArray, 0, destinationArray, 0, index + 1);
            if (flag) {
                destinationArray[destinationArray.Length - 1] = num;
            }
            return destinationArray;
        }

        private uint[] ToUInt32Array() {
            uint[] numArray;
            uint maxValue;
            if ((_bits == null) && (_sign == 0)) {
                return new uint[1];
            }
            if (_bits == null) {
                numArray = new uint[] { (uint)_sign };
                maxValue = (_sign < 0) ? uint.MaxValue : 0;
            } else if (_sign == -1) {
                numArray = (uint[])_bits.Clone();
                NumericsHelpers.DangerousMakeTwosComplement(numArray);
                maxValue = uint.MaxValue;
            } else {
                numArray = _bits;
                maxValue = 0;
            }
            int index = numArray.Length - 1;
            while (index > 0) {
                if (numArray[index] != maxValue) {
                    break;
                }
                index--;
            }
            bool flag = (numArray[index] & UmaskHighBit) != (maxValue & UmaskHighBit);
            uint[] destinationArray = new uint[(index + 1) + (flag ? 1 : 0)];
            Array.Copy(numArray, 0, destinationArray, 0, index + 1);
            if (flag) {
                destinationArray[destinationArray.Length - 1] = maxValue;
            }
            return destinationArray;
        }

		/// <summary>
		/// 将当前 BigInteger 对象的数值转换为其等效的字符串表示形式。
		/// </summary>
		/// <returns>
		/// 包含完全限定类型名的 <see cref="T:System.String"/>。
		/// </returns>
        public override string ToString() {
            return BigNumber.FormatBigInteger(this, null, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// 使用指定的区域性特定格式设置信息将当前 BigInteger 对象的数值转换为它的等效字符串表示形式。
        /// </summary>
        /// <param name="provider">一个提供区域性特定的格式设置信息的对象。</param>
        /// <returns>当前 BigInteger 值的字符串表示形式，该值使用 provider 参数指定的格式。</returns>
        /// <value>
        /// 返回的字符串使用通用格式说明符 ("G") 进行格式设置。ToString(IFormatProvider) 方法支持 50 位十进制数字的精度。也就是说，如果 BigInteger 值中的数字位数超过 50，则只有 50 个最高有效位保留在输出字符串中；所有其他数字都被替换为零。provider 参数是 IFormatProvider 实现。其 GetFormat 方法返回一个 NumberFormatInfo 对象，该对象提供有关此方法返回的字符串格式的区域性特定信息。如果 provider 为 null，则使用当前区域性的 NumberFormatInfo 对象对 BigInteger 值进行格式设置。控制使用一般格式说明符的 BigInteger 值的字符串表示形式的 NumberFormatInfo 对象的唯一属性是 NumberFormatInfo.NegativeSign，它定义表示负号的字符。provider 参数可以是以下项之一：提供格式设置信息的 NumberFormatInfo 对象。一个实现 IFormatProvider 的自定义对象。其 GetFormat 方法返回提供格式设置信息的 NumberFormatInfo 对象。
        /// </value>
        public string ToString(IFormatProvider provider) {
            return BigNumber.FormatBigInteger(this, null, NumberFormatInfo.GetInstance(provider));
        }

        /// <summary>
        /// 使用指定的格式将当前 BigInteger 对象的数值转换为它的等效字符串表示形式。
        /// </summary>
        /// <param name="format">标准或自定义的数值格式字符串。</param>
        /// <returns>
        /// 类型：System.String当前 BigInteger 值的字符串表示形式，该值使用 format 参数指定的格式。
        /// </returns>
        /// <exception>异常,FormatException format。条件，format 不是有效的格式字符串。</exception>          
        public string ToString(string format) {
            return BigNumber.FormatBigInteger(this, format, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// 使用指定的格式和区域性特定格式信息将当前 BigInteger 对象的数值转换为它的等效字符串表示形式。
        /// </summary>
        /// <param name="format">标准或自定义的数值格式字符串。</param>
        /// <param name="provider">一个提供区域性特定的格式设置信息的对象。</param>
        /// <returns>由 format 和 provider 参数指定的当前 BigInteger 值的字符串表示形式。</returns>
		/// <exception cref="FormatException"><paramref name="format"/> 不是有效的格式字符串。</exception>
        /// <remarks>
        /// format 参数可以是任何有效标准数值格式说明符，或者是自定义数值格式说明符的任何组合。如果 format 等于 String.Empty 或者为 null，则当前 BigInteger 对象的返回值用通用数值格式说明符（"G"）进行格式设置。如果 format 是任何其他值，则该方法将引发 FormatException。在大多数的情况下，ToString 方法支持 50 位十进制数字的精度。也就是说，如果 BigInteger 值中的数字位数超过 50，则只有 50 个最高有效位保留在输出字符串中；所有其他数字都被替换为零。然而，BigInteger 支持“R”标准格式说明符，用于往返的数值。使用 "R" 格式字符串的 ToString(String) 方法返回的字符串保留整个 BigInteger 值，然后可以使用 Parse 或 TryParse 方法解析以恢复原始值而不会有任何数据损失。.NET Framework 提供了广泛的格式设置支持，下面的格式设置主题中对此有更详细的描述：有关数值格式说明符的更多信息，请参见标准数字格式字符串和自定义数字格式字符串。有关对 .NET Framework 中的格式设置支持的更多信息，请参见格式化类型。provider 参数是 IFormatProvider 实现。其 GetFormat 方法返回一个 NumberFormatInfo 对象，该对象提供有关此方法返回的字符串格式的区域性特定信息。在调用 ToString(String, IFormatProvider) 方法时，它会调用 provider 参数的 GetFormat 方法，并向其传递一个表示 NumberFormatInfo 类型的 Type 对象。该 GetFormat 方法然后返回 NumberFormatInfo 对象，该对象提供用于设置 value 参数的格式的信息，例如负号、组分隔符或小数点符号。有三种使用 provider 参数向 ToString(String, IFormatProvider) 方法提供格式设置信息的方式：您可以传递一个 CultureInfo 对象，表示提供格式设置信息的区域性。其 GetFormat 方法返回 NumberFormatInfo 对象，该对象提供针对该区域性的数值格式设置信息。可以传递提供数值格式设置信息的实际 NumberFormatInfo 对象。（其 GetFormat 实现仅返回它自身。）可以传递一个实现 IFormatProvider 的自定义对象。其 GetFormat 方法实例化并返回提供格式设置信息的 NumberFormatInfo 对象。如果 provider 为 null，则返回的字符串的格式设置基于当前区域性的 NumberFormatInfo 对象。
        /// </remarks>
        public string ToString(string format, IFormatProvider provider) {
            return BigNumber.FormatBigInteger(this, format, NumberFormatInfo.GetInstance(provider));
        }
   
        /// <summary>
        /// 使用 32 位带符号整数值初始化 BigInteger 结构的新实例。
        /// </summary>
        /// <param name="value"> 32位带符号整数。</param>
        public BigInteger(int value) {
            if (value == MaskHighBit) {
                this = _minInt;
            } else {
                _sign = value;
                _bits = null;
            }
        }

        /// <summary>
        /// 使用 32 位无符号整数值初始化 BigInteger 结构的新实例。此 API 不兼容 CLS。 兼容 CLS 的替代 API 为 BigInteger(Int64)。
        /// </summary>
        /// <param name="value">32无符号整数值。</param>
        /// <remarks>
        /// 使用此构造函数实例化 BigInteger 时没有精度损失。调用此构造函数所产生的 BigInteger 值与将 UInt32 值赋值给 BigInteger 的结果相同。
        /// </remarks>
        [CLSCompliant(false)]
        public BigInteger(uint value) {
            if (value <= 0x7fffffff) {
                _sign = (int)value;
                _bits = null;
            } else {
                _sign = 1;
                _bits = new uint[] { value };
            }
        }

        /// <summary>
        /// 使用 64 位带符号整数值初始化 BigInteger 结构的新实例。
        /// </summary>
        /// <param name="value">64位带符号整数。</param>
        /// <remarks>
        /// 使用此构造函数实例化 BigInteger 对象时没有精度损失。调用此构造函数所产生的 BigInteger 值与将 Int64 值赋值给 BigInteger 的结果相同。
        /// </remarks>
        public BigInteger(long value) {
            if ((-2147483648L <= value) && (value <= 0x7fffffffL)) {
                if (value == -2147483648L) {
                    this = _minInt;
                } else {
                    _sign = (int)value;
                    _bits = null;
                }
            } else {
                ulong num = 0L;
                if (value < 0L) {
                    num = (ulong)-value;
                    _sign = -1;
                } else {
                    num = (ulong)value;
                    _sign = 1;
                }
                _bits = new uint[] { (uint)num, (uint)(num >> Buint) };
            }
        }

        /// <summary>
        /// 使用 64 位无符号整数值初始化 BigInteger 结构的新实例。此 API 不兼容 CLS。 兼容 CLS 的替代 API 为 BigInteger(Double)。
        /// </summary>
        /// <param name="value">64 位无符号整数。</param>
        /// <remarks>
        /// 使用此构造函数实例化 BigInteger 时没有精度损失。调用此构造函数所产生的 BigInteger 值与将 UInt64 值赋值给 BigInteger 的结果相同。
        /// </remarks>
        [CLSCompliant(false)]
        public BigInteger(ulong value) {
            if (value <= 0x7fffffffL) {
                _sign = (int)value;
                _bits = null;
            } else {
                _sign = 1;
                _bits = new uint[] { (uint)value, (uint)(value >> Buint) };
            }
        }

        /// <summary>
        /// 使用单精度浮点值初始化 BigInteger 结构的新实例。
        /// </summary>
        /// <param name="value">单精度浮点值。</param>
		/// <exception cref="OverflowException"> <paramref name="value"/>  的值为 Single.NaN。- 或 -<paramref name="value"/> 的值为 Single.NegativeInfinity。- 或 -<paramref name="value"/> 的值为 Single.PositiveInfinity。</exception>
        /// <remarks>
        /// value 参数的小数部分在实例化 BigInteger 对象时被截断。因为缺少 Single 数据类型的精度，调用此构造函数可能导致数据丢失。调用此构造函数所产生的 BigInteger 值与将 Single 值显式赋值给 BigInteger 的结果相同。
        /// </remarks>
        public BigInteger(float value) {
            if (float.IsInfinity(value)) {
                throw new OverflowException(Properties.Messages.BigIntegerIsInfinity);
            }
            if (float.IsNaN(value)) {
                throw new OverflowException(Properties.Messages.BigIntegerIsNaN);
            }
            _sign = 0;
            _bits = null;
            SetBitsFromDouble((double)value);
        }

        /// <summary>
        /// 使用双精度浮点值初始化 BigInteger 结构的新实例。
        /// </summary>
        /// <param name="value">一个双精度浮点值。</param>
		/// <exception cref="OverflowException"><paramref name="value"/> 的值为 Double.NaN。- 或 -<paramref name="value"/> 的值为 Double.NegativeInfinity。- 或 -<paramref name="value"/> 的值为 Double.PositiveInfinity。</exception>
        /// <remarks>
        /// 参数的小数部分在实例化 BigInteger 对象时被截断。因为缺少 Double 数据类型的精度，调用此构造函数可能导致数据丢失。调用此构造函数所产生的 BigInteger 值与将 Double 值显式赋值给 BigInteger 的结果相同。
        /// </remarks>
        public BigInteger(double value) {
            if (double.IsInfinity(value)) {
                throw new OverflowException(Properties.Messages.BigIntegerIsInfinity);
            }
            if (double.IsNaN(value)) {
                throw new OverflowException(Properties.Messages.BigIntegerIsNaN);
            }
            _sign = 0;
            _bits = null;
            SetBitsFromDouble(value);
        }

        /// <summary>
        /// 使用 Decimal 值初始化 BigInteger 结构的新实例。
        /// </summary>
        /// <param name="value">一个小数。</param>
        /// <remarks>
        /// 调用此构造函数的结果与将 Decimal 值显式赋给 BigInteger 变量相同。调用此构造函数可能导致数据丢失；实例化 BigInteger 对象时，将截断 value 的所有小数部分。
        /// </remarks>
        public BigInteger(decimal value) {
            int[] bits = decimal.GetBits(decimal.Truncate(value));
            int num = 3;
            while ((num > 0) && (bits[num - 1] == 0)) {
                num--;
            }
            if (num == 0) {
                this = _zeroInt;
            } else if ((num == 1) && (bits[0] > 0)) {
                _sign = bits[0];
                _sign *= ((bits[3] & -2147483648) != 0) ? -1 : 1;
                _bits = null;
            } else {
                _bits = new uint[num];
                _bits[0] = (uint)bits[0];
                if (num > 1) {
                    _bits[1] = (uint)bits[1];
                }
                if (num > 2) {
                    _bits[2] = (uint)bits[2];
                }
                _sign = ((bits[3] & -2147483648) != 0) ? -1 : 1;
            }
        }

        /// <summary>
        /// 使用字节数组中的值初始化 BigInteger 结构的新实例。
        /// </summary>
        /// <param name="value">顺序为 little-endian 的字节值的数组。</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> 为 null。</exception>
        /// <remarks>
        /// value 数组中的各个字节应该为 little-endian 顺序，从最低序位字节到最高序位字节。
        /// </remarks>
        [CLSCompliant(false)]
        public BigInteger(byte[] value) {
            Thrower.ThrowArgumentNullExceptionIf(value, "value");
            int length = value.Length;
            bool flag = (length > 0) && ((value[length - 1] & 0x80) == 0x80);
            while ((length > 0) && (value[length - 1] == 0)) {
                length--;
            }
            if (length == 0) {
                _sign = 0;
                _bits = null;
            } else if (length <= 4) {
                if (flag) {
                    _sign = -1;
                } else {
                    _sign = 0;
                }
                for (int i = length - 1; i >= 0; i--) {
                    _sign = _sign << 8;
                    _sign |= value[i];
                }
                _bits = null;
                if ((_sign < 0) && !flag) {
                    _bits = new uint[] { (uint)_sign };
                    _sign = 1;
                }
                if (_sign == -2147483648) {
                    this = _minInt;
                }
            } else {
                int num3 = length % 4;
                int num4 = (length / 4) + ((num3 == 0) ? 0 : 1);
                bool flag2 = true;
                uint[] d = new uint[num4];
                int index = 3;
                int num6 = 0;
                while (num6 < (num4 - ((num3 == 0) ? 0 : 1))) {
                    for (int j = 0; j < 4; j++) {
                        if (value[index] != 0) {
                            flag2 = false;
                        }
                        d[num6] = d[num6] << 8;
                        d[num6] |= value[index];
                        index--;
                    }
                    index += 8;
                    num6++;
                }
                if (num3 != 0) {
                    if (flag) {
                        d[num4 - 1] = uint.MaxValue;
                    }
                    for (index = length - 1; index >= (length - num3); index--) {
                        if (value[index] != 0) {
                            flag2 = false;
                        }
                        d[num6] = d[num6] << 8;
                        d[num6] |= value[index];
                    }
                }
                if (flag2) {
                    this = _zeroInt;
                } else if (flag) {
                    NumericsHelpers.DangerousMakeTwosComplement(d);
                    int num8 = d.Length;
                    while ((num8 > 0) && (d[num8 - 1] == 0)) {
                        num8--;
                    }
                    if ((num8 == 1) && (d[0] > 0)) {
                        if (d[0] == 1) {
                            this = _minusOneInt;
                        } else if (d[0] == UmaskHighBit) {
                            this = _minInt;
                        } else {
                            _sign = (int)(uint.MaxValue * d[0]);
                            _bits = null;
                        }
                    } else if (num8 != d.Length) {
                        _sign = -1;
                        _bits = new uint[num8];
                        Array.Copy(d, 0, _bits, 0, num8);
                    } else {
                        _sign = -1;
                        _bits = d;
                    }
                } else {
                    _sign = 1;
                    _bits = d;
                }
            }
        }

        /// <summary>
		/// 将数字的字符串表示形式转换为它的等效 BigInteger 表示形式。
        /// </summary>
        /// <param name="value">包含要转换的数字的字符串。</param>
        /// <returns>一个值，等于 value 参数中指定的数字。</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> 为空。</exception>
		/// <exception cref="FormatException"><paramref name="value"/> 的格式不正确。</exception>
        public static BigInteger Parse(string value) {
            int length = value.Length;
            int sign = 1;
            int num3 = 0;
            if (value[0] == '+') {
                num3++;
            } else if (value[0] == '-') {
                num3++;
                sign = -1;
            }
            Thrower.ThrowArgumentExceptionIf(Py.Core.Check.IsInt(value), Properties.Messages.BigIntegerIsIntError);
            uint u = 0;
            int num6 = ((length - num3) > 9) ? (9 + num3) : length;
            while (num3 < num6) {
                u *= 10;
                u += (uint)value[num3] - '0';
                num3++;
            }
            if (num3 == length) {
                return new BigInteger((int)(u * sign));
            }
            int num7 = (length - num3) / 9;
            int num8 = (length - num3) % 9;
            BigIntegerBuilder builder = new BigIntegerBuilder(num7 + 2);
            builder.Set(u);
            for (int j = 0; j < num7; j++) {
                u = 0;
                int num10 = 9 + num3;
                while (num3 < num10) {
                    u *= 10;
                    u += (uint)value[num3] - '0';
                    num3++;
                }
                builder.Mul((uint)0x3b9aca00);
                builder.Add(u);
            }
            if (num8 > 0) {
                uint num11 = 1;
                u = 0;
                int num12 = num8 + num3;
                while (num3 < num12) {
                    num11 *= 10;
                    u *= 10;
                    u += (uint)value[num3] - '0';
                    num3++;
                }
                builder.Mul(num11);
                builder.Add(u);
            }
            return builder.GetInteger(sign);
        }

        internal BigInteger(int n, uint[] rgu) {
            _sign = n;
            _bits = rgu;
        }

        internal BigInteger(uint[] value, bool negative) {
            Thrower.ThrowArgumentNullExceptionIf(value, "value");


            int length = value.Length;
            while ((length > 0) && (value[length - 1] == 0)) {
                length--;
            }
            if (length == 0) {
                this = _zeroInt;
            } else if ((length == 1) && (value[0] < UmaskHighBit)) {
                _sign = negative ? ((int)-(value[0])) : ((int)value[0]);
                _bits = null;
                if (_sign == -2147483648) {
                    this = _minInt;
                }
            } else {
                _sign = negative ? -1 : 1;
                _bits = new uint[length];
                Array.Copy(value, 0, _bits, 0, length);
            }
        }

        private BigInteger(uint[] value) {
            Thrower.ThrowArgumentNullExceptionIf(value, "value");
            int length = value.Length;
            bool flag = (length > 0) && ((value[length - 1] & UmaskHighBit) == UmaskHighBit);
            while ((length > 0) && (value[length - 1] == 0)) {
                length--;
            }
            switch (length) {
                case 0:
                    this = _zeroInt;
                    return;

                case 1:
                    if ((value[0] >= 0) || flag) {
                        if (UmaskHighBit == value[0]) {
                            this = _minInt;
                            return;
                        }
                        _sign = (int)value[0];
                        _bits = null;
                        return;
                    }
                    _bits = new uint[] { value[0] };
                    _sign = 1;
                    return;
            }
            if (!flag) {
                if (length != value.Length) {
                    _sign = 1;
                    _bits = new uint[length];
                    Array.Copy(value, 0, _bits, 0, length);
                } else {
                    _sign = 1;
                    _bits = value;
                }
            } else {
                NumericsHelpers.DangerousMakeTwosComplement(value);
                int num2 = value.Length;
                while ((num2 > 0) && (value[num2 - 1] == 0)) {
                    num2--;
                }
                if ((num2 == 1) && (value[0] > 0)) {
                    if (value[0] == 1) {
                        this = _minusOneInt;
                    } else if (value[0] == UmaskHighBit) {
                        this = _minInt;
                    } else {
                        _sign = (int)(uint.MaxValue * value[0]);
                        _bits = null;
                    }
                } else if (num2 != value.Length) {
                    _sign = -1;
                    _bits = new uint[num2];
                    Array.Copy(value, 0, _bits, 0, num2);
                } else {
                    _sign = -1;
                    _bits = value;
                }
            }
        }

		/// <summary>
		/// 比较两个 BigInteger 值，并返回一个整数，该整数指示第一个值是小于、等于还是大于第二个值。
		/// </summary>
		/// <param name="left">要比较的第一个值。</param>
		/// <param name="right">要比较的第二个值。</param>
		/// <returns>一个带符号整数，指示 left 与 right 的相对值，如下表所示。</returns>
        public static int Compare(BigInteger left, BigInteger right) {
            return left.CompareTo(right);
        }

        /// <summary>
        /// 获取 BigInteger 对象的绝对值。 
        /// </summary>
        /// <param name="value">数字。</param>
        /// <returns>value 的绝对值。</returns>
        /// <remarks>
        /// 数字的绝对值是该数字去掉其符号后的数字，如下表所示。
        /// </remarks>
        /// <example>
        /// 下面的示例使用 Abs 方法将 BigInteger 值从 2 的补码表示形式转换为符号数值表示形式，然后再将其序列化到文件。 对文件中的数据进行反序列化，并将其分配给新 BigInteger 对象。 
        /// </example>
        public static BigInteger Abs(BigInteger value) {
            if (value < Zero) {
                return -(value);
            }
            return value;
        }

        /// <summary>
        /// 将两个 BigInteger 值相加，并返回结果。
        /// </summary>
        /// <param name="left">要相加的第一个值。</param>
        /// <param name="right">要相加的第二个值。</param>
        /// <returns>left 与 right 的和。</returns>
        /// <remarks>
        /// 不支持运算符重载或自定义运算符的语言可以使用 Add 方法执行使用 BigInteger 值的加法。
        /// 在通过赋值加法的和的方法来实例化 BigInteger 变量时，Add 方法是加法运算符一个有用替代，这将在下面的示例中介绍。
        /// <example>
        /// <code lang="C#">
        /// // The statement:
        /// //    BigInteger number = Int64.MaxValue + Int32.MaxValue;
        /// // produces compiler error CS0220: The operation overflows at compile time in checked mode.
        /// // The alternative:
        /// BigInteger number = BigInteger.Add(Int64.MaxValue, Int32.MaxValue);
        /// </code>
        /// <code lang="VB.net">
        /// ' The statement
        /// '    Dim number As BigInteger = Int64.MaxValue + Int32.MaxValue
        /// ' produces compiler error BC30439: Constant expression not representable in type 'Long'.
        /// ' The alternative:
        /// Dim number As BigInteger = BigInteger.Add(Int64.MaxValue, Int32.MaxValue)
        /// 
        /// </code>
        /// </example>
        /// </remarks>
        public static BigInteger Add(BigInteger left, BigInteger right) {
            
            return (left + right);
        }

		/// <summary>
		/// 从另一个值中减去一个 BigInteger 值并返回结果。
		/// </summary>
		/// <param name="left">要从中减去的值（被减数）。</param>
		/// <param name="right">要减去的值（减数）。</param>
		/// <returns>right 减 left 所得的结果。</returns>
        public static BigInteger Subtract(BigInteger left, BigInteger right) {
            return (left - right);
        }

        /// <summary>
		/// 返回两个 BigInteger 值的乘积。
        /// </summary>
		/// <param name="left">要相乘的第一个数字。</param>
		/// <param name="right">要相乘的第二个数字。</param>
		/// <returns>left 与 right 参数的乘积。</returns>
        public static BigInteger Multiply(BigInteger left, BigInteger right) {
            return (left * right);
        }

		/// <summary>
		/// 用另一个值除 BigInteger 值并返回结果。
		/// </summary>
		/// <param name="dividend">要作为被除数的值。</param>
		/// <param name="divisor">要作为除数的值。</param>
		/// <returns>相除后的商。</returns>
		/// <exception cref="DivideByZeroException">divisor为 0 （零）。</exception>
        public static BigInteger Divide(BigInteger dividend, BigInteger divisor) {
            return (dividend / divisor);
        }

		/// <summary>
		/// 对两个 BigInteger 值执行整除并返回余数。
		/// </summary>
		/// <param name="dividend">要作为被除数的值。</param>
		/// <param name="divisor">要作为除数的值。</param>
		/// <returns>将 dividend 除以 divisor 后所得的余数。</returns>
		/// <exception cref="DivideByZeroException">divisor为 0 （零）。</exception>
        public static BigInteger Remainder(BigInteger dividend, BigInteger divisor) {
            return dividend % divisor;
        }

		/// <summary>
		/// 用另一个值除一个 BigInteger 值，返回结果，并在输出参数中返回余数。
		/// </summary>
		/// <param name="dividend">要作为被除数的值。</param>
		/// <param name="divisor">要作为除数的值。</param>
		/// <param name="remainder">当此方法返回时，包含一个表示相除余数的 BigInteger 值。该参数未经初始化即被传递。</param>
		/// <returns>相除后的商。</returns>
		/// <exception cref="DivideByZeroException">divisor为 0 （零）。</exception>
        public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out BigInteger remainder) {
            int sign = 1;
            int num2 = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(dividend, ref sign);
            BigIntegerBuilder regDen = new BigIntegerBuilder(divisor, ref num2);
            BigIntegerBuilder regQuo = new BigIntegerBuilder();
            builder.ModDiv(ref regDen, ref regQuo);
            remainder = builder.GetInteger(sign);
            return regQuo.GetInteger(sign * num2);
        }

		/// <summary>
		/// 对指定的 BigInteger 值求反。
		/// </summary>
		/// <param name="value">要求反的值。</param>
		/// <returns>value 参数乘以负一 (-1) 的结果。</returns>
        public static BigInteger Negate(BigInteger value) {
            return -(value);
        }

		/// <summary>
		/// 返回指定数字的自然对数（底为 e）。
		/// </summary>
		/// <param name="value">要查找其对数的数字。</param>
		/// <returns>value 的自然对数（底为 e），如“备注”部分中的表所示。</returns>
		/// <exception cref="ArgumentOutOfRangeException">value 的对数超出了 Double 数据类型的范围。</exception>
        public static double Log(BigInteger value) {
            return Log(value, 2.7182818284590451);
        }

		/// <summary>
		/// 返回指定数字在使用指定底时的对数。
		/// </summary>
		/// <param name="value">要查找其对数的数字。</param>
		/// <param name="baseValue">对数的底。</param>
		/// <returns>value 的以 baseValue 为底的对数，如“备注”部分中的表所示。</returns>
		/// <exception cref="ArgumentOutOfRangeException">value 的对数超出了 Double 数据类型的范围。</exception>
        public static double Log(BigInteger value, double baseValue) {
            if ((value._sign < 0) || (baseValue == 1.0)) {
                return double.NaN;
            }
            if (baseValue == double.PositiveInfinity) {
                if (!IsOne(value)) {
                    return double.NaN;
                }
                return 0.0;
            }
            if ((baseValue == 0.0) && !IsOne(value)) {
                return double.NaN;
            }
            if (value._bits == null) {
                double d = value._sign;
                if (double.IsNaN(d)) {
                    return d;
                }
                if (double.IsNaN(baseValue)) {
                    return baseValue;
                }
                if ((baseValue == 1.0) || ((d != 1.0) && ((baseValue == 0.0) || double.IsPositiveInfinity(baseValue)))) {
                    return double.NaN;
                }
                return (Math.Log(d) / Math.Log(baseValue));
            }
            double a = 0.0;
            double num3 = 0.5;
            int num4 = Length(value._bits);
            int num5 = BitLengthOfUInt(value._bits[num4 - 1]);
            int num6 = ((num4 - 1) * Buint) + num5;
            uint num7 = ((uint)1) << (num5 - 1);
            for (int i = num4 - 1; i >= 0; i--) {
                while (num7 != 0) {
                    if ((value._bits[i] & num7) != 0) {
                        a += num3;
                    }
                    num3 *= 0.5;
                    num7 = num7 >> 1;
                }
                num7 = UmaskHighBit;
            }
            return ((Math.Log(a) + (0.69314718055994529 * num6)) / Math.Log(baseValue));
        }

		/// <summary>
		/// 返回指定数字的自然对数（底为 10）。
		/// </summary>
		/// <param name="value">要查找其对数的数字。</param>
		/// <returns>value 的自然对数（底为 10），如“备注”部分中的表所示。</returns>
		/// <exception cref="ArgumentOutOfRangeException">value 的对数超出了 Double 数据类型的范围。</exception>
        public static double Log10(BigInteger value) {
            return Log(value, 10.0);
        }

		/// <summary>
		/// 查找两个 BigInteger 值的最大公约数
		/// </summary>
		/// <param name="left">第一个值。</param>
		/// <param name="right">第二个值。</param>
		/// <returns>left 和 right 的最大公约数。</returns>
        public static BigInteger GreatestCommonDivisor(BigInteger left, BigInteger right) {
            if (left._sign == 0) {
                return Abs(right);
            }
            if (right._sign == 0) {
                return Abs(left);
            }
            BigIntegerBuilder builder = new BigIntegerBuilder(left);
            BigIntegerBuilder builder2 = new BigIntegerBuilder(right);
            BigIntegerBuilder.GCD(ref builder, ref builder2);
            return builder.GetInteger(1);
        }

		/// <summary>
		/// 返回两个 BigInteger 值中的较大者。
		/// </summary>
		/// <param name="left">要比较的第一个值。</param>
		/// <param name="right">要比较的第二个值。</param>
		/// <returns>left 或 right 参数中较大的一个。</returns>
        public static BigInteger Max(BigInteger left, BigInteger right) {
            if (left.CompareTo(right) < 0) {
                return right;
            }
            return left;
        }

		/// <summary>
		/// 返回两个 BigInteger 值中的较小者。
		/// </summary>
		/// <param name="left">要比较的第一个值。</param>
		/// <param name="right">要比较的第二个值。</param>
		/// <returns>left 或 right 参数中较小的一个。</returns>
        public static BigInteger Min(BigInteger left, BigInteger right) {
            if (left.CompareTo(right) <= 0) {
                return left;
            }
            return right;
        }

        private static void ModPowUpdateResult(ref BigIntegerBuilder regRes, ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp) {
            Py.Core.Util.Swap<BigIntegerBuilder>(ref regRes, ref regTmp);
            regRes.Mul(ref regTmp, ref regVal);
            regRes.Mod(ref regMod);
        }

        private static void ModPowSquareModValue(ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp) {
            Py.Core.Util.Swap<BigIntegerBuilder>(ref regVal, ref regTmp);
            regVal.Mul(ref regTmp, ref regTmp);
            regVal.Mod(ref regMod);
        }

        private static void ModPowInner(uint exp, ref BigIntegerBuilder regRes, ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp) {
            while (exp != 0) {
                if ((exp & 1) == 1) {
                    ModPowUpdateResult(ref regRes, ref regVal, ref regMod, ref regTmp);
                }
                if (exp == 1) {
                    return;
                }
                ModPowSquareModValue(ref regVal, ref regMod, ref regTmp);
                exp = exp >> 1;
            }
        }

        private static void ModPowInner32(uint exp, ref BigIntegerBuilder regRes, ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp) {
            for (int i = 0; i < Buint; i++) {
                if ((exp & 1) == 1) {
                    ModPowUpdateResult(ref regRes, ref regVal, ref regMod, ref regTmp);
                }
                ModPowSquareModValue(ref regVal, ref regMod, ref regTmp);
                exp = exp >> 1;
            }
        }

		/// <summary>
		/// 对以某个数为底、以另一个数为指数的幂执行模数除法。
		/// </summary>
		/// <param name="value">要计算 exponent 次幂的数字。</param>
		/// <param name="exponent">对 value 进行幂运算的指数。</param>
		/// <param name="modulus">将 value指数除以的值。</param>
		/// <returns>将 value指数除以 modulus 后的余数。</returns>
		/// <exception cref="DivideByZeroException">modulus 是零。</exception>
		/// <exception cref="ArgumentOutOfRangeException">exponent 为负数。</exception>
        public static BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus) {
            Thrower.ThrowArgumentOutOfRangeExceptionIf(exponent.Sign < 0, Properties.Messages.BigIntegerBig);
            int sign = 1;
            int num2 = 1;
            int num3 = 1;
            bool isEven = IsEven(exponent);
            BigIntegerBuilder regRes = new BigIntegerBuilder(One, ref sign);
            BigIntegerBuilder regVal = new BigIntegerBuilder(value, ref num2);
            BigIntegerBuilder regDen = new BigIntegerBuilder(modulus, ref num3);
            BigIntegerBuilder regTmp = new BigIntegerBuilder(regVal.Size);
            regRes.Mod(ref regDen);
            if (exponent._bits == null) {
                ModPowInner((uint)exponent._sign, ref regRes, ref regVal, ref regDen, ref regTmp);
            } else {
                int num4 = Length(exponent._bits);
                for (int i = 0; i < (num4 - 1); i++) {
                    uint exp = exponent._bits[i];
                    ModPowInner32(exp, ref regRes, ref regVal, ref regDen, ref regTmp);
                }
                ModPowInner(exponent._bits[num4 - 1], ref regRes, ref regVal, ref regDen, ref regTmp);
            }
            return regRes.GetInteger((value._sign > 0) ? 1 : (isEven ? 1 : -1));
        }

		/// <summary>
		/// 求以 BigInteger 值为底、以指定的值为指数的幂。
		/// </summary>
		/// <param name="value">要计算 exponent 次幂的数字</param>
		/// <param name="exponent">value 的 exponent 次幂的计算结果。</param>
		/// <returns>value 的 exponent 次幂的计算结果。</returns>
		/// <exception cref="ArgumentOutOfRangeException">exponent 参数的值为负。</exception>
        public static BigInteger Pow(BigInteger value, int exponent) {
            Thrower.ThrowArgumentOutOfRangeExceptionIf(value.Sign < 0, Properties.Messages.BigIntegerBig);
            if (exponent == 0) {
                return One;
            }
            if (exponent == 1) {
                return value;
            }
            if (value._bits == null) {
                if (value._sign == 1) {
                    return value;
                }
                if (value._sign == -1) {
                    if ((exponent & 1) == 0) {
                        return 1;
                    }
                    return value;
                }
                if (value._sign == 0) {
                    return value;
                }
            }
            int sign = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(value, ref sign);
            int size = builder.Size;
            int cuMul = size;
            uint high = builder.High;
            uint uHiMul = high + 1;
            if (uHiMul == 0) {
                cuMul++;
                uHiMul = 1;
            }
            int cuRes = 1;
            int num7 = 1;
            uint uHiRes = 1;
            uint num9 = 1;
            int num10 = exponent;
        Label_009A:
            if ((num10 & 1) != 0) {
                MulUpper(ref num9, ref num7, uHiMul, cuMul);
                MulLower(ref uHiRes, ref cuRes, high, size);
            }
            num10 = num10 >> 1;
            if (num10 != 0) {
                MulUpper(ref uHiMul, ref cuMul, uHiMul, cuMul);
                MulLower(ref high, ref size, high, size);
                goto Label_009A;
            }
            if (num7 > 1) {
                builder.EnsureWritable(num7, 0);
            }
            BigIntegerBuilder b = new BigIntegerBuilder(num7);
            BigIntegerBuilder a = new BigIntegerBuilder(num7);
            a.Set((uint)1);
            if ((exponent & 1) == 0) {
                sign = 1;
            }
            int num11 = exponent;
        Label_0119:
            if ((num11 & 1) != 0) {
                Py.Core.Util.Swap<BigIntegerBuilder>(ref a, ref b);
                a.Mul(ref builder, ref b);
            }
            num11 = num11 >> 1;
            if (num11 != 0) {
                Py.Core.Util.Swap<BigIntegerBuilder>(ref builder, ref b);
                builder.Mul(ref b, ref b);
                goto Label_0119;
            }
            return a.GetInteger(sign);
        }

		/// <summary>
		/// 实现从 <see cref="System.Byte"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator BigInteger(byte value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.SByte"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(sbyte value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.Int16"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator BigInteger(short value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.UInt16"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(ushort value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.Int32"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator BigInteger(int value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.UInt32"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(uint value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.Int64"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator BigInteger(long value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.UInt64"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(ulong value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.Single"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator BigInteger(float value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.Double"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator BigInteger(double value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.Decimal"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator BigInteger(decimal value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// 实现从 <see cref="System.String"/> 到 <see cref="Py.Algorithm.Numerics.BigInteger"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(string value) {
            return Parse(value);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Byte"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator byte(BigInteger value) {
            return (byte)((int)value);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.SByte"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static explicit operator sbyte(BigInteger value) {
            return (sbyte)((int)value);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Int16"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator short(BigInteger value) {
            return (short)((int)value);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.UInt16"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static explicit operator ushort(BigInteger value) {
            return (ushort)((int)value);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Int32"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
		/// <exception cref="OverflowException">值太大或者太小。</exception>
        public static explicit operator int(BigInteger value) {
            if (value._bits == null) {
                return value._sign;
            }
			Thrower.ThrowOverflowExceptionIf(Length(value._bits) > 1, Properties.Messages.BigIntegerRange);
            if (value._sign > 0) {
                return (int)value._bits[0];
            }
            Thrower.ThrowOverflowExceptionIf(value._bits[0] > UmaskHighBit, Properties.Messages.BigIntegerRange);
            return (int)-(value._bits[0]);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.UInt32"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
		/// <exception cref="OverflowException">值太大或者太小。</exception>
        [CLSCompliant(false)]
        public static explicit operator uint(BigInteger value) {
            if (value._bits == null) {
                return (uint)value._sign;
            }
            Thrower.ThrowOverflowExceptionIf(Length(value._bits) > 1 || value._sign < 0, Properties.Messages.BigIntegerRange);
            return value._bits[0];
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Int64"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
		/// <exception cref="OverflowException">值太大或者太小。</exception>
        public static explicit operator long(BigInteger value) {
            ulong num;
            if (value._bits == null) {
                return (long)value._sign;
            }
            int num2 = Length(value._bits);
            Thrower.ThrowOverflowExceptionIf(num2 > 2, Properties.Messages.BigIntegerRange);
            if (num2 > 1) {
                num = NumericsHelpers.MakeUlong(value._bits[1], value._bits[0]);
            } else {
                num = value._bits[0];
            }
            long num3 = (value._sign > 0) ? ((long)num) : (-(long)num);
            Thrower.ThrowOverflowExceptionIf(((num3 <= 0L) || (value._sign <= 0)) && ((num3 >= 0L) || (value._sign >= 0)), Properties.Messages.BigIntegerRange);
            return num3;
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.UInt64"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
		/// <exception cref="OverflowException">值太大或者太小。</exception>
        [CLSCompliant(false)]
        public static explicit operator ulong(BigInteger value) {
            if (value._bits == null) {
                return (ulong)value._sign;
            }
            int num = Length(value._bits);
            Thrower.ThrowOverflowExceptionIf((num > 2) || (value._sign < 0), Properties.Messages.BigIntegerRange);
            if (num > 1) {
                return NumericsHelpers.MakeUlong(value._bits[1], value._bits[0]);
            }
            return (ulong)value._bits[0];
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Single"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator float(BigInteger value) {
            return (float)((double)value);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Double"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator double(BigInteger value) {
            ulong num;
            int num2;
            if (value._bits == null) {
                return (double)value._sign;
            }
            int sign = 1;
            new BigIntegerBuilder(value, ref sign).GetApproxParts(out num2, out num);
            return NumericsHelpers.GetDoubleFromParts(sign, num2, num);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.Decimal"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
		/// <exception cref="OverflowException">值太大或者太小。</exception>
        public static explicit operator decimal(BigInteger value) {
            if (value._bits == null) {
                return value._sign;
            }
            int num = Length(value._bits);
            Thrower.ThrowOverflowExceptionIf(num > 3, Properties.Messages.BigIntegerRange);
            int lo = 0;
            int mid = 0;
            int hi = 0;
            if (num > 2) {
                hi = (int)value._bits[2];
            }
            if (num > 1) {
                mid = (int)value._bits[1];
            }
            if (num > 0) {
                lo = (int)value._bits[0];
            }
            return new decimal(lo, mid, hi, value._sign < 0, 0);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="System.String"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator string(BigInteger value) {
            return value.ToString();
        }

		/// <summary>
		/// 实现操作 &amp; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator &(BigInteger left, BigInteger right) {
            if (IsZero(left) || IsZero(right)) {
                return Zero;
            }
            uint[] numArray = left.ToUInt32Array();
            uint[] numArray2 = right.ToUInt32Array();
            uint[] numArray3 = new uint[Math.Max(numArray.Length, numArray2.Length)];
            uint num = (left._sign < 0) ? uint.MaxValue : 0;
            uint num2 = (right._sign < 0) ? uint.MaxValue : 0;
            for (int i = 0; i < numArray3.Length; i++) {
                uint num4 = (i < numArray.Length) ? numArray[i] : num;
                uint num5 = (i < numArray2.Length) ? numArray2[i] : num2;
                numArray3[i] = num4 & num5;
            }
            return new BigInteger(numArray3);
        }

		/// <summary>
		/// 实现操作 | 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator |(BigInteger left, BigInteger right) {
            if (IsZero(left)) {
                return right;
            }
            if (IsZero(right)) {
                return left;
            }
            uint[] numArray = left.ToUInt32Array();
            uint[] numArray2 = right.ToUInt32Array();
            uint[] numArray3 = new uint[Math.Max(numArray.Length, numArray2.Length)];
            uint num = (left._sign < 0) ? uint.MaxValue : 0;
            uint num2 = (right._sign < 0) ? uint.MaxValue : 0;
            for (int i = 0; i < numArray3.Length; i++) {
                uint num4 = (i < numArray.Length) ? numArray[i] : num;
                uint num5 = (i < numArray2.Length) ? numArray2[i] : num2;
                numArray3[i] = num4 | num5;
            }
            return new BigInteger(numArray3);
        }

		/// <summary>
		/// 实现操作 ^ 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator ^(BigInteger left, BigInteger right) {
            uint[] numArray = left.ToUInt32Array();
            uint[] numArray2 = right.ToUInt32Array();
            uint[] numArray3 = new uint[Math.Max(numArray.Length, numArray2.Length)];
            uint num = (left._sign < 0) ? uint.MaxValue : 0;
            uint num2 = (right._sign < 0) ? uint.MaxValue : 0;
            for (int i = 0; i < numArray3.Length; i++) {
                uint num4 = (i < numArray.Length) ? numArray[i] : num;
                uint num5 = (i < numArray2.Length) ? numArray2[i] : num2;
                numArray3[i] = num4 ^ num5;
            }
            return new BigInteger(numArray3);
        }

		/// <summary>
		/// 实现操作 &lt;&lt; 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <param name="shift">The shift。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator <<(BigInteger value, int shift) {
            uint[] numArray;
            int num;
            if (shift == 0) {
                return value;
            }
            if (shift == -2147483648) {
                return value >> 0x7fffffff >> 1;
            }
            if (shift < 0) {
                return value >> -shift;
            }
            int num2 = shift / Buint;
            int num3 = shift - (num2 * Buint);
            bool negative = GetPartsForBitManipulation(ref value, out numArray, out num);
            int num4 = (num + num2) + 1;
            uint[] numArray2 = new uint[num4];
            if (num3 == 0) {
                for (int i = 0; i < num; i++) {
                    numArray2[i + num2] = numArray[i];
                }
            } else {
                int num6 = Buint - num3;
                uint num7 = 0;
                int index = 0;
                while (index < num) {
                    uint num9 = numArray[index];
                    numArray2[index + num2] = (num9 << num3) | num7;
                    num7 = num9 >> num6;
                    index++;
                }
                numArray2[index + num2] = num7;
            }
            return new BigInteger(numArray2, negative);
        }

		/// <summary>
		/// 实现操作 &gt;&gt; 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <param name="shift">The shift。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator >>(BigInteger value, int shift) {
            uint[] numArray;
            int num;
            if (shift == 0) {
                return value;
            }
            if (shift == -2147483648) {
                return value << 0x7fffffff << 1;
            }
            if (shift < 0) {
                return value << -shift;
            }
            int num2 = shift / Buint;
            int num3 = shift - (num2 * Buint);
            bool negative = GetPartsForBitManipulation(ref value, out numArray, out num);
            if (negative) {
                if (shift >= (Buint * num)) {
                    return MinusOne;
                }
                uint[] destinationArray = new uint[num];
                Array.Copy(numArray, 0, destinationArray, 0, num);
                numArray = destinationArray;
                NumericsHelpers.DangerousMakeTwosComplement(numArray);
            }
            int num4 = num - num2;
            if (num4 < 0) {
                num4 = 0;
            }
            uint[] d = new uint[num4];
            if (num3 == 0) {
                for (int i = num - 1; i >= num2; i--) {
                    d[i - num2] = numArray[i];
                }
            } else {
                int num6 = Buint - num3;
                uint num7 = 0;
                for (int j = num - 1; j >= num2; j--) {
                    uint num9 = numArray[j];
                    if (negative && (j == (num - 1))) {
                        d[j - num2] = (num9 >> num3) | ((unchecked((uint)(-1))) << num6);
                    } else {
                        d[j - num2] = (num9 >> num3) | num7;
                    }
                    num7 = num9 << num6;
                }
            }
            if (negative) {
                NumericsHelpers.DangerousMakeTwosComplement(d);
            }
            return new BigInteger(d, negative);
        }

		/// <summary>
		/// 实现操作 ~ 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator ~(BigInteger value) {
            return -(value + One);
        }

		/// <summary>
		/// 实现操作 - 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator -(BigInteger value) {
            value._sign = -value._sign;
            return value;
        }

		/// <summary>
		/// 实现操作 + 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator +(BigInteger value) {
            return value;
        }

		/// <summary>
		/// 实现操作 ++ 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator ++(BigInteger value) {
            return (value + One);
        }

		/// <summary>
		/// 实现操作 -- 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator --(BigInteger value) {
            return (value - One);
        }

		/// <summary>
		/// 实现操作 + 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator +(BigInteger left, BigInteger right) {
            if (IsZero(right)) {
                return left;
            }
            if (IsZero(left)) {
                return right;
            }
            int sign = 1;
            int num2 = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(left, ref sign), reg = new BigIntegerBuilder(right, ref num2);
            if (sign == num2) {
                builder.Add(ref reg);
            } else {
                builder.Sub(ref sign, ref reg);
            }
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// 实现操作 - 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator -(BigInteger left, BigInteger right) {
            if (IsZero(right)) {
                return left;
            }
            if (IsZero(left)) {
                return -(right);
            }
            int sign = 1;
            int num2 = -1;
            BigIntegerBuilder builder = new BigIntegerBuilder(left, ref sign);
            BigIntegerBuilder reg = new BigIntegerBuilder(right, ref num2);
            if (sign == num2) {
                builder.Add(ref reg);
            } else {
                builder.Sub(ref sign, ref reg);
            }
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// 实现操作 * 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator *(BigInteger left, BigInteger right) {
            int sign = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(left, ref sign);
            BigIntegerBuilder regMul = new BigIntegerBuilder(right, ref sign);
            builder.Mul(ref regMul);
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// 实现操作 / 。
		/// </summary>
		/// <param name="dividend">结束位置。</param>
		/// <param name="divisor">The divisor。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator /(BigInteger dividend, BigInteger divisor) {
            int sign = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(dividend, ref sign);
            BigIntegerBuilder regDen = new BigIntegerBuilder(divisor, ref sign);
            builder.Div(ref regDen);
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// 实现操作 % 。
		/// </summary>
		/// <param name="dividend">结束位置。</param>
		/// <param name="divisor">The divisor。</param>
		/// <returns>操作的结果。</returns>
        public static BigInteger operator %(BigInteger dividend, BigInteger divisor) {
            int sign = 1;
            int num2 = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(dividend, ref sign);
            BigIntegerBuilder regDen = new BigIntegerBuilder(divisor, ref num2);
            builder.Mod(ref regDen);
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// 实现操作 &lt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator <(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) < 0);
        }

		/// <summary>
		/// 实现操作 &lt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator <=(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) <= 0);
        }

		/// <summary>
		/// 实现操作 &gt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator >(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) > 0);
        }

		/// <summary>
		/// 实现操作 &gt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator >=(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) >= 0);
        }

		/// <summary>
		/// 实现操作 == 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator ==(BigInteger left, BigInteger right) {
            return left.Equals(right);
        }

		/// <summary>
		/// 实现操作 != 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator !=(BigInteger left, BigInteger right) {
            return !left.Equals(right);
        }

		/// <summary>
		/// 实现操作 &lt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator <(BigInteger left, long right) {
            return (left.CompareTo(right) < 0);
        }

		/// <summary>
		/// 实现操作 &lt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator <=(BigInteger left, long right) {
            return (left.CompareTo(right) <= 0);
        }

		/// <summary>
		/// 实现操作 &gt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator >(BigInteger left, long right) {
            return (left.CompareTo(right) > 0);
        }

		/// <summary>
		/// 实现操作 &gt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator >=(BigInteger left, long right) {
            return (left.CompareTo(right) >= 0);
        }

		/// <summary>
		/// 实现操作 == 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator ==(BigInteger left, long right) {
            return left.Equals(right);
        }

		/// <summary>
		/// 实现操作 != 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator !=(BigInteger left, long right) {
            return !left.Equals(right);
        }

		/// <summary>
		/// 实现操作 &lt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator <(long left, BigInteger right) {
            return (right.CompareTo(left) > 0);
        }

		/// <summary>
		/// 实现操作 &lt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator <=(long left, BigInteger right) {
            return (right.CompareTo(left) >= 0);
        }

		/// <summary>
		/// 实现操作 &gt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator >(long left, BigInteger right) {
            return (right.CompareTo(left) < 0);
        }

		/// <summary>
		/// 实现操作 &gt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator >=(long left, BigInteger right) {
            return (right.CompareTo(left) <= 0);
        }

		/// <summary>
		/// 实现操作 == 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator ==(long left, BigInteger right) {
            return right.Equals(left);
        }

		/// <summary>
		/// 实现操作 != 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator !=(long left, BigInteger right) {
            return !right.Equals(left);
        }

		/// <summary>
		/// 实现操作 &lt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator <(BigInteger left, ulong right) {
            return (left.CompareTo(right) < 0);
        }

		/// <summary>
		/// 实现操作 &lt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator <=(BigInteger left, ulong right) {
            return (left.CompareTo(right) <= 0);
        }

		/// <summary>
		/// 实现操作 &gt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator >(BigInteger left, ulong right) {
            return (left.CompareTo(right) > 0);
        }

		/// <summary>
		/// 实现操作 &gt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator >=(BigInteger left, ulong right) {
            return (left.CompareTo(right) >= 0);
        }

		/// <summary>
		/// 实现操作 == 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator ==(BigInteger left, ulong right) {
            return left.Equals(right);
        }

		/// <summary>
		/// 实现操作 != 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator !=(BigInteger left, ulong right) {
            return !left.Equals(right);
        }

		/// <summary>
		/// 实现操作 &lt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator <(ulong left, BigInteger right) {
            return (right.CompareTo(left) > 0);
        }

		/// <summary>
		/// 实现操作 &lt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator <=(ulong left, BigInteger right) {
            return (right.CompareTo(left) >= 0);
        }

		/// <summary>
		/// 实现操作 &gt; 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator >(ulong left, BigInteger right) {
            return (right.CompareTo(left) < 0);
        }

		/// <summary>
		/// 实现操作 &gt;= 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator >=(ulong left, BigInteger right) {
            return (right.CompareTo(left) <= 0);
        }

		/// <summary>
		/// 实现操作 == 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator ==(ulong left, BigInteger right) {
            return right.Equals(left);
        }

		/// <summary>
		/// 实现操作 != 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        [CLSCompliant(false)]
        public static bool operator !=(ulong left, BigInteger right) {
            return !right.Equals(left);
        }

        private void SetBitsFromDouble(double value) {
            int num;
            int num2;
            ulong num3;
            bool flag;
            NumericsHelpers.GetDoubleParts(value, out num, out num2, out num3, out flag);
            if (num3 == 0L) {
                this = Zero;
            } else if (num2 <= 0) {
                if (num2 <= -64) {
                    this = Zero;
                } else {
                    this = num3 >> -num2;
                    if (num < 0) {
                        _sign = -_sign;
                    }
                }
            } else if (num2 <= 11) {
                this = num3 << num2;
                if (num < 0) {
                    _sign = -_sign;
                }
            } else {
                num3 = num3 << 11;
                num2 -= 11;
                int index = ((num2 - 1) / Buint) + 1;
                int num5 = (index * Buint) - num2;
                _bits = new uint[index + 2];
                _bits[index + 1] = (uint)(num3 >> (num5 + Buint));
                _bits[index] = (uint)(num3 >> num5);
                if (num5 > 0) {
                    _bits[index - 1] = ((uint)num3) << (Buint - num5);
                }
                _sign = num;
            }
        }

        internal static int Length(uint[] rgu) {
            int length = rgu.Length;
            if (rgu[length - 1] != 0) {
                return length;
            }
            return (length - 1);
        }

        internal int _Sign {
            get {
                return _sign;
            }
        }
        internal uint[] _Bits {
            get {
                return _bits;
            }
        }
        internal static int BitLengthOfUInt(uint x) {
            int num = 0;
            while (x > 0) {
                x = x >> 1;
                num++;
            }
            return num;
        }

        private static bool GetPartsForBitManipulation(ref BigInteger x, out uint[] xd, out int xl) {
            if (x._bits == null) {
                if (x._sign < 0) {
                    xd = new uint[] { (uint)-x._sign };
                } else {
                    xd = new uint[] { (uint)x._sign };
                }
            } else {
                xd = x._bits;
            }
            xl = (x._bits == null) ? 1 : x._bits.Length;
            return (x._sign < 0);
        }

        private static void MulUpper(ref uint uHiRes, ref int cuRes, uint uHiMul, int cuMul) {
            ulong uu = ((ulong)uHiRes) * uHiMul;
            uint hi = NumericsHelpers.GetHi(uu);
            if (hi != 0) {
                if (((uint)uu != 0) && (++hi == 0)) {
                    hi = 1;
                    cuRes++;
                }
                uHiRes = hi;
                cuRes += cuMul;
            } else {
                uHiRes = (uint)uu;
                cuRes += cuMul - 1;
            }
        }

        private static void MulLower(ref uint uHiRes, ref int cuRes, uint uHiMul, int cuMul) {
            ulong uu = ((ulong)uHiRes) * uHiMul;
            uint hi = NumericsHelpers.GetHi(uu);
            if (hi != 0) {
                uHiRes = hi;
                cuRes += cuMul;
            } else {
                uHiRes = (uint)uu;
                cuRes += cuMul - 1;
            }
        }

        internal static int GetDiffLength(uint[] rgu1, uint[] rgu2, int cu) {
            int index = cu;
            while (--index >= 0) {
                if (rgu1[index] != rgu2[index]) {
                    return (index + 1);
                }
            }
            return 0;
        }
    }
}

