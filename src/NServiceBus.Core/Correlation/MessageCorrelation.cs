﻿namespace NServiceBus.Features
{
    class MessageCorrelation : Feature
    {
        public MessageCorrelation()
        {
            EnableByDefault();
        }
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register<AttachCorrelationIdBehavior.Registration>();
        }
    }
}