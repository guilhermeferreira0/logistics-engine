using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsEngine.lib.DistanceMatrix
{
    public interface IDistanceMatrixProvider
    {
        long[,] Build(Coordenada deposito, IEnumerable<Carga> cargas);
    }
}
