# Caching

## 5.0.0
- Update for .net 5 applications

## 4.0.2
- fix in CacheService: when data is found in one cache, break out the loop and don't look further

## 4.0.1
- fixed missing distributed cache handler registration when running in development mode with tier 2 cache enabled

## 4.0.0
- Update to .NET standard 2.0

## 3.0.1
- System.Text.Json package upgrade to 5.0.1 (support referenceloophandling)

## 3.0.0
- Changed to .NET standard 2.0
- Added multi-tier caching (tier 1: short lived in-memory caching, tier 2: Redis cache)

## 2.5.1
- .NET Core 2.2 upgrade with newtonsoft

## 0.1.0
- .NET Core 2.2 version without distributed caching

## 2.5.0
- .NET Core 2.2 upgrade

## 2.4.0
- Added configureawait(false)

## 2.3.0
- Distributed caching is now thread safe

## 2.2.2
- Working serialization & deserialization with utf8json

## 2.2.0
- Working serialization & deserialization
- Fixed exception handling

## 2.1.5
- Extended cache logging

## 2.1.3

- Added support for object serialization with utf8 json serialization fallback. 
- Known issue: self referencing loops

## 2.1.0

- Serialization loop fixes.

## 2.0.4

- Add support for non serializable objects.

## 2.0.3

- Added get cache value functionality for distributed cacher.

## 2.0.2

- Added support for nullable return types.

## 2.0.1

- Added error logging when distributed cache is offline.

## 2.0.0

- Added distributed cache support.

## 1.1.0

- Various memorycache bugfixes.

## 1.0.0

- Initial version.
