using NServiceBus.Hosting.Roles;
using NServiceBus.Unicast.Config;

namespace NServiceBus.Hosting.Windows.Roles.Handlers
{
    using Unicast.Queuing;
    using NServiceBus.Config;
    /// <summary>
    /// Handles configuration related to the server role
    /// </summary>
    public class ServerRoleHandler : IConfigureRole<AsA_Server>
    {
        /// <summary>
        /// Configures the UnicastBus with typical settings for a server
        /// </summary>
        /// <param name="specifier"></param>
        /// <returns></returns>
        public ConfigUnicastBus ConfigureRole(IConfigureThisEndpoint specifier)
        {
         
            return Configure.Instance
                .Sagas()
                .PurgeOnStartup(false)
                .UnicastBus()
                .ImpersonateSender(true);
        }
    }
}