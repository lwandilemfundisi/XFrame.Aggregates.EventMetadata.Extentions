using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using XFrame.Aggregates.Events;
using XFrame.Common.Extensions;
using XFrame.DomainContainers;

namespace XFrame.Aggregates.EventMetadata.Extentions
{
    public static class DomainContainerMetadataProvidersExtensions
    {
        public static IDomainContainer AddMetadataProvider<TMetadataProvider>(
            this IDomainContainer domainContainer)
            where TMetadataProvider : class, IMetadataProvider
        {
            domainContainer
                .ServiceCollection
                .AddTransient<IMetadataProvider, TMetadataProvider>();

            return domainContainer;
        }

        public static IDomainContainer AddMetadataProviders(
            this IDomainContainer domainContainer,
            params Type[] metadataProviderTypes)
        {
            return domainContainer
                .AddMetadataProviders((IEnumerable<Type>)metadataProviderTypes);
        }

        public static IDomainContainer AddMetadataProviders(
            this IDomainContainer domainContainer,
            Assembly fromAssembly,
            Predicate<Type> predicate = null)
        {
            predicate = predicate ?? (t => true);
            var metadataProviderTypes = fromAssembly
                .GetTypes()
                .Where(IsMetadataProvider)
                .Where(t => !t.HasConstructorParameterOfType(IsMetadataProvider))
                .Where(t => predicate(t));
            return domainContainer.AddMetadataProviders(metadataProviderTypes);
        }

        public static IDomainContainer AddMetadataProviders(
            this IDomainContainer domainContainer,
            IEnumerable<Type> metadataProviderTypes)
        {
            foreach (var t in metadataProviderTypes)
            {
                if (t.GetTypeInfo().IsAbstract) continue;
                if (!t.IsMetadataProvider())
                {
                    throw new ArgumentException($"Type '{t.PrettyPrint()}' is not an '{typeof(IMetadataProvider).PrettyPrint()}'");
                }

                domainContainer.ServiceCollection.AddTransient(typeof(IMetadataProvider), t);
            }
            return domainContainer;
        }

        private static bool IsMetadataProvider(this Type type)
        {
            return type.IsAssignableTo<IMetadataProvider>();
        }
    }
}
