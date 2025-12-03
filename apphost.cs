#:package Aspire.Hosting.Docker@13.1.0-preview.1.25602.7
#:package Aspire.Hosting.Redis@13.1.0-preview.1.25602.7
#:package Aspire.Hosting.Python@13.1.0-preview.1.25602.7
#:package Aspire.Hosting.JavaScript@13.1.0-preview.1.25602.7
#:sdk Aspire.AppHost.Sdk@13.1.0-preview.1.25602.7
#:project ./api

#pragma warning disable ASPIRECOMPUTE001
#pragma warning disable ASPIRECOMPUTE003
#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREPIPELINES003

using Aspire.Hosting.Pipelines;
using Aspire.Hosting.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

var endpoint = builder.AddParameter("containerRegistryEndpoint");
var repository = builder.AddParameter("containerRegistryRepository");
var registry = builder.AddContainerRegistry("container-registry", endpoint, repository);

builder.Pipeline.AddStep("push-images", async (context) =>
{
    var imageManager = context.Services.GetRequiredService<IResourceContainerImageManager>();
    foreach (var computeResource in context.Model.GetComputeResources())
    {
        if (computeResource.RequiresImageBuild())
        {
            context.Logger.LogInformation("Pushing image for compute resource: {resourceName}", computeResource.Name);
            await imageManager.PushImageAsync(computeResource, context.CancellationToken);
        }
    }
}, dependsOn: WellKnownPipelineSteps.Build);

// Has ContainerImageAnnotation, no build tag or push
builder.AddRedis("redis-cache");

// Has Dockerfile annotation, build, tag, and push owned by Aspire via Docker/Podman
var userName = Environment.UserName;
builder.AddDockerfile("go-app", "./goapp")
    .WithHttpEndpoint(port: 8080, targetPort: 8080)
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-go-app";
    })
    .WithContainerRegistry(registry);

// .NET project, .NET SDK owns build and tag, Aspire owns push
builder.AddProject<Projects.api>("api")
    .WithContainerRegistry(registry);

// Python app serving as host for front-end, built and tagged separately, only Python image is pushed
// Front-end image produces assets needed by backend image
var viteApp = builder.AddViteApp("viteapp", "./viteapp");
var pythonApp = builder.AddUvicornApp("pythonapp", "./pythonapp", "main:app")
    .WithHttpHealthCheck("/api/health")
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-pythonapp";
    })
    .WithContainerRegistry(registry);

pythonApp.PublishWithContainerFiles(viteApp, "./static");

// Independent Python and Vite apps. Both images built, tagged, and pushed separately
builder.AddUvicornApp("pythonapp-standalone", "./pythonapp-standalone", "main:app")
    .WithHttpHealthCheck("/api/health")
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-pythonapp-standalone";
    })
    .WithContainerRegistry(registry);
builder.AddViteApp("viteapp-standalone", "./viteapp-standalone")
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-viteapp-standalone";
    })
    .WithContainerRegistry(registry);

builder.Build().Run();