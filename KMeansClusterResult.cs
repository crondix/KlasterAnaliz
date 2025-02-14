using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlasterAnaliz
{
    /// <summary>
    /// Класс для хранения результата кластеризации методом k-средних.
    /// </summary>
    public class KMeansClusterResult
    {
        /// <summary>
        /// Массив, где для каждого объекта указан номер кластера (начиная с 0).
        /// </summary>
        public int[] Assignments { get; set; }
        /// <summary>
        /// Матрица центроидов (k x numFeatures).
        /// </summary>
        public double[,] Centroids { get; set; }
    }
}
