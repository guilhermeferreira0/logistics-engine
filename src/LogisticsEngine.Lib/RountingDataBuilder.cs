using LogisticsEngine.lib.DistanceMatrix;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsEngine.lib;

public interface IRoutingDataBuilder
{
    RoutingDataModel Build(PlanejamentoEntrega planejamento);
}

public class RountingDataBuilder(IDistanceMatrixProvider distanceMatrix) : IRoutingDataBuilder
{
    private readonly IDistanceMatrixProvider _distanceProvider = distanceMatrix;

    private sealed class NodeMap
    {
        public required List<Carga> Cargas { get; init; }
        public int TotalNodes => Cargas.Count + 1;
        public int DepotIndex => 0;
        public int ToNodeIndex(int cargaIndex) => cargaIndex + 1;
    }

    public RoutingDataModel Build(PlanejamentoEntrega planejamento)
    {
        var nodeMap = new NodeMap
        {
            Cargas = [.. planejamento.Cargas]
        };
        var matrix = _distanceProvider.Build(planejamento.Deposito, planejamento.Cargas);
        var demands = BuildDemands(nodeMap);
        var capacities = planejamento.Caminhoes.Select(x => (long)x.CapacidadeKg).ToArray();

        return new RoutingDataModel(matrix, demands, capacities, capacities.Length, 0);
    }

    private static long[] BuildDemands(NodeMap map)
    {
        var demands = new long[map.TotalNodes];
        demands[map.DepotIndex] = 0;

        for (int i = 0; i < map.Cargas.Count; i++)
        {
            var nodeIndex = map.ToNodeIndex(i);
            demands[nodeIndex] = (long)map.Cargas[i].PesoKg;
        }

        return demands;
    }
}
