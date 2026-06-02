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

        public override object ActivateJob(Type jobType)
        {
            return _container.Resolve(jobType);
        }
    }
}