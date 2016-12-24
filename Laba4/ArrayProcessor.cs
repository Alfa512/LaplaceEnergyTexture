using System.Threading;

namespace LaplaceEnergyTexture
{
    public unsafe struct ProcessTextureMapParams
    {
        //public int[][,] EnergyMaps;
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
            //EnergyMaps = parameters.EnergyMaps;
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
        public ProcessTextureMapParams(/*int[][,] energyMaps, */int[][,] filtMass, int startHeight, int endHeight, int startWidth, int endWidth, int z, int* min, int* max, int mapf, int maps)
        {
            //EnergyMaps = energyMaps;
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


        int _threadCount = 2;
        int[][,] _energyMaps;
        int[][,] _filtArray;
        int[,] _expanArray;
        int[,] _expanArrayFilter;
        int[,] _expanArrayPre;
        int[,] _workArray;
        int[,] _workArrayPre;
        Matrix[] _filters;


        public ArrayProcessor(int threadCount)
        {
            _threadCount = threadCount;
        }

        public void ProcessTextureMap(ref int[][,] energyMaps, int[][,] filtMass, int height, int width, int z, int* min, int* max, int mapf, int maps)
        {
            Thread[] threads = new Thread[_threadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(filtMass, 0, 0, height, width, z, min, max, mapf, maps);
            _energyMaps = energyMaps;
            for (int i = 0; i < _threadCount; i++)
            {
                if (maps < 0)
                    threads[i] = new Thread(ProcessArrayU);
                else
                    threads[i] = new Thread(ProcessArray);
                threads[i].Name = name + i;

                parameters.StartHeight = height / _threadCount * i;
                parameters.EndHeight = height / _threadCount * i + height / _threadCount;
                if (i + 1 == _threadCount && height % _threadCount > 0)
                    parameters.EndHeight = height / _threadCount * i + height / _threadCount + height % _threadCount;

                //parameters.StartWidth= width / _threadCount * i;
                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                //parameters.EndWidth= width / _threadCount * i + width / _threadCount;
                if (i + 1 == _threadCount && width % _threadCount > 0)
                    parameters.EndWidth = width / _threadCount * i + width / _threadCount + width % _threadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            energyMaps = _energyMaps;
        }

        public void ImageExpansionProcess(out int[,] expanMass, ref int[,] workMass, int ce, int height, int width)
        {
            Thread[] threads = new Thread[_threadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(null, 0, 0, height, width, ce, null, null, 0, 0);
            //_expanArray = expanMass;
            _workArray = workMass;
            for (int i = 0; i < _threadCount; i++)
            {
                //if (maps < 0)
                threads[i] = new Thread(ImageExpansion);
                //else
                // threads[i] = new Thread(ProcessArray);
                threads[i].Name = name + i;

                parameters.StartHeight = height / _threadCount * i;
                parameters.EndHeight = height / _threadCount * i + height / _threadCount;
                if (i + 1 == _threadCount && height % _threadCount > 0)
                    parameters.EndHeight = height / _threadCount * i + height / _threadCount + height % _threadCount;

                //parameters.StartWidth= width / _threadCount * i;
                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                //parameters.EndWidth= width / _threadCount * i + width / _threadCount;
                if (i + 1 == _threadCount && width % _threadCount > 0)
                    parameters.EndWidth = width / _threadCount * i + width / _threadCount + width % _threadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            expanMass = _expanArray;
        }

        public void PreHandlingProcess(int[,] expanArray, ref int[,] workMass, int ce, int height, int width) //int[,] expanMass, ref int[,] workMass, int ce, int height, int width
        {
            Thread[] threads = new Thread[_threadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(null, 0, 0, height, width, ce, null, null, 0, 0);
            _expanArrayPre= expanArray;
            _workArrayPre = workMass;
            for (int i = 0; i < _threadCount; i++)
            {
                threads[i] = new Thread(PreHandling);
                threads[i].Name = name + i;

                parameters.StartHeight = height / _threadCount * i;
                parameters.EndHeight = height / _threadCount * i + height / _threadCount;
                if (i + 1 == _threadCount && height % _threadCount > 0)
                    parameters.EndHeight = height / _threadCount * i + height / _threadCount + height % _threadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == _threadCount && width % _threadCount > 0)
                    parameters.EndWidth = width / _threadCount * i + width / _threadCount + width % _threadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            workMass = _workArrayPre;
        }
        public void FiltrationProcess(int[,] expanArrayFilter, ref int[][,] filt_mass, Matrix[] filters, int ce, int height, int width)
        {
            Thread[] threads = new Thread[_threadCount];

            string name = "ProcessTextureMap_";
            ProcessTextureMapParams parameters = new ProcessTextureMapParams(null, 0, 0, height, width, ce, null, null, 0, 0);
            _expanArrayFilter = expanArrayFilter;
            _filtArray= filt_mass;
            _filters = filters;
            for (int i = 0; i < _threadCount; i++)
            {
                threads[i] = new Thread(Filtration);
                threads[i].Name = name + i;

                parameters.StartHeight = height / _threadCount * i;
                parameters.EndHeight = height / _threadCount * i + height / _threadCount;
                if (i + 1 == _threadCount && height % _threadCount > 0)
                    parameters.EndHeight = height / _threadCount * i + height / _threadCount + height % _threadCount;

                parameters.StartWidth = 0;
                parameters.EndWidth = width;
                if (i + 1 == _threadCount && width % _threadCount > 0)
                    parameters.EndWidth = width / _threadCount * i + width / _threadCount + width % _threadCount;

                threads[i].Start(parameters);
            }
            foreach (var e in threads)
                e.Join();

            filt_mass = _filtArray;
        }

        void ProcessArray(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                {
                    _energyMaps[data.Z][i, j] = (data.FiltMass[data.Mapf][i, j] + data.FiltMass[data.Maps][i, j]) / 2;
                    if (_energyMaps[data.Z][i, j] < *data.Min)
                        *data.Min = _energyMaps[data.Z][i, j];
                    if (_energyMaps[data.Z][i, j] > *data.Max)
                        *data.Max = _energyMaps[data.Z][i, j];
                }

        }

        void ProcessArrayU(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            for (int i = data.StartHeight; i < data.EndHeight; i++)
                for (int j = data.StartWidth; j < data.EndWidth; j++)
                {
                    _energyMaps[data.Z][i, j] = (data.FiltMass[data.Mapf][i, j]) / 2;
                    if (_energyMaps[data.Z][i, j] < *data.Min)
                        *data.Min = _energyMaps[data.Z][i, j];
                    if (_energyMaps[data.Z][i, j] > *data.Max)
                        *data.Max = _energyMaps[data.Z][i, j];
                }

        }

        //искуственное расширение для фильтра
        void ImageExpansion(object parameters)
        {
            var data = new ProcessTextureMapParams((ProcessTextureMapParams)parameters);
            _expanArray = new int[data.EndHeight + (data.Z << 1), data.EndWidth + (data.Z << 1)];
            //переписываем из массива
            for (int i = data.StartWidth; i < data.EndWidth; i++)
                for (int j = data.StartHeight; j < data.EndHeight; j++)
                {
                    //для массива
                    _expanArray[j + data.Z, i + data.Z] = _workArray[j, i];
                    //если края - расширяем на массив
                    if (i == data.StartWidth)
                    {
                        for (int z = 0; z < data.Z; z++)
                            _expanArray[j + data.Z, i + z] = _workArray[j, i];
                        if (j == data.StartHeight)
                        {
                            //+ угол левый верхний
                            for (int z = 0; z < data.Z; z++)
                                for (int x = 0; x < data.Z; x++)
                                    _expanArray[j + x, i + z] = _workArray[j, i];
                        }
                        if (j == data.EndHeight - 1)
                        {
                            //+ угол левый нижний
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
                            //+ угол правый верхний
                            for (int z = 1; z <= data.Z; z++)
                                for (int x = 0; x < data.Z; x++)
                                    _expanArray[j + x, i + data.Z + z] = _workArray[j, i];
                        }
                        if (j == data.EndHeight - 1)
                        {
                            //+ угол правый нижний
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
            //return expanMass;
        }

        //искуственное расширение для фильтра
        public int[,] ImageExpansion(int[,] work_mass, int ce, int Height, int Width)
        {
            int[,] expanMass = new int[Height + (ce << 1), Width + (ce << 1)];
            //переписываем из массива
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    //для массива
                    expanMass[j + ce, i + ce] = work_mass[j, i];
                    //если края - расширяем на массив
                    if (i == 0)
                    {
                        for (int z = 0; z < ce; z++)
                            expanMass[j + ce, i + z] = work_mass[j, i];
                        if (j == 0)
                        {
                            //+ угол левый верхний
                            for (int z = 0; z < ce; z++)
                                for (int x = 0; x < ce; x++)
                                    expanMass[j + x, i + z] = work_mass[j, i];
                        }
                        if (j == Height - 1)
                        {
                            //+ угол левый нижний
                            for (int z = 0; z < ce; z++)
                                for (int x = 1; x <= ce; x++)
                                    expanMass[j + ce + x, i + z] = work_mass[j, i];
                        }
                    }
                    if (i == Width - 1)
                    {
                        for (int z = 1; z <= ce; z++)
                            expanMass[j + ce, i + ce + z] = work_mass[j, i];
                        if (j == 0)
                        {
                            //+ угол правый верхний
                            for (int z = 1; z <= ce; z++)
                                for (int x = 0; x < ce; x++)
                                    expanMass[j + x, i + ce + z] = work_mass[j, i];
                        }
                        if (j == Height - 1)
                        {
                            //+ угол правый нижний
                            for (int z = 1; z <= ce; z++)
                                for (int x = 1; x <= ce; x++)
                                    expanMass[j + ce + x, i + ce + z] = work_mass[j, i];
                        }
                    }
                    if (j == 0)
                    {
                        for (int z = 0; z < ce; z++)
                            expanMass[j + z, i + ce] = work_mass[j, i];
                    }
                    if (j == Height - 1)
                    {
                        for (int z = 1; z <= ce; z++)
                            expanMass[j + ce + z, i + ce] = work_mass[j, i];
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
                        _filtArray[z][i, j] = _filters[z + 1].FindeValue(_expanArrayFilter, j + data.Z, i + data.Z);
                    }
        }



    }
}