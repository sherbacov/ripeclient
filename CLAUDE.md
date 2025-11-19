# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ripeclient** is a .NET Core client library for interacting with the RIPE Database and hosted RPKI infrastructure. Published as the "RipeClient" NuGet package, it provides three main functional areas:

1. **RIPE Database Client** - Search and CRUD operations for RIPE database objects
2. **LIR Resources Client** - Access to LIR (Local Internet Registry) resource information
3. **RPKI Client** - Management of RPKI ROAs (Route Origin Authorizations)

## Development Commands

### Building
```bash
dotnet restore
dotnet build
dotnet build -c Release
```

### Testing
```bash
# Run all tests
dotnet test

# Note: Tests are integration tests requiring environment variables:
# - LIR_RESOURCES_API_KEY for LIR tests
# Tests are currently disabled in CI/CD pipeline
```

### HTTP Debugging
```csharp
// Enable HTTP tracing for any client
var client = new RipeClient(location, auth);
client.Debug = true;  // Logs all HTTP traffic to console
```

### NuGet Package
```bash
# Package is auto-generated on build (GeneratePackageOnBuild=true)
# CI/CD automatically publishes to NuGet.org on master branch pushes
dotnet build -c Release
```

## Architecture

### Core Client Components

**RipeClient** (`src/ClientsRipe/RipeClient.cs`)
- Primary interface for RIPE Database operations (search, CRUD)
- XML-based REST API communication
- Supports async and sync operations
- Authentication via `IRipeClientAuth` implementations:
  - `RipeClientAuthAnonymous` - Public searches (no credentials)
  - `RipeClientAuthPassword` - Password via query parameter (?password=xxx)
  - `RipeClientAuthApiKey` - API key via X-API-Key header (recommended)
  - `RipeClientAuthBasic` - HTTP Basic Authentication via Authorization header
- Enhanced auth interface: `IRipeClientAuthEnhanced` with `ApplyAuthentication(RestRequest)` method
- Location via `IRipeLocation` implementations:
  - `RipeSecureLocation` - HTTPS endpoint (https://rest.db.ripe.net/)
  - `RipeNonSecureLocation` - HTTP endpoint

**LirResourcesClient** (`src/ClientsRipe/LirResources/LirResourcesClient.cs`)
- Access LIR resources via RIPE NCC API (https://lirportal.ripe.net/myresources/v1/resources)
- API key authentication via `ncc-api-authorization` header
- Returns ASN and IP allocations/assignments

**RipeRpkiClient** (`src/ClientsRipe/RpkiClient/RipeRpkiClient.cs`)
- RPKI ROA management
- Production endpoint: https://my.ripe.net/api/rpki
- Test endpoint: https://localcert.ripe.net/api/rpki
- Operations: Get resources, Get ROAs, Add/Delete ROAs (batch publish)

**RipeRouteManager** (`src/ClientsRipe/RpkiClient/RipeRpkiManager.cs`)
- High-level orchestration layer coordinating RIPE Database + RPKI
- Features:
  - Cache management for ROAs (stored as `ripe.cache.json` in assembly directory)
  - RPKI validation state checking (Valid/Invalid/Unknown)
  - Automatic ROA creation when adding routes
  - Configuration-based setup via `IConfiguration` with pattern: `rpki:keys:{index}`
- Demonstrates multi-API coordination: queries multiple RPKI keys, caches results, correlates routes with ROAs

### Search API

**RipeSearchRequest** (`src/ClientsRipe/RipeSearchRequest.cs`)
- Fluent API for building search queries
- Type filtering using flag-based enum: `TypeFilter.Route | TypeFilter.Route6`
- Available filters: Route, Route6, Inetnum, Inetnum6, Autnum, Person
- Query flags:
  - `AllMore` (-M): All sub-ranges in hierarchies
  - `OneMore` (-m): One level more specific
  - `AllLess` (-L): All encompassing ranges
  - `OneLess` (-l): One level less specific
- Source filtering (default: "ripe")

### Data Models

**RipeObject** (`src/ClientsRipe/DatabaseObjects/RipeObject.cs`)
- Base class for all RIPE database objects
- List of key-value pairs with dynamic accessor: `obj["attribute"]`
- Multi-value support (multiple attributes with same key)
- Date parsing for `created` and `last-modified`
- Maintainer management (`AddMnt`)

**Serialization Strategy**
- XML: Used for RIPE Database write operations (POST/PUT)
- JSON: Used for responses and RPKI operations
- XML namespaces are stripped for cleaner output
- Response wrappers: `RipeObjects` (JSON), `WhoisResources` (XML)

### Exception Handling

Specific exception types for different scenarios:
- `RipeClientException` - General errors
- `RipeClientBadRequestException` - 400 errors with detailed error messages
- `RipeClientNotFoundException` - 404 errors
- `RipeClientConflictException` - 409 conflicts
- `RipeClientAuthPasswordException` - Authentication failures

### Key Dependencies

- `RestSharp` (v111.4.1) - HTTP client (modern async patterns)
- `Newtonsoft.Json` (v13.0.3) - JSON serialization
- `IPNetwork2` (v3.0.667) - IP address/network manipulation (used heavily for CIDR operations)
- `HttpTracer` (v2.1.1) - HTTP debugging
- `NodaTime` (v3.1.11) - Date/time handling
- `Microsoft.Extensions.Configuration.Abstractions` (v8.0.0) - Configuration

## Important Notes

### Namespace Organization
- `ClientsRipe` - Main client code
- `RipeDatabaseObjects` - Data models
- `ClientsRpki` - RPKI functionality

### Framework Targeting
- Main library: .NET 6.0
- Tests: .NET 8.0

### Testing Approach
Tests are integration tests requiring external API keys, not unit tests. They need environment variables configured to run.

### CI/CD Pipeline
- GitHub Actions with .NET workflow
- Triggers on push/PR to master branch
- Auto-publishes to NuGet.org on master builds with version 1.0.{run_number}
- Tests currently disabled in CI

### Authentication Patterns

The library supports four authentication methods for RIPE Database operations:

1. **Anonymous** (`RipeClientAuthAnonymous`) - For public read-only operations
   - No credentials required
   - Used for searches and public data retrieval

2. **Password** (`RipeClientAuthPassword`) - Legacy authentication method
   - Credentials sent as query parameter: `?password=xxx`
   - Suitable for manual operations and legacy systems
   - Lower security (password visible in URLs/logs)

3. **API Key** (`RipeClientAuthApiKey`) - **Recommended for production**
   - Credentials sent via HTTP header: `X-API-Key: xxx`
   - More secure than password authentication
   - Can be easily revoked without changing account password
   - Supports fine-grained permissions
   - Used for RIPE Database write operations

4. **Basic Authentication** (`RipeClientAuthBasic`) - Standard HTTP Basic Auth
   - Credentials sent via HTTP header: `Authorization: Basic base64(username:password)`
   - Suitable for systems requiring standard authentication methods

**Implementation details:**
- All auth classes implement `IRipeClientAuthEnhanced` interface
- `ApplyAuthentication(RestRequest)` method handles credential application
- Backward compatible with old `IRipeClientAuth` interface
- Each method encapsulates its own authentication logic

**Separate authentication for LIR/RPKI services:**
- LIR Resources and RPKI services use `ncc-api-authorization` header
- Different from RIPE Database authentication
- Passed directly as constructor parameters in respective clients

### Cache Management
RPKI ROAs cached in `ripe.cache.json` in assembly directory for performance optimization in multi-API scenarios.
