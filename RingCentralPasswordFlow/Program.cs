using System;
using RingCentral;
using dotenv.net;
using Flurl.Http;

namespace RingCentralPasswordFlow
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DotEnv.Config(filePath: "/Users/tyler.liu/src/ruby/ringcentral-ruby/.env");
            var rc = new RestClient(
                Environment.GetEnvironmentVariable("RINGCENTRAL_CLIENT_ID"),
                Environment.GetEnvironmentVariable("RINGCENTRAL_CLIENT_SECRET"),
                Environment.GetEnvironmentVariable("RINGCENTRAL_SERVER_URL")
            );
            try
            {
                var token = rc.Authorize(
                    Environment.GetEnvironmentVariable("RINGCENTRAL_USERNAME"),
                    Environment.GetEnvironmentVariable("RINGCENTRAL_EXTENSION"),
                    Environment.GetEnvironmentVariable("RINGCENTRAL_PASSWORD")
                ).Result;
                Console.WriteLine(token);

                var subscription = rc.Restapi().Subscription().New();
                subscription.EventFilters.Add("/restapi/v1.0/account/~/extension/~/message-store");
                subscription.NotificationEvent += (sender, ags) => {
                    var message = ags.message;
                    Console.WriteLine(message);
                };
                var result = subscription.Register().Result;
                Console.WriteLine(result);
            } catch(AggregateException ae) {
                var fhe = ae.InnerException as FlurlHttpException;
                var message = fhe.GetResponseStringAsync().Result;
                Console.WriteLine(message);
            }finally {
                rc.Revoke();
            }
        }
    }
}
