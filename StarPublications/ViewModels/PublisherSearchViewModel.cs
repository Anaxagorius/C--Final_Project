using Microsoft.EntityFrameworkCore;
using StarPublications.Commands;
using StarPublications.Data;
using StarPublications.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StarPublications.ViewModels
{
    /// <summary>
    /// ViewModel for the Publisher Search and Display view.
    /// Supports live debounced search, detail panel, and CSV export.
    /// </summary>
    public class PublisherSearchViewModel : BaseViewModel
    {
        // ── Events / Delegates ────────────────────────────────────────────────────
        public event Action<string>? StatusMessageChanged;

        // ── Live-search debounce timer ────────────────────────────────────────────
        private readonly DispatcherTimer _searchTimer;

        // ── Backing fields ────────────────────────────────────────────────────────
        private ObservableCollection<Publisher> _publishers = new();
        private Publisher? _selectedPublisher;
        private string _searchName = string.Empty;
        private string _searchCity = string.Empty;
        private string _searchState = string.Empty;
        private string _searchCountry = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _resultCount = string.Empty;

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
            set { if (SetProperty(ref _searchName, value)) RestartSearchTimer(); }
        }

        public string SearchCity
        {
            get => _searchCity;
            set { if (SetProperty(ref _searchCity, value)) RestartSearchTimer(); }
        }

        public string SearchState
        {
            get => _searchState;
            set { if (SetProperty(ref _searchState, value)) RestartSearchTimer(); }
        }

        public string SearchCountry
        {
            get => _searchCountry;
            set { if (SetProperty(ref _searchCountry, value)) RestartSearchTimer(); }
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

        public string ResultCount
        {
            get => _resultCount;
            set => SetProperty(ref _resultCount, value);
        }

        public string DetailTitles
        {
            get => _detailTitles;
            set => SetProperty(ref _detailTitles, value);
        }

        public string DetailPrInfo
        {
            get => _detailPrInfo;
            set => SetProperty(ref _detailPrInfo, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LoadAllCommand { get; }
        public ICommand ExportCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public PublisherSearchViewModel()
        {
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); Search(); };

            SearchCommand  = new RelayCommand(_ => Search());
            ClearCommand   = new RelayCommand(_ => Clear());
            LoadAllCommand = new RelayCommand(_ => LoadAll());
            ExportCommand  = new RelayCommand(_ => ExportCsv(), _ => Publishers.Count > 0);

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
                {
                    Publishers = new ObservableCollection<Publisher>(publishers);
                    ResultCount = $"📊  {publishers.Count} publishers";
                });

                StatusMessageChanged?.Invoke($"Loaded {publishers.Count} publishers.");
            });
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchName) &&
                string.IsNullOrWhiteSpace(SearchCity) &&
                string.IsNullOrWhiteSpace(SearchState) &&
                string.IsNullOrWhiteSpace(SearchCountry))
            {
                LoadAll();
                return;
            }

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
                {
                    Publishers = new ObservableCollection<Publisher>(results);
                    ResultCount = $"📊  {results.Count} of {ctx.Publishers.Count()} publishers";
                });

                StatusMessageChanged?.Invoke($"Found {results.Count} matching publishers.");
            });
        }

        private void Clear()
        {
            _searchTimer.Stop();
            SearchName    = string.Empty;
            SearchCity    = string.Empty;
            SearchState   = string.Empty;
            SearchCountry = string.Empty;
            DetailTitles  = string.Empty;
            DetailPrInfo  = string.Empty;
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

        private void ExportCsv()
        {
            var saved = CsvExporter.Export(
                Publishers,
                ["Pub ID", "Name", "City", "State", "Country"],
                p =>
                [
                    p.PubId,
                    p.PubName ?? string.Empty,
                    p.City ?? string.Empty,
                    p.State ?? string.Empty,
                    p.Country ?? string.Empty
                ],
                "publishers");

            if (saved)
                StatusMessageChanged?.Invoke($"✅  Exported {Publishers.Count} publishers to CSV.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void RestartSearchTimer()
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void TryExecute(Action action)
        {
            IsLoading    = true;
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
