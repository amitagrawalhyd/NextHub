using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.Residents.Commands.RegisterResident;
using NestHub.Application.Societies.Dtos;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.ViewModels.Resident;

public sealed partial class ResidentOnboardingViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;

    public ResidentOnboardingViewModel(ApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
    }

    public ObservableCollection<SocietyDto> Societies { get; } = new();

    [ObservableProperty]
    private SocietyDto? _selectedSociety;

    [ObservableProperty]
    private string _blockNumber = string.Empty;

    [ObservableProperty]
    private string _flatNumber = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task AppearingAsync()
    {
        var societies = await _apiClient.GetSocietiesAsync();
        Societies.Clear();
        foreach (var society in societies)
            Societies.Add(society);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedSociety is null || IsBusy) return;

        try
        {
            IsBusy = true;
            await _apiClient.RegisterResidentAsync(new RegisterResidentCommand(
                _authSession.UserId!.Value, SelectedSociety.Id, BlockNumber, FlatNumber, Name));

            await Shell.Current.GoToAsync("//ResidentShell");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
