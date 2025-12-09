#:package Aspire.Hosting.Docker@13.1.0-preview.1.25609.4
#:package Aspire.Hosting.Redis@13.1.0-preview.1.25609.4
#:package Aspire.Hosting.Python@13.1.0-preview.1.25609.4
#:package Aspire.Hosting.JavaScript@13.1.0-preview.1.25609.4
#:sdk Aspire.AppHost.Sdk@13.1.0-preview.1.25609.4
#:project ./api

#pragma warning disable ASPIRECOMPUTE003

using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var endpoint = builder.AddParameter("registry-endpoint");
var repository = builder.AddParameter("registry-repository");
builder.AddContainerRegistry("container-registry", endpoint, repository);

// Has ContainerImageAnnotation, no build tag or push
builder.AddRedis("redis-cache");

// Has Dockerfile annotation, build, tag, and push owned by Aspire via Docker/Podman
var userName = Environment.UserName;
builder.AddDockerfile("go-app", "./goapp")
    .WithHttpEndpoint(port: 8080, targetPort: 8080);
// .NET project, .NET SDK owns build and tag, Aspire owns push
builder.AddProject<Projects.api>("api");

// Python app serving as host for front-end, built and tagged separately, only Python image is pushed
// Front-end image produces assets needed by backend image
var viteApp = builder.AddViteApp("viteapp", "./viteapp");
var pythonApp = builder.AddUvicornApp("pythonapp", "./pythonapp", "main:app")
    .WithHttpHealthCheck("/api/health");

pythonApp.PublishWithContainerFiles(viteApp, "./static");

// Independent Python and Vite apps. Both images built, tagged, and pushed separately
builder.AddUvicornApp("pythonapp-standalone", "./pythonapp-standalone", "main:app")
    .WithHttpHealthCheck("/api/health");

builder.AddViteApp("viteapp-standalone", "./viteapp-standalone");

builder.Build().Run();