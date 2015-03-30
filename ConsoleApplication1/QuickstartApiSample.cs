using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using Cometd.Client;
using Cometd.Client.Transport;
using Newtonsoft.Json;
using Walkthrough.sforce;

namespace Walkthrough
{

    

    public class QuickstartApiSample
    {
        private SforceService binding;

        private StreamingEvents listener;
        private BayeuxClient client;
        public static UserInfo User;

        public void UpdateAccount(IDictionary<String, Object> data)
        {
            var t = GetEventType(data);
            return;
        }

        private EventType GetEventType(IDictionary<String, Object> data)
        {
            EventType ev;
            var v = (Dictionary<string, object>) data["event"];
            if (Enum.TryParse(v["type"].ToString(), true, out ev))
            {
                return ev;
            }
            return EventType.None;
        }

        private enum EventType
        {
            None,
            Updated,
            Created,
            Deleted,
            Undeleted

        }

        public void run()
        {
            try
            {
                // Make a login call 
                if (login())
                {
//                    listener = new StreamingEvents(UpdateAccount);
//                    listener.Connect(binding.SessionHeaderValue.sessionId);


//                Listener.Start(binding.SessionHeaderValue.sessionId);
//                Console.ReadLine();
                    // Do a describe global 
                describeGlobalSample();

                    // Describe an account object 
//                                describeSObjectsSample();

                    // Retrieve some data using a query 
                     querySample();

                    // Log out
//                logout();
                    Console.ReadLine();
                }
            }
            finally
            {
                logout();
            }
        }

        public void describeSObjectsSample()
        {
            try
            {
                // Call describeSObjectResults and pass it an array with
                // the names of the objects to describe.
                DescribeSObjectResult[] describeSObjectResults =
                                    binding.describeSObjects(
                                    //new string[] { "account", "contact", "lead" });
                new string[] { "User" });


                // Iterate through the list of describe sObject results
                foreach (DescribeSObjectResult describeSObjectResult in describeSObjectResults)
                {
                    // Get the name of the sObject
                    String objectName = describeSObjectResult.name;
                    Console.WriteLine("sObject name: " + objectName);

                    // For each described sObject, get the fields
                    Field[] fields = describeSObjectResult.fields;

                    // Get some other properties
                    if (describeSObjectResult.activateable) Console.WriteLine("\tActivateable");

                    // Iterate through the fields to get properties for each field
                    foreach (Field field in fields)
                    {
                        Console.WriteLine("\tField: " + field.name);
                        Console.WriteLine("\t\tLabel: " + field.label);
                        if (field.custom)
                            Console.WriteLine("\t\tThis is a custom field.");
                        Console.WriteLine("\t\tType: " + field.type);
                        if (field.length > 0)
                            Console.WriteLine("\t\tLength: " + field.length);
                        if (field.precision > 0)
                            Console.WriteLine("\t\tPrecision: " + field.precision);

                        // Determine whether this is a picklist field
                        if (field.type == fieldType.picklist)
                        {
                            // Determine whether there are picklist values
                            PicklistEntry[] picklistValues = field.picklistValues;
                            if (picklistValues != null && picklistValues[0] != null)
                            {
                                Console.WriteLine("\t\tPicklist values = ");
                                for (int j = 0; j < picklistValues.Length; j++)
                                {
                                    Console.WriteLine("\t\t\tItem: " + picklistValues[j].label);
                                }
                            }
                        }

                        // Determine whether this is a reference field
                        if (field.type == fieldType.reference)
                        {
                            // Determine whether this field refers to another object
                            string[] referenceTos = field.referenceTo;
                            if (referenceTos != null && referenceTos[0] != null)
                            {
                                Console.WriteLine("\t\tField references the following objects:");
                                for (int j = 0; j < referenceTos.Length; j++)
                                {
                                    Console.WriteLine("\t\t\t" + referenceTos[j]);
                                }
                            }
                        }
                    }
                }
            }
            catch (SoapException e)
            {
                Console.WriteLine("An unexpected error has occurred: " + e.Message
                    + "\n" + e.StackTrace);
            }
        }

        private bool login()
        {
            Console.Write("Enter username: ");
            //string username = Console.ReadLine();
            string username = "jedwards@connectwise.com";
            string password = "jn4HuxUdXiHr1";

//            string username = "cwsf@webteks.com";
//            string password = "dBSX9BoxhY0e";
            Console.Write("Enter password: ");
            //string password = Console.ReadLine();
            
            // Create a service object 
            binding = new SforceService();

            // Timeout after a minute 
            binding.Timeout = 60000;

            // Try logging in   
            LoginResult lr;
            try
            {

                Console.WriteLine("\nLogging in...\n");
                lr = binding.login(username, password);
            }

            // ApiFault is a proxy stub generated from the WSDL contract when     
            // the web service was imported 
            catch (SoapException e)
            {
                // Write the fault code to the console 
                Console.WriteLine(e.Code);

                // Write the fault message to the console 
                Console.WriteLine("An unexpected error has occurred: " + e.Message);

                // Write the stack trace to the console 
                Console.WriteLine(e.StackTrace);

                // Return False to indicate that the login was not successful 
                return false;
            }



            // Check if the password has expired 
            if (lr.passwordExpired)
            {
                Console.WriteLine("An error has occurred. Your password has expired.");
                return false;
            }


            /** Once the client application has logged in successfully, it will use
             * the results of the login call to reset the endpoint of the service
             * to the virtual server instance that is servicing your organization
             */
            // Save old authentication end point URL
            String authEndPoint = binding.Url;
            // Set returned service endpoint URL
            binding.Url = lr.serverUrl;

            /** The sample client application now has an instance of the SforceService
             * that is pointing to the correct endpoint. Next, the sample client
             * application sets a persistent SOAP header (to be included on all
             * subsequent calls that are made with SforceService) that contains the
             * valid sessionId for our login credentials. To do this, the sample
             * client application creates a new SessionHeader object and persist it to
             * the SforceService. Add the session ID returned from the login to the
             * session header
             */
            binding.SessionHeaderValue = new SessionHeader();
            binding.SessionHeaderValue.sessionId = lr.sessionId;

            User = new UserInfo
            {
                UserId = lr.userId,
                UserName = lr.userInfo.userName
            };

            printUserInfo(lr, authEndPoint);

            // Return true to indicate that we are logged in, pointed  
            // at the right URL and have our security token in place.     
            return true;
        }

        private void printUserInfo(LoginResult lr, String authEP)
        {
            try
            {
                GetUserInfoResult userInfo = lr.userInfo;

                Console.WriteLine("\nLogging in ...\n");
                Console.WriteLine("UserID: " + userInfo.userId);
                Console.WriteLine("User Full Name: " +
                    userInfo.userFullName);
                Console.WriteLine("User Email: " +
                    userInfo.userEmail);
                Console.WriteLine();
                Console.WriteLine("SessionID: " +
                    lr.sessionId);
                Console.WriteLine("Auth End Point: " +
                    authEP);
                Console.WriteLine("Service End Point: " +
                    lr.serverUrl);
                Console.WriteLine();
            }
            catch (SoapException e)
            {
                Console.WriteLine("An unexpected error has occurred: " + e.Message +
                    " Stack trace: " + e.StackTrace);
            }
        }

        private void logout()
        {
            try
            {
                binding.logout();
                Console.WriteLine("Logged out.");
            }
            catch (SoapException e)
            {
                // Write the fault code to the console 
                Console.WriteLine(e.Code);

                // Write the fault message to the console 
                Console.WriteLine("An unexpected error has occurred: " + e.Message);

                // Write the stack trace to the console 
                Console.WriteLine(e.StackTrace);
            }
        }

        /**
        * To determine the objects that are available to the logged-in
        * user, the sample client application executes a describeGlobal
        * call, which returns all of the objects that are visible to
        * the logged-in user. This call should not be made more than
        * once per session, as the data returned from the call likely
        * does not change frequently. The DescribeGlobalResult is
        * simply echoed to the console.
        */
        private void describeGlobalSample()
        {
            try
            {
                // describeGlobal() returns an array of object results that  
                // includes the object names that are available to the logged-in user. 
                DescribeGlobalResult dgr = binding.describeGlobal();

                Console.WriteLine("\nDescribe Global Results:\n");
                // Loop through the array echoing the object names to the console             
                for (int i = 0; i < dgr.sobjects.Length; i++)
                {
                    Console.WriteLine(dgr.sobjects[i].name);
                }
            }
            catch (SoapException e)
            {
                Console.WriteLine("An exception has occurred: " + e.Message +
                    "\nStack trace: " + e.StackTrace);
            }
        }

        /**
        * The following method illustrates the type of metadata
        * information that can be obtained for each object available
        * to the user. The sample client application executes a
        * describeSObject call on a given object and then echoes  
        * the returned metadata information to the console. Object
        * metadata information includes permissions, field types
        * and length and available values for picklist fields
        * and types for referenceTo fields.
        */
//        private void describeSObjectsSample()
//        {
//            Console.Write("\nType the name of the object to " +
//                "describe (try Account): ");
//            string objectType = Console.ReadLine();
//            try
//            {
//
//                // Call describeSObjects() passing in an array with one object type name 
//                DescribeSObjectResult[] dsrArray =
//                      binding.describeSObjects(new string[] { objectType });
//
//                // Since we described only one sObject, we should have only
//                // one element in the DescribeSObjectResult array.
//                DescribeSObjectResult dsr = dsrArray[0];
//
//                // First, get some object properties                  
//                Console.WriteLine("\n\nObject Name: " + dsr.name);
//
//                if (dsr.custom) Console.WriteLine("Custom Object");
//                if (dsr.label != null) Console.WriteLine("Label: " + dsr.label);
//
//                // Get the permissions on the object 
//                if (dsr.createable) Console.WriteLine("Createable");
//                if (dsr.deletable) Console.WriteLine("Deleteable");
//                if (dsr.queryable) Console.WriteLine("Queryable");
//                if (dsr.replicateable) Console.WriteLine("Replicateable");
//                if (dsr.retrieveable) Console.WriteLine("Retrieveable");
//                if (dsr.searchable) Console.WriteLine("Searchable");
//                if (dsr.undeletable) Console.WriteLine("Undeleteable");
//                if (dsr.updateable) Console.WriteLine("Updateable");
//
//                Console.WriteLine("Number of fields: " + dsr.fields.Length);
//
//                // Now, retrieve metadata for each field
//                for (int i = 0; i < dsr.fields.Length; i++)
//                {
//                    // Get the field 
//                    Field field = dsr.fields[i];
//
//                    // Write some field properties
//                    Console.WriteLine("Field name: " + field.name);
//                    Console.WriteLine("\tField Label: " + field.label);
//
//                    // This next property indicates that this  
//                    // field is searched when using 
//                    // the name search group in SOSL 
//                    if (field.nameField)
//                        Console.WriteLine("\tThis is a name field.");
//
//                    if (field.restrictedPicklist)
//                        Console.WriteLine("This is a RESTRICTED picklist field.");
//
//                    Console.WriteLine("\tType is: " + field.type.ToString());
//
//                    if (field.length > 0)
//                        Console.WriteLine("\tLength: " + field.length);
//
//                    if (field.scale > 0)
//                        Console.WriteLine("\tScale: " + field.scale);
//
//                    if (field.precision > 0)
//                        Console.WriteLine("\tPrecision: " + field.precision);
//
//                    if (field.digits > 0)
//                        Console.WriteLine("\tDigits: " + field.digits);
//
//                    if (field.custom)
//                        Console.WriteLine("\tThis is a custom field.");
//
//                    // Write the permissions of this field
//                    if (field.nillable) Console.WriteLine("\tCan be nulled.");
//                    if (field.createable) Console.WriteLine("\tCreateable");
//                    if (field.filterable) Console.WriteLine("\tFilterable");
//                    if (field.updateable) Console.WriteLine("\tUpdateable");
//
//                    // If this is a picklist field, show the picklist values   
//                    if (field.type.Equals(fieldType.picklist))
//                    {
//                        Console.WriteLine("\tPicklist Values");
//                        for (int j = 0; j < field.picklistValues.Length; j++)
//                            Console.WriteLine("\t\t" + field.picklistValues[j].value);
//                    }
//
//                    // If this is a foreign key field (reference),     
//                    // show the values 
//                    if (field.type.Equals(fieldType.reference))
//                    {
//                        Console.WriteLine("\tCan reference these objects:");
//                        for (int j = 0; j < field.referenceTo.Length; j++)
//                            Console.WriteLine("\t\t" + field.referenceTo[j]);
//                    }
//                    Console.WriteLine("");
//                }
//            }
//            catch (SoapException e)
//            {
//                Console.WriteLine("An exception has occurred: " + e.Message +
//                    "\nStack trace: " + e.StackTrace);
//            }
//            Console.WriteLine("Press ENTER to continue...");
//            Console.ReadLine();
//        }

        private void querySample()
        {

            string soqlQuery =

//                "SELECT Id, Name,  MailingAddress FROM Contact";
                string.Format("SELECT Id, Name, Query, ApiVersion, IsActive, NotifyForFields, NotifyForOperations, NotifyForOperationCreate, NotifyForOperationUpdate, NotifyForOperationDelete, NotifyForOperationUndelete FROM PushTopic");

//                "SELECT Id, Name, OwnerId, Site, AnnualRevenue," +
//                " BillingStreet, BillingCity, BillingState, BillingPostalCode, BillingCountry, BillingLatitude, BillingLongitude, BillingAddress, " +
//                "NumberOfEmployees, Fax, Industry, ParentId, Phone,  " +
//                "ShippingStreet, ShippingCity, ShippingState, ShippingPostalCode, ShippingCountry, ShippingLatitude, ShippingLongitude, ShippingAddress," +
//                "Type, Website, LastModifiedDate FROM Account";
//
//            String soqlQuery = "SELECT Id, AccountId, Email, LastModifiedDate FROM Contact Where AccountId = '001G000001g1WlZIAU'";
            try
            {
                QueryResult qr = binding.query(soqlQuery);
                bool done = false;

                if (qr.size > 0)
                {
                    Console.WriteLine("Logged-in user can see "
                          + qr.records.Length + " contact records.");

                    while (!done)
                    {
                        Console.WriteLine("");
                        sObject[] records = qr.records;
                        for (int i = 0; i < records.Length; i++)
                        {
                            var con = records[i];

                            foreach (var any in con.Any)
                            {
                                Console.WriteLine("Name Field: '" + any.LocalName + "' = '" + any.InnerText + "'");
                            }



                             Console.WriteLine("Contact " + (i + 1) + ": " + con.Id);

                            

                            //                   string fName = con.FirstName;
                            //                 string lName = con.LastName;
                            //               if (fName == null)
                            //                 Console.WriteLine("Contact " + (i + 1) + ": " + lName);
                            //           else
                            //             Console.WriteLine("Contact " + (i + 1) + ": " + fName
                            //                  + " " + lName);
                        }

                        if (qr.done)
                        {
                            done = true;
                        }
                        else
                        {
                            qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No records found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nFailed to execute query succesfully," +
                    "error message was: \n{0}", ex.Message);
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
        }
    }


    public static class Extentions
    {
        public static string GetValue(this sObject obj, string fieldName)
        {
            var field =
                obj.Any.FirstOrDefault(x => x.LocalName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
            if (field != null)
                return field.InnerText;
            return null;
        }

        public static DateTime? GetDateTime(this sObject obj, string fieldName)
        {
            var value = obj.GetValue(fieldName);
            if (string.IsNullOrEmpty(value)) return null;
            return DateTime.Parse(value);
        }
    }


//    public class Listener : IMessageListener
//    {
//        private const int READ_TIMEOUT = 120 * 1000;
//        public void onMessage(IClientSessionChannel channel, IMessage message)
//        {
//            throw new NotImplementedException();
//        }
//
//        public static void Start(string sessionId)
//        {
//
//            var sesId = "OAuth " + sessionId;
//            var options = new Dictionary<String, Object>
//            {
//                { ClientTransport.TIMEOUT_OPTION, READ_TIMEOUT },
//                {"sid",  sesId }
//            };
//            var transport = new LongPollingTransport(options);
//
//   
//            String url = "https://na11.salesforce.com/cometd/33.0";
//            BayeuxClient client = new BayeuxClient(url, new List<ClientTransport>() { transport });
//
//
//
//
//
//
//
//
//            client.handshake();
//            client.waitFor(1000, new List<BayeuxClient.State>() { BayeuxClient.State.CONNECTED });
//
//            // Subscription to channels
//            IClientSessionChannel channel = client.getChannel("/topic/cwsi_AccountWasChanged");
//            channel.subscribe(new Listener());
//        }
//    }

    /*
 * Main class for setting up streaming events.
 * */
    public class StreamingEvents
    {
        protected BayeuxClient client;
        protected InitializerListener initListener;
        protected ConnectionListener connectionListener = null;

        //https://na11.salesforce.com/services/Soap/u/33.0/00DG0000000lASS
        protected String url = "https://na11.salesforce.com/cometd/33.0";
        private CallEvent onCallEvent;

        /*
         * Set up, log-in and listen to streaming events from Oyatel.
         * This example listens for call-events.
         * */
        public StreamingEvents(CallEvent onCallEvent)
        {
            this.onCallEvent = onCallEvent;
        }
        private const int READ_TIMEOUT = 120 * 1000;
        public void Connect(String access_token)
        {
            IList<ClientTransport> transports = new List<ClientTransport>();
                        var options = new Dictionary<String, Object>
                        {
                            { ClientTransport.TIMEOUT_OPTION, READ_TIMEOUT }
                        };
                        var transport = new LongPollingTransport(options);
            
                        // add the needed auth headers
                        var headers = new NameValueCollection();
                        headers.Add("Authorization", "OAuth " + access_token);
                        transport.AddHeaders(headers);
                        transports.Add(transport);

            client = new BayeuxClient(url, transports);

            // Subscribe and call 'Initialize' after successful login
            initListener = new InitializerListener(Initialize);
            client.getChannel(Channel_Fields.META_HANDSHAKE).addListener(initListener);

            // Subscribe to connect/disconnect-events
            connectionListener = new ConnectionListener();
            client.getChannel(Channel_Fields.META_CONNECT).addListener(connectionListener);

            // Handshaking with oauth2
//            IDictionary<String, Object> handshakeAuth = new Dictionary<String, Object>();

//            handshakeAuth.Add("authType", "oauth2");
//            handshakeAuth.Add("oauth_token", access_token);

//            IDictionary<String, Object> ext = new Dictionary<String, Object>();

//            ext.Add("ext", handshakeAuth);
            client.handshake();
//            client.handshake();
        }

        public void Disconnect()
        {
            client.disconnect();
        }

        public bool Connected
        {
            get
            {
                if (connectionListener == null) return false;

                return connectionListener.Connected;
            }
        }

        private void Initialize(bool success)
        {
            if (success)
            {
                BatchCallEventListener batch = new BatchCallEventListener(client, onCallEvent);
                client.batch(new BatchDelegate(batch.Run));
            }
        }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

}