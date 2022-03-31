using Autofac;
using Autofac.Integration.Mvc;
using BizSol.Tracker.Api.Filters;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Web.Controllers;

namespace BizSol.Tracker.Api
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            if (!isActiveModule && DataSettings.DatabaseIsInstalled())
                return;
            builder.RegisterType<BizSolTrackerFilter>()
                    .AsActionFilterFor<CustomerController>().InstancePerRequest();

        }

        public int Order
        {
            get { return 1; }
        }
    }
}