using System.Collections.Generic;
using System.Linq;
using Discussions.DbModel;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct ScreenshotResponse
    {
        //photon cannot handle large messages, so we use DB
        public Dictionary<int, byte[]> screenshots;

        public static ScreenshotResponse Read(Dictionary<byte, object> par)
        {
            var res = new ScreenshotResponse();
            res.screenshots = new Dictionary<int, byte[]>();
            using (var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr))
            {
                var shapeIds = (int[]) par[(byte) DiscussionParamKey.ArrayOfInts];
                for (int i = 0; i < shapeIds.Length; i++)
                {
                    int mediaId = (int) par[(byte) i];
                    var mediaEntity = ctx.MediaDataSet.Single(m => m.Id == mediaId);
                    res.screenshots.Add(shapeIds[i], mediaEntity.Data);
                    ctx.DeleteObject(mediaEntity);
                }
                ctx.SaveChanges(); //deleted entities 
                return res;
            }
        }

        public static Dictionary<byte, object> Write(Dictionary<int, byte[]> screenshots)
        {
            using (var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr))
            {
                var res = new Dictionary<byte, object>();

                //array of integers (shape Ids)          
                var shapeIds = screenshots.Keys.ToArray();
                res.Add((byte) DiscussionParamKey.ArrayOfInts, shapeIds);

                for (int i = 0; i < shapeIds.Length; i++)
                {
                    var md = new MediaData {Data = screenshots[shapeIds[i]]};
                    ctx.MediaDataSet.AddObject(md);
                    ctx.SaveChanges();//save here, need Id
                    res.Add((byte) i, md.Id);
                }                
                return res;
            }
        }
    }
}