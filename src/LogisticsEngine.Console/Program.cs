using LogisticsEngine.lib;
using LogisticsEngine.lib.DistanceMatrix;
using LogisticsEngine.lib.Optimization;

var cargas = new List<Carga>
{
    new(1, 4000, 1, 1),
    new(2, 3000, 1, 1),
    new(3, 5000, 1, 1),
    new(4, 2000, 1, 1),
    new(5, 2000, 1, 1),
    new(6, 2000, 1, 1),
    new(7, 2000, 1, 1),
};
var caminhoes = new List<Caminhao> { new(1, 10000), new(2, 3000), new(3, 5000), new(4, 5000) };
var deposito = new Coordenada(1, 1);

var planejamento = new PlanejamentoEntrega(deposito, cargas, caminhoes);
var builder = new RountingDataBuilder(new HaversineDistanceMatrixProvider());
var rountingData = builder.Build(planejamento);
var optimizer = new OrToolsRouteOptimizer();

var resultado = optimizer.Optimize(rountingData);

Console.WriteLine();
Console.WriteLine("================================================");
Console.WriteLine("          RESULTADO DA OTIMIZAÇÃO");
Console.WriteLine("================================================");

foreach (var rota in resultado.Rotas)
{
    Console.WriteLine();
    Console.WriteLine($"🚚 Caminhão {rota.CaminhaoId}");
    Console.WriteLine($"Entregas........: {string.Join(" -> ", rota.Entregas)}");
    Console.WriteLine($"Carga Total.....: {rota.PesoKg:N0} kg");
    Console.WriteLine($"Distância.......: {rota.DistanciaMetros:N0} m");
}

Console.WriteLine();
Console.WriteLine("================================================");
Console.WriteLine($"Distância Total: {resultado.DistanciaTotal:N0} m");
Console.WriteLine("================================================");

