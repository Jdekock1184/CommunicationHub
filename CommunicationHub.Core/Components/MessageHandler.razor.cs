using Blazored.FluentValidation;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommunicationHub.Core.Components
{
    public partial class MessageHandler : ComponentBase
    {
        [Inject] ISnackbar Snackbar { get; set; }


        private FluentValidationValidator fluentValidator;
        private bool messageSent;
        public bool ValidateMessage;
        private string newMessage;

        public string Message { get; set; }

        protected override void OnInitialized()
        {
            fluentValidator = new();
            base.OnInitialized();
        }

        private async Task MessageChanged(string message)
        {
            ValidateMessage = false;
            messageSent = false;
            Message = message;
        }

        private async Task SendMessage()
        {
            ValidateMessage = true;
            if (fluentValidator.Validate())
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        var uri = "http://RookieRockstar:8428/message/sendmessage"; // Url for local testing. Exchange for api url once live
                        var requestData = new
                        {
                            Message = this.Message
                        };

                        string json = JsonConvert.SerializeObject(requestData);
                        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(uri, content);
                        response.EnsureSuccessStatusCode(); 

                        string responseBody = await response.Content.ReadAsStringAsync();
                        newMessage = responseBody;
                        messageSent = true;
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine(e.Message);
                        Snackbar.Add("Oops! Something went wrong", MudBlazor.Severity.Error);
                    }
                }

            }
        }
    }

    public class MessageHandlerValidator : AbstractValidator<MessageHandler>
    {
        public MessageHandlerValidator()
        {
            RuleFor(x => x.Message).NotEmpty().WithMessage("Message can not be empty").When(x => x.ValidateMessage);
        }
    }
}
