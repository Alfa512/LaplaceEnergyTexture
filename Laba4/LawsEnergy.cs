using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

// compile with: /unsafe

namespace LawsEnergyTexture
{
    public static unsafe class LawsEnergy
    {
        static Stopwatch timer = new Stopwatch();

        static int threads = 2;
        static ArrayProcessor _arrayProcessor = new ArrayProcessor(threads);
        //рабочий массив    
        static int[,] work_mass;
        //изображение на данной стадии
        static Bitmap image;
        static Bitmap temp_image;
        //ширина/высота
        static int Width = 0, Height = 0;
        //фильтры
        static Matrix[] filters = new Matrix[16];
        //для 15 фильтрованных изображений
        static int[][,] filt_mass = new int[15][,];
        //девять энерго-карт
        static int[][,] energy_maps = new int[9][,];
        //векторы
        static Vectors[] vect = new Vectors[4];
        //коэффициенты
        static double[] coef = new double[9];
        //коэффициенты для полутона
        static double R_coeff = 0.299, Gcoeff = 0.587, Bcoeff = 0.114;

        public static Bitmap Image
        {
            get { return image; }
            set { image = value; }
        }
        public static Bitmap Temp_image
        {
            get { return temp_image; }
            set { temp_image = value; }
        }





        //инициация
        public static void Init(string File_name)
        {
            //загружаем изображение
            image = new Bitmap(File_name);
            temp_image = new Bitmap(File_name);
            //ширина/высота для более быстрого доступа
            Width = image.Width;
            Height = image.Height;
            //создаем рабочий массив
            work_mass = new int[Height, Width];
            //по всему изображению
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    //обрабатываем цвет пикселя
                    Color Color = image.GetPixel(i, j);
                    //int New_color = (int)Math.Round(R_coeff * Color.R + Gcoeff * Color.G + Bcoeff * Color.B);
                    int New_color = (int)(R_coeff * Color.R + Gcoeff * Color.G + Bcoeff * Color.B);
                    //для массива
                    work_mass[j, i] = New_color;
                }
            //инициируем
            for (int i = 0; i < 15; i++)
                filt_mass[i] = new int[Height, Width];
            for (int i = 0; i < 9; i++)
                energy_maps[i] = new int[Height, Width];
            //векторы
            for (int i = 0; i < 4; i++)
                vect[i] = new Vectors(i);
            //фильтры НУЛЕВОЙ НЕ ИСПОЛЬЗУЕТСЯ!!!
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
			        filters[i * 4 + j] = new Matrix(vect[i], vect[j]);
        }


        //расчет евклидова расстояния
        static double Euclidean_dist(int y1, int x1, int y2, int x2)
        {
            double val = 0;
            //квадраты разностей
            for (int i = 0; i < 9; i++)
                val += Math.Pow((energy_maps[i][y1, x1] - energy_maps[i][y2, x2]) * coef[i], 2);
            val = Math.Sqrt(val / 9);
            return val;
        }

        // compile with: /unsafe
        

        //расчет
        public static void Сalculation()
        {
            int ce = 7;
            //расширение
            var timer = new Stopwatch();
            timer.Start();
            int[,] exp_mass = _arrayProcessor.ImageExpansion(work_mass, ce, Height, Width);
            timer.Stop();
            //_arrayProcessor.ImageExpansionProcess(out exp_mass, ref work_mass, ce, Height, Width);

            //предварительная обработка
            _arrayProcessor.PreHandlingProcess(exp_mass, ref work_mass, ce, Height, Width);
            /*for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    int sum = 0;
                    for (int y = -ce; y <= ce; y++)
                        for (int x = -ce; x <= ce; x++)
                            sum += exp_mass[i + ce + y, j + ce + x];
                    sum /= 225;
                    work_mass[i, j] -= sum;
                }*/
            ce = 2;
            //расширение
            //exp_mass = ImageExpansion(work_mass, ce);
            //_arrayProcessor.ImageExpansionProcess(out exp_mass, ref work_mass, ce, Height, Width);
            timer.Start();
            exp_mass = _arrayProcessor.ImageExpansion(work_mass, ce, Height, Width);
            timer.Stop();
            //фильтрация
            /*for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width ; j++)
                    for (int z = 0; z < 15; z++)
                    {
                        filt_mass[z][i, j] = filters[z + 1].FindeValue(exp_mass, j + ce, i + ce);
                    }*/
            _arrayProcessor.FiltrationProcess(exp_mass, ref filt_mass, filters, ce, Height, Width);
            ce = 7;
            //текстурные карты
            for (int z = 0; z < 15; z++)
            {
                //расширение для каждой
                //exp_mass = ImageExpansion(filt_mass[z], ce);
                //_arrayProcessor.ImageExpansionProcess(out exp_mass, ref filt_mass[z], ce, Height, Width);
                timer.Start();
                exp_mass = _arrayProcessor.ImageExpansion(work_mass, ce, Height, Width);
                timer.Stop();
                for (int i = 0; i < Height; i++)
                    for (int j = 0; j < Width; j++)
                    {
                        int sum = 0;
                        for (int y = -ce; y <= ce; y++)
                            for (int x = -ce; x <= ce; x++)
                                sum += Math.Abs(exp_mass[i + ce + y, j + ce + x]);
                        filt_mass[z][i, j] = sum;
                    }
            }
            var t = timer.ElapsedMilliseconds;
            int[] min = new int[9];
            for (int i = 0; i < 9; i++)
                min[i] = int.MaxValue;
            int[] max = new int[9];
            //окончательный набор карт
            for (int z = 0; z < 9; z++)
            {
                switch (z)
                {
                    case 0:
                        /*timer.Start();
                        for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = (filt_mass[0][i, j] + filt_mass[3][i, j]) / 2;
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }
                        timer.Stop();*/
                        var a = timer.ElapsedMilliseconds;
                        timer.Start();
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 0, 3);
                        }
                        timer.Stop();
                        var b = timer.ElapsedMilliseconds;
                        
                        timer.Reset();
                        break;
                    case 1:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = (filt_mass[1][i, j] + filt_mass[7][i, j]) / 2;
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 1, 7);
                        }
                        break;
                    case 2:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = (filt_mass[2][i, j] + filt_mass[11][i, j]) / 2;
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 2, 11);
                        }
                        break;
                    case 3:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = (filt_mass[5][i, j] + filt_mass[8][i, j]) / 2;
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 5, 8);
                        }
                        break;
                    case 4:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = (filt_mass[6][i, j] + filt_mass[12][i, j]) / 2;
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 6, 12);
                        }
                        break;
                    case 5:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = (filt_mass[10][i, j] + filt_mass[13][i, j]) / 2;
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 10, 13);
                        }
                        break;
                    case 6:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = filt_mass[4][i, j];
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 4, -1);
                        }
                        break;
                    case 7:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = filt_mass[9][i, j];
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 9, -1);
                        }
                        break;
                    case 8:
                        /*for (int i = 0; i < Height; i++)
                            for (int j = 0; j < Width; j++)
                            {
                                energy_maps[z][i, j] = filt_mass[14][i, j];
                                if (energy_maps[z][i, j] < min[z])
                                    min[z] = energy_maps[z][i, j];
                                if (energy_maps[z][i, j] > max[z])
                                    max[z] = energy_maps[z][i, j];
                            }*/
                        fixed (int* minv = &min[z], maxv = &max[z])
                        {
                            _arrayProcessor.ProcessTextureMap(ref energy_maps, filt_mass, Height, Width, z, minv, maxv, 14, -1);
                        }
                        break;
                }
            }

            for (int i = 0; i < 9; i++)
                coef[i] = 1.0 / (max[i] - min[i]);

            //кластеризация
            List<Segment> seg = new List<Segment>();
            int R = 0;
            int G = 0;
            int B = 0;
            seg.Add(new Segment(0, 0, R, G, B));
            image.SetPixel(0, 0, Color.FromArgb(R, G, B));
            R += 5;
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    bool new_class = true;
                    //подсчет расстояния ко всем представителям
                    for (int z = 0; z < seg.Count; z++)
                        //если принадлежит - закрашиваем
                        if (Euclidean_dist(i, j, seg[z].Y, seg[z].X) < 0.05)
                        {
                            new_class = false;
                            temp_image.SetPixel(j, i, Color.FromArgb(seg[z].R, seg[z].G, seg[z].B));
                            break;
                        }
                    //если не вошел ни в один класс - новый класс
                    if (new_class)
                    {
                        seg.Add(new Segment(j, i, R, G, B));
                        temp_image.SetPixel(j, i, Color.FromArgb(R, G, B));
                        if (R < 254)
                            R += 5;
                        else if (G < 254)
                        {
                            R = 0;
                            G += 5;
                        }
                        else
                        {
                            R = 0;
                            G = 0;
                            B += 5;
                        }
                    }
                }

            //обработка
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (temp_image.GetPixel(j, i) == Color.FromArgb(seg[0].R, seg[0].G, seg[0].B))
                        image.SetPixel(j, i, Color.White);
        }

    }
}
