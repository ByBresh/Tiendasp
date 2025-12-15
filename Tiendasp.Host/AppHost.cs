using System.Security.Principal;

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
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050)
        .WithLifetime(ContainerLifetime.Persistent));

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("redis-data-identity")
    .WithRedisInsight();

var identityDb = postgres.AddDatabase("identity");
var productsDb = postgres.AddDatabase("products");

var identity = builder.AddProject<Projects.Tiendasp_API_Identity>("tiendasp-api-identity")
    .WaitFor(identityDb)
    .WaitFor(rabbit)
    .WithReference(identityDb)
    .WithReference(rabbit);

var products = builder.AddProject<Projects.Tiendasp_API_Products>("tiendasp-api-products")
    .WaitFor(productsDb)
    .WaitFor(rabbit)
    .WithReference(productsDb)
    .WithReference(rabbit);

var gateway = builder.AddProject<Projects.Tiendasp_API_Gateway>("tiendasp-api-gateway")
    .WithReference(redis)
    .WithReference(identity)
    .WithReference(products)
    .WaitFor(redis)
    .WaitFor(identity)
    .WaitFor(products);

builder.Build().Run();
