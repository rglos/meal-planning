using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendarAPI
{
    class GoogleCalendarService
    {
        internal CalendarListEntry GetCalendar(CalendarService calendarService, string name)
        {
            Trace.TraceInformation("GoogleCalendarService.GetCalendar - start");
            CalendarListEntry result = null;
            Trace.TraceInformation("Getting list of calendars");
            var calendarsRequest = calendarService.CalendarList.List();
            var calendars = calendarsRequest.Execute();
            if (calendars.Items != null && calendars.Items.Count > 0)
            {
                Trace.TraceInformation($"Calendars: {calendars.Items.Count}");
            }
            result = calendars.Items.Where(x => x.Summary.Equals(name)).SingleOrDefault();
            Trace.TraceInformation("GoogleCalendarService.GetCalendar - finish");
            return result;
        }

        internal IList<Event> GetAllEvents(CalendarService service, CalendarListEntry calendar)
        {
            Trace.TraceInformation("GoogleCalendarService.GetAllEvents - start");

            var pages = new List<Events>(); // we get the events in batches of 250, so this is the pages of those events
            var result = new List<Event>(); // we assume we only care about the individual events so we'll only return those

            // https://developers.google.com/google-apps/calendar/v3/reference/events/list
            Console.WriteLine($"Calendar Id: {calendar.Id}");

            var complete = false;
            string nextPageToken = null;
            while (!complete)
            {
                var eventsRequest = service.Events.List(calendar.Id);
                eventsRequest.PageToken = nextPageToken;
                // How many get returned?
                //  maxResults: Maximum number of events returned on one result page. By default the value is 250 events. The page size can never be larger than 2500 events. Optional.
                var events = eventsRequest.Execute();

                pages.Add(events);
                result.AddRange(events.Items);

                nextPageToken = events.NextPageToken;
                if (string.IsNullOrEmpty(nextPageToken))
                {
                    complete = true;
                }
            }

            Trace.TraceInformation("GoogleCalendarService.GetAllEvents - finish");

            return result;
        }
    }
}
