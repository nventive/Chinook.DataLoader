# Chinook.DataLoader
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-3-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

Async data loading made visually pleasant!

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

## Getting Started

Install the latest version of `Chinook.DataLoader.Uno` in your project.

Create a `IDataLoader` using the builder (see below on how to use with [Chinook.DynamicMvvm](https://github.com/nventive/Chinook.DynamicMvvm)).

```csharp
var dataLoader = DataLoaderBuilder<MyData>
  .Empty
  .WithName("MyDataLoader")
  .WithLoadMethod(LoadData)
  .Build();

Task<MyData> LoadData(CancellationToken ct)
{
  // Your async operation here.
}
```

Add a `DataLoaderView` in your xaml.

```xml
<!-- Add the DataLoaderView namespace: xmlns:dl="using:Chinook.DataLoader"> -->
<dl:DataLoaderView Source="{Binding MyDataLoader}" />
```

As the task is being executed, the `IDataLoader` will call its `StateChanged` event which will be used by `DataLoaderView` to visually represent the state of the async operation.

## Features

### Async data loading

The loading method of the `IDataLoader` must simply return a `Task` of the data type (here `MyData`).

```csharp
Task<MyData> LoadData(CancellationToken ct)
{
  // Your async operation here.
}
```

This is where you would typically execute an API call or get data from any other async source.

This method receives a `CancellationToken` as the operation might be cancelled.
- If the `IDataLoader` is disposed.
- If another request was made and the concurrent mode is `CancelPrevious`.

### Data loader states

At any given time, the `IDataLoader` can be in a combination of the following states.

|   State   | Description                            |
|:---------:|:---------------------------------------|
| IsLoading | Is the data loader executing a request |
|   Data    | Last data from a successful request    |
|   Error   | Error if the request failed (if any)   |

Imagine a scenario where you land on a page where the data has never been loaded before from an API.

1. The system would first be on an initial state (not loading, no data, no error).
2. The system would then start loading the data from the API.
3. The system would then show the data returned from the API.
4. You lose connection and refresh the page.
5. The system notifies you that there was an error, but you still have your previous data.

This could be represented by the following state flow.

```
#1                  #2                 #3                  #4                 #5
IsLoading(false) -> IsLoading(true) -> IsLoading(false) -> IsLoading(true) -> IsLoading(false)
Data(null)       -> Data(null)      -> Data(myData)     -> Data(myData)    -> Data(myData)
Error(null)      -> Error(null)     -> Error(null)      -> Error(null)     -> Error(WebException)
```

_Note: The error is cleared as soon as there is a successful request_

Other useful states are available:

|   State   | Description                         |
|:---------:|:------------------------------------|
| IsInitial | Was a request ever made             |
|  IsEmpty  | Should the data be considered empty |

### Triggers

As the data might need to be updated, you can trigger a new load request on a `IDataLoader` using triggers.

|           Trigger           | Description                                                   |
|:---------------------------:|:--------------------------------------------------------------|
|   ManualDataLoaderTrigger   | Manually trigger a load request                               |
| ObservableDataLoaderTrigger | Trigger a load request when the observable pushes a new value |

You can create your own triggers by inheriting from `DataLoaderTriggerBase`.

Simply call `RaiseLoadRequested` when a load request should be requested.

_Note: `DataLoaderView` has a `AutoLoad` property which, as its name suggests, automatically load the data of the `DataLoader` when it's displayed._

### Requests and contexts

To provide metadata on the loading operation, two important pieces are used.

|      Metadata      | Description                                        |
|:------------------:|:---------------------------------------------------|
| IDataLoaderRequest | Request to load data loading operation             |
| IDataLoaderContext | Metadata on the loading operation (key-value pair) |

You can get additional information from the `IDataLoader` by adding the `IDataLoaderRequest` parameter.

```csharp
Task<MyData> LoadData(CancellationToken ct, IDataLoaderRequest request)
{
  // This is the trigger that caused that loading operation.
  var sourceTrigger = request.SourceTrigger;

  // This is the context that was passed to the loading operation from the trigger.
  var context = request.Context;

  // Your async operation here.
}
```

The request is also available as part of the `IDataLoaderState`.

### DataLoaderView

The `DataLoaderView` represents the different states of the `IDataLoader` using the following visual states.

|     Group      | State                      |
|:--------------:|:---------------------------|
|   DataStates   | Initial                    |
|   DataStates   | Data                       |
|   DataStates   | Empty                      |
|  ErrorStates   | NoError                    |
|  ErrorStates   | Error                      |
| LoadingStates  | NotLoading                 |
| LoadingStates  | Loading                    |
| CombinedStates | Initial_NoError_NotLoading |
| CombinedStates | Initial_NoError_Loading    |
| CombinedStates | Initial_Error_NotLoading   |
| CombinedStates | Initial_Error_Loading      |
| CombinedStates | Data_NoError_NotLoading    |
| CombinedStates | Data_NoError_Loading       |
| CombinedStates | Data_Error_NotLoading      |
| CombinedStates | Data_Error_Loading         |
| CombinedStates | Empty_NoError_NotLoading   |
| CombinedStates | Empty_NoError_Loading      |
| CombinedStates | Empty_Error_NotLoading     |
| CombinedStates | Empty_Error_Loading        |

You can easily define the template displayed in these states by using the following template properties.

|         Template          | Description |
|:-------------------------:|:------------|
|      ContentTemplate      | Template used when there is data to show     |
|       EmptyTemplate       | Template used when there is no data to show        |
|       ErrorTemplate       | Template used when an error occurred       |
| ErrorNotificationTemplate | Template used when an error occured by there is data to show     |

To avoid seeing flickery transitions between the different visual states, the `DataLoaderView` supports different throttling options.

|          Options           | Description                                                                                                                     |
|:--------------------------:|:--------------------------------------------------------------------------------------------------------------------------------|
|    StateMinimumDuration    | The minimum duration a visual state should be in                                                                                |
| StateChangingThrottleDelay | Delay before going to a new visual state (e.g. if the loading is really quick, you probably don't want to show a loading state) |

```xml
<dl:DataLoaderView Source="{Binding MyDataLoader}"
                   StateMinimumDuration="0:0:1.5"
                   StateChangingThrottleDelay="0:0:0.100" />
```

### DynamicMvvm

Extension methods are available on `Chinook.DynamicMvvm` to create data loaders with refresh commands.

Simply add the `Chinook.DataLoader.DynamicMvvm` package to your project.

```csharp
public class MyViewModel : ViewModelBase
{
  public IDynamicCommand RefreshData => this.GetCommandFromDataLoaderRefresh(Data);

  public IDataLoader Data => this.GetDataLoader(GetData, b => b
    // You can optionally configure your data loader here
  );

  private async Task<MyData> GetData(CancellationToken ct)
  {
    // Get data from your API...
  }
}
```

### Code Snippets

You can install the Visual Studio Extension [Chinook Snippets](https://marketplace.visualstudio.com/items?itemName=nventivecorp.ChinookSnippets) and use code snippets to quickly generate data loaders when using [Chinook.DynamicMvvm](https://github.com/nventive/Chinook.DynamicMvvm).
All snippets related to `IDataLoader` start with `ckdl`.

## Changelog

Please consult the [CHANGELOG](CHANGELOG.md) for more information about version
history.

## License

This project is licensed under the Apache 2.0 license - see the
[LICENSE](LICENSE) file for details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for
contributing to this project.

Be mindful of our [Code of Conduct](CODE_OF_CONDUCT.md).

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://github.com/jeanplevesque"><img src="https://avatars3.githubusercontent.com/u/39710855?v=4" width="100px;" alt=""/><br /><sub><b>Jean-Philippe Lévesque</b></sub></a><br /><a href="https://github.com/nventive/Chinook.DataLoader/commits?author=jeanplevesque" title="Code">💻</a> <a href="https://github.com/nventive/Chinook.DataLoader/commits?author=jeanplevesque" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://github.com/jeremiethibeault"><img src="https://avatars3.githubusercontent.com/u/5444226?v=4" width="100px;" alt=""/><br /><sub><b>Jérémie Thibeault</b></sub></a><br /><a href="https://github.com/nventive/Chinook.DataLoader/commits?author=jeremiethibeault" title="Code">💻</a> <a href="https://github.com/nventive/Chinook.DataLoader/commits?author=jeremiethibeault" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://github.com/MatFillion"><img src="https://avatars0.githubusercontent.com/u/7029537?v=4" width="100px;" alt=""/><br /><sub><b>Mathieu Fillion</b></sub></a><br /><a href="https://github.com/nventive/Chinook.DataLoader/commits?author=MatFillion" title="Code">💻</a> <a href="https://github.com/nventive/Chinook.DataLoader/commits?author=MatFillion" title="Tests">⚠️</a></td>
  </tr>
</table>

<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->
<!-- ALL-CONTRIBUTORS-LIST:END -->
<!-- ALL-CONTRIBUTORS-LIST:END -->
