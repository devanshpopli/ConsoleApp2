using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class Helper
    {
        static NotificationHubClient hub;
        static string topicName = "demo";
        static string subscriptionName = "S1";
        public static async Task CreateSubscriptionAsync(string connectionString)
        {
            // Create the subscription if it does not exist already
            ServiceBusAdministrationClient client = new ServiceBusAdministrationClient(connectionString);

            if (!await client.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                await client.CreateSubscriptionAsync(topicName, subscriptionName);
            }
        }

        public static async Task ReceiveMessageAndSendNotificationAsync(string connectionString)
        {
            string hubConnectionString = "Endpoint=sb://Insurejoy.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=zW0VPQnJtKNh9hfuZEw/2IGz7W9BU1GqroPmsv2mdns=";

            hub = NotificationHubClient.CreateClientFromConnectionString
                    (hubConnectionString, "demoapp");

            ServiceBusClient Client = new ServiceBusClient(connectionString);
            ServiceBusReceiver receiver = Client.CreateReceiver(topicName, subscriptionName);

            // Continuously process messages received from the subscription
            while (true)
            {
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync();

                if (message != null)
                {
                    try
                    {
                        Console.WriteLine(message.MessageId);
                        Console.WriteLine(message.SequenceNumber);
                        string messageBody = message.Body.ToString();
                        Console.WriteLine("Body: " + messageBody + "\n");

                        Product product = new Product();
                        product.notification = new
                        {
                            title = message.MessageId,
                            body = messageBody
                        };
                        var x = JsonConvert.SerializeObject(product);
                        SendNotificationAsync(x);

                        // Remove message from subscription
                        await receiver.CompleteMessageAsync(message);
                    }
                    catch (Exception)
                    {
                        // Indicate a problem, unlock message in subscription
                        await receiver.AbandonMessageAsync(message);
                    }
                }
            }
        }
        static async void SendNotificationAsync(string message)
        {
            await hub.SendFcmNativeNotificationAsync(message);
        }
    }

    public class Product
    {
        public object notification;
    }
}
