namespace NServiceBus
{
    using System;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Routing;

    class DetermineRoutingForMessageBehavior : Behavior<OutgoingContext>
    {
        readonly string localAddress;
        readonly MessageRouter messageRouter;
  
        public DetermineRoutingForMessageBehavior(string localAddress,MessageRouter messageRouter)
        {
            this.localAddress = localAddress;
            this.messageRouter = messageRouter;
        }

        public override void Invoke(OutgoingContext context, Action next)
        {
            RoutingStrategy routingStrategy = null;

            MessageIntentEnum intent = MessageIntentEnum.Send;

            if (context.IsSend())
            {
                var state = context.Extensions.GetOrCreate<State>();

                var destination = state.ExplicitDestination;

                if (string.IsNullOrEmpty(destination))
                {
                    if (state.RouteToLocalInstance)
                    {
                        destination = localAddress;
                    }
                    else
                    {
                        if (!messageRouter.TryGetRoute(context.MessageType, out destination))
                        {
                            throw new InvalidOperationException("No destination specified for message: " + context.MessageType);
                        }
                    }
                }

                routingStrategy = new DirectToTargetDestination(destination);
                intent = MessageIntentEnum.Send;
  
            }
            if (context.IsPublish())
            {
                routingStrategy = new ToAllSubscribers(context.MessageType);
                intent = MessageIntentEnum.Publish;
            }

            if (context.IsReply())
            {
                //var sendOptions = new SendMessageOptions(MessageBeingProcessed.ReplyToAddress);
                intent = MessageIntentEnum.Reply;
            }

            if (routingStrategy == null)
            {
                throw new Exception("No routing strategy could be determined for message");
            }

            context.SetHeader(Headers.MessageIntent,intent.ToString());

            context.Set(routingStrategy);

            next();
        }


        //string GetDestinationForSend(Type messageType)
        //{
        //    var destinations = GetAtLeastOneAddressForMessageType(messageType);

        //    if (destinations.Count > 1)
        //    {
        //        throw new InvalidOperationException("Sends can only target one address.");
        //    }

        //    return destinations.SingleOrDefault();
        //}


        //var operationType = transportOperation.Options["Operation"];

        //switch (operationType)
        //{
        //    case "Audit":
        //        defaultAuditer.Audit(new TransportSendOptions(transportOperation.Options["Destination"],null,false,false), message);
        //        break;
        //    case "Send":
        //        defaultDispatcher.NativeSendOrDefer(transportOperation.Options["Destination"],deliveryOptions, message);
        //        break;
        //    case "Publish":

        //        var options= new TransportPublishOptions(Type.GetType(transportOperation.Options["EventType"]));

        //        defaultDispatcher.NativePublish(options, message);
        //        break;
        //    default:
        //        throw new InvalidOperationException("Unknown operation type: " + operationType);
        //}

        public class State
        {
            public string ExplicitDestination { get; set; }
            public bool RouteToLocalInstance { get; set; }
        }


    }


    //public void NativePublish(TransportPublishOptions publishOptions, OutgoingMessage message)
    // {
    //     SetTransportHeaders(publishOptions.TimeToBeReceived, publishOptions.NonDurable, message);

    //     try
    //     {
    //         Publish(message, publishOptions);
    //     }
    //     catch (QueueNotFoundException ex)
    //     {
    //         var messageDescription = "ControlMessage";

    //         string enclosedMessageTypes;

    //         if (message.Headers.TryGetValue(Headers.EnclosedMessageTypes, out enclosedMessageTypes))
    //         {
    //             messageDescription = enclosedMessageTypes;
    //         }
    //         throw new Exception(string.Format("The destination queue '{0}' could not be found. You may have misconfigured the destination for this kind of message ({1}) in the MessageEndpointMappings of the UnicastBusConfig section in your configuration file. " + "It may also be the case that the given queue just hasn't been created yet, or has been deleted.", ex.Queue, messageDescription), ex);
    //     }
    // }

    // public void NativeSendOrDefer(string destination,DeliveryMessageOptions deliveryMessageOptions, OutgoingMessage message)
    // {
    //     SetTransportHeaders(deliveryMessageOptions.TimeToBeReceived, deliveryMessageOptions.NonDurable, message);

    //     try
    //     {
    //         SendOrDefer(destination,message, deliveryMessageOptions as SendMessageOptions);
    //     }
    //     catch (QueueNotFoundException ex)
    //     {
    //         var messageDescription = "ControlMessage";

    //         string enclosedMessageTypes;

    //         if (message.Headers.TryGetValue(Headers.EnclosedMessageTypes, out enclosedMessageTypes))
    //         {
    //             messageDescription = enclosedMessageTypes;
    //         }
    //         throw new Exception(string.Format("The destination queue '{0}' could not be found. You may have misconfigured the destination for this kind of message ({1}) in the MessageEndpointMappings of the UnicastBusConfig section in your configuration file. " + "It may also be the case that the given queue just hasn't been created yet, or has been deleted.", ex.Queue, messageDescription), ex);
    //     }
    // }

    // void SetTransportHeaders(TimeSpan? timeToBeReceived, bool? nonDurable, OutgoingMessage message)
    // {
    //     message.Headers[Headers.MessageId] = message.MessageId;


    //     if (timeToBeReceived.HasValue)
    //     {
    //         message.Headers[Headers.TimeToBeReceived] = timeToBeReceived.Value.ToString("c");
    //     }

    //     if (nonDurable.HasValue && nonDurable.Value)
    //     {
    //         message.Headers[Headers.NonDurableMessage] = true.ToString();
    //     }
    // }

    // void SendOrDefer(string destination,OutgoingMessage message, SendMessageOptions options)
    // {
    //     if ((options.DelayDeliveryFor.HasValue && options.DelayDeliveryFor > TimeSpan.Zero) ||
    //         (options.DeliverAt.HasValue && options.DeliverAt.Value.ToUniversalTime() > DateTime.UtcNow))
    //     {
    //         SetIsDeferredHeader(message.Headers);
    //         MessageDeferral.Defer(message, new TransportDeferOptions(
    //             destination,
    //             options.DelayDeliveryFor,
    //             options.DeliverAt,
    //             options.NonDurable ?? true,
    //             options.EnlistInReceiveTransaction));

    //         return;
    //     }

    //     MessageSender.Send(message, new TransportSendOptions(destination,
    //                                                             options.TimeToBeReceived,
    //                                                             options.NonDurable ?? true,
    //                                                             options.EnlistInReceiveTransaction));
    // }

    // static void SetIsDeferredHeader(Dictionary<string, string> headers)
    // {
    //     headers[Headers.IsDeferredMessage] = true.ToString();
    // }

    // void Publish(OutgoingMessage message, TransportPublishOptions publishOptions)
    // {
    //     if (MessagePublisher == null)
    //     {
    //         throw new InvalidOperationException("No message publisher has been registered. If you're using a transport without native support for pub/sub please enable the message driven publishing feature by calling config.EnableFeature<MessageDrivenSubscriptions>() in your configuration");
    //     }
    //     MessagePublisher.Publish(message, publishOptions);
    // }
}