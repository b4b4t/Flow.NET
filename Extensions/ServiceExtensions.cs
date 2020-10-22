using Flow.Store;
using Flow.Store.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Flow.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Add Flow in the service collection.
        /// </summary>
        /// <param name="serviceCollection">Service collection</param>
        /// <param name="stores">Collection of stores</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddFlow(this IServiceCollection serviceCollection, ICollection<IStore> stores = null)
        {
            // Create store container
            StoreContainer storeContainer = new StoreContainer();

            if (stores != null)
            {
                // Register stores in the store container
                foreach (IStore store in stores)
                {
                    storeContainer.Register(store);
                }
            }

            // Add singleton
            serviceCollection.AddSingleton<StoreContainer>(storeContainer);

            return serviceCollection;
        }
    }
}
