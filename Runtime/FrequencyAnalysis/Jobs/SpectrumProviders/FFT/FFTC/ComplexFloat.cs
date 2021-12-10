// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using Unity.Burst;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Audio
{

    [BurstCompile]
    public struct ComplexFloat : IEquatable<ComplexFloat>
    {

        public const float LOG_10_INV = 0.43429448190325f;

        public float real;

        public float imaginary;

        public float magnitude{ get{ return ComplexFloat.Abs(this); } }

        public float phase{ get{ return atan2(imaginary, real); } }

        
        public static readonly ComplexFloat zero = new ComplexFloat(0f, 0f);
        public static readonly ComplexFloat one = new ComplexFloat(1f, 0f);
        public static readonly ComplexFloat imaginaryOne = new ComplexFloat(0f, 1f);


        #region Constructor & factory

        public ComplexFloat(float re, float im) 
        {
            real = re;
            imaginary = im;
        }

        public ComplexFloat(float re)
        {
            real = re;
            imaginary = 0f;
        }

        public static ComplexFloat FromPolarCoordinates(float magnitude, float phase) 
        {
            return new ComplexFloat((magnitude * cos(phase)), (magnitude * sin(phase)));
        }

        public static ComplexFloat Negate(ComplexFloat value){ return -value; }

        public static ComplexFloat Add(ComplexFloat left, ComplexFloat right){ return left + right; }

        public static ComplexFloat Subtract(ComplexFloat left, ComplexFloat right){ return left - right; }

        public static ComplexFloat Multiply(ComplexFloat left, ComplexFloat right){ return left * right; }

        public static ComplexFloat Divide(ComplexFloat dividend, ComplexFloat divisor){ return dividend / divisor; }

        #endregion

        #region Arithmetic

        public static ComplexFloat operator -(ComplexFloat value) 
        {

            return (new ComplexFloat((-value.real), (-value.imaginary)));
        }
                
        public static ComplexFloat operator +(ComplexFloat left, ComplexFloat right)
        {
            return (new ComplexFloat((left.real + right.real), (left.imaginary + right.imaginary)));
        }

        public static ComplexFloat operator -(ComplexFloat left, ComplexFloat right)
        {
            return (new ComplexFloat((left.real - right.real), (left.imaginary - right.imaginary)));
        }

        public static ComplexFloat operator *(ComplexFloat left, ComplexFloat right)
        {
            float result_Realpart = (left.real * right.real) - (left.imaginary * right.imaginary);
            float result_Imaginarypart = (left.imaginary * right.real) + (left.real * right.imaginary);
            return (new ComplexFloat(result_Realpart, result_Imaginarypart));
        }

        public static ComplexFloat operator /(ComplexFloat left, ComplexFloat right)
        {
            float a = left.real;
            float b = left.imaginary;
            float c = right.real;
            float d = right.imaginary;

            if (abs(d) < abs(c))
            {
                float doc = d / c;
                return new ComplexFloat((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
            }
            else
            {
                float cod = c / d;
                return new ComplexFloat((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
            }
        }

        public static float Abs(ComplexFloat value)
        {
            
            if (isinf(value.real) || isinf(value.imaginary))
                return float.PositiveInfinity;

            float c = abs(value.real);
            float d = abs(value.imaginary);

            if (c > d)
            {
                float r = d / c;
                return c * sqrt(1f + r * r);
            }
            else if (d == 0.0)
            {
                return c;
            }
            else
            {
                float r = c / d;
                return d * sqrt(1f + r * r);
            }
        }

        public static ComplexFloat Conjugate(ComplexFloat value) { return new ComplexFloat(value.real, -value.imaginary); }

        public static ComplexFloat Reciprocal(ComplexFloat value) { return (value.real == 0) && (value.imaginary == 0) ? ComplexFloat.zero : ComplexFloat.one / value; }

        #endregion

        #region Comparison

        public static bool operator ==(ComplexFloat left, ComplexFloat right) { return (left.real == right.real) && (left.imaginary == right.imaginary); }

        public static bool operator !=(ComplexFloat left, ComplexFloat right) { return (left.real != right.real) || (left.imaginary != right.imaginary); }

        public override bool Equals(object obj) { return (!(obj is ComplexFloat)) ? false : this == ((ComplexFloat)obj);  }

        public bool Equals(ComplexFloat value) { return this.real.Equals(value.real) && this.imaginary.Equals(value.imaginary); }

        public override int GetHashCode()
        {
            int n1 = 99999997;
            int hash_real = this.real.GetHashCode() % n1;
            int hash_imaginary = this.imaginary.GetHashCode();
            int final_hashcode = hash_real ^ hash_imaginary;
            return (final_hashcode);
        }

        #endregion

        #region Type-casting

        public static implicit operator ComplexFloat(short value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(int value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(long value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(ushort value) { return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(uint value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(ulong value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(sbyte value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(byte value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(float value){ return new ComplexFloat(value); }
        public static implicit operator ComplexFloat(double value){ return new ComplexFloat((float)value); }
        public static explicit operator ComplexFloat(decimal value){ return new ComplexFloat((float)value); }

        #endregion

        #region Trigonometric ops

        public static ComplexFloat Sin(ComplexFloat value)
        {
            float a = value.real;
            float b = value.imaginary;
            return new ComplexFloat(sin(a) * cosh(b), cos(a) * sinh(b));
        }

        public static ComplexFloat Sinh(ComplexFloat value)
        {
            float a = value.real;
            float b = value.imaginary;
            return new ComplexFloat(sinh(a) * cos(b), cosh(a) * sin(b));

        }

        public static ComplexFloat Asin(ComplexFloat value)
        {
            return (-imaginaryOne) * Log(imaginaryOne * value + Sqrt(one - value * value));
        }

        public static ComplexFloat Cos(ComplexFloat value)
        {
            float a = value.real;
            float b = value.imaginary;
            return new ComplexFloat(cos(a) * cosh(b), -(sin(a) * sinh(b)));
        }

        public static ComplexFloat Cosh(ComplexFloat value)
        {
            float a = value.real;
            float b = value.imaginary;
            return new ComplexFloat(cosh(a) * cos(b), sinh(a) * sin(b));
        }

        public static ComplexFloat Acos(ComplexFloat value)
        {
            return (-imaginaryOne) * Log(value + imaginaryOne * Sqrt(one - (value * value)));

        }

        public static ComplexFloat Tan(ComplexFloat value)
        {
            return (Sin(value) / Cos(value));
        }

        public static ComplexFloat Tanh(ComplexFloat value)
        {
            return (Sinh(value) / Cosh(value));
        }

        public static ComplexFloat Atan(ComplexFloat value)
        {
            ComplexFloat Two = new ComplexFloat(2f, 0f);
            return (imaginaryOne / Two) * (Log(one - imaginaryOne * value) - Log(one + imaginaryOne * value));
        }

        #endregion

        #region Other numerical functions

        public static ComplexFloat Log(ComplexFloat value)
        {
            return (new ComplexFloat((log(Abs(value))), (atan2(value.imaginary, value.real))));

        }
        public static ComplexFloat Log(ComplexFloat value, float baseValue)
        {
            return (Log(value) / Log(baseValue));
        }
        public static ComplexFloat Log10(ComplexFloat value)
        {

            ComplexFloat temp_log = Log(value);
            return (Scale(temp_log, (float)LOG_10_INV));

        }
        public static ComplexFloat Exp(ComplexFloat value)
        {
            float temp_factor = exp(value.real);
            float result_re = temp_factor * cos(value.imaginary);
            float result_im = temp_factor * sin(value.imaginary);
            return (new ComplexFloat(result_re, result_im));
        }

        public static ComplexFloat Sqrt(ComplexFloat value)
        {
            return ComplexFloat.FromPolarCoordinates(sqrt(value.magnitude), value.phase / 2f);
        }

        public static ComplexFloat Pow(ComplexFloat value, ComplexFloat power)
        {

            if (power == ComplexFloat.zero)
            {
                return ComplexFloat.one;
            }

            if (value == ComplexFloat.zero)
            {
                return ComplexFloat.zero;
            }

            float a = value.real;
            float b = value.imaginary;
            float c = power.real;
            float d = power.imaginary;

            float rho = ComplexFloat.Abs(value);
            float theta = atan2(b, a);
            float newRho = c * theta + d * log(rho);

            float t = pow(rho, c) * pow(math.E, -d * theta);

            return new ComplexFloat(t * cos(newRho), t * sin(newRho));
        }

        public static ComplexFloat Pow(ComplexFloat value, float power)
        {
            return Pow(value, new ComplexFloat(power, 0));
        }

        #endregion

        #region Internal maths

        private static ComplexFloat Scale(ComplexFloat value, float factor)
        {

            float result_re = factor * value.real;
            float result_im = factor * value.imaginary;
            return (new ComplexFloat(result_re, result_im));
        }

        #endregion

    }
}