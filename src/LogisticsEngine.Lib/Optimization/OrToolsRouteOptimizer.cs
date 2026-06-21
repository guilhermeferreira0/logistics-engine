using Google.OrTools.ConstraintSolver;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsEngine.lib.Optimization
{
    public class OrToolsRouteOptimizer : IRouteOptmizer
    {
        public RouteOptimizationResult Optimize(RoutingDataModel data)
        {
            var manager = new RoutingIndexManager(
            data.DistanceMatrix.GetLength(0),
            data.VehicleNumber,
            data.Depot);

            var routing = new RoutingModel(manager);
            ConfigureDistance(data, manager, routing);
            ConfigureCapacity(data, manager, routing);

            var searchParameters = CreateSearchParameters();
            var solution = routing.SolveWithParameters(searchParameters)
                ?? throw new InvalidOperationException("Nenhuma rota viável foi encontrada.");

            return BuildResult(data, manager, routing, solution);
        }

        private static void ConfigureDistance(RoutingDataModel data, RoutingIndexManager manager, RoutingModel routing)
        {
            var transitCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
            {
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);

                return data.DistanceMatrix[fromNode, toNode];
            });

            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
        }

        private static void ConfigureCapacity(RoutingDataModel data, RoutingIndexManager manager, RoutingModel routing)
        {
            var demandCallbackIndex = routing.RegisterUnaryTransitCallback(fromIndex =>
            {
                var node = manager.IndexToNode(fromIndex);

                return data.Demands[node];
            });

            routing.AddDimensionWithVehicleCapacity(demandCallbackIndex, 0, data.VehicleCapacities, true, "Capacity");
        }

        private static RoutingSearchParameters CreateSearchParameters()
        {
            var parameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            parameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            parameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
            parameters.TimeLimit = new Duration { Seconds = 1 };

            return parameters;
        }

        private static RouteOptimizationResult BuildResult(RoutingDataModel data, RoutingIndexManager manager, RoutingModel routing, Assignment solution)
        {
            var rotas = new List<RotaVeiculo>();
            long distanciaTotal = 0;

            for (int veiculo = 0;
                 veiculo < data.VehicleNumber;
                 veiculo++)
            {
                if (!routing.IsVehicleUsed(solution, veiculo))
                    continue;

                long distanciaRota = 0;
                long cargaRota = 0;

                var entregas = new List<int>();
                var index = routing.Start(veiculo);

                while (!routing.IsEnd(index))
                {
                    var node = manager.IndexToNode(index);

                    if (node != data.Depot)
                    {
                        entregas.Add(node);
                        cargaRota += data.Demands[node];
                    }

                    var previousIndex = index;

                    index = solution.Value(routing.NextVar(index));
                    distanciaRota += routing.GetArcCostForVehicle(previousIndex, index, veiculo);
                }

                distanciaTotal += distanciaRota;

                rotas.Add(new RotaVeiculo(veiculo + 1, entregas, distanciaRota, cargaRota));
            }

            return new RouteOptimizationResult(rotas, distanciaTotal);
        }

    }

}
