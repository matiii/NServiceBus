namespace NServiceBus
{
    using System;
    using NServiceBus.Encryption;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Unicast.Transport;

    class DecryptBehavior : LogicalMessageProcessingStageBehavior
    {
        EncryptionMutator messageMutator;

        public DecryptBehavior(EncryptionMutator messageMutator)
        {
            this.messageMutator = messageMutator;
        }
        public override void Invoke(Context context, Action next)
        {
            if (TransportMessageExtensions.IsControlMessage(context.Headers))
            {
                next();
                return;
            }
            var current = context.IncomingLogicalMessage.Instance;
            current = messageMutator.MutateIncoming(current);
            context.IncomingLogicalMessage.UpdateMessageInstance(current);
            next();
        }

        public class DecryptRegistration : RegisterStep
        {
            public DecryptRegistration()
                : base("InvokeDecryption", typeof(DecryptBehavior), "Invokes the decryption logic")
            {
                InsertBefore(WellKnownStep.MutateIncomingMessages);
            }

        }
    }
}