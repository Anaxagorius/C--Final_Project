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
    /// ViewModel for the Book Search and Display view.
    /// Supports live debounced search, detail panel, and CSV export.
    /// </summary>
    public class BookSearchViewModel : BaseViewModel
    {
        // ── Events / Delegates ────────────────────────────────────────────────────
        public event Action<string>? StatusMessageChanged;

        // ── Live-search debounce timer ────────────────────────────────────────────
        private readonly DispatcherTimer _searchTimer;

        // ── Backing fields ────────────────────────────────────────────────────────
        private ObservableCollection<Title> _titles = new();
        private Title? _selectedTitle;
        private string _searchTitle = string.Empty;
        private string _searchType = string.Empty;
        private string _searchPublisher = string.Empty;
        private string _searchAuthor = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _resultCount = string.Empty;

        // Detail panel
        private string _detailAuthors = string.Empty;
        private string _detailStores = string.Empty;

        // ── Properties ────────────────────────────────────────────────────────────
        public ObservableCollection<Title> Titles
        {
            get => _titles;
            set => SetProperty(ref _titles, value);
        }

        public Title? SelectedTitle
        {
            get => _selectedTitle;
            set
            {
                if (SetProperty(ref _selectedTitle, value) && value != null)
                    LoadTitleDetails(value);
            }
        }

        public string SearchTitle
        {
            get => _searchTitle;
            set { if (SetProperty(ref _searchTitle, value)) RestartSearchTimer(); }
        }

        public string SearchType
        {
            get => _searchType;
            set { if (SetProperty(ref _searchType, value)) RestartSearchTimer(); }
        }

        public string SearchPublisher
        {
            get => _searchPublisher;
            set { if (SetProperty(ref _searchPublisher, value)) RestartSearchTimer(); }
        }

        public string SearchAuthor
        {
            get => _searchAuthor;
            set { if (SetProperty(ref _searchAuthor, value)) RestartSearchTimer(); }
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

        public string DetailAuthors
        {
            get => _detailAuthors;
            set => SetProperty(ref _detailAuthors, value);
        }

        public string DetailStores
        {
            get => _detailStores;
            set => SetProperty(ref _detailStores, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LoadAllCommand { get; }
        public ICommand ExportCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public BookSearchViewModel()
        {
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); Search(); };

            SearchCommand  = new RelayCommand(_ => Search());
            ClearCommand   = new RelayCommand(_ => Clear());
            LoadAllCommand = new RelayCommand(_ => LoadAll());
            ExportCommand  = new RelayCommand(_ => ExportCsv(), _ => Titles.Count > 0);

            LoadAll();
        }

        // ── Data Operations ───────────────────────────────────────────────────────

        private void LoadAll()
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var titles = ctx.Titles
                    .Include(t => t.Publisher)
                    .OrderBy(t => t.TitleName)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Titles = new ObservableCollection<Title>(titles);
                    ResultCount = $"📊  {titles.Count} books";
                });

                StatusMessageChanged?.Invoke($"Loaded {titles.Count} book titles.");
            });
        }

        private void Search()
        {
            // If all filters are empty, just load all
            if (string.IsNullOrWhiteSpace(SearchTitle) &&
                string.IsNullOrWhiteSpace(SearchType) &&
                string.IsNullOrWhiteSpace(SearchPublisher) &&
                string.IsNullOrWhiteSpace(SearchAuthor))
            {
                LoadAll();
                return;
            }

            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var query = ctx.Titles
                    .Include(t => t.Publisher)
                    .Include(t => t.TitleAuthors)
                        .ThenInclude(ta => ta.Author)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTitle))
                    query = query.Where(t => t.TitleName.Contains(SearchTitle));

                if (!string.IsNullOrWhiteSpace(SearchType))
                    query = query.Where(t => t.Type != null && t.Type.Contains(SearchType));

                if (!string.IsNullOrWhiteSpace(SearchPublisher))
                    query = query.Where(t => t.Publisher != null &&
                        t.Publisher.PubName != null &&
                        t.Publisher.PubName.Contains(SearchPublisher));

                if (!string.IsNullOrWhiteSpace(SearchAuthor))
                    query = query.Where(t => t.TitleAuthors.Any(ta =>
                        ta.Author != null &&
                        (ta.Author.AuFname.Contains(SearchAuthor) ||
                         ta.Author.AuLname.Contains(SearchAuthor))));

                var results = query.OrderBy(t => t.TitleName).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Titles = new ObservableCollection<Title>(results);
                    ResultCount = $"📊  {results.Count} of {ctx.Titles.Count()} books";
                });

                StatusMessageChanged?.Invoke($"Found {results.Count} matching titles.");
            });
        }

        private void Clear()
        {
            _searchTimer.Stop();
            SearchTitle     = string.Empty;
            SearchType      = string.Empty;
            SearchPublisher = string.Empty;
            SearchAuthor    = string.Empty;
            DetailAuthors   = string.Empty;
            DetailStores    = string.Empty;
            SelectedTitle   = null;
            LoadAll();
        }

        private void LoadTitleDetails(Title title)
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var authors = ctx.TitleAuthors
                    .Include(ta => ta.Author)
                    .Where(ta => ta.TitleId == title.TitleId)
                    .OrderBy(ta => ta.AuOrd)
                    .Select(ta => ta.Author != null ? $"{ta.Author.AuFname} {ta.Author.AuLname}" : "Unknown")
                    .ToList();

                var stores = ctx.Sales
                    .Include(s => s.Store)
                    .Where(s => s.TitleId == title.TitleId)
                    .Select(s => s.Store)
                    .Distinct()
                    .Where(st => st != null)
                    .Select(st => $"{st!.StorName} ({st.City}, {st.State})")
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DetailAuthors = authors.Count > 0
                        ? string.Join(", ", authors)
                        : "No authors listed";

                    DetailStores = stores.Count > 0
                        ? string.Join("; ", stores)
                        : "Not available in any store";
                });
            });
        }

        private void ExportCsv()
        {
            var saved = CsvExporter.Export(
                Titles,
                ["Title ID", "Title", "Type", "Publisher", "Price", "YTD Sales", "Pub Date"],
                t =>
                [
                    t.TitleId,
                    t.TitleName,
                    t.Type ?? string.Empty,
                    t.Publisher?.PubName ?? string.Empty,
                    t.Price.HasValue ? t.Price.Value.ToString("F2") : string.Empty,
                    t.YtdSales?.ToString() ?? string.Empty,
                    t.PubDate.ToString("yyyy-MM-dd")
                ],
                "books");

            if (saved)
                StatusMessageChanged?.Invoke($"✅  Exported {Titles.Count} books to CSV.");
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
