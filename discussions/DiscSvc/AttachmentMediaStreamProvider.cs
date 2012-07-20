using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Services;
using System.Data.Services.Providers;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Discussions;
using Discussions.DbModel;
using Discussions.model;

namespace DiscSvc
{
    public class AttachmentMediaStreamProvider : IDataServiceStreamProvider, IDisposable
    {
        private DiscCtx context;
        private Attachment cachedEntity;
        private MemoryStream mediaStream;

        public AttachmentMediaStreamProvider(DiscCtx context)
        {
            this.context = context;
        }

        #region IDataServiceStreamProvider Members

        public void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            var attachment = entity as Attachment;
            if (attachment == null)
            {
                throw new DataServiceException(500, "No such attachment");
            }

            try
            {
                // Delete the requested file by using the key value.
                attachment.Person = null;
                attachment.PersonWithAvatar = null;
                attachment.Discussion = null;
                attachment.ArgPoint = null;
                var mediaData = attachment.MediaData;
                attachment.MediaData = null;
                if (mediaData != null)
                    context.DeleteObject(mediaData);
                context.DeleteObject(attachment);
            }
            catch (IOException ex)
            {
                throw new DataServiceException("Error during attachment removal: ", ex);
            }
        }

        public Stream GetReadStream(object entity, string etag, bool?
            checkETagForEquality, DataServiceOperationContext operationContext)
        {
            if (checkETagForEquality != null)
            {
                // This stream provider implementation does not support 
                // ETag headers for media resources. This means that we do not track 
                // concurrency for a media resource and last-in wins on updates.
                throw new DataServiceException(400,
                    "This sample service does not support the ETag header for a media resource.");
            }

            Attachment attach = entity as Attachment;
            if (attach == null)
            {
                throw new DataServiceException(500, "No such attachment");
            }

            if (attach.MediaData == null || attach.MediaData.Data == null)
            {
                throw new DataServiceException(500, "Cannot find requested media resource");
            }

            return new MemoryStream(attach.MediaData.Data);
        }

        public Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            // Allow the runtime set the URI of the Media Resource.
            return null;
        }

        public string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            // Get the PhotoInfo entity instance.
            Attachment attach = entity as Attachment;
            if (attach == null)
            {
                throw new DataServiceException(500, "Internal Server Error.");
            }

            switch ((AttachmentFormat)attach.Format)
            {
                case AttachmentFormat.Bmp:
                    return "image/x-ms-bmp";
                case AttachmentFormat.Jpg:
                    return "image/jpeg";
                case AttachmentFormat.Png:
                    return "image/x-png";
                case AttachmentFormat.Pdf:
                    return "application/pdf";
                default:
                    return "application/x-unknown";
            }
        }

        public string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            // This sample provider does not support the eTag header with media resources.
            // This means that we do not track concurrency for a media resource 
            // and last-in wins on updates.
            return null;
        }

        public Stream GetWriteStream(object entity, string etag, bool?
            checkETagForEquality, DataServiceOperationContext operationContext)
        {
            if (checkETagForEquality != null)
            {
                // This stream provider implementation does not support ETags associated with BLOBs.
                // This means that we do not track concurrency for a media resource 
                // and last-in wins on updates.
                throw new DataServiceException(400,
                    "This demo does not support ETags associated with BLOBs");
            }

            Attachment attach = entity as Attachment;
            if (attach == null)
            {
                throw new DataServiceException(500, "Internal Server Error: "
                + "the Media Link Entry could not be determined.");
            }
            String contentType = operationContext.RequestHeaders["Content-Type"];
            if (contentType == "image/jpeg")
            {
                attach.Format = (int)AttachmentFormat.Jpg;
            }
            else if (contentType == "application/pdf")
            {
                attach.Format = (int)AttachmentFormat.Pdf;
            }
            else
            {
                attach.Format = (int)AttachmentFormat.None;
            }
            attach.Name = "Name";
            attach.Title = "title";
            attach.Link = "Link";

            if (attach.MediaData == null)
            {
                //if media data doesn't exist, it's POST request and attach is newly created MLE. we create the media data 
                attach.MediaData = new MediaData();
            }

            //handle POST and PUT
            cachedEntity = attach;
            mediaStream = new MemoryStream();
            return mediaStream;
        }

        public string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            // We should only be handling Attachment types.
            if (entitySetName == "Attachment")
            {
                return "DbModel.Attachment";
            }
            else
            {
                // This will raise an DataServiceException.
                return null;
            }
        }

        public int StreamBufferSize
        {
            // Use a buffer size of 64K bytes.
            get { return 64000; }
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (cachedEntity != null && mediaStream != null)
            {
                // Get the new entity from the Entity Framework object state manager.
                ObjectStateEntry entry = this.context.ObjectStateManager.GetObjectStateEntry(cachedEntity);

                if (entry.State == System.Data.EntityState.Unchanged)
                {
                    {
                        var ctx = new DiscCtx(ConfigManager.ConnStr);
                        ctx.Attach(cachedEntity);
                        cachedEntity.MediaData.Data = mediaStream.ToArray();
                        ctx.SaveChanges();

                    }

                }
                else
                {
                    // A problem must have occurred when saving the entity to the database, 
                    // so we should delete the entity
                    var mediaData = cachedEntity.MediaData;
                    cachedEntity.MediaData = null;
                    var ctx = new DiscCtx(ConfigManager.ConnStr);
                    if (mediaData != null)
                    {
                        ctx.Attach(mediaData);
                        ctx.DeleteObject(mediaData);
                    }
                    ctx.Attach(cachedEntity);
                    ctx.DeleteObject(cachedEntity);

                    throw new DataServiceException("An error occurred. The media resource could not be saved.");
                }
            }
        }

        #endregion
    }

}