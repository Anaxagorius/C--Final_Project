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
    /// ViewModel for the Book Search and Display view.
    /// Allows searching for book titles by title, type, publisher, and author.
    /// </summary>
    public class BookSearchViewModel : BaseViewModel
    {
        // ── Events / Delegates ────────────────────────────────────────────────────
        /// <summary>Raised when the status message should be updated.</summary>
        public event Action<string>? StatusMessageChanged;

        // ── Backing fields ────────────────────────────────────────────────────────
        private ObservableCollection<Title> _titles = new();
        private Title? _selectedTitle;
        private string _searchTitle = string.Empty;
        private string _searchType = string.Empty;
        private string _searchPublisher = string.Empty;
        private string _searchAuthor = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // Detail panel
        private string _detailAuthors = string.Empty;
        private string _detailStores = string.Empty;

        // ── Properties ────────────────────────────────────────────────────────────
        /// <summary>Titles that match the current search criteria.</summary>
        public ObservableCollection<Title> Titles
        {
            get => _titles;
            set => SetProperty(ref _titles, value);
        }

        /// <summary>Currently selected title (shows detail panel).</summary>
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
            set => SetProperty(ref _searchTitle, value);
        }

        public string SearchType
        {
            get => _searchType;
            set => SetProperty(ref _searchType, value);
        }

        public string SearchPublisher
        {
            get => _searchPublisher;
            set => SetProperty(ref _searchPublisher, value);
        }

        public string SearchAuthor
        {
            get => _searchAuthor;
            set => SetProperty(ref _searchAuthor, value);
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

        /// <summary>Comma-separated list of authors for the selected title.</summary>
        public string DetailAuthors
        {
            get => _detailAuthors;
            set => SetProperty(ref _detailAuthors, value);
        }

        /// <summary>Store availability details for the selected title.</summary>
        public string DetailStores
        {
            get => _detailStores;
            set => SetProperty(ref _detailStores, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LoadAllCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public BookSearchViewModel()
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

                var titles = ctx.Titles
                    .Include(t => t.Publisher)
                    .OrderBy(t => t.TitleName)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                    Titles = new ObservableCollection<Title>(titles));

                StatusMessageChanged?.Invoke($"Loaded {titles.Count} book titles.");
            });
        }

        private void Search()
        {
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
                    Titles = new ObservableCollection<Title>(results));

                StatusMessageChanged?.Invoke($"Found {results.Count} matching titles.");
            });
        }

        private void Clear()
        {
            SearchTitle = string.Empty;
            SearchType = string.Empty;
            SearchPublisher = string.Empty;
            SearchAuthor = string.Empty;
            DetailAuthors = string.Empty;
            DetailStores = string.Empty;
            SelectedTitle = null;
            LoadAll();
        }

        private void LoadTitleDetails(Title title)
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                // Authors
                var authors = ctx.TitleAuthors
                    .Include(ta => ta.Author)
                    .Where(ta => ta.TitleId == title.TitleId)
                    .OrderBy(ta => ta.AuOrd)
                    .Select(ta => ta.Author != null ? $"{ta.Author.AuFname} {ta.Author.AuLname}" : "Unknown")
                    .ToList();

                // Stores that have sold this title
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
