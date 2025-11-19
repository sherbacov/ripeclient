# ripeclient
.NET Core RIPE Database client

Build status master: [![.NET](https://github.com/sherbacov/ripeclient/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/sherbacov/ripeclient/actions/workflows/dotnet.yml)

## Installation

```bash
dotnet add package RipeClient
```

## Quick Start

```c#
var ripe = new RipeClient(new RipeSecureLocation(),
            new RipeClientAuthAnonymous());

var result = await ripe.Search(new RipeSearchRequest(request.Prefix, TypeFilter.Inetnum));
```

![screenshot](https://github.com/user-attachments/assets/74531318-9bab-4460-851b-227b239ecbdc)

## Authentication

The RIPE Database supports multiple authentication methods. Choose the appropriate method based on your use case.

### Anonymous Authentication

Use anonymous authentication for public read-only operations such as searching the database.

```c#
var auth = new RipeClientAuthAnonymous();
var client = new RipeClient(new RipeSecureLocation(), auth);

// Search for objects
var results = await client.Search(
    new RipeSearchRequest("192.0.2.0/24", TypeFilter.Inetnum)
);
```

### Password Authentication

Traditional authentication method using password query parameter. The password is sent as a query string parameter (`?password=xxx`).

```c#
var auth = new RipeClientAuthPassword("your-password");
var client = new RipeClient(new RipeSecureLocation(), auth);

// Create a new person object
var person = new Person();
person["person"] = "John Doe";
person["address"] = "123 Main St";
person["phone"] = "+1-234-567-8900";
person["nic-hdl"] = "AUTO-1";
person["mnt-by"] = "YOUR-MNT";
person["source"] = "RIPE";

await client.AddObject(person);
```

### API Key Authentication

Modern authentication method using the `X-API-Key` HTTP header. This is the recommended method for automated systems and applications.

**How to get an API key:**
1. Log in to your RIPE NCC Access account
2. Navigate to API Keys management
3. Create a new API key with appropriate permissions

```c#
var auth = new RipeClientAuthApiKey("your-api-key-here");
var client = new RipeClient(new RipeSecureLocation(), auth);

// Update an existing object
var route = await client.GetObjectByKey("192.0.2.0/24", "route", "ripe");
route["descr"] = "Updated description";

await client.UpdateObject(route);
```

**Advantages of API Key authentication:**
- More secure than password authentication
- Can be easily revoked without changing your account password
- Supports fine-grained permissions
- Recommended by RIPE NCC for API access

### Basic Authentication

HTTP Basic Authentication sends credentials as a Base64-encoded `username:password` in the `Authorization` header.

```c#
var auth = new RipeClientAuthBasic("username", "password");
var client = new RipeClient(new RipeSecureLocation(), auth);

// Delete an object
var objectToDelete = await client.GetObjectByKey("PP1-RIPE", "person", "ripe");
await client.RemoveObject(objectToDelete);
```

## Advanced Examples

### Searching with Multiple Filters

```c#
var auth = new RipeClientAuthAnonymous();
var client = new RipeClient(new RipeSecureLocation(), auth);

// Search for both IPv4 and IPv6 routes
var request = new RipeSearchRequest("AS64512");
request.AddFilter(TypeFilter.Route | TypeFilter.Route6);

var results = await client.Search(request);
```

### Search with Flags

```c#
// Find one level more specific
var request = new RipeSearchRequest("192.0.2.0/24", TypeFilter.Inetnum);
request.AddFlag(RipeSearchRequestFlags.OneMore);

var results = await client.Search(request);
```

### HTTP Debugging

Enable HTTP request/response debugging to troubleshoot API calls:

```c#
var client = new RipeClient(new RipeSecureLocation(), auth);
client.Debug = true; // Logs all HTTP traffic to console

var results = await client.Search(request);
```

## Authentication Method Comparison

| Method | Use Case | Security | Sent Via |
|--------|----------|----------|----------|
| **Anonymous** | Public searches | N/A | None |
| **Password** | Legacy systems, manual operations | Low | Query parameter |
| **API Key** | Automated systems, applications | High | HTTP Header (`X-API-Key`) |
| **Basic Auth** | Systems requiring standard auth | Medium | HTTP Header (`Authorization`) |

**Recommendation:** Use **API Key** authentication for production applications and automated systems. Use **Anonymous** for read-only operations.

## Features

- Full CRUD operations for RIPE Database objects
- Multiple authentication methods (Anonymous, Password, API Key, Basic)
- Async/await support
- Type-safe search filters
- HTTP request/response debugging
- LIR Resources client
- RPKI client for ROA management

## Documentation

For more information about RIPE Database API:
- [RIPE Database Documentation](https://www.ripe.net/manage-ips-and-asns/db)
- [API Keys Guide](https://labs.ripe.net/author/ed_shryane/using-api-keys-in-the-ripe-database/)
- [REST API Reference](https://github.com/RIPE-NCC/whois/wiki/WHOIS-REST-API)
