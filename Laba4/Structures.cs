namespace LawsEnergyTexture
{
    public class Matrix
    {
        int[,] matrix = new int[5, 5];

        public Matrix(Vectors v1, Vectors v2)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    matrix[i, j] = v1.vect[i] * v2.vect[j];
        }

        public int FindeValue(int[,] mass, int x, int y)
        {
            int val = 0;
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    val += matrix[i, j] * mass[y - 2 + i, x - 2 + j];
            return val;
        }
    }

    public class Vectors
    {
        public int[] vect { get; set; }

        public Vectors(int num)
        {
            switch (num)
            {
                case 0:
                    vect = new int[5] { 1, 4, 6, 4, 1 };
                    break;
                case 1:
                    vect = new int[5] { -1, -2, 0, 2, 1 };
                    break;
                case 2:
                    vect = new int[5] { -1, 0, 2, 0, -1 };
                    break;
                case 3:
                    vect = new int[5] { 1, -4, 6, -4, 1 };
                    break;
            }
        }
    }

    public class Segment
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Segment(int x, int y, int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
            X = x;
            Y = y;
        }
    }
}
