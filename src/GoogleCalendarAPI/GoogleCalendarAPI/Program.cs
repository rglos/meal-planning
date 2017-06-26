using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleCalendarAPI
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            //ListNextTenEvents(service);

            //var calendar = GetCalendar("Meals");

            Console.WriteLine("Getting list of calendars...");
            var calendarsRequest = service.CalendarList.List();
            var calendars = calendarsRequest.Execute();
            if (calendars.Items != null && calendars.Items.Count > 0)
            {
                foreach (var calendarItem in calendars.Items)
                {
                    Console.WriteLine($"Summary: {calendarItem.Summary}");

                    if (calendarItem.Summary.Equals("Meals"))
                    {
                        // https://developers.google.com/google-apps/calendar/v3/reference/events/list
                        Console.WriteLine($"Id: {calendarItem.Id}");
                        var eventsRequest = service.Events.List(calendarItem.Id);
                        // Ordering by StartTime throws exception: perhaps we need another parameter?
                        //  The requested ordering is not available for the particular query.
                        //eventsRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                        
                        // How many get returned?
                        //  maxResults: Maximum number of events returned on one result page. By default the value is 250 events. The page size can never be larger than 2500 events. Optional.
                        var events = eventsRequest.Execute();
                        if (events.Items != null && events.Items.Count > 0)
                        {
                            Console.WriteLine($"Events: {events.Items.Count}");
                        }

                        // TODO: Come up with a method to get all of the past items, then going forward, only new or changed items
                        //  ideas:
                        //      use nextPageToken
                        //      use timeMax & timeMin, looping through months at a time
                        //      nextSyncToken: Token used at a later point in time to retrieve only the entries that have changed since this result was returned. Omitted if further results are available, in which case nextPageToken is provided.
                        //      first time use nextPageToken, then after that switch to nextSyncToken
                    }
                }
            }

        }

        private static void ListNextTenEvents(CalendarService service)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
        }
    }
}
