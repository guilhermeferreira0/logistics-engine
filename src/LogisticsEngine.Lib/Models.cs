using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsEngine.lib
{
    public record Entrega(int Id, double PesoKg, double LatitudeDestino, double LongitudeDestino);
    public record Caminhao(int Id, double CapacidadeKg);
    public record RoutingDataModel(long[,] DistanceMatrix, long[] Demands, long[] VehicleCapacities, int VehicleNumber, int Depot);
    public record Carga(int Id, double PesoKg, double Latitude, double Longetude);
    public record Coordenada(double Latitude, double Longitude);
    public record PlanejamentoEntrega(Coordenada Deposito, IReadOnlyCollection<Carga> Cargas, IReadOnlyCollection<Caminhao> Caminhoes);
    public record RotaVeiculo(int CaminhaoId, IReadOnlyCollection<int> Entregas, long DistanciaMetros, long PesoKg);
    public record RouteOptimizationResult(IReadOnlyCollection<RotaVeiculo> Rotas, long DistanciaTotal);
}
