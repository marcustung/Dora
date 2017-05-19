﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Dora.ExceptionHandling;
using Dora.ExceptionHandling.Interception;
using Dora.ExceptionHandling.Logging;

namespace Demo1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<IFoobar, Foobar>()
                .AddExceptionHandling(managerBuilder=>managerBuilder
                    .SetDefaultPolicy("default")
                    .AddPolicy("default", policyBuilder=>policyBuilder
                        .Configure(handlerBuilder=>handlerBuilder.Use<LoggingHandler>("Exception Handling", new Func<ExceptionContext, string>(Format))
                            .Use<ReplaceHandler>(typeof(Exception), "xxx"))))
                .AddMvc();
            return services.BuilderInterceptableServiceProvider(builder=>builder.SetDynamicProxyFactory());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole( LogLevel.Error);
            app
                .UseDeveloperExceptionPage()
                .UseMvc();
        }

        private string Format(ExceptionContext context)
        {
            return context.Exception.Message;
        }
    }

    public class HomeController : Controller
    {
        private IFoobar _foobar;
        public HomeController(IFoobar foobar)
        {
            _foobar = foobar;
        }

        [HttpGet("/")]
        public async Task<string> Index()
        {
            await _foobar.InvokeAsync();
            return "Hello world";
        }
    }

    public interface IFoobar
    {
        Task InvokeAsync();
    }

    [HandleException("default")]
    public class Foobar : IFoobar
    {
        public Task InvokeAsync()
        {
            return Task.FromException(new InvalidOperationException("Manually thrown exception."));
        }
    }
}
