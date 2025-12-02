#:package Aspire.Hosting.Docker@13.1.0-preview.1.25602.2
#:package Aspire.Hosting.Redis@13.1.0-preview.1.25602.2
#:package Aspire.Hosting.Python@13.1.0-preview.1.25602.2
#:package Aspire.Hosting.JavaScript@13.1.0-preview.1.25602.2
#:sdk Aspire.AppHost.Sdk@13.1.0-preview.1.25602.2
#:project ./api

#pragma warning disable ASPIRECOMPUTE001
#pragma warning disable ASPIRECOMPUTE002
#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREPIPELINES003

using Aspire.Hosting.Pipelines;
using Aspire.Hosting.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var endpointParameter = builder.AddParameter("containerRegistryEndpoint");
var repositoryParameter = builder.AddParameter("containerRegistryRepository");
var registry = builder.AddResource(new ParameterizedContainerRegistry("param-container-registry", endpointParameter.Resource, repositoryParameter.Resource));

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
    .WithAnnotation(new ContainerRegistryReferenceAnnotation(registry.Resource));

// .NET project, .NET SDK owns build and tag, Aspire owns push
builder.AddProject<Projects.api>("api")
    .WithAnnotation(new ContainerRegistryReferenceAnnotation(registry.Resource));

// Python app serving as host for front-end, built and tagged separately, only Python image is pushed
// Front-end image produces assets needed by backend image
var viteApp = builder.AddViteApp("viteapp", "./viteapp");
var pythonApp = builder.AddUvicornApp("pythonapp", "./pythonapp", "main:app")
    .WithHttpHealthCheck("/api/health")
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-pythonapp";
    })
    .WithAnnotation(new ContainerRegistryReferenceAnnotation(registry.Resource));

pythonApp.PublishWithContainerFiles(viteApp, "./static");

// Independent Python and Vite apps. Both images built, tagged, and pushed separately
builder.AddUvicornApp("pythonapp-standalone", "./pythonapp-standalone", "main:app")
    .WithHttpHealthCheck("/api/health")
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-pythonapp-standalone";
    })
    .WithAnnotation(new ContainerRegistryReferenceAnnotation(registry.Resource));
builder.AddViteApp("viteapp-standalone", "./viteapp-standalone")
    .WithContainerBuildOptions(ctx =>
    {
        ctx.LocalImageName = $"{userName}-viteapp-standalone";
    })
    .WithAnnotation(new ContainerRegistryReferenceAnnotation(registry.Resource));

builder.Build().Run();

class ParameterizedContainerRegistry(string name, ParameterResource endpointParameter, ParameterResource repositoryParameter) : Resource(name), IContainerRegistry
{
    ReferenceExpression IContainerRegistry.Name => ReferenceExpression.Create($"{Name}");

    ReferenceExpression IContainerRegistry.Endpoint => ReferenceExpression.Create($"{endpointParameter}");

    // For DockerHub, "captainsafia"
    // For GHCR, "captainsafia/my-repo"
    ReferenceExpression? IContainerRegistry.Repository => ReferenceExpression.Create($"{repositoryParameter}");
}