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
            new Product {Name="PR_FENUP15-01",PrintJobCount=153, UniqueUserCount=8, PrintedSheetCount=3473},
            new Product {Name="PR_FENCO30-01",PrintJobCount=23, UniqueUserCount=3, PrintedSheetCount=463},
            new Product {Name="PR_ZsmZamNACH1",PrintJobCount=137, UniqueUserCount=5, PrintedSheetCount=3317},
            new Product {Name="PR_ZSMLAB111",PrintJobCount=46, UniqueUserCount=3, PrintedSheetCount=1225},
            new Product {Name="PR_ZAMKOM201",PrintJobCount=4, UniqueUserCount=2, PrintedSheetCount=62},
            new Product {Name="PR_UURIST312",PrintJobCount=413, UniqueUserCount=5, PrintedSheetCount=3618},
            new Product {Name="PR_UUOV211",PrintJobCount=347, UniqueUserCount=8, PrintedSheetCount=12173},
            new Product {Name="PR_UUOV1",PrintJobCount=583, UniqueUserCount=10, PrintedSheetCount=11347},
            new Product {Name="PR_UUCH231",PrintJobCount=187, UniqueUserCount=9, PrintedSheetCount=3675},
            new Product {Name="PR_UUCH222",PrintJobCount=171, UniqueUserCount=7, PrintedSheetCount=4156} };

             double[,] dataMatrix = ClusteringHelper.ObjectsToMatrix(products, new Expression<Func<Product, double>>[]
            {
                x => x.PrintJobCount,
                x => x.UniqueUserCount,
                x => x.PrintedSheetCount,
            });

            // 3. Нормализуем матрицу по столбцам
            double[,] normalizedMatrix = ClusteringHelper.Mutation (ClusteringHelper.NormalizeMatrix(dataMatrix));
            Console.WriteLine("Нормализованная матрица данных:");
            ClusteringHelper.PrintMatrix(normalizedMatrix);

            // 4. Проводим кластеризацию методом k-средних (например, k = 2)
            int k = 3;
            var kMeansResult = KMeansClustering.Cluster(normalizedMatrix, k);

            // 5. Выводим результаты кластеризации
            Console.WriteLine("\nНазначение кластеров для объектов:");
            for (int i = 0; i < products.Length; i++)
            {
                Console.WriteLine($"Объект {i + 1} (\"{products[i].Name}\") -> кластер {kMeansResult.Assignments[i] }");
            }
            var min = kMeansResult.Assignments[products.Length];
            var midle = kMeansResult.Assignments[products.Length+1];
            var max = kMeansResult.Assignments[products.Length+2];
            Console.WriteLine("\nРасшифровка кластеров:");
          
                Console.WriteLine($"минимальная загруженность -> кластер {min}");
                Console.WriteLine($"средняя загруженность -> кластер {midle}");
                Console.WriteLine($"максимальная загруженность -> кластер {max}");

            Console.WriteLine("\nРасшифровка кластеров:");
            for (int i = 0; i < products.Length; i++)
            {
                Console.Write($"\nОбъект {i + 1} (\"{products[i].Name}\") ->");
                if (kMeansResult.Assignments[i] == min)
                {
                    Console.Write(" минимальная загруженность");
                }
                else if (kMeansResult.Assignments[i] == midle)
                {
                    Console.Write(" средняя загруженность");
                }
                else
                {
                    Console.Write(" максимальная загруженность");
                }
            }
            Console.WriteLine("\nЦентроиды кластеров:");
            ClusteringHelper.PrintMatrix(kMeansResult.Centroids);

            Console.ReadLine();
        }
    };


        }
       


