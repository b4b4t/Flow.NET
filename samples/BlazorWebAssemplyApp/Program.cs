using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWebAssemplyApp;
using Flow.Extensions;
using Flow.Store;
using Flow.Store.Contracts;
using BlazorWebAssemplyApp.Stores;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;
services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

List<IStore> stores =
[
    // Dictionary store
    new Store("CounterStore", new DictionaryStoreDefinition(["StoreCounterNode"])),
    new Store<TypedStore>("TypedCounterStore"),
];
services.AddFlow(stores);

await builder.Build().RunAsync();
