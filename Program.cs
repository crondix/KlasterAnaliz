using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KlasterAnaliz
{
    internal class Program
    {
        static void Main(string[] args)
        {

        

            Product[] products = new Product[]
{
            new Product { Price = 45, Weight = 567, Name = "Product1" },
            new Product { Price = 76, Weight = 345, Name = "Product2" },
            new Product { Price = 1230, Weight = 3240, Name = "Product3" },
            new Product { Price = 3467, Weight = 541, Name = "Product3" },
            new Product { Price = 234, Weight = 456, Name = "Product3" },
            new Product { Price = 25, Weight = 678, Name = "Product3" },
};

            var arr= ObjectsToMatrix(products, new Expression<Func<Product, double>>[]
            {
            x => x.Price,
            x => x.Weight
            });

            var normalized = NormalizeMatrix(arr);



            int rows = normalized.GetUpperBound(0) + 1;
            int columns = normalized.Length / rows;
            var evk = EvklidMatrix(normalized);

            for (int i = 0; i < rows; i++)
            {
               
                for (int j = 0; j < columns; j++)
                {
                
                    Console.Write($"{arr[i, j]}\t");

                }

                Console.WriteLine();
            }
            Console.WriteLine("\n");
            for (int i = 0; i < rows; i++)
            {
               
                for (int j = 0; j < columns; j++)
                {
                
                
                    Console.Write($"{normalized[i, j]}\t");
                }

                Console.WriteLine();
            }
            Console.WriteLine("\n");

            for (int i=0; i<(evk.GetUpperBound(0) + 1); i++)
            {
                for (int j = 0; j < (evk.Length/(evk.GetUpperBound(0) + 1)); j++)
                {
                    Console.Write($"{Math.Round(evk[i,j], 3)}\t");
                }
                Console.WriteLine();
            }


            Console.WriteLine("Кластеры");

            var clasters = CompleteLinkageClustering(evk, 1);
          
            foreach (var item in clasters)
            {
                foreach (var item2 in item)
                {
                    Console.WriteLine(item2+1);
                }
                Console.WriteLine();
            }

            //var matrix= ObjectsToMatrix(products, new Expression<Func<Product, double>>[]
            //   {
            //   x => x.Price,
            //   x => x.Weight
            //   });


        }

 public static double[] ObjectComponentsSumm<T>(T[,] arr) where T : INumber<T>
        {
            int rows = arr.GetUpperBound(0) + 1;    // количество строк
            int columns = arr.Length / rows;        // количество столбцов
           double[] res= new double[rows];
            for (int i = 0; i < rows; i++)
            {

                for (int j = 0; j < columns; j++)
                {

                    res[i] += Com arr[i, j];
                                        
                }

            }
            return res;
        } 
        // Метод для объединения кластеров с использованием метода полной связи
        public static List<List<int>> CompleteLinkageClustering(double[,] distanceMatrix, int numberOfClusters)
        {
            // Инициализация: каждый объект сначала рассматривается как отдельный кластер
            int numObjects = distanceMatrix.GetLength(0);
            List<List<int>> clusters = new List<List<int>>();
            for (int i = 0; i < numObjects; i++)
            {
                clusters.Add(new List<int> { i });
            }

            while (clusters.Count > numberOfClusters)
            {
                double minDistance = double.MaxValue;
                int clusterToMergeA = 0;
                int clusterToMergeB = 0;

                // Поиск двух кластеров с минимальным максимальным расстоянием
                for (int i = 0; i < clusters.Count - 1; i++)
                {
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        double maxDistance = double.MinValue;

                        // Вычисляем максимальное расстояние между элементами кластеров i и j
                        foreach (int elementA in clusters[i])
                        {
                            foreach (int elementB in clusters[j])
                            {
                                double distance = distanceMatrix[elementA, elementB];
                                if (distance > maxDistance)
                                {
                                    maxDistance = distance;
                                }
                            }
                        }

                        // Находим минимальное максимальное расстояние между всеми парами кластеров
                        if (maxDistance < minDistance)
                        {
                            minDistance = maxDistance;
                            clusterToMergeA = i;
                            clusterToMergeB = j;
                        }
                    }
                }

                // Объединяем найденные кластеры
                clusters[clusterToMergeA].AddRange(clusters[clusterToMergeB]);
                clusters.RemoveAt(clusterToMergeB); // Удаляем объединённый кластер из списка
            }

            return clusters;
        }
        public static T[,] MatrixTranspos<T>(T[,] arr) where T : INumber<T>
        {
            int ArrRows = arr.GetUpperBound(0) + 1;    // количество строк
            int ArrColumns = arr.Length / ArrRows;        // количество столбцов

            T[,] TempArr = new T[ArrColumns, ArrRows];
            int TempRows = ArrColumns;
            int TempColumns = ArrRows;
            for (int i = 0; i < ArrRows; i++)
            {
                TempRows = ArrColumns;
                TempColumns = ArrRows;
                for (int j = 0; j < ArrColumns; j++)
                {
                    TempArr[j, i] = arr[i, j];
                }
            }
            return TempArr;
        }

        public static double[,] NormalizeMatrix<T>(T[,] matrix) where T : INumber<T>, IMinMaxValue<T>
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            double[,] normalizedMatrix = new double[rows, columns];

            for (int j = 0; j < columns; j++)
            {
                var min = T.MaxValue;
                var max = T.MinValue;

                // Поиск минимума и максимума в каждом столбце
                for (int i = 0; i < rows; i++)
                {
                    if (matrix[i, j] < min) min = matrix[i, j];
                    if (matrix[i, j] > max) max = matrix[i, j];
                }

                // Нормализация каждого элемента в столбце
                for (int i = 0; i < rows; i++)
                {
                    normalizedMatrix[i, j] = Normalize(min, max, matrix[i, j]);
                }
            }

            return normalizedMatrix;
        }

        public static T findMinColumnMatrix<T>(T[,] matrix, int columnIndex) where T : INumber<T>, IMinMaxValue<T>
        {
            int rows = matrix.GetLength(0);
            T min = T.MaxValue;

            for (int j = 0; j < columnIndex; j++)
            {

                // Поиск минимума и максимума в каждом столбце
                for (int i = 0; i < rows; i++)
                {
                    if (matrix[i, j] < min) min = matrix[i, j];

                }
            }
            return min;
        }

        public static T findMaxColumnMatrix<T>(T[,] matrix, int columnIndex) where T : INumber<T>, IMinMaxValue<T>
        {
            int rows = matrix.GetLength(0);
            T max = T.MinValue;

            for (int j = 0; j < columnIndex; j++)
            {

                // Поиск минимума и максимума в каждом столбце
                for (int i = 0; i < rows; i++)
                {
                    if (matrix[i, j] > max) max = matrix[i, j];

                }
            }
            return max;
        }

        public static double[] NormalizeArray<T>(IEnumerable<T> arr) where T : INumber<T>
        {
            double[] result = new double[arr.Count()];
            var min = arr.Min();
            var max = arr.Max();
            if (min != null && max != null)
            {
                for (int i = 0; i < arr.Count(); i++)
                {
                    result[i] = Normalize(min, max, arr.ElementAt(i));
                }
            }
            return result;
        }

        public static double Normalize<T>(T min, T max, T value) where T : INumber<T>
        {


            return Convert.ToDouble(value - min) / Convert.ToDouble(max - min);

        }

        public static double[,] EvklidMatrix<T>(T[,] arr) where T : INumber<T>
        {
            int ArrRows = arr.GetUpperBound(0) + 1; // количество строк (объектов)
            int ArrColumns = arr.Length / ArrRows; // количество столбцов (параметров)

            // Инициализируем квадратную матрицу расстояний
            double[,] distanceMatrix = new double[ArrRows, ArrRows];

            for (int i = 0; i < ArrRows; i++)
            {
                for (int j = i; j < ArrRows; j++)
                {
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0; // Расстояние до самого себя равно 0
                        continue;
                    }

                    double sum = 0;

                    for (int k = 0; k < ArrColumns; k++)
                    {
                        // Используем встроенные методы для конвертации
                        double valueI = Convert.ToDouble(arr[i, k]);
                        double valueJ = Convert.ToDouble(arr[j, k]);
                        sum += Math.Pow(valueI - valueJ, 2);
                    }

                    double distance = Math.Sqrt(sum);

                    // Заполняем оба симметричных элемента, т.к. расстояние между i и j равно расстоянию между j и i
                    distanceMatrix[i, j] = distance;
                    distanceMatrix[j, i] = distance;
                }
            }

            return distanceMatrix;
        }


        public static double[,] ObjectsToMatrix<T>(T[] objects, Expression<Func<T, double>>[] propertySelectors)
        {
            // Компилируем все выражения в функции
            var propertyFuncs = propertySelectors.Select(selector => selector.Compile()).ToArray();

            // Инициализируем двумерный массив для результатов
            double[,] matrix = new double[objects.Length, propertyFuncs.Length];

            // Пройдемся по каждому объекту и заполним матрицу
            for (int i = 0; i < objects.Length; i++)
            {
                T obj = objects[i];

                // Проверяем, не является ли obj null (на случай, если T - ссылочный тип)
                if (obj == null)
                {
                    throw new ArgumentNullException($"The object at index {i} is null.");
                }

                // Получаем значение каждого свойства, указанного в propertyFuncs
                var values = propertyFuncs.Select(func => func(obj)).ToArray();

                for (int j = 0; j < values.Length; j++)
                {
                    matrix[i, j] = values[j];
                }
            }

            return matrix;
        }

        public static T[,] ArraysToMatrix<T>(T[][] arrays)where T : INumber<T>
        {
            int rows = arrays.Length;
            int cols = arrays[0].Length;
            T[,] matrix = new T[rows, cols];
            if (IsMatrix(arrays)) new Exception("Arrays is not a matrix");
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = arrays[i][j];
                }
            }

            return matrix;
        }

        public static bool IsMatrix<T>(T[][] arrays) where T : INumber<T>
        {
            var length = arrays[0].Length;
            for (int i = 0; i < arrays.Length; i++)
            {

               if(arrays[i].Length!= length)return false;
            }
            return true;
        }


        //public static double[] Evklid<T>(T[,] arr) where T : INumber<T>
        //{
        //    int ArrRows = arr.GetUpperBound(0) + 1; // количество строк (объектов)
        //    int ArrColumns = arr.Length / ArrRows; // количество столбцов (параметров)

        //    // Количество пар, для которых нужно вычислить расстояние
        //    int numDistances = (ArrRows * (ArrRows - 1)) / 2;
        //    double[] res = new double[numDistances];

        //    int index = 0; // Индекс для записи результата в массив res

        //    for (int i = 0; i < ArrRows - 1; i++)
        //    {
        //        for (int j = i + 1; j < ArrRows; j++)
        //        {
        //            double sum = 0;

        //            for (int k = 0; k < ArrColumns; k++)
        //            {
        //                sum += Math.Pow(Convert.ToDouble(arr[i, k] - arr[j, k]), 2);
        //            }

        //            res[index] = Math.Sqrt(sum);
        //            index++;
        //        }
        //    }

        //    return res;
        //}
    }
    public class Product
    {
        public double Price { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; }
    }
}

