
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Py.Algorithm.Numerics {

	/// <summary>
	/// 表示一个复数。
	/// </summary>
	/// <remarks>见 <see href="http://msdn.microsoft.com/zh-cn/library/system.numerics.complex.aspx"/> 。</remarks>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Complex :IEquatable<Complex>, IFormattable {
        private const double LOG_10_INV = 0.43429448190325;
        private double _real;
        private double _imaginary;

		/// <summary>
		/// 返回新的 Complex 实例，其实数和虚数都等于零。
		/// </summary>
        public static readonly Complex Zero;

		/// <summary>
		/// 返回新的 Complex 实例，其实数等于一，虚数等于零。
		/// </summary>
        public static readonly Complex One;

		/// <summary>
		/// 返回新的 Complex 实例，其实数等于零，虚数等于一。
		/// </summary>
        public static readonly Complex ImaginaryOne;

		/// <summary>
		/// 获取当前 Complex 对象的实部。
		/// </summary>
        public double Real {
            get {
                return _real;
            }
        }

		/// <summary>
		/// 获取当前 Complex 对象的虚部。
		/// </summary>
        public double Imaginary {
            get {
                return _imaginary;
            }
        }

		/// <summary>
		/// 获取复数的量值（或绝对值）。
		/// </summary>
        public double Magnitude {
            get {
                return Abs(this);
            }
        }

		/// <summary>
		/// 获取复数的相位。
		/// </summary>
        public double Phase {
            get {
                return Math.Atan2(_imaginary, _real);
            }
        }

		/// <summary>
		/// 使用指定的实数值和虚数值初始化 Complex 结构的新实例。
		/// </summary>
		/// <param name="real">复数的实部。</param>
		/// <param name="imaginary">复数的虚部。</param>
        public Complex(double real, double imaginary) {
            _real = real;
            _imaginary = imaginary;
        }

		/// <summary>
		/// 从点的极坐标创建复数。
		/// </summary>
		/// <param name="magnitude">量值，它是从原点（x 轴与 y 轴的交点）到数字的距离。</param>
		/// <param name="phase">相位，它是直线相对于水平轴的角度，以弧度为单位。</param>
		/// <returns>一个复数。</returns>
        public static Complex FromPolarCoordinates(double magnitude, double phase) {
            return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
        }

		/// <summary>
		/// 返回指定复数的加法逆元。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>value 参数的 Real 和 Imaginary 部分乘以 -1 的结果。</returns>
        public static Complex Negate(Complex value) {
            return -(value);
        }

		/// <summary>
		/// 从一个复数中减去另一个复数并返回结果。
		/// </summary>
		/// <param name="left">要从中减去的值（被减数）。</param>
		/// <param name="right">要减去的值（减数）。</param>
		/// <returns>right 减 left 所得的结果。</returns>
        public static Complex Add(Complex left, Complex right) {
            return (left + right);
        }

		/// <summary>
		/// 从一个复数中减去另一个复数。
		/// </summary>
		/// <param name="left">要从中减去的值（被减数）。</param>
		/// <param name="right">要减去的值（减数）。</param>
		/// <returns>right 减 left 所得的结果。</returns>
        public static Complex Subtract(Complex left, Complex right) {
            return (left - right);
        }

		/// <summary>
		/// 将两个指定复数相乘。
		/// </summary>
		/// <param name="left">要相乘的第一个值。</param>
		/// <param name="right">要相乘的第二个值。</param>
		/// <returns>left 与 right 的乘积。</returns>
        public static Complex Multiply(Complex left, Complex right) {
            return (left * right);
        }

		/// <summary>
		/// 用一个指定复数除另一个指定复数。
		/// </summary>
		/// <param name="dividend">要作为被除数的值。</param>
		/// <param name="divisor">要作为除数的值。</param>
		/// <returns>left 除以 right 的结果。</returns>
        public static Complex Divide(Complex dividend, Complex divisor) {
            return (dividend / divisor);
        }

		/// <summary>
		/// 实现操作 - 。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>操作的结果。</returns>
        public static Complex operator -(Complex value) {
            return new Complex(-value._real, -value._imaginary);
        }

		/// <summary>
		/// 实现操作 + 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static Complex operator +(Complex left, Complex right) {
            return new Complex(left._real + right._real, left._imaginary + right._imaginary);
        }

		/// <summary>
		/// 实现操作 - 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static Complex operator -(Complex left, Complex right) {
            return new Complex(left._real - right._real, left._imaginary - right._imaginary);
        }

		/// <summary>
		/// 实现操作 * 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static Complex operator *(Complex left, Complex right) {
            return new Complex((left._real * right._real) - (left._imaginary * right._imaginary), (left._imaginary * right._real) + (left._real * right._imaginary));
        }

		/// <summary>
		/// 实现操作 / 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
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
		/// 获取复数的绝对值（或量值）。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>value 的绝对值。</returns>
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
		/// 计算复数的共轭，并返回结果。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>value 的共轭。</returns>
        public static Complex Conjugate(Complex value) {
            return new Complex(value._real, -value._imaginary);
        }

		/// <summary>
		/// 返回复数的乘法倒数。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>value 的倒数。</returns>
        public static Complex Reciprocal(Complex value) {
            if ((value._real == 0.0) && (value._imaginary == 0.0)) {
                return Zero;
            }
            return (One / value);
        }


		/// <summary>
		/// 实现操作 == 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator ==(Complex left, Complex right) {
            return ((left._real == right._real) && (left._imaginary == right._imaginary));
        }

		/// <summary>
		/// 实现操作 != 。
		/// </summary>
		/// <param name="left">要计算的第一个值。</param>
		/// <param name="right">要计算的第二个值。</param>
		/// <returns>操作的结果。</returns>
        public static bool operator !=(Complex left, Complex right) {
            if (left._real == right._real) {
                return (left._imaginary != right._imaginary);
            }
            return true;
        }

		/// <summary>
		/// 指示此实例与指定对象是否相等。
		/// </summary>
		/// <param name="obj">要比较的另一个对象。</param>
		/// <returns>
		/// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 true；否则为 false。
		/// </returns>
        public override bool Equals(object obj) {
            return ((obj is Complex) && (this == ((Complex)obj)));
        }

		/// <summary>
		/// 指示此实例与指定对象是否相等。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>
		/// 如果 <paramref name="value"/> 和该实例具有相同的类型并表示相同的值，则为 true；否则为 false。
		/// </returns>
        public bool Equals(Complex value) {
            return (_real.Equals(value._real) && _imaginary.Equals(value._imaginary));
        }

		/// <summary>
		/// 实现从 <see cref="System.Int16"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator Complex(short value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.Int32"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator Complex(int value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.Int64"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator Complex(long value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.UInt16"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(ushort value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.UInt32"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(uint value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.UInt64"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(ulong value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.SByte"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(sbyte value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.Byte"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator Complex(byte value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.Single"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator Complex(float value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.Double"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的隐性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static implicit operator Complex(double value) {
            return new Complex(value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="Py.Algorithm.Numerics.BigInteger"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator Complex(BigInteger value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 实现从 <see cref="System.Decimal"/> 到 <see cref="Py.Algorithm.Numerics.Complex"/> 的显性的转换。
		/// </summary>
		/// <param name="value">值。</param>
		/// <returns>转换的结果。</returns>
        public static explicit operator Complex(decimal value) {
            return new Complex((double)value, 0.0);
        }

		/// <summary>
		/// 将当前复数的值转换为其采用笛卡尔形式的等效字符串表示形式。
		/// </summary>
		/// <returns>
		/// 当前实例的采用笛卡尔形式的字符串表示形式。
		/// </returns>
        public override string ToString() {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", _real, _imaginary );
        }

		/// <summary>
		/// 通过对当前复数的实部和虚部使用指定格式，将它的值转换为其采用笛卡尔形式的等效字符串表示形式。
		/// </summary>
		/// <param name="format">标准或自定义的数值格式字符串。</param>
		/// <returns>当前实例的采用笛卡尔形式的字符串表示形式。</returns>
		/// <exception cref="FormatException"><paramref name="format"/>  不是有效的格式字符串。</exception>
        public string ToString(string format) {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", _real.ToString(format, CultureInfo.CurrentCulture), _imaginary.ToString(format, CultureInfo.CurrentCulture) );
        }

		/// <summary>
		/// 通过对当前复数的实部和虚部使用指定格式和区域性特定格式信息，将它的值转换为其采用笛卡尔形式的等效字符串表示形式。
		/// </summary>
		/// <param name="provider">一个提供区域性特定的格式设置信息的对象。</param>
		/// <returns>由 format 和 provider 指定的当前实例的采用笛卡尔形式的字符串表示形式。</returns>
        public string ToString(IFormatProvider provider) {
            return string.Format(provider, "({0}, {1})",_real, _imaginary);
        }

		/// <summary>
		/// 通过对当前复数的实部和虚部使用指定格式和区域性特定格式信息，将它的值转换为其采用笛卡尔形式的等效字符串表示形式。
		/// </summary>
		/// <param name="format">标准或自定义的数值格式字符串。</param>
		/// <param name="provider">一个提供区域性特定的格式设置信息的对象。</param>
		/// <returns>由 format 和 provider 指定的当前实例的采用笛卡尔形式的字符串表示形式。</returns>
        public string ToString(string format, IFormatProvider provider) {
            return string.Format(provider, "({0}, {1})", _real.ToString(format, provider), _imaginary.ToString(format, provider));
        }

		/// <summary>
		/// 返回此实例的哈希代码。
		/// </summary>
		/// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public override int GetHashCode() {
            int num = 0x5f5e0fd;
            int num2 = _real.GetHashCode() % num;
            int hashCode = _imaginary.GetHashCode();
            return (num2 ^ hashCode);
        }

		/// <summary>
		/// 返回指定复数的正弦值。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>value 的正弦值。</returns>
        public static Complex Sin(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Sin(real) * Math.Cosh(imaginary), Math.Cos(real) * Math.Sinh(imaginary));
        }

		/// <summary>
		/// 返回指定复数的双曲正弦值。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>value 的双曲正弦值。</returns>
        public static Complex Sinh(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Sinh(real) * Math.Cos(imaginary), Math.Cosh(real) * Math.Sin(imaginary));
        }

		/// <summary>
		/// 返回表示指定复数的反正弦值的角度。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>表示 value 的反正弦值的角度。</returns>
        public static Complex Asin(Complex value) {
            return (-(ImaginaryOne) * Log((ImaginaryOne * value) + Sqrt(One - (value * value))));
        }

		/// <summary>
		/// 返回指定复数的双曲余弦值。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的双曲余弦值。</returns>
        public static Complex Cos(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Cos(real) * Math.Cosh(imaginary), -(Math.Sin(real) * Math.Sinh(imaginary)));
        }

		/// <summary>
		/// 返回指定复数的双曲余弦值。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的双曲余弦值。</returns>
        public static Complex Cosh(Complex value) {
            double real = value._real;
            double imaginary = value._imaginary;
            return new Complex(Math.Cosh(real) * Math.Cos(imaginary), Math.Sinh(real) * Math.Sin(imaginary));
        }

		/// <summary>
		/// 返回表示指定复数的反余弦值的角度。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的反余弦值的角度。</returns>
        public static Complex Acos(Complex value) {
            return (-(ImaginaryOne) * Log(value + (ImaginaryOne * Sqrt(One - (value * value)))));
        }

		/// <summary>
		/// 返回指定复数的正切值。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的正切值。</returns>
        public static Complex Tan(Complex value) {
            return (Sin(value) / Cos(value));
        }

		/// <summary>
		/// 返回指定复数的双曲正切值。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的双曲正切值。</returns>
        public static Complex Tanh(Complex value) {
            return (Sinh(value) / Cosh(value));
        }

		/// <summary>
		/// 返回表示指定复数的反正切值的角度。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的反正切值的角度。</returns>
        public static Complex Atan(Complex value) {
            Complex complex = new Complex(2.0, 0.0);
            return ((ImaginaryOne / complex) * (Log(One - (ImaginaryOne * value)) - Log(One + (ImaginaryOne * value))));
        }

		/// <summary>
		/// 返回指定复数的自然对数（底为 e）。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的自然对数（底为 e）。</returns>
        public static Complex Log(Complex value) {
            return new Complex(Math.Log(Abs(value)), Math.Atan2(value._imaginary, value._real));
        }

		/// <summary>
		/// 返回指定复数在使用指定底时的对数。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <param name="baseValue">进行取对数的底。</param>
		/// <returns>复数 value 在使用指定底时的对数。</returns>
        public static Complex Log(Complex value, double baseValue) {
            return (Log(value) / Log(baseValue));
        }

		/// <summary>
		/// 返回指定复数以 10 为底的对数。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 以 10 为底的对数。</returns>
        public static Complex Log10(Complex value) {
            return Scale(Log(value), 0.43429448190325);
        }

		/// <summary>
		/// 返回 e 的由一个复数指定的次幂。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns> e 的由一个复数 value 指定的次幂。</returns>
        public static Complex Exp(Complex value) {
            double num = Math.Exp(value._real);
            return new Complex(num * Math.Cos(value._imaginary), num * Math.Sin(value._imaginary));
        }

		/// <summary>
		/// 返回指定复数的平方根。
		/// </summary>
		/// <param name="value">一个复数。</param>
		/// <returns>复数 value 的平方根。</returns>
        public static Complex Sqrt(Complex value) {
            return FromPolarCoordinates(Math.Sqrt(value.Magnitude), value.Phase / 2.0);
        }

		/// <summary>
		/// 返回指定复数的由复数指定的次幂。
		/// </summary>
		/// <param name="value">要对其求幂的复数。</param>
		/// <param name="power">指定幂的双精度浮点数。</param>
		/// <returns>复数 value 的由复数指定的次幂。</returns>
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
		/// 返回指定复数的由复数指定的次幂。
		/// </summary>
		/// <param name="value">要对其求幂的复数。</param>
		/// <param name="power">指定幂的复数。</param>
		/// <returns>复数 value 的 power 次幂。</returns>
        public static Complex Pow(Complex value, double power) {
            return Pow(value, new Complex(power, 0.0));
        }

		private static Complex Scale(Complex value, double factor) {
            return new Complex(factor * value._real, factor * value._imaginary);
        }

		/// <summary>
		/// 初始化 <see cref="Py.Algorithm.Numerics.Complex"/> 的静态成员。
		/// </summary>
        static Complex() {
            Zero = new Complex(0.0, 0.0);
            One = new Complex(1.0, 0.0);
            ImaginaryOne = new Complex(0.0, 1.0);
        }
    }
}

