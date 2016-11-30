using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using BotImage.Utils;
using Microsoft.ProjectOxford.Vision;
using System.Configuration;
using Microsoft.ProjectOxford.Vision.Contract;

namespace BotImage
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = activity.CreateReply($"Your message contains no images");
                bool imageFound = false;
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;
                OcrResults res = null;
                if (activity?.Attachments.Count() > 0)
                {
                    var attachment = activity.Attachments.First();
                    if (attachment.ContentType == "image")
                    {
                        var token = await AuthorizationHelper.GetBearerToken();

                        var client = new HttpClient();
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);

                        var image = await client.GetStreamAsync(attachment.ContentUrl);


                        // System.IO.File.WriteAllBytes(@"c:\output.png", image);

                        var visionClient = new VisionServiceClient(ConfigurationManager.AppSettings["VisionKey"]);

                        res = await visionClient.RecognizeTextAsync(image);
                        // return our reply to the user
                        reply = activity.CreateReply($"Your image contains");
                        await connector.Conversations.ReplyToActivityAsync(reply);

                        foreach (var region in res?.Regions)

                        {
                            reply = activity.CreateReply($"{string.Join<string>(" ", region.Lines.SelectMany(s => s.Words.Select(w => w.Text)))}");
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                        imageFound = true;
                    }
                }
                
                if (!imageFound)
                {
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                
                


            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}