﻿using System;
using Bottles;
using Bottles.Services.Remote;
using FubuCore.Binding;
using FubuLocalization;
using FubuMVC.Core;
using FubuMVC.Katana;
using FubuMVC.OwinHost;

namespace Fubu.Running
{
    public class RemoteFubuMvcProxy : IDisposable
    {
        private readonly ApplicationRequest _request;
        private RemoteServiceRunner _runner;

        public RemoteFubuMvcProxy(ApplicationRequest request)
        {
            _request = request;
        }

        public void Start(object listener, Action<RemoteDomainExpression> configuration = null)
        {
            _runner = RemoteServiceRunner.For<RemoteFubuMvcBootstrapper>(x => {
                x.RequireAssemblyContainingType<EmbeddedFubuMvcServer>();
                x.RequireAssemblyContainingType<RemoteFubuMvcProxy>();
                x.RequireAssemblyContainingType<RemoteServiceRunner>();
                x.RequireAssemblyContainingType<Owin.IAppBuilder>();
                x.RequireAssemblyContainingType<IActivator>(); // Bottles
                x.RequireAssemblyContainingType<IModelBinder>(); // FubuCore
                x.RequireAssemblyContainingType<StringToken>(); // FubuLocalization
                x.RequireAssemblyContainingType<FubuApplication>(); // FubuMVC.Core

                x.RequireAssembly("Owin.Extensions");
                x.RequireAssembly("Newtonsoft.Json");
                x.RequireAssembly("FubuMVC.OwinHost");
                x.RequireAssembly("Microsoft.Owin.Hosting");
                x.RequireAssembly("Microsoft.Owin.Host.HttpListener");

                x.ServiceDirectory = _request.DirectoryFlag;

                x.Setup.PrivateBinPath = _request.DetermineBinPath();

                if (configuration != null)
                {
                    configuration(x);
                }

                Console.WriteLine("Assembly bin path is " + x.Setup.PrivateBinPath);
            });

            _runner.WaitForServiceToStart<RemoteFubuMvcBootstrapper>();

            _runner.Messaging.AddListener(listener);

            

            _runner.SendRemotely(new StartApplication
            {
                ApplicationName = _request.ApplicationFlag,
                PhysicalPath = _request.DirectoryFlag,
                PortNumber = PortFinder.FindPort(_request.PortFlag),
                UseProductionMode = _request.ProductionModeFlag
            });
        
        }

        public void Recycle()
        {
            _runner.SendRemotely(new RecycleApplication());
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}