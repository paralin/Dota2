Dota2 [![Build Status](https://travis-ci.org/paralin/Dota2.png)](https://travis-ci.org/paralin/Dota2)
---

[![forthebadge](http://forthebadge.com/images/badges/compatibility-betamax.svg)](http://forthebadge.com) [![forthebadge](http://forthebadge.com/images/badges/fuck-it-ship-it.svg)](http://forthebadge.com)

Dota2 is a .NET library designed as a plugin for [SteamKit](http://github.com/SteamRE/SteamKit). It provides a handler for the DOTA 2 game coordinator. The goal is to implement as much functionality of the client as possible.

## Getting Binaries


### Visual Studio

Dota2 is distributed as a [NuGet package](http://nuget.org/packages/dota2).

Simply install SteamKit2 and Dota2 using the package manager in Visual Studio, and NuGet will add all the required dependencies and references to your project.  
  
### Other

We additionally distribute binaries on our [releases page](https://github.com/paralin/Dota2/releases).

For more information on installing SteamKit2 and Dota2, please refer to the [Installation Guide](https://github.com/SteamRE/SteamKit/wiki/Installation) on the SteamKit wiki.


## Documentation

Documentation consists primarily of XML code documentation provided with the binaries. Please see the SteamKit documentation on how to set up a Steam client.

To use the GC handler, it's simple:

```
client = new SteamClient();
DotaGCHandler.Bootstrap(client);
dota = client.GetHandler<DotaGCHandler>();
```

You can register callbacks like any other Steam network functionality from Steamkit.

## License

SteamKit2 and Dota2 (this package) are released under the [LGPL-2.1 license](http://www.tldrlegal.com/license/gnu-lesser-general-public-license-v2.1-%28lgpl-2.1%29).


## Dependencies

In order to compile and use SteamKit2 and Dota2, the following dependencies are required:

  - .NET 4.0 or [Mono ≥2.8](http://mono-project.com)
  - [protobuf-net](http://code.google.com/p/protobuf-net/) ([NuGet package](http://nuget.org/packages/protobuf-net))

Note: If you're using the NuGet package, the protobuf-net dependency _should_ be resolved for you. See the SteamKit [Installation Guide](https://github.com/SteamRE/SteamKit/wiki/Installation) for more information.


## Contact

IRC: [irc.gamesurge.net / #opensteamworks](irc://irc.gamesurge.net/opensteamworks)

