var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder
    .AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("tiendasp-rabbitmq-data")
    .WithManagementPlugin();

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("tiendasp-postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var db = postgres.AddDatabase("identity");

builder.AddProject<Projects.Tiendasp_API_Identity>("tiendasp-api-identity")
    .WaitFor(db)
    .WaitFor(rabbit)
    .WithReference(db)
    .WithReference(rabbit);

builder.Build().Run();
