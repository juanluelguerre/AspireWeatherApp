// ReSharper disable All

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

// TODO: NEW
var _ = builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount("../deploy/prometheus", "/etc/prometheus")
    .WithEndpoint(port: 9090, targetPort: 9090, name: "prometheus-http", scheme: "http");

var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithBindMount("../deploy/grafana/config", "/etc/grafana")
    .WithBindMount("../deploy/grafana/dashboards", "/var/lib/grafana/dashboards")
    // .WithBindMount("../deploy/grafana/config/provisioning", "/etc/grafana/provisioning")
    .WithEndpoint(port: 3000, targetPort: 3000, name: "grafana-http", scheme: "http");

var apiService = builder.AddProject<Projects.ElGuerre_WeatherApp_Api>("apiservice")
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"));
//.WithEnvironment("PROMETHEUS_URL", prometheus.GetEndpoint("prometheus-http"));


builder.AddProject<Projects.ElGuerre_WeatherApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService)
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"));
//.WithEnvironment("PROMETHEUS_URL", prometheus.GetEndpoint("prometheus-http"));

builder.Build().Run();
