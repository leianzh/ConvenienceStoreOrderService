using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire;
using Unity;

namespace ConvenienceStoreOrderService.App_Start
{
    public class UnityJobActivator : JobActivator
    {
        private readonly IUnityContainer _container;

        public UnityJobActivator(IUnityContainer container)
        {
            _container = container;
        }
        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new UnityJobActivatorScope(_container.CreateChildContainer());
        }

        private class UnityJobActivatorScope : JobActivatorScope
        {
            private readonly IUnityContainer _childContainer;

            public UnityJobActivatorScope(IUnityContainer childContainer)
            {
                _childContainer = childContainer;
            }

            public override object Resolve(Type type)
            {
                return _childContainer.Resolve(type);
            }

            public override void DisposeScope()
            {
                _childContainer.Dispose();
            }
        }
        //public override object ActivateJob(Type jobType)
        //{
        //    return _container.Resolve(jobType);
        //}
    }
}