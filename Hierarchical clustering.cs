using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;

namespace KlasterAnaliz
{
    public static class Hierarchical_clustering
    {
        /// <summary>
        /// Преобразует массив объектов в матрицу числовых характеристик.
        /// </summary>
        public static double[,] ObjectsToMatrix<T>(T[] objects, Expression<Func<T, double>>[] propertySelectors)
        {
            var propertyFuncs = propertySelectors.Select(selector => selector.Compile()).ToArray();
            double[,] matrix = new double[objects.Length, propertyFuncs.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                T obj = objects[i] ?? throw new ArgumentNullException($"Объект с индексом {i} равен null.");
                var values = propertyFuncs.Select(func => func(obj)).ToArray();
                for (int j = 0; j < values.Length; j++)
                {
                    matrix[i, j] = values[j];
                }
            }
            return matrix;
        }

        /// <summary>
        /// Нормализует матрицу по столбцам: для каждого столбца вычисляются min и max, затем
        /// каждое значение нормализуется по формуле: (value - min) / (max - min).
        /// </summary>
        public static double[,] NormalizeMatrix<T>(T[,] matrix) where T : INumber<T>, IMinMaxValue<T>
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            double[,] normalizedMatrix = new double[rows, columns];

            for (int j = 0; j < columns; j++)
            {
                T min = T.MaxValue;
                T max = T.MinValue;

                // Поиск минимума и максимума в столбце
                for (int i = 0; i < rows; i++)
                {
                    if (matrix[i, j] < min)
                        min = matrix[i, j];
                    if (matrix[i, j] > max)
                        max = matrix[i, j];
                }

                // Нормализация каждого элемента столбца
                for (int i = 0; i < rows; i++)
                {
                    normalizedMatrix[i, j] = Normalize(min, max, matrix[i, j]);
                }
            }
            return normalizedMatrix;
        }

        /// <summary>
        /// Нормализует значение по формуле: (value - min) / (max - min).
        /// </summary>
        public static double Normalize<T>(T min, T max, T value) where T : INumber<T>
        {
            return Convert.ToDouble(value - min) / Convert.ToDouble(max - min);
        }

        /// <summary>
        /// Вычисляет матрицу евклидовых расстояний между объектами.
        /// Каждый объект представляется строкой матрицы, столбцы – признаки.
        /// </summary>
        public static double[,] EvklidMatrix<T>(T[,] arr) where T : INumber<T>
        {
            int arrRows = arr.GetUpperBound(0) + 1; // количество строк (объектов)
            int arrColumns = arr.Length / arrRows; // количество столбцов (признаков)
            double[,] distanceMatrix = new double[arrRows, arrRows];

            for (int i = 0; i < arrRows; i++)
            {
                for (int j = i; j < arrRows; j++)
                {
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0;
                        continue;
                    }

                    double sum = 0;
                    for (int k = 0; k < arrColumns; k++)
                    {
                        double valueI = Convert.ToDouble(arr[i, k]);
                        double valueJ = Convert.ToDouble(arr[j, k]);
                        sum += Math.Pow(valueI - valueJ, 2);
                    }
                    double distance = Math.Sqrt(sum);
                    distanceMatrix[i, j] = distance;
                    distanceMatrix[j, i] = distance;
                }
            }
            return distanceMatrix;
        }

        /// <summary>
        /// Кластеризация методом полной (максимальной) связи.
        /// Каждый объект изначально считается отдельным кластером, затем объединяются пары кластеров с минимальным максимальным расстоянием.
        /// </summary>
        public static List<List<int>> CompleteLinkageClustering(double[,] distanceMatrix, int numberOfClusters)
        {
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
                        // Находим пару кластеров с минимальным максимальным расстоянием
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
                clusters.RemoveAt(clusterToMergeB);
            }

            return clusters;
        }
    }
}
