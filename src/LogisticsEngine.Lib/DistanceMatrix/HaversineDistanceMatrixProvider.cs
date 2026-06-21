using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsEngine.lib.DistanceMatrix
{
    public class HaversineDistanceMatrixProvider : IDistanceMatrixProvider
    {
        public long[,] Build(Coordenada deposito, IEnumerable<Carga> cargas)
        {
            var pontos = new List<Coordenada>
            {
                deposito
            };

            pontos.AddRange(cargas.Select(x => new Coordenada(x.Latitude, x.Longetude)));

            var size = pontos.Count;
            var matrix = new long[size, size];

            for (int i = 0; i < size; i++)
            {
                matrix[i, i] = 0;

                for (int j = i + 1; j < size; j++)
                {
                    var distance = (long)Haversine(
                        pontos[i].Latitude,
                        pontos[i].Longitude,
                        pontos[j].Latitude,
                        pontos[j].Longitude);

                    matrix[i, j] = distance;
                    matrix[j, i] = distance;
                }
            }

            return matrix;
        }

        private static double Haversine(double lat1, double lon1, double lat2,double lon2)
        {
            const double R = 6371000;

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) *
                Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double ToRad(double value) => value * Math.PI / 180;
    }
}
