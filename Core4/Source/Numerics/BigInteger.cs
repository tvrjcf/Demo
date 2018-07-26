
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Py.RunTime;

namespace Py.Algorithm.Numerics {
    
	/// <summary>
	/// ��ʾ�����Ĵ�����������
	/// </summary>
	/// <remarks>
	/// �� <see href="http://msdn.microsoft.com/zh-cn/library/system.numerics.biginteger.aspx"/> ��
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
        /// ��ȡһ����ʾ���� 0���㣩��ֵ��
        /// </summary>
        /// <value>��ֵΪ 0���㣩��������</value>
        /// <remarks>
        /// �ɴ����Է��ص� BigInteger �����ṩһ���������ֵԴ�����ڸ�ֵ�ͱȽ���ʹ�á�
        /// </remarks>
        public static BigInteger Zero {
            get {
                return _zeroInt;
            }
        }

        /// <summary>
        /// ��ȡһ����ʾ����һ (1) ��ֵ��
        /// </summary>
        /// <value>��ֵΪһ (1) �Ķ���</value>
        /// <remarks>
        /// One ����ͨ�����ڽ� BigInteger ֵ�� 1 �Ƚ��Լ��� 1 ��ֵ�� BigInteger ����
        /// </remarks>
        public static BigInteger One {
            get {
                return _oneInt;
            }
        }

        /// <summary>
        /// ��ȡһ����ʾ���ָ�һ (-1) ��ֵ��
        /// </summary>
        /// <value>��ֵΪ��һ (-1) ��������</value>
        /// <remarks>
        /// MinusOne �������ڽ� BigInteger ֵ�� -1 �Ƚ��Լ��� -1 ��ֵ�� BigInteger ����
        /// </remarks>
        public static BigInteger MinusOne {
            get {
                return _minusOneInt;
            }
        }

        /// <summary>
        /// ָʾ��ǰ BigInteger �����ֵ�Ƿ��� 2 ���ݡ�
        /// </summary>
        /// <value>��� BigInteger �����ֵ�� 2 ���ݣ���Ϊ true������Ϊ false��</value>
        /// <remarks>
        /// ������ȷ�� BigInteger ֵ�Ƿ����һ������λ��������ζ�������ǰ BigInteger �����ֵΪ 1���� 20���� 2 ������ϴ��ݣ��������� true�������ǰ BigInteger �����ֵΪ 0���򷵻� false��
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
        /// ָʾ��ǰ BigInteger �����ֵ�Ƿ��� BigInteger.Zero��
        /// </summary>
        /// <value>BigInteger �����ֵ�� BigInteger.Zero����Ϊ true������Ϊ false��</value>
        /// <remarks>
        /// �������ṩ�������������� BigInteger.Equals(BigInteger.Zero)��
        /// </remarks>
        public static bool IsZero(BigInteger value) {
            return (value._sign == 0);
        }
        
        /// <summary>
        /// ָʾ��ǰ BigInteger �����ֵ�Ƿ��� BigInteger.One��
        /// </summary>
        /// <value>��� BigInteger �����ֵ�� BigInteger.One����Ϊ true������Ϊ false��</value>
        /// <remarks>
        /// �������ṩ�������������������ȽϷ��������� thisBigInteger.Equals(BigInteger.One)��
        /// </remarks>
        public static bool IsOne(BigInteger value) {
            return ((value._sign == 1) && (value._bits == null));
        }

        /// <summary>
        /// ָʾ��ǰ BigInteger �����ֵ�Ƿ���ż����
        /// </summary>
        /// <value>��� BigInteger �����ֵ��ż������Ϊ true������Ϊ false��</value>
        /// <remarks>
        /// ��������һ���������ܣ�ָʾ BigInteger ֵ�Ƿ��ܱ� 2 ����������Ч�����±��ʽ��[c#]value % 2 == 0;
        /// </remarks>
        public static bool IsEven(BigInteger value) {
            if (value._bits != null) {
                return ((value._bits[0] & 1) == 0);
            }
            return ((value._sign & 1) == 0);
        }

        /// <summary>
        /// ��ȡһ�����֣�������ָʾ��ǰ BigInteger ����ķ��ţ����������㣩��
        /// </summary>
        /// <value>һ��ָʾ BigInteger ����ķ��ŵ����֣����±���ʾ��</value>
        /// <remarks>
        /// Sign ���������ڻ�Ԫ��ֵ���͵� Math.Sign ������Ч��
        /// </remarks>
        public int Sign {
            get {
                return ((_sign >> 0x1f) - (-_sign >> 0x1f));
            }
        }

		/// <summary>
		/// ָʾ��ʵ����ָ�������Ƿ���ȡ�
		/// </summary>
		/// <param name="obj">Ҫ�Ƚϵ���һ������</param>
		/// <returns>
		/// ��� <paramref name="obj"/> �͸�ʵ��������ͬ�����Ͳ���ʾ��ͬ��ֵ����Ϊ true������Ϊ false��
		/// </returns>
        public override bool Equals(object obj) {
            return ((obj is BigInteger) && Equals((BigInteger)obj));
        }

		/// <summary>
		/// ���ش�ʵ���Ĺ�ϣ���롣
		/// </summary>
		/// <returns>һ�� 32 λ�з������������Ǹ�ʵ���Ĺ�ϣ���롣</returns>
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
        /// ����һ��ֵ����ֵָʾ��ǰʵ���� 64 λ�����������Ƿ������ͬ��ֵ��
        /// </summary>
        /// <param name="other">Ҫ�Ƚϵ� 64 λ����������ֵ��</param>
        /// <returns>��� 64 λ�����������뵱ǰʵ��������ͬ��ֵ����Ϊ true������Ϊ false��</returns>
        /// <remarks>
        /// ��� other �� Byte��Int16��Int32��SByte��UInt16 �� UInt32 ֵ�����ڵ��÷���ʱ����������ʽת��Ϊ Int64 ֵ����Ҫȷ����������֮��Ĺ�ϵ�������ǲ���������ԣ������ BigInteger.CompareTo(Int64) ������
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
		/// ����ʵ���� 64 λ�޷����������бȽϣ�������һ��������������ָʾ��ʵ����ֵ��С�ڡ����ڻ��Ǵ��� 64 λ�޷���������ֵ��
		/// </summary>
		/// <param name="other">Ҫ�Ƚϵ� 64 λ�޷���������</param>
		/// <returns>һ��������������ָʾ��ʵ���� other �����ֵ������ֵС����˵����ǰʵ��С�� other������ֵ��˵����ǰʵ������ other������ֵ������˵����ǰʵ������ other��</returns>
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
        /// ����һ��ֵ����ֵָʾ��ǰʵ����ָ���� BigInteger �����Ƿ������ͬ��ֵ��
        /// </summary>
        /// <param name="other">Ҫ�ȽϵĶ���</param>
        /// <returns>����� BigInteger ������ other ������ͬ��ֵ����Ϊ true������Ϊ false��</returns>
        /// <remarks>
        /// �˷���ʵ�� IEquatable&lt;T&gt; �ӿڣ�����ִ��Ч���Ժ��� Equals(Object)��������Ϊ�����ؽ� other ����ת��Ϊ BigInteger ������Ҫȷ������ BigInteger ����֮��Ĺ�ϵ�������ǲ���������ԣ������ BigInteger.CompareTo(BigInteger) ������
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
        /// ����ʵ������һ�� BigInteger ���бȽϣ�������һ��������������ָʾ��ʵ����ֵ��С�ڡ����ڻ��Ǵ���ָ�������ֵ��
        /// </summary>
        /// <param>Ҫ�ȽϵĶ���</param>
        /// <returns>һ������������ֵ��ָʾ��ʵ���� other �Ĺ�ϵ�����±���ʾ������ֵС����˵����ǰʵ��С�� other������ֵ��˵����ǰʵ������ other������ֵ������˵����ǰʵ������ other��</returns>
        /// <remarks>
        /// CompareTo �����Ĵ����ؽ�ʵ�� IComparable.CompareTo �������������ͼ��϶���ʹ�����������е��
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
        /// ����ʵ���� 64 λ�������������бȽϣ�������һ��������������ָʾ��ʵ����ֵ��С�ڡ����ڻ��Ǵ��� 64 λ������������ֵ��
        /// </summary>
        /// <param name="other">Ҫ�Ƚϵ� 64 λ����������������ֵ</param>
        /// <returns>
        /// һ������������ֵ��ָʾ��ʵ���� other �Ĺ�ϵ������ֵС����˵����ǰʵ��С�� other������ֵ��˵����ǰʵ������ other������ֵ������˵����ǰʵ������ other��
        /// </returns>
        /// <remarks>
        /// ��� other �� Byte��Int16��Int32��SByte��UInt16 �� UInt32 ֵ�����ڵ��� CompareTo(Int64) ����ʱ����������ʽת��Ϊ Int64 ֵ��
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
        /// ����ʵ���� 64 λ�޷����������бȽϣ�������һ��������������ָʾ��ʵ����ֵ��С�ڡ����ڻ��Ǵ��� 64 λ�޷���������ֵ��
        /// </summary>
        /// <param name="other">Ҫ�Ƚϵ� 64 λ�޷���������</param>
        /// <returns>һ��������������ָʾ��ʵ���� other �����ֵ������ֵС����˵����ǰʵ��С�� other������ֵ��˵����ǰʵ������ other������ֵ������˵����ǰʵ������ other��</returns>
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
        /// ����ʵ������һ�� BigInteger ���бȽϣ�������һ��������������ָʾ��ʵ����ֵ��С�ڡ����ڻ��Ǵ���ָ�������ֵ��
        /// </summary>
        /// <param name="other">System.Numerics.BigIntegerҪ�ȽϵĶ���</param>
        /// <returns>һ������������ֵ��ָʾ��ʵ���� other �Ĺ�ϵ������ֵС����˵����ǰʵ��С�� other�� ����ֵ��˵����ǰʵ������ other������ֵ������˵����ǰʵ������ other��
		/// </returns>
        /// <remarks>
        /// CompareTo �����Ĵ����ؽ�ʵ�� IComparable&lt;T&gt;.CompareTo �������������ͼ��϶���ʹ�����������е��
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
        /// ����ʵ����ָ��������бȽϣ�������һ��������������ָʾ��ʵ����ֵ��С�ڡ����ڻ��Ǵ���ָ�������ֵ��
        /// </summary>
        /// <param name="obj">Ҫ�ȽϵĶ���</param>
        /// <returns>һ��������������ָʾ��ǰʵ���� obj �����Ĺ�ϵ�����±���ʾ������ֵС����˵����ǰʵ��С�� obj������ֵ��˵����ǰʵ������ obj�� ����ֵ������˵����ǰʵ������ obj������ obj ����Ϊ null��</returns>
		/// <exception cref="ArgumentException"><paramref name="obj"/> ���� BigInteger��</exception>
        /// <remarks>CompareTo �����Ĵ����ؽ�ʵ�� IComparable.CompareTo �����������Ƿ��ͼ��϶���ʹ�����������е��obj ��������������֮һ������ʱ����Ϊ BigInteger �Ķ���ֵΪ null �� Object ��������� obj ������ֵ�� null����÷������� 1����˵����ǰʵ������ obj��
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
		/// ����ǰ BigInteger �������ֵת��Ϊ���Ч���ַ�����ʾ��ʽ��
		/// </summary>
		/// <returns>
		/// ������ȫ�޶��������� <see cref="T:System.String"/>��
		/// </returns>
        public override string ToString() {
            return BigNumber.FormatBigInteger(this, null, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// ʹ��ָ�����������ض���ʽ������Ϣ����ǰ BigInteger �������ֵת��Ϊ���ĵ�Ч�ַ�����ʾ��ʽ��
        /// </summary>
        /// <param name="provider">һ���ṩ�������ض��ĸ�ʽ������Ϣ�Ķ���</param>
        /// <returns>��ǰ BigInteger ֵ���ַ�����ʾ��ʽ����ֵʹ�� provider ����ָ���ĸ�ʽ��</returns>
        /// <value>
        /// ���ص��ַ���ʹ��ͨ�ø�ʽ˵���� ("G") ���и�ʽ���á�ToString(IFormatProvider) ����֧�� 50 λʮ�������ֵľ��ȡ�Ҳ����˵����� BigInteger ֵ�е�����λ������ 50����ֻ�� 50 �������Чλ����������ַ����У������������ֶ����滻Ϊ�㡣provider ������ IFormatProvider ʵ�֡��� GetFormat ��������һ�� NumberFormatInfo ���󣬸ö����ṩ�йش˷������ص��ַ�����ʽ���������ض���Ϣ����� provider Ϊ null����ʹ�õ�ǰ�����Ե� NumberFormatInfo ����� BigInteger ֵ���и�ʽ���á�����ʹ��һ���ʽ˵������ BigInteger ֵ���ַ�����ʾ��ʽ�� NumberFormatInfo �����Ψһ������ NumberFormatInfo.NegativeSign���������ʾ���ŵ��ַ���provider ����������������֮һ���ṩ��ʽ������Ϣ�� NumberFormatInfo ����һ��ʵ�� IFormatProvider ���Զ�������� GetFormat ���������ṩ��ʽ������Ϣ�� NumberFormatInfo ����
        /// </value>
        public string ToString(IFormatProvider provider) {
            return BigNumber.FormatBigInteger(this, null, NumberFormatInfo.GetInstance(provider));
        }

        /// <summary>
        /// ʹ��ָ���ĸ�ʽ����ǰ BigInteger �������ֵת��Ϊ���ĵ�Ч�ַ�����ʾ��ʽ��
        /// </summary>
        /// <param name="format">��׼���Զ������ֵ��ʽ�ַ�����</param>
        /// <returns>
        /// ���ͣ�System.String��ǰ BigInteger ֵ���ַ�����ʾ��ʽ����ֵʹ�� format ����ָ���ĸ�ʽ��
        /// </returns>
        /// <exception>�쳣,FormatException format��������format ������Ч�ĸ�ʽ�ַ�����</exception>          
        public string ToString(string format) {
            return BigNumber.FormatBigInteger(this, format, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// ʹ��ָ���ĸ�ʽ���������ض���ʽ��Ϣ����ǰ BigInteger �������ֵת��Ϊ���ĵ�Ч�ַ�����ʾ��ʽ��
        /// </summary>
        /// <param name="format">��׼���Զ������ֵ��ʽ�ַ�����</param>
        /// <param name="provider">һ���ṩ�������ض��ĸ�ʽ������Ϣ�Ķ���</param>
        /// <returns>�� format �� provider ����ָ���ĵ�ǰ BigInteger ֵ���ַ�����ʾ��ʽ��</returns>
		/// <exception cref="FormatException"><paramref name="format"/> ������Ч�ĸ�ʽ�ַ�����</exception>
        /// <remarks>
        /// format �����������κ���Ч��׼��ֵ��ʽ˵�������������Զ�����ֵ��ʽ˵�������κ���ϡ���� format ���� String.Empty ����Ϊ null����ǰ BigInteger ����ķ���ֵ��ͨ����ֵ��ʽ˵������"G"�����и�ʽ���á���� format ���κ�����ֵ����÷��������� FormatException���ڴ����������£�ToString ����֧�� 50 λʮ�������ֵľ��ȡ�Ҳ����˵����� BigInteger ֵ�е�����λ������ 50����ֻ�� 50 �������Чλ����������ַ����У������������ֶ����滻Ϊ�㡣Ȼ����BigInteger ֧�֡�R����׼��ʽ˵������������������ֵ��ʹ�� "R" ��ʽ�ַ����� ToString(String) �������ص��ַ����������� BigInteger ֵ��Ȼ�����ʹ�� Parse �� TryParse ���������Իָ�ԭʼֵ���������κ�������ʧ��.NET Framework �ṩ�˹㷺�ĸ�ʽ����֧�֣�����ĸ�ʽ���������жԴ��и���ϸ���������й���ֵ��ʽ˵�����ĸ�����Ϣ����μ���׼���ָ�ʽ�ַ������Զ������ָ�ʽ�ַ������йض� .NET Framework �еĸ�ʽ����֧�ֵĸ�����Ϣ����μ���ʽ�����͡�provider ������ IFormatProvider ʵ�֡��� GetFormat ��������һ�� NumberFormatInfo ���󣬸ö����ṩ�йش˷������ص��ַ�����ʽ���������ض���Ϣ���ڵ��� ToString(String, IFormatProvider) ����ʱ��������� provider ������ GetFormat �����������䴫��һ����ʾ NumberFormatInfo ���͵� Type ���󡣸� GetFormat ����Ȼ�󷵻� NumberFormatInfo ���󣬸ö����ṩ�������� value �����ĸ�ʽ����Ϣ�����縺�š���ָ�����С������š�������ʹ�� provider ������ ToString(String, IFormatProvider) �����ṩ��ʽ������Ϣ�ķ�ʽ�������Դ���һ�� CultureInfo ���󣬱�ʾ�ṩ��ʽ������Ϣ�������ԡ��� GetFormat �������� NumberFormatInfo ���󣬸ö����ṩ��Ը������Ե���ֵ��ʽ������Ϣ�����Դ����ṩ��ֵ��ʽ������Ϣ��ʵ�� NumberFormatInfo ���󡣣��� GetFormat ʵ�ֽ����������������Դ���һ��ʵ�� IFormatProvider ���Զ�������� GetFormat ����ʵ�����������ṩ��ʽ������Ϣ�� NumberFormatInfo ������� provider Ϊ null���򷵻ص��ַ����ĸ�ʽ���û��ڵ�ǰ�����Ե� NumberFormatInfo ����
        /// </remarks>
        public string ToString(string format, IFormatProvider provider) {
            return BigNumber.FormatBigInteger(this, format, NumberFormatInfo.GetInstance(provider));
        }
   
        /// <summary>
        /// ʹ�� 32 λ����������ֵ��ʼ�� BigInteger �ṹ����ʵ����
        /// </summary>
        /// <param name="value"> 32λ������������</param>
        public BigInteger(int value) {
            if (value == MaskHighBit) {
                this = _minInt;
            } else {
                _sign = value;
                _bits = null;
            }
        }

        /// <summary>
        /// ʹ�� 32 λ�޷�������ֵ��ʼ�� BigInteger �ṹ����ʵ������ API ������ CLS�� ���� CLS ����� API Ϊ BigInteger(Int64)��
        /// </summary>
        /// <param name="value">32�޷�������ֵ��</param>
        /// <remarks>
        /// ʹ�ô˹��캯��ʵ���� BigInteger ʱû�о�����ʧ�����ô˹��캯���������� BigInteger ֵ�뽫 UInt32 ֵ��ֵ�� BigInteger �Ľ����ͬ��
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
        /// ʹ�� 64 λ����������ֵ��ʼ�� BigInteger �ṹ����ʵ����
        /// </summary>
        /// <param name="value">64λ������������</param>
        /// <remarks>
        /// ʹ�ô˹��캯��ʵ���� BigInteger ����ʱû�о�����ʧ�����ô˹��캯���������� BigInteger ֵ�뽫 Int64 ֵ��ֵ�� BigInteger �Ľ����ͬ��
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
        /// ʹ�� 64 λ�޷�������ֵ��ʼ�� BigInteger �ṹ����ʵ������ API ������ CLS�� ���� CLS ����� API Ϊ BigInteger(Double)��
        /// </summary>
        /// <param name="value">64 λ�޷���������</param>
        /// <remarks>
        /// ʹ�ô˹��캯��ʵ���� BigInteger ʱû�о�����ʧ�����ô˹��캯���������� BigInteger ֵ�뽫 UInt64 ֵ��ֵ�� BigInteger �Ľ����ͬ��
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
        /// ʹ�õ����ȸ���ֵ��ʼ�� BigInteger �ṹ����ʵ����
        /// </summary>
        /// <param name="value">�����ȸ���ֵ��</param>
		/// <exception cref="OverflowException"> <paramref name="value"/>  ��ֵΪ Single.NaN��- �� -<paramref name="value"/> ��ֵΪ Single.NegativeInfinity��- �� -<paramref name="value"/> ��ֵΪ Single.PositiveInfinity��</exception>
        /// <remarks>
        /// value ������С��������ʵ���� BigInteger ����ʱ���ضϡ���Ϊȱ�� Single �������͵ľ��ȣ����ô˹��캯�����ܵ������ݶ�ʧ�����ô˹��캯���������� BigInteger ֵ�뽫 Single ֵ��ʽ��ֵ�� BigInteger �Ľ����ͬ��
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
        /// ʹ��˫���ȸ���ֵ��ʼ�� BigInteger �ṹ����ʵ����
        /// </summary>
        /// <param name="value">һ��˫���ȸ���ֵ��</param>
		/// <exception cref="OverflowException"><paramref name="value"/> ��ֵΪ Double.NaN��- �� -<paramref name="value"/> ��ֵΪ Double.NegativeInfinity��- �� -<paramref name="value"/> ��ֵΪ Double.PositiveInfinity��</exception>
        /// <remarks>
        /// ������С��������ʵ���� BigInteger ����ʱ���ضϡ���Ϊȱ�� Double �������͵ľ��ȣ����ô˹��캯�����ܵ������ݶ�ʧ�����ô˹��캯���������� BigInteger ֵ�뽫 Double ֵ��ʽ��ֵ�� BigInteger �Ľ����ͬ��
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
        /// ʹ�� Decimal ֵ��ʼ�� BigInteger �ṹ����ʵ����
        /// </summary>
        /// <param name="value">һ��С����</param>
        /// <remarks>
        /// ���ô˹��캯���Ľ���뽫 Decimal ֵ��ʽ���� BigInteger ������ͬ�����ô˹��캯�����ܵ������ݶ�ʧ��ʵ���� BigInteger ����ʱ�����ض� value ������С�����֡�
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
        /// ʹ���ֽ������е�ֵ��ʼ�� BigInteger �ṹ����ʵ����
        /// </summary>
        /// <param name="value">˳��Ϊ little-endian ���ֽ�ֵ�����顣</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> Ϊ null��</exception>
        /// <remarks>
        /// value �����еĸ����ֽ�Ӧ��Ϊ little-endian ˳�򣬴������λ�ֽڵ������λ�ֽڡ�
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
		/// �����ֵ��ַ�����ʾ��ʽת��Ϊ���ĵ�Ч BigInteger ��ʾ��ʽ��
        /// </summary>
        /// <param name="value">����Ҫת�������ֵ��ַ�����</param>
        /// <returns>һ��ֵ������ value ������ָ�������֡�</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> Ϊ�ա�</exception>
		/// <exception cref="FormatException"><paramref name="value"/> �ĸ�ʽ����ȷ��</exception>
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
		/// �Ƚ����� BigInteger ֵ��������һ��������������ָʾ��һ��ֵ��С�ڡ����ڻ��Ǵ��ڵڶ���ֵ��
		/// </summary>
		/// <param name="left">Ҫ�Ƚϵĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ�Ƚϵĵڶ���ֵ��</param>
		/// <returns>һ��������������ָʾ left �� right �����ֵ�����±���ʾ��</returns>
        public static int Compare(BigInteger left, BigInteger right) {
            return left.CompareTo(right);
        }

        /// <summary>
        /// ��ȡ BigInteger ����ľ���ֵ�� 
        /// </summary>
        /// <param name="value">���֡�</param>
        /// <returns>value �ľ���ֵ��</returns>
        /// <remarks>
        /// ���ֵľ���ֵ�Ǹ�����ȥ������ź�����֣����±���ʾ��
        /// </remarks>
        /// <example>
        /// �����ʾ��ʹ�� Abs ������ BigInteger ֵ�� 2 �Ĳ����ʾ��ʽת��Ϊ������ֵ��ʾ��ʽ��Ȼ���ٽ������л����ļ��� ���ļ��е����ݽ��з����л��������������� BigInteger ���� 
        /// </example>
        public static BigInteger Abs(BigInteger value) {
            if (value < Zero) {
                return -(value);
            }
            return value;
        }

        /// <summary>
        /// ������ BigInteger ֵ��ӣ������ؽ����
        /// </summary>
        /// <param name="left">Ҫ��ӵĵ�һ��ֵ��</param>
        /// <param name="right">Ҫ��ӵĵڶ���ֵ��</param>
        /// <returns>left �� right �ĺ͡�</returns>
        /// <remarks>
        /// ��֧����������ػ��Զ�������������Կ���ʹ�� Add ����ִ��ʹ�� BigInteger ֵ�ļӷ���
        /// ��ͨ����ֵ�ӷ��ĺ͵ķ�����ʵ���� BigInteger ����ʱ��Add �����Ǽӷ������һ������������⽫�������ʾ���н��ܡ�
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
		/// ����һ��ֵ�м�ȥһ�� BigInteger ֵ�����ؽ����
		/// </summary>
		/// <param name="left">Ҫ���м�ȥ��ֵ������������</param>
		/// <param name="right">Ҫ��ȥ��ֵ����������</param>
		/// <returns>right �� left ���õĽ����</returns>
        public static BigInteger Subtract(BigInteger left, BigInteger right) {
            return (left - right);
        }

        /// <summary>
		/// �������� BigInteger ֵ�ĳ˻���
        /// </summary>
		/// <param name="left">Ҫ��˵ĵ�һ�����֡�</param>
		/// <param name="right">Ҫ��˵ĵڶ������֡�</param>
		/// <returns>left �� right �����ĳ˻���</returns>
        public static BigInteger Multiply(BigInteger left, BigInteger right) {
            return (left * right);
        }

		/// <summary>
		/// ����һ��ֵ�� BigInteger ֵ�����ؽ����
		/// </summary>
		/// <param name="dividend">Ҫ��Ϊ��������ֵ��</param>
		/// <param name="divisor">Ҫ��Ϊ������ֵ��</param>
		/// <returns>�������̡�</returns>
		/// <exception cref="DivideByZeroException">divisorΪ 0 ���㣩��</exception>
        public static BigInteger Divide(BigInteger dividend, BigInteger divisor) {
            return (dividend / divisor);
        }

		/// <summary>
		/// ������ BigInteger ִֵ������������������
		/// </summary>
		/// <param name="dividend">Ҫ��Ϊ��������ֵ��</param>
		/// <param name="divisor">Ҫ��Ϊ������ֵ��</param>
		/// <returns>�� dividend ���� divisor �����õ�������</returns>
		/// <exception cref="DivideByZeroException">divisorΪ 0 ���㣩��</exception>
        public static BigInteger Remainder(BigInteger dividend, BigInteger divisor) {
            return dividend % divisor;
        }

		/// <summary>
		/// ����һ��ֵ��һ�� BigInteger ֵ�����ؽ����������������з���������
		/// </summary>
		/// <param name="dividend">Ҫ��Ϊ��������ֵ��</param>
		/// <param name="divisor">Ҫ��Ϊ������ֵ��</param>
		/// <param name="remainder">���˷�������ʱ������һ����ʾ��������� BigInteger ֵ���ò���δ����ʼ���������ݡ�</param>
		/// <returns>�������̡�</returns>
		/// <exception cref="DivideByZeroException">divisorΪ 0 ���㣩��</exception>
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
		/// ��ָ���� BigInteger ֵ�󷴡�
		/// </summary>
		/// <param name="value">Ҫ�󷴵�ֵ��</param>
		/// <returns>value �������Ը�һ (-1) �Ľ����</returns>
        public static BigInteger Negate(BigInteger value) {
            return -(value);
        }

		/// <summary>
		/// ����ָ�����ֵ���Ȼ��������Ϊ e����
		/// </summary>
		/// <param name="value">Ҫ��������������֡�</param>
		/// <returns>value ����Ȼ��������Ϊ e�����硰��ע�������еı���ʾ��</returns>
		/// <exception cref="ArgumentOutOfRangeException">value �Ķ��������� Double �������͵ķ�Χ��</exception>
        public static double Log(BigInteger value) {
            return Log(value, 2.7182818284590451);
        }

		/// <summary>
		/// ����ָ��������ʹ��ָ����ʱ�Ķ�����
		/// </summary>
		/// <param name="value">Ҫ��������������֡�</param>
		/// <param name="baseValue">�����ĵס�</param>
		/// <returns>value ���� baseValue Ϊ�׵Ķ������硰��ע�������еı���ʾ��</returns>
		/// <exception cref="ArgumentOutOfRangeException">value �Ķ��������� Double �������͵ķ�Χ��</exception>
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
		/// ����ָ�����ֵ���Ȼ��������Ϊ 10����
		/// </summary>
		/// <param name="value">Ҫ��������������֡�</param>
		/// <returns>value ����Ȼ��������Ϊ 10�����硰��ע�������еı���ʾ��</returns>
		/// <exception cref="ArgumentOutOfRangeException">value �Ķ��������� Double �������͵ķ�Χ��</exception>
        public static double Log10(BigInteger value) {
            return Log(value, 10.0);
        }

		/// <summary>
		/// �������� BigInteger ֵ�����Լ��
		/// </summary>
		/// <param name="left">��һ��ֵ��</param>
		/// <param name="right">�ڶ���ֵ��</param>
		/// <returns>left �� right �����Լ����</returns>
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
		/// �������� BigInteger ֵ�еĽϴ��ߡ�
		/// </summary>
		/// <param name="left">Ҫ�Ƚϵĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ�Ƚϵĵڶ���ֵ��</param>
		/// <returns>left �� right �����нϴ��һ����</returns>
        public static BigInteger Max(BigInteger left, BigInteger right) {
            if (left.CompareTo(right) < 0) {
                return right;
            }
            return left;
        }

		/// <summary>
		/// �������� BigInteger ֵ�еĽ�С�ߡ�
		/// </summary>
		/// <param name="left">Ҫ�Ƚϵĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ�Ƚϵĵڶ���ֵ��</param>
		/// <returns>left �� right �����н�С��һ����</returns>
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
		/// ����ĳ����Ϊ�ס�����һ����Ϊָ������ִ��ģ��������
		/// </summary>
		/// <param name="value">Ҫ���� exponent ���ݵ����֡�</param>
		/// <param name="exponent">�� value �����������ָ����</param>
		/// <param name="modulus">�� valueָ�����Ե�ֵ��</param>
		/// <returns>�� valueָ������ modulus ���������</returns>
		/// <exception cref="DivideByZeroException">modulus ���㡣</exception>
		/// <exception cref="ArgumentOutOfRangeException">exponent Ϊ������</exception>
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
		/// ���� BigInteger ֵΪ�ס���ָ����ֵΪָ�����ݡ�
		/// </summary>
		/// <param name="value">Ҫ���� exponent ���ݵ�����</param>
		/// <param name="exponent">value �� exponent ���ݵļ�������</param>
		/// <returns>value �� exponent ���ݵļ�������</returns>
		/// <exception cref="ArgumentOutOfRangeException">exponent ������ֵΪ����</exception>
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
		/// ʵ�ִ� <see cref="System.Byte"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator BigInteger(byte value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.SByte"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(sbyte value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Int16"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator BigInteger(short value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.UInt16"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(ushort value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Int32"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator BigInteger(int value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.UInt32"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(uint value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Int64"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator BigInteger(long value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.UInt64"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(ulong value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Single"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator BigInteger(float value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Double"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator BigInteger(double value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Decimal"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator BigInteger(decimal value) {
            return new BigInteger(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.String"/> �� <see cref="Py.Algorithm.Numerics.BigInteger"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator BigInteger(string value) {
            return Parse(value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Byte"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator byte(BigInteger value) {
            return (byte)((int)value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.SByte"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static explicit operator sbyte(BigInteger value) {
            return (sbyte)((int)value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Int16"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator short(BigInteger value) {
            return (short)((int)value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.UInt16"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static explicit operator ushort(BigInteger value) {
            return (ushort)((int)value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Int32"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
		/// <exception cref="OverflowException">ֵ̫�����̫С��</exception>
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
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.UInt32"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
		/// <exception cref="OverflowException">ֵ̫�����̫С��</exception>
        [CLSCompliant(false)]
        public static explicit operator uint(BigInteger value) {
            if (value._bits == null) {
                return (uint)value._sign;
            }
            Thrower.ThrowOverflowExceptionIf(Length(value._bits) > 1 || value._sign < 0, Properties.Messages.BigIntegerRange);
            return value._bits[0];
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Int64"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
		/// <exception cref="OverflowException">ֵ̫�����̫С��</exception>
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
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.UInt64"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
		/// <exception cref="OverflowException">ֵ̫�����̫С��</exception>
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
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Single"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator float(BigInteger value) {
            return (float)((double)value);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Double"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
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
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.Decimal"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
		/// <exception cref="OverflowException">ֵ̫�����̫С��</exception>
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
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="System.String"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator string(BigInteger value) {
            return value.ToString();
        }

		/// <summary>
		/// ʵ�ֲ��� &amp; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� | ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� ^ ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� &lt;&lt; ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <param name="shift">The shift��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� &gt;&gt; ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <param name="shift">The shift��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� ~ ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator ~(BigInteger value) {
            return -(value + One);
        }

		/// <summary>
		/// ʵ�ֲ��� - ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator -(BigInteger value) {
            value._sign = -value._sign;
            return value;
        }

		/// <summary>
		/// ʵ�ֲ��� + ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator +(BigInteger value) {
            return value;
        }

		/// <summary>
		/// ʵ�ֲ��� ++ ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator ++(BigInteger value) {
            return (value + One);
        }

		/// <summary>
		/// ʵ�ֲ��� -- ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator --(BigInteger value) {
            return (value - One);
        }

		/// <summary>
		/// ʵ�ֲ��� + ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� - ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
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
		/// ʵ�ֲ��� * ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator *(BigInteger left, BigInteger right) {
            int sign = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(left, ref sign);
            BigIntegerBuilder regMul = new BigIntegerBuilder(right, ref sign);
            builder.Mul(ref regMul);
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// ʵ�ֲ��� / ��
		/// </summary>
		/// <param name="dividend">����λ�á�</param>
		/// <param name="divisor">The divisor��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator /(BigInteger dividend, BigInteger divisor) {
            int sign = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(dividend, ref sign);
            BigIntegerBuilder regDen = new BigIntegerBuilder(divisor, ref sign);
            builder.Div(ref regDen);
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// ʵ�ֲ��� % ��
		/// </summary>
		/// <param name="dividend">����λ�á�</param>
		/// <param name="divisor">The divisor��</param>
		/// <returns>�����Ľ����</returns>
        public static BigInteger operator %(BigInteger dividend, BigInteger divisor) {
            int sign = 1;
            int num2 = 1;
            BigIntegerBuilder builder = new BigIntegerBuilder(dividend, ref sign);
            BigIntegerBuilder regDen = new BigIntegerBuilder(divisor, ref num2);
            builder.Mod(ref regDen);
            return builder.GetInteger(sign);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator <(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) < 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator <=(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) <= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator >(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) > 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator >=(BigInteger left, BigInteger right) {
            return (left.CompareTo(right) >= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� == ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator ==(BigInteger left, BigInteger right) {
            return left.Equals(right);
        }

		/// <summary>
		/// ʵ�ֲ��� != ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator !=(BigInteger left, BigInteger right) {
            return !left.Equals(right);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator <(BigInteger left, long right) {
            return (left.CompareTo(right) < 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator <=(BigInteger left, long right) {
            return (left.CompareTo(right) <= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator >(BigInteger left, long right) {
            return (left.CompareTo(right) > 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator >=(BigInteger left, long right) {
            return (left.CompareTo(right) >= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� == ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator ==(BigInteger left, long right) {
            return left.Equals(right);
        }

		/// <summary>
		/// ʵ�ֲ��� != ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator !=(BigInteger left, long right) {
            return !left.Equals(right);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator <(long left, BigInteger right) {
            return (right.CompareTo(left) > 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator <=(long left, BigInteger right) {
            return (right.CompareTo(left) >= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator >(long left, BigInteger right) {
            return (right.CompareTo(left) < 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator >=(long left, BigInteger right) {
            return (right.CompareTo(left) <= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� == ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator ==(long left, BigInteger right) {
            return right.Equals(left);
        }

		/// <summary>
		/// ʵ�ֲ��� != ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator !=(long left, BigInteger right) {
            return !right.Equals(left);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator <(BigInteger left, ulong right) {
            return (left.CompareTo(right) < 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator <=(BigInteger left, ulong right) {
            return (left.CompareTo(right) <= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator >(BigInteger left, ulong right) {
            return (left.CompareTo(right) > 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator >=(BigInteger left, ulong right) {
            return (left.CompareTo(right) >= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� == ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator ==(BigInteger left, ulong right) {
            return left.Equals(right);
        }

		/// <summary>
		/// ʵ�ֲ��� != ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator !=(BigInteger left, ulong right) {
            return !left.Equals(right);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator <(ulong left, BigInteger right) {
            return (right.CompareTo(left) > 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &lt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator <=(ulong left, BigInteger right) {
            return (right.CompareTo(left) >= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt; ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator >(ulong left, BigInteger right) {
            return (right.CompareTo(left) < 0);
        }

		/// <summary>
		/// ʵ�ֲ��� &gt;= ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator >=(ulong left, BigInteger right) {
            return (right.CompareTo(left) <= 0);
        }

		/// <summary>
		/// ʵ�ֲ��� == ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        [CLSCompliant(false)]
        public static bool operator ==(ulong left, BigInteger right) {
            return right.Equals(left);
        }

		/// <summary>
		/// ʵ�ֲ��� != ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
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

