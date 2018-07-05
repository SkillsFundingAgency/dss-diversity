using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Helpers;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;

namespace NCS.DSS.Diversity.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IGetDiversityHttpTriggerService, GetDiversityHttpTriggerService>();
            services.AddTransient<IPostDiversityHttpTriggerService, PostDiversityHttpTriggerService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
