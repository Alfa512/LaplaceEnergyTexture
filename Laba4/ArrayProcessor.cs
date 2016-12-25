using System;
using System.Threading;

namespace LawsEnergyTexture
{
    public unsafe struct ProcessTextureMapParams
    {
        public int Number;
        public int[][,] FiltMass;
        public int StartHeight;
        public int EndHeight;
        public int StartWidth;
        public int EndWidth;
        public int Z;
        public int* Min;
        public int* Max;
        public int Mapf;
        public int Maps;

        public ProcessTextureMapParams(ProcessTextureMapParams parameters)
        {
            Number = parameters.Number;
            FiltMass = parameters.FiltMass;
            StartHeight = parameters.StartHeight;
            EndHeight = parameters.EndHeight;
            StartWidth = parameters.StartWidth;
            EndWidth = parameters.EndWidth;
            Z = parameters.Z;
            Min = parameters.Min;
            Max = parameters.Max;
            Mapf = parameters.Mapf;
            Maps = parameters.Maps;
        }
        public ProcessTextureMapParams(int number, int[][,] filtMass, int startHeight, int endHeight, int startWidth, int endWidth, int z, int* min, int* max, int mapf, int maps)
        {
            Number = number;
            FiltMass = filtMass;
            StartHeight = startHeight;
            EndHeight = endHeight;
            StartWidth = startWidth;
            EndWidth = endWidth;
            Z = z;
            Min = min;
            Max = max;
            Mapf = mapf;
            Maps = maps;
        }
    }

    unsafe class ArrayProcessor
    {
        public int ThreadCount;
        int[][,] _energyMaps;
        int[][,] _filtArray;
        int[,] _expanArray;
        int[,] _expanArrayFilter;
        int[,] _expanArrayPre;
        int[,] _workArray;
        int[,] _workArrayPre;
        Matrix[] _filters;
        int[] _minArray;
        int[] _maxArray;


        public ArrayProcessor(int threadCount)
        {
            ThreadCount = threadCount;
        }

        public void ProcessTextureMap(ref int[][,] energyMaps, int[][,] filtMass, int height, int width, int z, int* min, int* max, int mapf, int maps)
        {
            Thread[] threads = new Thread[ThreadCount];
            _minArray = new int[ThreadCount];
            _maxArray = new int[ThreadCount];
            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(0, filtMass, 0, 0, height, width, z, min, max, mapf, maps);
            _energyMaps = energyMaps;
            for (int i = 0; i < ThreadCount; i++)
            {
                if (maps < 0)
                    threads[i] = new Thread(ProcessArrayU);
                else
                    threads[i] = new Thread(ProcessArray);
                threads[i].Name = name + i;
                _minArray[i] = *min;
                _maxArray[i] = *max;
                parameters.Number = i;
                parameters.StartHeight = height / ThreadCount * i;
                parameters.EndHeight = height / ThreadCount * i + height / ThreadCount;
                if (i + 1 == ThreadCount && height % ThreadCount > 0)
                    parameters.EndHeight = height / ThreadCount * i + height / ThreadCount + height % ThreadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == ThreadCount && width % ThreadCount > 0)
                    parameters.EndWidth = width / ThreadCount * i + width / ThreadCount + width % ThreadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            foreach (var val in _minArray)
            {
                if (val < *min)
                    *min = val;
            }
            foreach (var val in _maxArray)
            {
                if (val > *max)
                    *max = val;
            }

            energyMaps = _energyMaps;
        }

        public void ImageExpansionProcess(out int[,] expanMass, ref int[,] workMass, int expPixels, int height, int width)
        {
            Thread[] threads = new Thread[ThreadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(0, null, 0, 0, height, width, expPixels, null, null, 0, 0);
            _workArray = workMass;
            for (int i = 0; i < ThreadCount; i++)
            {
                threads[i] = new Thread(ImageExpansion);
                threads[i].Name = name + i;

                parameters.Number = i;
                parameters.StartHeight = height / ThreadCount * i;
                parameters.EndHeight = height / ThreadCount * i + height / ThreadCount;
                if (i + 1 == ThreadCount && height % ThreadCount > 0)
                    parameters.EndHeight = height / ThreadCount * i + height / ThreadCount + height % ThreadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == ThreadCount && width % ThreadCount > 0)
                    parameters.EndWidth = width / ThreadCount * i + width / ThreadCount + width % ThreadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            expanMass = _expanArray;
        }

        public void PreHandlingProcess(int[,] expanArray, ref int[,] workMass, int expPixels, int height, int width)
        {
            Thread[] threads = new Thread[ThreadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(0, null, 0, 0, height, width, expPixels, null, null, 0, 0);
            _expanArrayPre = expanArray;
            _workArrayPre = workMass;
            for (int i = 0; i < ThreadCount; i++)
            {
                threads[i] = new Thread(PreHandling);
                threads[i].Name = name + i;

                parameters.Number = i;
                parameters.StartHeight = height / ThreadCount * i;
                parameters.EndHeight = height / ThreadCount * i + height / ThreadCount;
                if (i + 1 == ThreadCount && height % ThreadCount > 0)
                    parameters.EndHeight = height / ThreadCount * i + height / ThreadCount + height % ThreadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == ThreadCount && width % ThreadCount > 0)
                    parameters.EndWidth = width / ThreadCount * i + width / ThreadCount + width % ThreadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            workMass = _workArrayPre;
        }

        public void FiltrationProcess(int[,] expanArrayFilter, ref int[][,] filtMass, Matrix[] filters, int expPixels, int height, int width)
        {
            Thread[] threads = new Thread[ThreadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(0, null, 0, 0, height, width, expPixels, null, null, 0, 0);
            _expanArrayFilter = expanArrayFilter;
            _filtArray = filtMass;
            _filters = filters;
            for (int i = 0; i < ThreadCount; i++)
            {
                threads[i] = new Thread(Filtration);
                threads[i].Name = name + i;

                parameters.Number = i;
                parameters.StartHeight = height / ThreadCount * i;
                parameters.EndHeight = height / ThreadCount * i + height / ThreadCount;
                if (i + 1 == ThreadCount && height % ThreadCount > 0)
                    parameters.EndHeight = height / ThreadCount * i + height / ThreadCount + height % ThreadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == ThreadCount && width % ThreadCount > 0)
                    parameters.EndWidth = width / ThreadCount * i + width / ThreadCount + width % ThreadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            filtMass = _filtArray;
        }

        public void TextureMapProcess(int[,] expanArrayFilter, ref int[][,] filtMass, int z, int expPixels, int height, int width)
        {
            Thread[] threads = new Thread[ThreadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(0, null, 0, 0, height, width, z, null, null, expPixels, 0);
            _expanArrayFilter = expanArrayFilter;
            _filtArray = filtMass;
            for (int i = 0; i < ThreadCount; i++)
            {
                threads[i] = new Thread(TextureMap);
                threads[i].Name = name + i;

                parameters.Number = i;
                parameters.StartHeight = height / ThreadCount * i;
                parameters.EndHeight = height / ThreadCount * i + height / ThreadCount;
                if (i + 1 == ThreadCount && height % ThreadCount > 0)
                    parameters.EndHeight = height / ThreadCount * i + height / ThreadCount + height % ThreadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == ThreadCount && width % ThreadCount > 0)
                    parameters.EndWidth = width / ThreadCount * i + width / ThreadCount + width % ThreadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            filtMass = _filtArray;
        }

        void ProcessArray(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                {
                    _energyMaps[data.Z][i, j] = (data.FiltMass[data.Mapf][i, j] + (data.FiltMass[data.Maps][i, j]) >> 1);
                    if (_energyMaps[data.Z][i, j] < _minArray[data.Number])
                        _minArray[data.Number] = _energyMaps[data.Z][i, j];
                    if (_energyMaps[data.Z][i, j] > _maxArray[data.Number])
                        _maxArray[data.Number] = _energyMaps[data.Z][i, j];
                }

        }

        void ProcessArrayU(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                {
                    _energyMaps[data.Z][i, j] = (data.FiltMass[data.Mapf][i, j]) >> 1;
                    if (_energyMaps[data.Z][i, j] < _minArray[data.Number])
                        _minArray[data.Number] = _energyMaps[data.Z][i, j];
                    if (_energyMaps[data.Z][i, j] > _maxArray[data.Number])
                        _maxArray[data.Number] = _energyMaps[data.Z][i, j];
                }

        }

        void ImageExpansion(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            _expanArray = new int[data.EndHeight + (data.Z << 1), data.EndWidth + (data.Z << 1)];
            for (int i = data.StartWidth; i < data.EndWidth; i++)
                for (int j = data.StartHeight; j < data.EndHeight; j++)
                {
                    _expanArray[j + data.Z, i + data.Z] = _workArray[j, i];
                    if (i == data.StartWidth)
                    {
                        for (int z = 0; z < data.Z; z++)
                            _expanArray[j + data.Z, i + z] = _workArray[j, i];
                        if (j == data.StartHeight)
                        {
                            for (int z = 0; z < data.Z; z++)
                                for (int x = 0; x < data.Z; x++)
                                    _expanArray[j + x, i + z] = _workArray[j, i];
                        }
                        if (j == data.EndHeight - 1)
                        {
                            for (int z = 0; z < data.Z; z++)
                                for (int x = 1; x <= data.Z; x++)
                                    _expanArray[j + data.Z + x, i + z] = _workArray[j, i];
                        }
                    }
                    if (i == data.EndWidth - 1)
                    {
                        for (int z = 1; z <= data.Z; z++)
                            _expanArray[j + data.Z, i + data.Z + z] = _workArray[j, i];
                        if (j == data.StartHeight)
                        {
                            for (int z = 1; z <= data.Z; z++)
                                for (int x = 0; x < data.Z; x++)
                                    _expanArray[j + x, i + data.Z + z] = _workArray[j, i];
                        }
                        if (j == data.EndHeight - 1)
                        {
                            for (int z = 1; z <= data.Z; z++)
                                for (int x = 1; x <= data.Z; x++)
                                    _expanArray[j + data.Z + x, i + data.Z + z] = _workArray[j, i];
                        }
                    }
                    if (j == data.StartHeight)
                    {
                        for (int z = 0; z < data.Z; z++)
                            _expanArray[j + z, i + data.Z] = _workArray[j, i];
                    }
                    if (j == data.EndHeight - 1)
                    {
                        for (int z = 1; z <= data.Z; z++)
                            _expanArray[j + data.Z + z, i + data.Z] = _workArray[j, i];
                    }
                }
        }

        public int[,] ImageExpansion(int[,] workMass, int expPixels, int height, int width)
        {
            int[,] expanMass = new int[height + (expPixels << 1), width + (expPixels << 1)];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    expanMass[j + expPixels, i + expPixels] = workMass[j, i];
                    if (i == 0)
                    {
                        for (int z = 0; z < expPixels; z++)
                            expanMass[j + expPixels, i + z] = workMass[j, i];
                        if (j == 0)
                        {
                            for (int z = 0; z < expPixels; z++)
                                for (int x = 0; x < expPixels; x++)
                                    expanMass[j + x, i + z] = workMass[j, i];
                        }
                        if (j == height - 1)
                        {
                            for (int z = 0; z < expPixels; z++)
                                for (int x = 1; x <= expPixels; x++)
                                    expanMass[j + expPixels + x, i + z] = workMass[j, i];
                        }
                    }
                    if (i == width - 1)
                    {
                        for (int z = 1; z <= expPixels; z++)
                            expanMass[j + expPixels, i + expPixels + z] = workMass[j, i];
                        if (j == 0)
                        {
                            for (int z = 1; z <= expPixels; z++)
                                for (int x = 0; x < expPixels; x++)
                                    expanMass[j + x, i + expPixels + z] = workMass[j, i];
                        }
                        if (j == height - 1)
                        {
                            for (int z = 1; z <= expPixels; z++)
                                for (int x = 1; x <= expPixels; x++)
                                    expanMass[j + expPixels + x, i + expPixels + z] = workMass[j, i];
                        }
                    }
                    if (j == 0)
                    {
                        for (int z = 0; z < expPixels; z++)
                            expanMass[j + z, i + expPixels] = workMass[j, i];
                    }
                    if (j == height - 1)
                    {
                        for (int z = 1; z <= expPixels; z++)
                            expanMass[j + expPixels + z, i + expPixels] = workMass[j, i];
                    }
                }
            return expanMass;
        }

        private void PreHandling(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                {
                    int sum = 0;
                    for (int y = -data.Z; y <= data.Z; y++)
                        for (int x = -data.Z; x <= data.Z; x++)
                            sum += _expanArrayPre[i + data.Z + y, j + data.Z + x];
                    sum /= 225;
                    _workArrayPre[i, j] -= sum;
                }
        }

        private void Filtration(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                    for (int z = 0; z < 15; z++)
                    {
                        _filtArray[z][i, j] = _filters[z + 1].FindItem(_expanArrayFilter, j + data.Z, i + data.Z);
                    }
        }

        private void TextureMap(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                {
                    int sum = 0;
                    for (int y = -data.Mapf; y <= data.Mapf; y++)
                        for (int x = -data.Mapf; x <= data.Mapf; x++)
                            sum += Math.Abs(_expanArrayFilter[i + data.Mapf + y, j + data.Mapf + x]);
                    _filtArray[data.Z][i, j] = sum;
                }
        }

    }
}