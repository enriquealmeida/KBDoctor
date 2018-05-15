using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concepto.Packages.KBDoctorCore.Sources
{
   /* class KBDMatrix
    {
        private readonly double[,] _matrix;

        public KBDMatrix(int dim1, int dim2)
        {
            _matrix = new double[dim1, dim2];
        }

        public int Height { get { return _matrix.GetLength(0); } }
        public int Width { get { return _matrix.GetLength(1); } }

        public double this[int x, int y]
        {
            get { return _matrix[x, y]; }
            set { _matrix[x, y] = value; }
        }


        public unsafe static KBDMatrix Multiplication(KBDMatrix m1, KBDMatrix m2)
        {
            int h = m1.Height;
            int w = m2.Width;
            int l = m1.Width;
            KBDMatrix resultMatrix = new KBDMatrix(h, w);
            unsafe
            {
                fixed (double* pm = resultMatrix._matrix, pm1 = m1._matrix, pm2 = m2._matrix)
                {
                    int i1, i2;
                    for (int i = 0; i < h; i++)
                    {
                        i1 = i * l;
                        for (int j = 0; j < w; j++)
                        {
                            i2 = j;
                            double res = 0;
                            for (int k = 0; k < l; k++, i2 += w)
                            {
                                res += pm1[i1 + k] * pm2[i2];
                            }
                            pm[i * w + j] = res;
                        }
                    }
                }
            }
            return resultMatrix;
        }
        
    }*/
}
