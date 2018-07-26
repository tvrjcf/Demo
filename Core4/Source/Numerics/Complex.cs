
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Py.Algorithm.Numerics {

	/// <summary>
	/// ��ʾһ��������
	/// </summary>
	/// <remarks>�� <see href="http://msdn.microsoft.com/zh-cn/library/system.numerics.complex.aspx"/> ��</remarks>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Complex :IEquatable<Complex>, IFormattable {
        private const double LOG_10_INV = 0.43429448190325;
        private double _real;
        private double _imaginary;

		/// <summary>
		/// �����µ� Complex ʵ������ʵ���������������㡣
		/// </summary>
        public static readonly Complex Zero;

		/// <summary>
		/// �����µ� Complex ʵ������ʵ������һ�����������㡣
		/// </summary>
        public static readonly Complex One;

		/// <summary>
		/// �����µ� Complex ʵ������ʵ�������㣬��������һ��
		/// </summary>
        public static readonly Complex ImaginaryOne;

		/// <summary>
		/// ��ȡ��ǰ Complex �����ʵ����
		/// </summary>
        public double Real {
            get {
                return _real;
            }
        }

		/// <summary>
		/// ��ȡ��ǰ Complex ������鲿��
		/// </summary>
        public double Imaginary {
            get {
                return _imaginary;
            }
        }

		/// <summary>
		/// ��ȡ��������ֵ�������ֵ����
		/// </summary>
        public double Magnitude {
            get {
                return Abs(this);
            }
        }

		/// <summary>
		/// ��ȡ��������λ��
		/// </summary>
        public double Phase {
            get {
                return Math.Atan2(_imaginary, _real);
            }
        }

		/// <summary>
		/// ʹ��ָ����ʵ��ֵ������ֵ��ʼ�� Complex �ṹ����ʵ����
		/// </summary>
		/// <param name="real">������ʵ����</param>
		/// <param name="imaginary">�������鲿��</param>
        public Complex(double real, double imaginary) {
            _real = real;
            _imaginary = imaginary;
        }

		/// <summary>
		/// �ӵ�ļ����괴��������
		/// </summary>
		/// <param name="magnitude">��ֵ�����Ǵ�ԭ�㣨x ���� y ��Ľ��㣩�����ֵľ��롣</param>
		/// <param name="phase">��λ������ֱ�������ˮƽ��ĽǶȣ��Ի���Ϊ��λ��</param>
		/// <returns>һ��������</returns>
        public static Complex FromPolarCoordinates(double magnitude, double phase) {
            return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
        }

		/// <summary>
		/// ����ָ�������ļӷ���Ԫ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>value ������ Real �� Imaginary ���ֳ��� -1 �Ľ����</returns>
        public static Complex Negate(Complex value) {
            return -(value);
        }

		/// <summary>
		/// ��һ�������м�ȥ��һ�����������ؽ����
		/// </summary>
		/// <param name="left">Ҫ���м�ȥ��ֵ������������</param>
		/// <param name="right">Ҫ��ȥ��ֵ����������</param>
		/// <returns>right �� left ���õĽ����</returns>
        public static Complex Add(Complex left, Complex right) {
            return (left + right);
        }

		/// <summary>
		/// ��һ�������м�ȥ��һ��������
		/// </summary>
		/// <param name="left">Ҫ���м�ȥ��ֵ������������</param>
		/// <param name="right">Ҫ��ȥ��ֵ����������</param>
		/// <returns>right �� left ���õĽ����</returns>
        public static Complex Subtract(Complex left, Complex right) {
            return (left - right);
        }

		/// <summary>
		/// ������ָ��������ˡ�
		/// </summary>
		/// <param name="left">Ҫ��˵ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ��˵ĵڶ���ֵ��</param>
		/// <returns>left �� right �ĳ˻���</returns>
        public static Complex Multiply(Complex left, Complex right) {
            return (left * right);
        }

		/// <summary>
		/// ��һ��ָ����������һ��ָ��������
		/// </summary>
		/// <param name="dividend">Ҫ��Ϊ��������ֵ��</param>
		/// <param name="divisor">Ҫ��Ϊ������ֵ��</param>
		/// <returns>left ���� right �Ľ����</returns>
        public static Complex Divide(Complex dividend, Complex divisor) {
            return (dividend / divisor);
        }

		/// <summary>
		/// ʵ�ֲ��� - ��
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static Complex operator -(Complex value) {
            return new Complex(-value._real, -value._imaginary);
        }

		/// <summary>
		/// ʵ�ֲ��� + ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static Complex operator +(Complex left, Complex right) {
            return new Complex(left._real + right._real, left._imaginary + right._imaginary);
        }

		/// <summary>
		/// ʵ�ֲ��� - ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static Complex operator -(Complex left, Complex right) {
            return new Complex(left._real - right._real, left._imaginary - right._imaginary);
        }

		/// <summary>
		/// ʵ�ֲ��� * ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static Complex operator *(Complex left, Complex right) {
            return new Complex((left._real * right._real) - (left._imaginary * right._imaginary), (left._imaginary * right._real) + (left._real * right._imaginary));
        }

		/// <summary>
		/// ʵ�ֲ��� / ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static Complex operator /(Complex left, Complex right) {
            double real = left._real;
            double imaginary = left._imaginary;
            double a = right._real;
            double num4 = right._imaginary;
            if (Math.Abs(num4) < Math.Abs(a)) {
                return new Complex((real + (imaginary * (num4 / a))) / (a + (num4 * (num4 / a))), (imaginary - (real * (num4 / a))) / (a + (num4 * (num4 / a))));
            }
            return new Complex((imaginary + (real * (a / num4))) / (num4 + (a * (a / num4))), (-real + (imaginary * (a / num4))) / (num4 + (a * (a / num4))));
        }

		/// <summary>
		/// ��ȡ�����ľ���ֵ������ֵ����
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>value �ľ���ֵ��</returns>
        public static double Abs(Complex value) {
            if (double.IsInfinity(value._real) || double.IsInfinity(value._imaginary)) {
                return double.PositiveInfinity;
            }
            double num = Math.Abs(value._real);
            double num2 = Math.Abs(value._imaginary);
            if (num > num2) {
                double num3 = num2 / num;
                return (num * Math.Sqrt(1.0 + (num3 * num3)));
            }
            if (num2 == 0.0) {
                return num;
            }
            double num4 = num / num2;
            return (num2 * Math.Sqrt(1.0 + (num4 * num4)));
        }

		/// <summary>
		/// ���㸴���Ĺ�������ؽ����
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>value �Ĺ��</returns>
        public static Complex Conjugate(Complex value) {
            return new Complex(value._real, -value._imaginary);
        }

		/// <summary>
		/// ���ظ����ĳ˷�������
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>value �ĵ�����</returns>
        public static Complex Reciprocal(Complex value) {
            if ((value._real == 0.0) && (value._imaginary == 0.0)) {
                return Zero;
            }
            return (One / value);
        }


		/// <summary>
		/// ʵ�ֲ��� == ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator ==(Complex left, Complex right) {
            return ((left._real == right._real) && (left._imaginary == right._imaginary));
        }

		/// <summary>
		/// ʵ�ֲ��� != ��
		/// </summary>
		/// <param name="left">Ҫ����ĵ�һ��ֵ��</param>
		/// <param name="right">Ҫ����ĵڶ���ֵ��</param>
		/// <returns>�����Ľ����</returns>
        public static bool operator !=(Complex left, Complex right) {
            if (left._real == right._real) {
                return (left._imaginary != right._imaginary);
            }
            return true;
        }

		/// <summary>
		/// ָʾ��ʵ����ָ�������Ƿ���ȡ�
		/// </summary>
		/// <param name="obj">Ҫ�Ƚϵ���һ������</param>
		/// <returns>
		/// ��� <paramref name="obj"/> �͸�ʵ��������ͬ�����Ͳ���ʾ��ͬ��ֵ����Ϊ true������Ϊ false��
		/// </returns>
        public override bool Equals(object obj) {
            return ((obj is Complex) && (this == ((Complex)obj)));
        }

		/// <summary>
		/// ָʾ��ʵ����ָ�������Ƿ���ȡ�
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>
		/// ��� <paramref name="value"/> �͸�ʵ��������ͬ�����Ͳ���ʾ��ͬ��ֵ����Ϊ true������Ϊ false��
		/// </returns>
        public bool Equals(Complex value) {
            return (_real.Equals(value._real) && _imaginary.Equals(value._imaginary));
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Int16"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator Complex(short value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Int32"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator Complex(int value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Int64"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator Complex(long value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.UInt16"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(ushort value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.UInt32"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(uint value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.UInt64"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(ulong value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.SByte"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(sbyte value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Byte"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator Complex(byte value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Single"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator Complex(float value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Double"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static implicit operator Complex(double value) {
            return new Complex(value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="Py.Algorithm.Numerics.BigInteger"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator Complex(BigInteger value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ʵ�ִ� <see cref="System.Decimal"/> �� <see cref="Py.Algorithm.Numerics.Complex"/> �����Ե�ת����
		/// </summary>
		/// <param name="value">ֵ��</param>
		/// <returns>ת���Ľ����</returns>
        public static explicit operator Complex(decimal value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// ����ǰ������ֵת��Ϊ����õѿ�����ʽ�ĵ�Ч�ַ�����ʾ��ʽ��
		/// </summary>
		/// <returns>
		/// ��ǰʵ���Ĳ��õѿ�����ʽ���ַ�����ʾ��ʽ��
		/// </returns>
        public override string ToString() {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", _real, _imaginary );
        }

		/// <summary>
		/// ͨ���Ե�ǰ������ʵ�����鲿ʹ��ָ����ʽ��������ֵת��Ϊ����õѿ�����ʽ�ĵ�Ч�ַ�����ʾ��ʽ��
		/// </summary>
		/// <param name="format">��׼���Զ������ֵ��ʽ�ַ�����</param>
		/// <returns>��ǰʵ���Ĳ��õѿ�����ʽ���ַ�����ʾ��ʽ��</returns>
		/// <exception cref="FormatException"><paramref name="format"/>  ������Ч�ĸ�ʽ�ַ�����</exception>
        public string ToString(string format) {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", _real.ToString(format, CultureInfo.CurrentCulture), _imaginary.ToString(format, CultureInfo.CurrentCulture) );
        }

		/// <summary>
		/// ͨ���Ե�ǰ������ʵ�����鲿ʹ��ָ����ʽ���������ض���ʽ��Ϣ��������ֵת��Ϊ����õѿ�����ʽ�ĵ�Ч�ַ�����ʾ��ʽ��
		/// </summary>
		/// <param name="provider">һ���ṩ�������ض��ĸ�ʽ������Ϣ�Ķ���</param>
		/// <returns>�� format �� provider ָ���ĵ�ǰʵ���Ĳ��õѿ�����ʽ���ַ�����ʾ��ʽ��</returns>
        public string ToString(IFormatProvider provider) {
            return string.Format(provider, "({0}, {1})",_real, _imaginary);
        }

		/// <summary>
		/// ͨ���Ե�ǰ������ʵ�����鲿ʹ��ָ����ʽ���������ض���ʽ��Ϣ��������ֵת��Ϊ����õѿ�����ʽ�ĵ�Ч�ַ�����ʾ��ʽ��
		/// </summary>
		/// <param name="format">��׼���Զ������ֵ��ʽ�ַ�����</param>
		/// <param name="provider">һ���ṩ�������ض��ĸ�ʽ������Ϣ�Ķ���</param>
		/// <returns>�� format �� provider ָ���ĵ�ǰʵ���Ĳ��õѿ�����ʽ���ַ�����ʾ��ʽ��</returns>
        public string ToString(string format, IFormatProvider provider) {
            return string.Format(provider, "({0}, {1})", _real.ToString(format, provider), _imaginary.ToString(format, provider));
        }

		/// <summary>
		/// ���ش�ʵ���Ĺ�ϣ���롣
		/// </summary>
		/// <returns>һ�� 32 λ�з������������Ǹ�ʵ���Ĺ�ϣ���롣</returns>
        public override int GetHashCode() {
            int num = 0x5f5e0fd;
            int num2 = _real.GetHashCode() % num;
            int hashCode = _imaginary.GetHashCode();
            return (num2 ^ hashCode);
        }

		/// <summary>
		/// ����ָ������������ֵ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>value ������ֵ��</returns>
        public static Complex Sin(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Sin(real) * Math.Cosh(imaginary), Math.Cos(real) * Math.Sinh(imaginary));
        }

		/// <summary>
		/// ����ָ��������˫������ֵ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>value ��˫������ֵ��</returns>
        public static Complex Sinh(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Sinh(real) * Math.Cos(imaginary), Math.Cosh(real) * Math.Sin(imaginary));
        }

		/// <summary>
		/// ���ر�ʾָ�������ķ�����ֵ�ĽǶȡ�
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>��ʾ value �ķ�����ֵ�ĽǶȡ�</returns>
        public static Complex Asin(Complex value) {
            return (-(ImaginaryOne) * Log((ImaginaryOne * value) + Sqrt(One - (value * value))));
        }

		/// <summary>
		/// ����ָ��������˫������ֵ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value ��˫������ֵ��</returns>
        public static Complex Cos(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Cos(real) * Math.Cosh(imaginary), -(Math.Sin(real) * Math.Sinh(imaginary)));
        }

		/// <summary>
		/// ����ָ��������˫������ֵ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value ��˫������ֵ��</returns>
        public static Complex Cosh(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Cosh(real) * Math.Cos(imaginary), Math.Sinh(real) * Math.Sin(imaginary));
        }

		/// <summary>
		/// ���ر�ʾָ�������ķ�����ֵ�ĽǶȡ�
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value �ķ�����ֵ�ĽǶȡ�</returns>
        public static Complex Acos(Complex value) {
            return (-(ImaginaryOne) * Log(value + (ImaginaryOne * Sqrt(One - (value * value)))));
        }

		/// <summary>
		/// ����ָ������������ֵ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value ������ֵ��</returns>
        public static Complex Tan(Complex value) {
            return (Sin(value) / Cos(value));
        }

		/// <summary>
		/// ����ָ��������˫������ֵ��
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value ��˫������ֵ��</returns>
        public static Complex Tanh(Complex value) {
            return (Sinh(value) / Cosh(value));
        }

		/// <summary>
		/// ���ر�ʾָ�������ķ�����ֵ�ĽǶȡ�
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value �ķ�����ֵ�ĽǶȡ�</returns>
        public static Complex Atan(Complex value) {
            Complex complex = new Complex(2.0, 0.0);
            return ((ImaginaryOne / complex) * (Log(One - (ImaginaryOne * value)) - Log(One + (ImaginaryOne * value))));
        }

		/// <summary>
		/// ����ָ����������Ȼ��������Ϊ e����
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value ����Ȼ��������Ϊ e����</returns>
        public static Complex Log(Complex value) {
            return new Complex(Math.Log(Abs(value)), Math.Atan2(value._imaginary, value._real));
        }

		/// <summary>
		/// ����ָ��������ʹ��ָ����ʱ�Ķ�����
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <param name="baseValue">����ȡ�����ĵס�</param>
		/// <returns>���� value ��ʹ��ָ����ʱ�Ķ�����</returns>
        public static Complex Log(Complex value, double baseValue) {
            return (Log(value) / Log(baseValue));
        }

		/// <summary>
		/// ����ָ�������� 10 Ϊ�׵Ķ�����
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value �� 10 Ϊ�׵Ķ�����</returns>
        public static Complex Log10(Complex value) {
            return Scale(Log(value), 0.43429448190325);
        }

		/// <summary>
		/// ���� e ����һ������ָ���Ĵ��ݡ�
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns> e ����һ������ value ָ���Ĵ��ݡ�</returns>
        public static Complex Exp(Complex value) {
            double num = Math.Exp(value._real);
            return new Complex(num * Math.Cos(value._imaginary), num * Math.Sin(value._imaginary));
        }

		/// <summary>
		/// ����ָ��������ƽ������
		/// </summary>
		/// <param name="value">һ��������</param>
		/// <returns>���� value ��ƽ������</returns>
        public static Complex Sqrt(Complex value) {
            return FromPolarCoordinates(Math.Sqrt(value.Magnitude), value.Phase / 2.0);
        }

		/// <summary>
		/// ����ָ���������ɸ���ָ���Ĵ��ݡ�
		/// </summary>
		/// <param name="value">Ҫ�������ݵĸ�����</param>
		/// <param name="power">ָ���ݵ�˫���ȸ�������</param>
		/// <returns>���� value ���ɸ���ָ���Ĵ��ݡ�</returns>
        public static Complex Pow(Complex value, Complex power) {
            if (power == Zero) {
                return One;
            }
            if (value == Zero) {
                return Zero;
            }
            double real = value._real;
            double imaginary = value._imaginary;
            double y = power._real;
            double num4 = power._imaginary;
            double a = Abs(value);
            double num6 = Math.Atan2(imaginary, real);
            double d = (y * num6) + (num4 * Math.Log(a));
            double num8 = Math.Pow(a, y) * Math.Pow(2.7182818284590451, -num4 * num6);
            return new Complex(num8 * Math.Cos(d), num8 * Math.Sin(d));
        }

		/// <summary>
		/// ����ָ���������ɸ���ָ���Ĵ��ݡ�
		/// </summary>
		/// <param name="value">Ҫ�������ݵĸ�����</param>
		/// <param name="power">ָ���ݵĸ�����</param>
		/// <returns>���� value �� power ���ݡ�</returns>
        public static Complex Pow(Complex value, double power) {
            return Pow(value, new Complex(power, 0.0));
        }

		private static Complex Scale(Complex value, double factor) {
            return new Complex(factor * value._real, factor * value._imaginary);
        }

		/// <summary>
		/// ��ʼ�� <see cref="Py.Algorithm.Numerics.Complex"/> �ľ�̬��Ա��
		/// </summary>
        static Complex() {
            Zero = new Complex(0.0, 0.0);
            One = new Complex(1.0, 0.0);
            ImaginaryOne = new Complex(0.0, 1.0);
        }
    }
}

