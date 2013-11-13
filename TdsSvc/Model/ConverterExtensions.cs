using Discussions.DbModel;


namespace TdsSvc.Model
{
    public static class ConverterExtensions
    {
        public static SArgPoint ToServiceEntity(this ArgPoint ap)
        {
            return new SArgPoint
            {
                Id = ap.Id,
                Point = ap.Point,
                SideCode = ap.SideCode,
                SharedToPublic = ap.SharedToPublic,
                RecentlyEnteredSource = ap.RecentlyEnteredSource,
                RecentlyEnteredMediaUrl = ap.RecentlyEnteredMediaUrl,
                OrderNumber = ap.OrderNumber,
                PersonId = ap.Person.Id,
                Description = ap.Description.Text,
            };
        }
    }
}