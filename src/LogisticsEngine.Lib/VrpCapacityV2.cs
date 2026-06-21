// Copyright 2010-2025 Google LLC
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// [START program]
// [START import]
using Google.OrTools.ConstraintSolver;
using Google.Protobuf.WellKnownTypes; // Duration
using System;
using System.Collections.Generic;
// [END import]

/// <summary>
///   Minimal TSP using distance matrix.
/// </summary>
/// 

namespace LogisticsEngine.lib;


public class VrpCapacityV2
{
    private static void PrintSolution(
        in RoutingDataModel data,
        in RoutingModel routing,
        in RoutingIndexManager manager,
        in Assignment solution)
    {
        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine("           RESULTADO DA OTIMIZAÇÃO");
        Console.WriteLine("==================================================");
        Console.WriteLine($"Custo Total da Solução: {solution.ObjectiveValue():N0}");
        Console.WriteLine();
        long distanciaTotal = 0;
        long cargaTotal = 0;

        for (int veiculo = 0; veiculo < data.VehicleNumber; veiculo++)
        {
            if (!routing.IsVehicleUsed(solution, veiculo))
            {
                Console.WriteLine($"🚚 Caminhão {veiculo + 1}: Não utilizado");
                Console.WriteLine();
                continue;
            }

            long distanciaRota = 0;
            long cargaRota = 0;

            var index = routing.Start(veiculo);

            Console.WriteLine($"🚚 Caminhão {veiculo + 1}");
            Console.WriteLine(new string('-', 50));
            Console.Write("Rota: ");

            while (!routing.IsEnd(index))
            {
                var nodeIndex = manager.IndexToNode(index);

                cargaRota += data.Demands[nodeIndex];

                Console.Write(
                    $"[{nodeIndex} | Carga Acumulada: {cargaRota:N0} kg] -> ");

                var previousIndex = index;
                index = solution.Value(routing.NextVar(index));

                distanciaRota +=
                    routing.GetArcCostForVehicle(
                        previousIndex,
                        index,
                        veiculo);
            }

            Console.WriteLine($"[{manager.IndexToNode(index)}]");

            Console.WriteLine($"Peso Transportado : {cargaRota:N0} kg");
            Console.WriteLine($"Distância da Rota : {distanciaRota:N0} m");
            Console.WriteLine();

            distanciaTotal += distanciaRota;
            cargaTotal += cargaRota;
        }

        Console.WriteLine("==================================================");
        Console.WriteLine("                 RESUMO GERAL");
        Console.WriteLine("==================================================");
        Console.WriteLine($"Carga Total Transportada : {cargaTotal:N0} kg");
        Console.WriteLine($"Distância Total Percorrida: {distanciaTotal:N0} m");
        Console.WriteLine("==================================================");
    }


    public void Start(RoutingDataModel data)
    {
        var manager = new RoutingIndexManager(data.DistanceMatrix.GetLength(0), data.VehicleNumber, data.Depot);
        var routing = new RoutingModel(manager);

        int transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
        {
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return data.DistanceMatrix[fromNode, toNode];
        });
        routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        int demandCallbackIndex = routing.RegisterUnaryTransitCallback((long fromIndex) =>
        {
            var fromNode =
                manager.IndexToNode(fromIndex);
            return data.Demands[fromNode];
        });
        routing.AddDimensionWithVehicleCapacity(demandCallbackIndex, 0, // null capacity slack
                                                data.VehicleCapacities, // vehicle maximum capacities
                                                true,                   // start cumul to zero
                                                "Capacity");
        RoutingSearchParameters searchParameters =
            operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Duration { Seconds = 1 };

        Assignment solution = routing.SolveWithParameters(searchParameters);

        PrintSolution(data, routing, manager, solution);
    }
}