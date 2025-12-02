# Aspire Container Image Push Demo

This repository demonstrates .NET Aspire's container image building and pushing capabilities using a custom pipeline step. It showcases how to build and push container images for various resource types to a configurable container registry.

## Overview

The AppHost (`apphost.cs`) defines a distributed application with multiple services:

- **Redis Cache** - Standard container image (no build/push)
- **Go App** - Built from Dockerfile, tagged, and pushed via Docker/Podman
- **API** - .NET project built and pushed using the .NET SDK container tooling
- **Python App with Vite Frontend** - Python app that serves static assets from a Vite build
- **Standalone Python App** - Independent Python application
- **Standalone Vite App** - Independent Vite/Node.js application

## Features

- **Parameterized Container Registry** - Registry endpoint and repository are configurable via parameters
- **Custom Pipeline Step** - A `push-images` pipeline step that builds and pushes all compute resources
- **Custom Local Image Names** - Images are tagged with the current user's name for local development
- **Multi-language Support** - Demonstrates Go, .NET, Python, and Node.js/Vite applications

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/aspire-cli)
- [Docker](https://www.docker.com/) or [Podman](https://podman.io/)
- Node.js (for Vite apps)
- Python with uv (for Python apps)
- Go (for the Go app)

## Running Locally

### Run the application

```bash
aspire run 
```

### Build and push images locally

Set the required parameters and run the push-images pipeline step:

```bash
# Optional: Set parameters as environment variables
export Parameters__containerRegistryEndpoint=ghcr.io
export Parameters__containerRegistryRepository=your-username/your-repo

# Run the push-images pipeline step
aspire do push-images
```

For debug output:

```bash
aspire do push-images --log-level debug
```

## Configuration

### Parameters

The application requires two parameters for container registry configuration:

| Parameter | Description | Example |
|-----------|-------------|---------|
| `containerRegistryEndpoint` | The container registry host | `ghcr.io`, `docker.io`, `myregistry.azurecr.io` |
| `containerRegistryRepository` | The repository path within the registry | `captainsafia/my-repo` (for GHCR), `captainsafia` (for Docker Hub) |

### Setting Parameters

Parameters can be set via environment variables using the `Parameters__` prefix:

```bash
export Parameters__containerRegistryEndpoint=ghcr.io
export Parameters__containerRegistryRepository=owner/repo
```

## CI/CD

A GitHub Actions workflow is included at `.github/workflows/push-images.yml` that:

1. Checks out the code
2. Sets up .NET using the `global.json` configuration
3. Installs the Aspire CLI
4. Logs into GitHub Container Registry (GHCR)
5. Pushes all container images to GHCR

The workflow runs on:

- Pushes to `main` branch
- Pull requests to `main` branch
- Manual dispatch

Images are pushed to `ghcr.io/{owner}/{repo}/{image-name}`.

## Project Structure

```text
.
├── apphost.cs                 # Main Aspire AppHost definition
├── apphost.run.json           # Run configuration
├── global.json                # .NET SDK version configuration
├── NuGet.config               # NuGet feed configuration
├── api/                       # .NET API project
├── goapp/                     # Go application with Dockerfile
├── pythonapp/                 # Python app (serves Vite assets)
├── pythonapp-standalone/      # Standalone Python app
├── viteapp/                   # Vite frontend (assets for pythonapp)
├── viteapp-standalone/        # Standalone Vite app
└── .github/workflows/         # GitHub Actions workflows
    └── push-images.yml        # Container image push workflow
```