var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("tiendasp-postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

builder.Build().Run();
