namespace ArcOrganization.Infrastructure
{
    using System.Windows;
    using System.Threading.Tasks;

    public static class MessageService
    {
        public async static Task ShowMessageAsync(string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK);
        }

        public async static Task<bool> ShowMessageOkCancel(string text, string title)
        {
            var  result = MessageBox.Show(text, title, MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
        }
    }
}
