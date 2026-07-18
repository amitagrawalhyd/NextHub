using NestHub.Mobile.ViewModels.Vendor;

namespace NestHub.Mobile.Views.Vendor;

public partial class PostBroadcastPage : ContentPage
{
    public PostBroadcastPage(PostBroadcastViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
