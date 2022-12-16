namespace VPenExample.Fft
{
    using System;

    public static class FourierTransform
    {
        public enum Direction
        {
            Forward = 1,

            Backward = -1
        };

        public static void DFT(Complex[] data, Direction direction)
        {
            int n = data.Length;
            double arg, cos, sin;
            Complex[] dst = new Complex[n];

            for (int i = 0; i < n; i++)
            {
                dst[i] = Complex.Zero;

                arg = -(int)direction * 2.0 * System.Math.PI * (double)i / (double)n;

                for (int j = 0; j < n; j++)
                {
                    cos = System.Math.Cos(j * arg);
                    sin = System.Math.Sin(j * arg);

                    dst[i].Re += (data[j].Re * cos - data[j].Im * sin);
                    dst[i].Im += (data[j].Re * sin + data[j].Im * cos);
                }
            }

            if (direction == Direction.Forward)
            {
                for (int i = 0; i < n; i++)
                {
                    data[i].Re = dst[i].Re / n;
                    data[i].Im = dst[i].Im / n;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    data[i].Re = dst[i].Re;
                    data[i].Im = dst[i].Im;
                }
            }
        }

        public static void DFT2(Complex[,] data, Direction direction)
        {
            int n = data.GetLength(0);
            int m = data.GetLength(1);
            double arg, cos, sin;
            Complex[] dst = new Complex[System.Math.Max(n, m)];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    dst[j] = Complex.Zero;

                    arg = -(int)direction * 2.0 * System.Math.PI * (double)j / (double)m;

                    for (int k = 0; k < m; k++)
                    {
                        cos = System.Math.Cos(k * arg);
                        sin = System.Math.Sin(k * arg);

                        dst[j].Re += (data[i, k].Re * cos - data[i, k].Im * sin);
                        dst[j].Im += (data[i, k].Re * sin + data[i, k].Im * cos);
                    }
                }

                if (direction == Direction.Forward)
                {
                    for (int j = 0; j < m; j++)
                    {
                        data[i, j].Re = dst[j].Re / m;
                        data[i, j].Im = dst[j].Im / m;
                    }
                }
                else
                {
                    for (int j = 0; j < m; j++)
                    {
                        data[i, j].Re = dst[j].Re;
                        data[i, j].Im = dst[j].Im;
                    }
                }
            }

            for (int j = 0; j < m; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    dst[i] = Complex.Zero;

                    arg = -(int)direction * 2.0 * System.Math.PI * (double)i / (double)n;

                    for (int k = 0; k < n; k++)
                    {
                        cos = System.Math.Cos(k * arg);
                        sin = System.Math.Sin(k * arg);

                        dst[i].Re += (data[k, j].Re * cos - data[k, j].Im * sin);
                        dst[i].Im += (data[k, j].Re * sin + data[k, j].Im * cos);
                    }
                }

                if (direction == Direction.Forward)
                {
                    for (int i = 0; i < n; i++)
                    {
                        data[i, j].Re = dst[i].Re / n;
                        data[i, j].Im = dst[i].Im / n;
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        data[i, j].Re = dst[i].Re;
                        data[i, j].Im = dst[i].Im;
                    }
                }
            }
        }

        public static void FFT(Complex[] data, Direction direction)
        {
            int n = data.Length;
            int m = Tools.Log2(n);

            ReorderData(data);

            int tn = 1, tm;

            for (int k = 1; k <= m; k++)
            {
                Complex[] rotation = FourierTransform.GetComplexRotation(k, direction);

                tm = tn;
                tn <<= 1;

                for (int i = 0; i < tm; i++)
                {
                    Complex t = rotation[i];

                    for (int even = i; even < n; even += tn)
                    {
                        int odd = even + tm;
                        Complex ce = data[even];
                        Complex co = data[odd];

                        double tr = co.Re * t.Re - co.Im * t.Im;
                        double ti = co.Re * t.Im + co.Im * t.Re;

                        data[even].Re += tr;
                        data[even].Im += ti;

                        data[odd].Re = ce.Re - tr;
                        data[odd].Im = ce.Im - ti;
                    }
                }
            }

            if (direction == Direction.Forward)
            {
                for (int i = 0; i < n; i++)
                {
                    data[i].Re /= (double)n;
                    data[i].Im /= (double)n;
                }
            }
        }

        public static void FFT2(Complex[,] data, Direction direction)
        {
            int k = data.GetLength(0);
            int n = data.GetLength(1);

            if (
                    (!Tools.IsPowerOf2(k)) ||
                    (!Tools.IsPowerOf2(n)) ||
                    (k < minLength) || (k > maxLength) ||
                    (n < minLength) || (n > maxLength)
                    )
            {
                throw new ArgumentException("Incorrect data length.");
            }

            Complex[] row = new Complex[n];

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < n; j++)
                    row[j] = data[i, j];
                FourierTransform.FFT(row, direction);
                for (int j = 0; j < n; j++)
                    data[i, j] = row[j];
            }

            Complex[] col = new Complex[k];

            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < k; i++)
                    col[i] = data[i, j];
                FourierTransform.FFT(col, direction);
                for (int i = 0; i < k; i++)
                    data[i, j] = col[i];
            }
        }

        #region Private Region

        private const int minLength = 2;
        private const int maxLength = 1048576;
        private const int minBits = 1;
        private const int maxBits = 20;
        private static int[][] reversedBits = new int[maxBits][];
        private static Complex[,][] complexRotation = new Complex[maxBits, 2][];

        private static int[] GetReversedBits(int numberOfBits)
        {
            if ((numberOfBits < minBits) || (numberOfBits > maxBits))
                throw new ArgumentOutOfRangeException();

            if (reversedBits[numberOfBits - 1] == null)
            {
                int n = Tools.Pow2(numberOfBits);
                int[] rBits = new int[n];

                for (int i = 0; i < n; i++)
                {
                    int oldBits = i;
                    int newBits = 0;

                    for (int j = 0; j < numberOfBits; j++)
                    {
                        newBits = (newBits << 1) | (oldBits & 1);
                        oldBits = (oldBits >> 1);
                    }
                    rBits[i] = newBits;
                }
                reversedBits[numberOfBits - 1] = rBits;
            }
            return reversedBits[numberOfBits - 1];
        }

        private static Complex[] GetComplexRotation(int numberOfBits, Direction direction)
        {
            int directionIndex = (direction == Direction.Forward) ? 0 : 1;

            if (complexRotation[numberOfBits - 1, directionIndex] == null)
            {
                int n = 1 << (numberOfBits - 1);
                double uR = 1.0;
                double uI = 0.0;
                double angle = System.Math.PI / n * (int)direction;
                double wR = System.Math.Cos(angle);
                double wI = System.Math.Sin(angle);
                double t;
                Complex[] rotation = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    rotation[i] = new Complex(uR, uI);
                    t = uR * wI + uI * wR;
                    uR = uR * wR - uI * wI;
                    uI = t;
                }

                complexRotation[numberOfBits - 1, directionIndex] = rotation;
            }
            return complexRotation[numberOfBits - 1, directionIndex];
        }

        private static void ReorderData(Complex[] data)
        {
            int len = data.Length;

            if ((len < minLength) || (len > maxLength) || (!Tools.IsPowerOf2(len)))
                throw new ArgumentException("Incorrect data length.");

            int[] rBits = GetReversedBits(Tools.Log2(len));

            for (int i = 0; i < len; i++)
            {
                int s = rBits[i];

                if (s > i)
                {
                    Complex t = data[i];
                    data[i] = data[s];
                    data[s] = t;
                }
            }
        }

        #endregion Private Region
    }
}