using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RESTly.ViewModels
{
    public class RestViewModel : INotifyPropertyChanged
    {
        private string _requestUrl;
        private string _requestMethod;
        private string _requestContentType;
        private string _requestBody;
        private string _response;
        private string _responseHeader;
        private string _responseContentType;
        private IUserDialogs _dialogs;

        public RestViewModel(IUserDialogs dialogs)
        {
            _dialogs = dialogs;
            RequestMethods = new List<string> { "GET", "POST", "PUT", "DELETE" };
            ContentTypes = new List<string>
            {
                "application/json",
                "application/xml",
                "text/html",
                "text/plain"
            };
            SubmitCommand = new Command(async () => await DoSubmitAsync(), CanSubmit);
            ResponseHeaderCommand = new Command(async () => await DoShowResponseHeaderAsync(), CanShowResponseHeader);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Command SubmitCommand { get; private set; }
        public Command ResponseHeaderCommand { get; private set; }

        public string RequestUrl
        {
            get { return _requestUrl; }
            set
            {
                _requestUrl = value;
                NotifyPropertChanged(RequestUrl);
                SubmitCommand.ChangeCanExecute();
            }
        }

        public List<string> RequestMethods { get; set; }

        public List<string> ContentTypes { get; set; }

        public string RequestMethod
        {
            get { return _requestMethod; }
            set
            {
                _requestMethod = value;
                NotifyPropertChanged(RequestMethod);
                SubmitCommand.ChangeCanExecute();
            }
        }

        public string RequestContentType
        {
            get { return _requestContentType; }
            set
            {
                _requestContentType = value;
                NotifyPropertChanged(RequestContentType);
            }
        }

        public string RequestBody
        {
            get { return _requestBody; }
            set
            {
                _requestBody = value;
                NotifyPropertChanged(RequestBody);
            }
        }

        public string ResponseBody
        {
            get { return _response; }
            set
            {
                _response = value;
                NotifyPropertChanged(nameof(ResponseBody));
            }
        }

        public string ResponseContentType
        {
            get { return _responseContentType; }
            set
            {
                _responseContentType = value;
                NotifyPropertChanged(nameof(ResponseContentType));
            }
        }

        public string ResponseHeader
        {
            get { return _responseHeader; }
            set
            {
                _responseHeader = value;
                NotifyPropertChanged(nameof(ResponseHeader));
                ResponseHeaderCommand.ChangeCanExecute();
            }
        }

        protected virtual void NotifyPropertChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanSubmit()
        {
            return !string.IsNullOrWhiteSpace(RequestUrl) && !string.IsNullOrWhiteSpace(RequestMethod);
        }

        private async Task DoSubmitAsync()
        {
            try
            {
                ResponseHeader = "";
                ResponseBody = "";
                ResponseContentType = "";

                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri(RequestUrl);

                if (!string.IsNullOrWhiteSpace(RequestBody))
                {
                    if (string.IsNullOrWhiteSpace(RequestContentType))
                        throw new ContentTypeException("Please select a content type.");

                    request.Content = new StringContent(RequestBody, Encoding.UTF8, RequestContentType);
                }

                switch (RequestMethod)
                {
                    case "GET":
                        request.Method = HttpMethod.Get;
                        break;
                    case "POST":
                        request.Method = HttpMethod.Post;
                        break;
                    case "PUT":
                        request.Method = HttpMethod.Put;
                        break;
                    case "DELETE":
                        request.Method = HttpMethod.Delete;
                        break;
                }

                var response = await client.SendAsync(request);

                ResponseBody = await response.Content.ReadAsStringAsync();
                ResponseHeader = response.Headers.ToString();
                ResponseContentType = response.Content.Headers.ContentType.MediaType;
            }
            catch (ContentTypeException e)
            {
                await _dialogs.AlertAsync("Please select a content type.", "Error", "OK");
            }
            catch (FormatException e)
            {
                await _dialogs.AlertAsync("The URL is malformed. Please change the URL and try again.", "Error", "OK");
            }
            catch (Exception e)
            {
                await _dialogs.AlertAsync("An unexpected error occured: " + e.Message, "Error", "OK");
            }
        }

        private bool CanShowResponseHeader()
        {
            return !string.IsNullOrWhiteSpace(ResponseHeader);
        }

        private async Task DoShowResponseHeaderAsync()
        {
            await _dialogs.AlertAsync(ResponseHeader, "ResponseHeader", "OK");
        }

    }

    class ContentTypeException : Exception
    {
        public ContentTypeException(string msg) : base(msg) { }
    }
}
