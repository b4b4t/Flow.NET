# Flow

## :house: Introduction

Flow is a front-end library which provides tools for managing application state and data flow. 
It was designed for being used in the Blazor WASM applications to implement the flux pattern.

## :rocket: Getting started

The library provides a store container that contains stores with their nodes and values.
You need to declare these stores in the service collection of our Blazor WASM application.
Each store implements the `IStore` interface that allows to dispatch an action or get a node value. So you can use the default implementation `DictionaryStoreDefinition` or another custom store definition.

### Installation

Package manager : 

`NuGet\Install-Package Flow.NET -Version 2.1.0`

.NET CLI :
`dotnet add package Flow.NET --version 2.1.0`

### Declare the stores 

To use Flow, you can use the following extension method to add it in the service collection of your application :

`IServiceCollection AddFlow(this IServiceCollection serviceCollection, ICollection<IStore> stores)`

Any store can be added if they implement the `IStore` interface. In these stores, you can declare the nodes that you want to use.

Example :

```cs
List<IStore> stores = new()
{
    new Store("MyStoreIdentifier", new DictionaryStoreDefinition(new string[] { "MyStoreNode" })),
};
services.AddFlow(stores);
```

### Dispatch an action

An action is an object that inherits of the `IAction` interface. We can update the state of the application by dispatching it to a store node.

For the moment, the `StoreAction` is the default implementation of the `IAction` interface and it allows to set the value of a node and to propagate the change to the node subscribers.

Example :

```cs
IStore store = storeContainer.GetStore("MyStoreIdentifier");
var action = new StoreAction("MyStoreIdentifier", "MyStoreNode", valueObject);
store.Dispatch(action);
```

### Get a node value

```cs
IStore store = storeContainer.GetStore("MyStoreIdentifier");
store.GetNodeValue("MyStoreNode") as MyClass;
```

### Update the properties that are connected to the store node

The component properties can subscribe to a store node. If an action is dispatched on this node, the properties are automatically impacted by the update.
You can connect the properties with the following steps :

#### Connect a blazor component to the stores

```cs
@inherits ConnectedComponent
```

#### Connect a blazor component property to a node in a store

```cs
[StoreConnector("MyStore", "MyStoreNode")]
public MyClass MyProperty { get; set; }
```
