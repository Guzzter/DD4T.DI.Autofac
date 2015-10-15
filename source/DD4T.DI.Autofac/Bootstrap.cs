﻿using Autofac;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.ContentModel.Contracts.Configuration;
using DD4T.ContentModel.Contracts.Logging;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Contracts.Resolvers;
using DD4T.ContentModel.Factories;
using DD4T.Core.Contracts.ViewModels;
using DD4T.DI.Autofac.Exceptions;
using DD4T.Factories;
using DD4T.Utils;
using DD4T.Utils.Caching;
using DD4T.Utils.Logging;
using DD4T.Utils.Resolver;
using DD4T.ViewModels;
using DD4T.ViewModels.Reflection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DD4T.DI.Autofac
{
    public static class Bootstrap
    {
        public static void UseDD4T(this ContainerBuilder builder)
        {
            //not all dll's are loaded in the app domain. we will load the assembly in the appdomain to be able map the mapping
            var binDirectory = string.Format(@"{0}\bin\", AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(binDirectory))
                return;

            var file = Directory.GetFiles(binDirectory, "DD4T.Providers.*").FirstOrDefault();
            if (file == null)
                throw new ProviderNotFoundException();

            var load = Assembly.LoadFile(file);

            var provider = AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.FullName.StartsWith("DD4T.Providers")).FirstOrDefault();
            var providerTypes = provider.GetTypes();
            var pageprovider = providerTypes.Where(a => typeof(IPageProvider).IsAssignableFrom(a)).FirstOrDefault();
            var cpProvider = providerTypes.Where(a => typeof(IComponentPresentationProvider).IsAssignableFrom(a)).FirstOrDefault();
            var linkProvider = providerTypes.Where(a => typeof(ILinkProvider).IsAssignableFrom(a)).FirstOrDefault();
            var binaryProvider = providerTypes.Where(a => typeof(IBinaryProvider).IsAssignableFrom(a)).FirstOrDefault();
            var componentProvider = providerTypes.Where(a => typeof(IComponentProvider).IsAssignableFrom(a)).FirstOrDefault();
            var commonServices = providerTypes.Where(a => typeof(IProvidersCommonServices).IsAssignableFrom(a)).FirstOrDefault();

            builder.RegisterType<DD4TConfiguration>().As<IDD4TConfiguration>().SingleInstance().PreserveExistingDefaults();
            builder.RegisterType<DefaultPublicationResolver>().As<IPublicationResolver>().SingleInstance().PreserveExistingDefaults();
            builder.RegisterType<DefaultLogger>().As<ILogger>().SingleInstance().PreserveExistingDefaults();

            builder.RegisterType<DefaultCacheAgent>().As<ICacheAgent>().PreserveExistingDefaults();

            
            builder.RegisterRestProviderTypes();
            builder.RegisterMVCTypes();

            if (commonServices != null)
                builder.RegisterType(commonServices).As<IProvidersCommonServices>().PreserveExistingDefaults();
            if (pageprovider != null)
                builder.RegisterType(pageprovider).As<IPageProvider>().PreserveExistingDefaults();
            if (cpProvider != null)
                builder.RegisterType(cpProvider).As<IComponentPresentationProvider>().PreserveExistingDefaults();
            if (binaryProvider != null)
                builder.RegisterType(binaryProvider).As<IBinaryProvider>().PreserveExistingDefaults();
            if (linkProvider != null)
                builder.RegisterType(linkProvider).As<ILinkProvider>().PreserveExistingDefaults();
            if (componentProvider != null)
                builder.RegisterType(componentProvider).As<IComponentProvider>().PreserveExistingDefaults();


            builder.RegisterType<FactoryCommonServices>().As<IFactoryCommonServices>().PreserveExistingDefaults();
            builder.RegisterType<PageFactory>().As<IPageFactory>().PreserveExistingDefaults();
            builder.RegisterType<ComponentPresentationFactory>().As<IComponentPresentationFactory>().PreserveExistingDefaults();
            builder.RegisterType<ComponentFactory>().As<IComponentFactory>().PreserveExistingDefaults();
            builder.RegisterType<BinaryFactory>().As<IBinaryFactory>().PreserveExistingDefaults();
            builder.RegisterType<LinkFactory>().As<ILinkFactory>().PreserveExistingDefaults();

            //viewmodels
            builder.RegisterType<ReflectionOptimizer>()
                 .As<IReflectionHelper>()
                 .SingleInstance()
                 .PreserveExistingDefaults();

            builder.RegisterType<DefaultViewModelResolver>()
               .As<IViewModelResolver>()
               .SingleInstance()
               .PreserveExistingDefaults(); 

            builder.RegisterType<ViewModelFactory>()
               .As<IViewModelFactory>()
               .SingleInstance()
               .PreserveExistingDefaults();

            builder.RegisterType<WebConfigViewModelKeyProvider>()
             .As<IViewModelKeyProvider>()
             .SingleInstance()
             .PreserveExistingDefaults();

            //caching JMS
            builder.RegisterType<JMSMessageProvider>()
                .As<IMessageProvider>()
                .SingleInstance()
                .PreserveExistingDefaults();
        }
    }
}
