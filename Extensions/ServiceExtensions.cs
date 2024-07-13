using Flow.Store;
using Flow.Store.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Flow.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Add Flow in the service collection.
    /// </summary>
    /// <param name="serviceCollection">Service collection</param>
    /// <param name="stores">Collection of stores</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddFlow(this IServiceCollection serviceCollection, ICollection<IStore> stores)
    {
        ArgumentNullException.ThrowIfNull(stores);

        serviceCollection.AddSingleton(sp => 
        {
            // Create store container
            ILogger<StoreContainer> logger = sp.GetRequiredService<ILogger<StoreContainer>>();
            StoreContainer storeContainer = new(logger);

            // Register stores in the store container
            foreach (IStore store in stores)
            {
                storeContainer.Register(store);
            }

            return storeContainer;
        });

        return serviceCollection;
    }
}
