using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsEngine.lib.Optimization
{
    public interface IRouteOptmizer
    {
        RouteOptimizationResult Optimize(RoutingDataModel data);
    }
}
