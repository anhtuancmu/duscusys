using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.stats;

namespace Reporter
{
    public class CsvEventExporter
    {
        public static void Export(string reportPathName,
                                  ExportEventSelectorVM eventsFilter,
                                  List<ReportParameters> reportParams)
        {
            System.IO.File.WriteAllText(reportPathName, Export(eventsFilter, reportParams));
        }

        private static string Export(ExportEventSelectorVM eventsFilter, 
                                     List<ReportParameters> reportParams)
        {
            //write header
            var sb = new StringBuilder();
            sb.Append("Id;");
            sb.Append("Event;");
            sb.Append("EventName;");
            sb.Append("DiscussionId;");
            sb.Append("DiscussionName;");
            sb.Append("TopicId;");
            sb.Append("TopicName;");
            sb.Append("UserId;");
            sb.Append("UserName;");
            sb.Append("Time;");
            sb.Append("DeviceType;");
            sb.AppendLine("DeviceTypeName");

            var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var ev in ctx.StatsEvent)
            {
                var addEvent = false;

                if (ev.Id == 2)
                {
                    int i = 0;
                }

                if(eventsFilter.EventExported((StEvent)ev.Event))
                {
                    if (ev.UserId == -1)
                    {
                        addEvent = true;
                    }
                    else if (EventPassesFilter(ev, reportParams))
                    {
                        addEvent = true;
                    }
                }

                if (addEvent)
                    AddEventRow(sb, ev);
            }

            return sb.ToString();
        }

        static bool EventPassesFilter(StatsEvent ev, List<ReportParameters> reportParams)
        {
            bool userTest = reportParams.Any(p => p.requiredUsers.Contains(ev.UserId));

            if (!userTest)
                return false;

            if (ev.TopicId == -1)
                return true;

            return reportParams.Any(p => p.topic.Id == ev.TopicId);
        }

        private static void AddEventRow(StringBuilder sb, StatsEvent ev)
        {
            var eventView = new EventViewModel((StEvent) ev.Event, ev.UserId, ev.Time, (DeviceType)ev.DeviceType);

            sb.Append(ev.Id);
            sb.Append(";");
            sb.Append(ev.Event);
            sb.Append(";");
            sb.Append("\"" + eventView.evt + "\"");

            sb.Append(";");
            sb.Append(ev.DiscussionId);
            sb.Append(";");
            sb.Append("\"" + ev.DiscussionName + "\"");
            sb.Append(";");
            sb.Append(ev.TopicId);
            sb.Append(";");
            sb.Append("\"" + ev.TopicName + "\"");
            sb.Append(";");
            sb.Append(ev.UserId);
            sb.Append(";");
            sb.Append("\"" + ev.UserName + "\"");
            sb.Append(";");
            sb.Append(ev.Time.ToString("dd.MM.yyyy HH:mm:ss"));
            sb.Append(";");
            sb.Append(ev.DeviceType);
            sb.Append(";");
            sb.AppendLine("\"" + eventView.devType + "\"");
        }
    }
}