using BlazorBootstrap;

namespace DinoTrans.BlazorWebAssembly.Helper
{
    public class ToastHelper
    {
        public List<ToastMessage> messages = new List<ToastMessage>();

        public void ShowMessage(ToastType toastType, string Title, string Message) => messages.Add(CreateToastMessage(toastType, Title, Message));

        private ToastMessage CreateToastMessage(ToastType toastType, string Title, string Message)
        => new ToastMessage
        {
            Type = toastType,
            Title = Title,
            HelpText = $"{DateTime.Now}",
            Message = Message,
        };
    }
}
