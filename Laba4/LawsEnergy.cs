using System;
using System.Collections.Generic;
using System.Drawing;

namespace LawsEnergyTexture
{
    public static unsafe class LawsEnergy
    {
        public static int Threads = 6;
        static ArrayProcessor _arrayProcessor = new ArrayProcessor(Threads);
        //рабочий массив    
        static int[,] _workArray;
        //изображение на данной стадии
        static Bitmap _image;
        static Bitmap _tempImage;
        //ширина/высота
        public static int Width, Height;
        //фильтры
        static readonly Matrix[] Filters = new Matrix[16];
        //для 15 фильтрованных изображений
        static int[][,] _filtArray = new int[15][,];
        //девять энерго-карт
        static int[][,] _energyArray = new int[9][,];
        //векторы
        static readonly Vectors[] Vect = new Vectors[4];
        //коэффициенты
        static readonly double[] Coef = new double[9];
        //коэффициенты для полутона
        static readonly double rCoeff = 0.299, gCoeff = 0.587, bCoeff = 0.114;

        public static Bitmap Image
        {
            get { return _image; }
            set { _image = value; }
        }
        public static Bitmap Temp_image
        {
            get { return _tempImage; }
            set { _tempImage = value; }
        }

        public static void SetThreads(int threads)
        {
            Threads = threads;
            _arrayProcessor.ThreadCount = threads;
        }

        //инициация
        public static void Init(string fileName)
        {
            //загружаем изображение
            _image = new Bitmap(fileName);
            _tempImage = new Bitmap(fileName);
            //ширина/высота для более быстрого доступа
            Width = _image.Width;
            Height = _image.Height;
            //создаем рабочий массив
            _workArray = new int[Height, Width];
            //по всему изображению
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    //обрабатываем цвет пикселя
                    Color color = _image.GetPixel(i, j);
                    int newColor = (int)(rCoeff * color.R + gCoeff * color.G + bCoeff * color.B);
                    //для массива
                    _workArray[j, i] = newColor;
                }
            //инициируем
            for (int i = 0; i < 15; i++)
                _filtArray[i] = new int[Height, Width];
            for (int i = 0; i < 9; i++)
                _energyArray[i] = new int[Height, Width];
            //векторы
            for (int i = 0; i < 4; i++)
                Vect[i] = new Vectors(i);
            //фильтры НУЛЕВОЙ НЕ ИСПОЛЬЗУЕТСЯ!!!
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
			        Filters[(i << 2) + j] = new Matrix(Vect[i], Vect[j]);
        }


        //расчет евклидова расстояния
        static double Euclidean_dist(int y1, int x1, int y2, int x2)
        {
            double val = 0;
            //квадраты разностей
            for (int i = 0; i < 9; i++)
                val += Math.Pow((_energyArray[i][y1, x1] - _energyArray[i][y2, x2]) * Coef[i], 2);
            val = Math.Sqrt(val / 9);
            return val;
        }

        public static void Processing()
        {
            int ce = 7;
            int[,] expMass = _arrayProcessor.ImageExpansion(_workArray, ce, Height, Width);

            _arrayProcessor.PreHandlingProcess(expMass, ref _workArray, ce, Height, Width);
            ce = 2;
            expMass = _arrayProcessor.ImageExpansion(_workArray, ce, Height, Width);
            _arrayProcessor.FiltrationProcess(expMass, ref _filtArray, Filters, ce, Height, Width);
            ce = 7;

            for (int z = 0; z < 15; z++)
            {
                expMass = _arrayProcessor.ImageExpansion(_workArray, ce, Height, Width);

                _arrayProcessor.TextureMapProcess(expMass, ref _filtArray, z, ce, Height, Width);
            }

            int[] min = new int[9];
            for (int i = 0; i < 9; i++)
                min[i] = int.MaxValue;
            int[] max = new int[9];

            for (int z = 0; z < 9; z++)
            {
                switch (z)
                {
                    case 0:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 0, 3);
                        }
                        break;
                    case 1:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 1, 7);
                        }
                        break;
                    case 2:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 2, 11);
                        }
                        break;
                    case 3:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 5, 8);
                        }
                        break;
                    case 4:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 6, 12);
                        }
                        break;
                    case 5:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 10, 13);
                        }
                        break;
                    case 6:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 4, -1);
                        }
                        break;
                    case 7:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 9, -1);
                        }
                        break;
                    case 8:
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref _energyArray, _filtArray, Height, Width, z, minv, maxv, 14, -1);
                        }
                        break;
                }
            }

            for (int i = 0; i < 9; i++)
                Coef[i] = 1.0 / (max[i] - min[i]);

            List<Segment> seg = new List<Segment>();
            byte r = 0;
            byte g = 0;
            byte b = 0;
            seg.Add(new Segment(0, 0, r, g, b));
            _image.SetPixel(0, 0, Color.FromArgb(r, g, b));
            r += 5;
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    bool newClass = true;
                    for (int z = 0; z < seg.Count; z++)
                        if (Euclidean_dist(i, j, seg[z].Y, seg[z].X) < 0.05)
                        {
                            newClass = false;
                            _tempImage.SetPixel(j, i, Color.FromArgb(seg[z].R, seg[z].G, seg[z].B));
                            break;
                        }
                    if (newClass)
                    {
                        seg.Add(new Segment(j, i, r, g, b));
                        _tempImage.SetPixel(j, i, Color.FromArgb(r, g, b));
                        if (r < 254)
                            r += 5;
                        else if (g < 254)
                        {
                            r = 0;
                            g += 5;
                        }
                        else
                        {
                            r = 0;
                            g = 0;
                            b += 5;
                        }
                    }
                }

            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (_tempImage.GetPixel(j, i) == Color.FromArgb(seg[0].R, seg[0].G, seg[0].B))
                        _image.SetPixel(j, i, Color.White);
        }

    }
}
