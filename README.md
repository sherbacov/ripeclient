# ripeclient
.net core RIPE Database client

Build status master: [![.NET](https://github.com/sherbacov/ripeclient/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/sherbacov/ripeclient/actions/workflows/dotnet.yml)


# Examples
```
  var ripe = new RipeClient(new RipeSecureLocation(), 
            new RipeClientAuthAnonymous()); 

  var result = await ripe.Search(new RipeSearchRequest(request.Prefix, TypeFilter.Inetnum));
```

![screenshot](https://github.com/user-attachments/assets/74531318-9bab-4460-851b-227b239ecbdc)
