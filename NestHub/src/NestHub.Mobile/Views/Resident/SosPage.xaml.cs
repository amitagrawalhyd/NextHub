using NestHub.Mobile.ViewModels.Resident;

namespace NestHub.Mobile.Views.Resident;

public partial class SosPage : ContentPage
{
    private readonly SosViewModel _viewModel;

    public SosPage(SosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.AppearingCommand.Execute(null);
    }
}
