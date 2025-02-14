using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Collections.Generic;
using System.Data;

namespace KlasterAnaliz
{
    public static class ClusteringHelper
    {
        /// <summary>
        /// Преобразует массив объектов в матрицу числовых характеристик.
        /// propertySelectors – массив выражений для выбора числовых свойств.
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
        /// Нормализует матрицу по столбцам: для каждого столбца выполняется нормализация:
        /// (value - min) / (max - min)
        /// </summary>
        public static double[,] NormalizeMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            double[,] normalized = new double[rows, columns];

            for (int j = 0; j < columns; j++)
            {
                double min = double.MaxValue;
                double max = double.MinValue;
                for (int i = 0; i < rows; i++)
                {
                    if (matrix[i, j] < min) min = matrix[i, j];
                    if (matrix[i, j] > max) max = matrix[i, j];
                }
                for (int i = 0; i < rows; i++)
                {
                    normalized[i, j] = (matrix[i, j] - min) / (max - min);
                }
            }
        
          
            return normalized;
        }
        public static double[,] Mutation(double[,] normalized)
        {
            var rows = normalized.GetLength(0);
            var columns = normalized.GetLength(1);
            double[,] newNormalized = new double[normalized.GetLength(0) + 3, normalized.GetLength(1)];
            for (int i = 0; i < rows; i++)
            {
                Array.Copy(normalized, i * columns, newNormalized, i * columns, columns);
            }
            for (int j = 0; j < columns; j++)
            {
                newNormalized[rows, j] = 0;
            }
            for (int j = 0; j < columns; j++)
            {
                newNormalized[rows + 1, j] = 0.5;
            }
            for (int j = 0; j < columns; j++)
            {
                newNormalized[rows + 2, j] = 1;
            }
            return newNormalized;
        }
        /// <summary>
        /// Выводит матрицу в консоль.
        /// </summary>
        public static void PrintMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Console.Write($"{Math.Round(matrix[i, j], 3)}\t");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Реализация алгоритма кластеризации методом k-средних.
        /// data – нормализованная матрица данных (каждая строка – объект, столбцы – признаки).
        /// k – количество кластеров.
        /// maxIterations – максимальное число итераций, threshold – порог для остановки (изменение центроидов).
        /// Возвращает объект с назначениями кластеров и матрицей центроидов.
        /// </summary>
        public static KMeansClusterResult KMeansClustering(double[,] data, int k, int maxIterations = 100, double threshold = 1e-6)
        {
            int numObjects = data.GetLength(0);
            int numFeatures = data.GetLength(1);
            int[] assignments = new int[numObjects];

            // Инициализация центроидов: выбираем k уникальных случайных объектов
            double[,] centroids = new double[k, numFeatures];
            Random rand = new Random();
            HashSet<int> chosenIndices = new HashSet<int>();
            for (int i = 0; i < k; i++)
            {
                int index;
                do
                {
                    index = rand.Next(numObjects);
                } while (chosenIndices.Contains(index));
                chosenIndices.Add(index);
                for (int j = 0; j < numFeatures; j++)
                {
                    centroids[i, j] = data[index, j];
                }
            }

            bool changed = true;
            int iterations = 0;
            while (changed && iterations < maxIterations)
            {
                changed = false;
                // Назначаем объекты к ближайшему центроиду
                for (int i = 0; i < numObjects; i++)
                {
                    double minDist = double.MaxValue;
                    int bestCluster = 0;
                    for (int cluster = 0; cluster < k; cluster++)
                    {
                        double dist = 0;
                        for (int j = 0; j < numFeatures; j++)
                        {
                            double diff = data[i, j] - centroids[cluster, j];
                            dist += diff * diff;
                        }
                        if (dist < minDist)
                        {
                            minDist = dist;
                            bestCluster = cluster;
                        }
                    }
                    if (assignments[i] != bestCluster)
                    {
                        assignments[i] = bestCluster;
                        changed = true;
                    }
                }

                // Пересчитываем центроиды
                double[,] newCentroids = new double[k, numFeatures];
                int[] counts = new int[k];
                for (int i = 0; i < numObjects; i++)
                {
                    int cluster = assignments[i];
                    counts[cluster]++;
                    for (int j = 0; j < numFeatures; j++)
                    {
                        newCentroids[cluster, j] += data[i, j];
                    }
                }
                for (int cluster = 0; cluster < k; cluster++)
                {
                    if (counts[cluster] > 0)
                    {
                        for (int j = 0; j < numFeatures; j++)
                        {
                            newCentroids[cluster, j] /= counts[cluster];
                        }
                    }
                    else
                    {
                        // Если кластер пустой, сохраняем старый центроид.
                        for (int j = 0; j < numFeatures; j++)
                        {
                            newCentroids[cluster, j] = centroids[cluster, j];
                        }
                    }
                }

                // Проверяем изменение центроидов
                double maxChange = 0;
                for (int cluster = 0; cluster < k; cluster++)
                {
                    double change = 0;
                    for (int j = 0; j < numFeatures; j++)
                    {
                        double diff = newCentroids[cluster, j] - centroids[cluster, j];
                        change += diff * diff;
                    }
                    change = Math.Sqrt(change);
                    if (change > maxChange)
                        maxChange = change;
                }
                centroids = newCentroids;
                iterations++;
                if (maxChange < threshold)
                    break;
            }

            return new KMeansClusterResult
            {
                Assignments = assignments,
                Centroids = centroids
            };
        }
    }


}
