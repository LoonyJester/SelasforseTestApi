//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Cometd.Bayeux;
//using Cometd.Bayeux.Client;
//using Cometd.Client;
//using Cometd.Client.Transport;
////using CredentialManagement;
////using Sample.partner.wsdl;
//using System.Collections.Specialized;
//using System.Threading;
//
//namespace Sample
//{
//    class Program
//    {
//        private const String CRED_KEY = "ndrees@23demo.com";
//        private const String CHANNEL = "/topic/InvoiceStatementUpdates";
//        private const String STREAMING_ENDPOINT_URI = "/cometd/29.0";
//
//        // long pull durations
//        private const int READ_TIMEOUT = 120 * 1000;
//        private const int THREAD_TIMEOUT = 60 * 1000;
//
////        static void Main(string[] args)
////        {
////            try
////            {
////                RunExample();
////            }
////            catch (Exception e)
////            {
////                Console.WriteLine(e.Message);
////                Console.WriteLine(e.StackTrace);
////
////                Exception innerException = e.InnerException;
////                while (innerException != null)
////                {
////                    Console.WriteLine(e.Message);
////                    Console.WriteLine(e.StackTrace);
////
////                    innerException = innerException.InnerException;
////                }
////            }
////        }
//
//        private static void RunExample()
//        {
//            BayeuxClient client = null;
//
//            using (var cred = new Credential { Target = CRED_KEY })
//            {
//                Console.WriteLine("Loading credentials from windows credential vault.");
//
//                if (!cred.Load())
//                {
//                    Console.WriteLine("Could not find credential with key {0} in windows credential vault.", CRED_KEY);
//                    return;
//                }
//
//                client = CreateClient(cred);
//            }
//
//            Console.WriteLine("Handshaking.");
//            client.handshake();
//            client.waitFor(1000, new[] { BayeuxClient.State.CONNECTED });
//
//            Console.WriteLine("Connected.");
//
//            client.getChannel(CHANNEL).subscribe(new SampleListener());
//            Console.WriteLine("Waiting for data from server...");
//
//            Console.WriteLine("Press any key to shut down.");
//            Console.ReadKey();
//
//            Console.WriteLine("Shutting down...");
//            client.disconnect();
//            client.waitFor(1000, new[] { BayeuxClient.State.DISCONNECTED });
//        }
//
//        private static BayeuxClient CreateClient(Credential cred)
//        {
//            Console.WriteLine("Authenticating with Salesforce.");
//
//            var soapClient = new SoapClient();
//            var result = soapClient.login(null, null, cred.Username, cred.Password);
//            if (result.passwordExpired)
//                throw new ArgumentOutOfRangeException("Password has expired");
//
//            Console.WriteLine("Authenticated.");
//
//            var options = new Dictionary<String, Object>
//            {
//                { ClientTransport.TIMEOUT_OPTION, READ_TIMEOUT }
//            };
//            var transport = new LongPollingTransport(options);
//
//            // add the needed auth headers
//            var headers = new NameValueCollection();
//            headers.Add("Authorization", "OAuth " + result.sessionId);
//            transport.AddHeaders(headers);
//
//            // only need the scheme and host, strip out the rest
//            var serverUri = new Uri(result.serverUrl);
//            String endpoint = String.Format("{0}://{1}{2}", serverUri.Scheme, serverUri.Host, STREAMING_ENDPOINT_URI);
//
//            return new BayeuxClient(endpoint, new[] { transport });
//        }
//     }
//}
//
// 
//
//
//
//namespace Sample
//{
//    class SampleListener : IMessageListener
//    {
//        public void onMessage(IClientSessionChannel channel, IMessage message)
//        {
//            Console.WriteLine(message);
//        }
//    }
//}