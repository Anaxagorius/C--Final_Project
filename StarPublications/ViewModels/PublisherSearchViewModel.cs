using Microsoft.EntityFrameworkCore;
using StarPublications.Commands;
using StarPublications.Data;
using StarPublications.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StarPublications.ViewModels
{
    /// <summary>
    /// ViewModel for the Publisher Search and Display view.
    /// Allows searching for publishers by name, city, state, or country.
    /// </summary>
    public class PublisherSearchViewModel : BaseViewModel
    {
        // ── Events / Delegates ────────────────────────────────────────────────────
        /// <summary>Raised when the status message should be updated.</summary>
        public event Action<string>? StatusMessageChanged;

        // ── Backing fields ────────────────────────────────────────────────────────
        private ObservableCollection<Publisher> _publishers = new();
        private Publisher? _selectedPublisher;
        private string _searchName = string.Empty;
        private string _searchCity = string.Empty;
        private string _searchState = string.Empty;
        private string _searchCountry = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // Detail panel
        private string _detailTitles = string.Empty;
        private string _detailPrInfo = string.Empty;

        // ── Properties ────────────────────────────────────────────────────────────
        public ObservableCollection<Publisher> Publishers
        {
            get => _publishers;
            set => SetProperty(ref _publishers, value);
        }

        public Publisher? SelectedPublisher
        {
            get => _selectedPublisher;
            set
            {
                if (SetProperty(ref _selectedPublisher, value) && value != null)
                    LoadPublisherDetails(value);
            }
        }

        public string SearchName
        {
            get => _searchName;
            set => SetProperty(ref _searchName, value);
        }

        public string SearchCity
        {
            get => _searchCity;
            set => SetProperty(ref _searchCity, value);
        }

        public string SearchState
        {
            get => _searchState;
            set => SetProperty(ref _searchState, value);
        }

        public string SearchCountry
        {
            get => _searchCountry;
            set => SetProperty(ref _searchCountry, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>Newline-separated list of titles for the selected publisher.</summary>
        public string DetailTitles
        {
            get => _detailTitles;
            set => SetProperty(ref _detailTitles, value);
        }

        /// <summary>PR info / description for the selected publisher.</summary>
        public string DetailPrInfo
        {
            get => _detailPrInfo;
            set => SetProperty(ref _detailPrInfo, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LoadAllCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public PublisherSearchViewModel()
        {
            SearchCommand = new RelayCommand(_ => Search());
            ClearCommand = new RelayCommand(_ => Clear());
            LoadAllCommand = new RelayCommand(_ => LoadAll());

            LoadAll();
        }

        // ── Data Operations ───────────────────────────────────────────────────────

        private void LoadAll()
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var publishers = ctx.Publishers
                    .OrderBy(p => p.PubName)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                    Publishers = new ObservableCollection<Publisher>(publishers));

                StatusMessageChanged?.Invoke($"Loaded {publishers.Count} publishers.");
            });
        }

        private void Search()
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var query = ctx.Publishers.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchName))
                    query = query.Where(p => p.PubName != null && p.PubName.Contains(SearchName));

                if (!string.IsNullOrWhiteSpace(SearchCity))
                    query = query.Where(p => p.City != null && p.City.Contains(SearchCity));

                if (!string.IsNullOrWhiteSpace(SearchState))
                    query = query.Where(p => p.State != null && p.State.Contains(SearchState));

                if (!string.IsNullOrWhiteSpace(SearchCountry))
                    query = query.Where(p => p.Country != null && p.Country.Contains(SearchCountry));

                var results = query.OrderBy(p => p.PubName).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                    Publishers = new ObservableCollection<Publisher>(results));

                StatusMessageChanged?.Invoke($"Found {results.Count} matching publishers.");
            });
        }

        private void Clear()
        {
            SearchName = string.Empty;
            SearchCity = string.Empty;
            SearchState = string.Empty;
            SearchCountry = string.Empty;
            DetailTitles = string.Empty;
            DetailPrInfo = string.Empty;
            SelectedPublisher = null;
            LoadAll();
        }

        private void LoadPublisherDetails(Publisher publisher)
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var titles = ctx.Titles
                    .Where(t => t.PubId == publisher.PubId)
                    .OrderBy(t => t.TitleName)
                    .Select(t => $"• {t.TitleName} ({(t.Price.HasValue ? t.Price.Value.ToString("C") : "N/A")})")
                    .ToList();

                var pubInfo = ctx.PubInfos
                    .Where(pi => pi.PubId == publisher.PubId)
                    .Select(pi => pi.PrInfo)
                    .FirstOrDefault();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DetailTitles = titles.Count > 0
                        ? string.Join(Environment.NewLine, titles)
                        : "No titles published";

                    DetailPrInfo = !string.IsNullOrWhiteSpace(pubInfo)
                        ? pubInfo
                        : "No publisher information available.";
                });
            });
        }

        private void TryExecute(Action action)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                Application.Current.Dispatcher.Invoke(() =>
                    ErrorMessage = $"Error: {message}");
                StatusMessageChanged?.Invoke($"Operation failed: {message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
