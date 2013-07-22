using System;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using Discussions.DbModel;

namespace DiscSvc
{
    [System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class DiscSvc : DataService<DiscCtx>, IServiceProvider
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            // Examples:
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.UseVerboseErrors = true;
            config.SetEntitySetAccessRule("Discussion", EntitySetRights.All);
            config.SetEntitySetAccessRule("Attachment",
                                          EntitySetRights.ReadMultiple |
                                          EntitySetRights.ReadSingle |
                                          EntitySetRights.AllWrite);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof (IDataServiceStreamProvider))
            {
                // Return the stream provider to the data service.
                return new AttachmentMediaStreamProvider(this.CurrentDataSource);
            }
            return null;
        }
    }
}