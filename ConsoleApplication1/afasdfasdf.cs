/*
 * These classes are examples for using Oyatels streaming events API through CometD.
 * */

using System;
using System.Collections.Generic;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using Cometd.Client;
using Cometd.Client.Transport;

namespace Walkthrough
{
    // Delegate for callevents:
    public delegate void CallEvent(IDictionary<String, Object> data);
    // Delegate for events taking bool parameter:
    public delegate void BoolEvent(bool success);

    /*
     * Subscribe and listen to connected/disconnected-events.
     * */
    public class ConnectionListener : IMessageListener
    {
        public bool Connected = false;

        public ConnectionListener()
            : base()
        {
        }

        public void onMessage(IClientSessionChannel channel, IMessage message)
        {
            if (message.Successful)
            {
                if (!Connected)
                {
                    Connected = true;
                }
            }
            else
            {
                if (Connected)
                {
                    Connected = false;
                }
            }
        }
    }

    /*
     * Subscribe and listen to logincompleted-events.
     * */
    public class InitializerListener : IMessageListener
    {
        private event BoolEvent onLoginCompleted;
        public String userId = "";
        public String username = "";

        public InitializerListener(BoolEvent onLoginCompleted)
            : base()
        {
            this.onLoginCompleted += onLoginCompleted;
            userId = QuickstartApiSample.User.UserId;
            username = QuickstartApiSample.User.UserName;
        }

        public void onMessage(IClientSessionChannel channel, IMessage message)
        {
            try
            {
                IDictionary<String, Object> ext = (IDictionary<String, Object>)message.Ext["authentication"];
                userId = ext["userId"].ToString();
                username = ext["username"].ToString();
            }
            catch (Exception)
            {
            }

            onLoginCompleted(message.Successful);
        }
    }

    /*
     * Subscribe and listen to callevents.
     * */
    public class BatchCallEventListener : IMessageListener
    {
        public BayeuxClient client;
        private event CallEvent onCallEvent;

        public BatchCallEventListener(BayeuxClient client, CallEvent onCallEvent)
        {
            this.client = client;
            this.onCallEvent = onCallEvent;
        }

        public void Run()
        {
            IClientSessionChannel callEventChannel = client.getChannel("/topic/cwsi_ContactWasChanged");

            callEventChannel.unsubscribe(this);
            callEventChannel.subscribe(this);
        }

        public void onMessage(IClientSessionChannel channel, IMessage message)
        {
            try
            {
                IDictionary<String, Object> data = message.DataAsDictionary;
                // Trigger callback:
                onCallEvent(data);
            }
            catch (Exception)
            {
            }
        }
    }


}