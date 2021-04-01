# . NET Core clients

## Usage

### Install

The clients for .NET Core are available as a NuGet package (requires netstandard2.0 or higher):

| Package                                                                               | Release version                                                              |
| ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| [Tributech.Dsk.Api.Clients](https://www.nuget.org/packages/Tributech.Dsk.Api.Clients) | ![Release version](https://img.shields.io/nuget/v/Tributech.Dsk.Api.Clients) |

### Authentication / Authorization

You can use the included [APIAuthHandler](./APIAuthHandler.cs) as demonstrated in [the .NET Core Usage Example](../../examples/netcore).

```csharp
var authHandler = new APIAuthHandler(tokenUrl, scope, clientId, clientSecret);
```

You will need to provide the following parameters:
| Parameter | Value | Remark |
|-|-|-|
| tokenUrl | https://auth.your-hub.dataspace-hub.com/auth/realms/your-node/protocol/openid-connect/token | Url to retrieve the access token from the dataspace hub Identity Server |
| scope | profile / email / data-api / trust-api / node-id | Defines the scope of what can be accessed: Depending on which Api you wish to access either data-api (Data API) or data-api and trust-api (Trust API) |
| clientId | your-api-specific-client-id | Can be found in the Dataspace Admin (Profile -> Administration)
| clientSecret | your-api-specific-client-secret | Can be found in the Dataspace Admin (Profile -> Administration)

This authHandler instance can then be used to create a HttpClient

```csharp
var authorizedHttpClient = new HttpClient(authHandler);
```

### Interacting with the Apis

Create a DataAPIClient or TrustAPIClient using the authorizedHttpClient from the previous step. Note that you have to pass in the baseUrl.

| Parameter | Value                                                                                              | Remark                                                                    |
| --------- | -------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| baseUrl   | http://data-api.your-node.dataspace-node.com / http://trust-api.your-node.dataspace-node.com | Data Api / Trust Api Endpoint Url for the node you wish to integrate with |

```csharp
var apiClient = new DataAPIClient(baseUrl, authorizedHttpClient);

// or for the Trust Api:
var trustApiClient = new TrustAPIClient(baseUrl, authorizedHttpClient);
```

Now you can access all methods of the respective Api client like following:

```csharp
// Get values within the last 7 days
ICollection<ReadValueDoubleModel> data = await apiClient.GetValuesAsDoubleAsync(dataStreamId, DateTime.Now, DateTime.Now.AddDays(-7), fromSyncNumber: null, "asc", pageNumber: null, pageSize: null);
```
