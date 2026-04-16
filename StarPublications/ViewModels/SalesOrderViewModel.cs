using Microsoft.EntityFrameworkCore;
using StarPublications.Commands;
using StarPublications.Data;
using StarPublications.Models;
using StarPublications.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StarPublications.ViewModels
{
    /// <summary>
    /// ViewModel for the Sales Order Management view.
    /// Supports Add, Edit, Delete, Search (with date range), and CSV Export of sales orders.
    /// </summary>
    public class SalesOrderViewModel : BaseViewModel
    {
        // ── Events / Delegates ────────────────────────────────────────────────────
        /// <summary>Raised when the status message should be updated.</summary>
        public event Action<string>? StatusMessageChanged;

        // ── Backing fields ────────────────────────────────────────────────────────
        private ObservableCollection<Sale> _salesOrders = new();
        private ObservableCollection<Store> _stores = new();
        private ObservableCollection<Title> _titles = new();
        private Sale? _selectedSale;
        private string _searchOrderNumber = string.Empty;
        private string _searchStoreId = string.Empty;
        private DateTime? _searchDateFrom;
        private DateTime? _searchDateTo;
        private bool _isEditing;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _resultCount = string.Empty;

        // Edit form fields
        private string _editStorId = string.Empty;
        private string _editOrdNum = string.Empty;
        private DateTime _editOrdDate = DateTime.Today;
        private short _editQty = 1;
        private string _editPayTerms = string.Empty;
        private string _editTitleId = string.Empty;

        // ── Properties ────────────────────────────────────────────────────────────
        public ObservableCollection<Sale> SalesOrders
        {
            get => _salesOrders;
            set => SetProperty(ref _salesOrders, value);
        }

        public ObservableCollection<Store> Stores
        {
            get => _stores;
            set => SetProperty(ref _stores, value);
        }

        public ObservableCollection<Title> Titles
        {
            get => _titles;
            set => SetProperty(ref _titles, value);
        }

        public Sale? SelectedSale
        {
            get => _selectedSale;
            set
            {
                if (SetProperty(ref _selectedSale, value) && value != null)
                    PopulateEditForm(value);
            }
        }

        public string SearchOrderNumber
        {
            get => _searchOrderNumber;
            set => SetProperty(ref _searchOrderNumber, value);
        }

        public string SearchStoreId
        {
            get => _searchStoreId;
            set => SetProperty(ref _searchStoreId, value);
        }

        /// <summary>Filter: earliest order date (inclusive).</summary>
        public DateTime? SearchDateFrom
        {
            get => _searchDateFrom;
            set => SetProperty(ref _searchDateFrom, value);
        }

        /// <summary>Filter: latest order date (inclusive).</summary>
        public DateTime? SearchDateTo
        {
            get => _searchDateTo;
            set => SetProperty(ref _searchDateTo, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
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

        /// <summary>Friendly record-count string shown in the toolbar.</summary>
        public string ResultCount
        {
            get => _resultCount;
            set => SetProperty(ref _resultCount, value);
        }

        // Edit form properties
        public string EditStorId
        {
            get => _editStorId;
            set => SetProperty(ref _editStorId, value);
        }

        public string EditOrdNum
        {
            get => _editOrdNum;
            set => SetProperty(ref _editOrdNum, value);
        }

        public DateTime EditOrdDate
        {
            get => _editOrdDate;
            set => SetProperty(ref _editOrdDate, value);
        }

        public short EditQty
        {
            get => _editQty;
            set => SetProperty(ref _editQty, value);
        }

        public string EditPayTerms
        {
            get => _editPayTerms;
            set => SetProperty(ref _editPayTerms, value);
        }

        public string EditTitleId
        {
            get => _editTitleId;
            set => SetProperty(ref _editTitleId, value);
        }

        /// <summary>Common payment term options shown in the dropdown.</summary>
        public string[] PayTermOptions { get; } =
        [
            "Net 30",
            "Net 60",
            "Net 90",
            "ON Invoice",
            "COD",
            "30 Days",
            "60 Days",
            "Due on Receipt"
        ];

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ExportCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public SalesOrderViewModel()
        {
            LoadDataCommand   = new RelayCommand(_ => LoadData());
            SearchCommand     = new RelayCommand(_ => Search());
            AddNewCommand     = new RelayCommand(_ => AddNew());
            EditCommand       = new RelayCommand(_ => BeginEdit(), _ => SelectedSale != null);
            SaveCommand       = new RelayCommand(_ => Save(), _ => IsEditing);
            DeleteCommand     = new RelayCommand(_ => DeleteSelected(), _ => SelectedSale != null && !IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
            ExportCommand     = new RelayCommand(_ => ExportCsv(), _ => SalesOrders.Count > 0);

            LoadData();
        }

        // ── Data Operations ───────────────────────────────────────────────────────

        private void LoadData()
        {
            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var stores = ctx.Stores.OrderBy(s => s.StorName).ToList();
                var titles = ctx.Titles.OrderBy(t => t.TitleName).ToList();
                var sales = ctx.Sales
                    .Include(s => s.Store)
                    .Include(s => s.Title)
                    .OrderByDescending(s => s.OrdDate)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stores = new ObservableCollection<Store>(stores);
                    Titles = new ObservableCollection<Title>(titles);
                    SalesOrders = new ObservableCollection<Sale>(sales);
                    ResultCount = $"📊  {sales.Count} orders";
                });

                StatusMessageChanged?.Invoke($"Loaded {sales.Count} sales orders.");
            });
        }

        private void Search()
        {
            // Validate date range before hitting the DB
            if (SearchDateFrom.HasValue && SearchDateTo.HasValue && SearchDateFrom.Value > SearchDateTo.Value)
            {
                ErrorMessage = "The 'From' date must be on or before the 'To' date.";
                return;
            }

            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                var query = ctx.Sales
                    .Include(s => s.Store)
                    .Include(s => s.Title)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchOrderNumber))
                    query = query.Where(s => s.OrdNum.Contains(SearchOrderNumber));

                if (!string.IsNullOrWhiteSpace(SearchStoreId))
                    query = query.Where(s => s.StorId.Contains(SearchStoreId));

                if (SearchDateFrom.HasValue)
                    query = query.Where(s => s.OrdDate >= SearchDateFrom.Value);

                if (SearchDateTo.HasValue)
                    query = query.Where(s => s.OrdDate <= SearchDateTo.Value);

                var results = query.OrderByDescending(s => s.OrdDate).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SalesOrders = new ObservableCollection<Sale>(results);
                    ResultCount = $"📊  {results.Count} of {ctx.Sales.Count()} orders";
                });

                StatusMessageChanged?.Invoke($"Found {results.Count} matching sales orders.");
            });
        }

        private void ClearSearch()
        {
            SearchOrderNumber = string.Empty;
            SearchStoreId = string.Empty;
            SearchDateFrom = null;
            SearchDateTo = null;
            LoadData();
        }

        private void AddNew()
        {
            SelectedSale = null;
            EditStorId    = Stores.FirstOrDefault()?.StorId ?? string.Empty;
            EditOrdNum    = string.Empty;
            EditOrdDate   = DateTime.Today;
            EditQty       = 1;
            EditPayTerms  = "Net 30";
            EditTitleId   = Titles.FirstOrDefault()?.TitleId ?? string.Empty;
            IsEditing     = true;
            ErrorMessage  = string.Empty;
        }

        private void BeginEdit()
        {
            if (SelectedSale == null) return;
            PopulateEditForm(SelectedSale);
            IsEditing    = true;
            ErrorMessage = string.Empty;
        }

        private void Save()
        {
            if (!Validate()) return;

            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();

                if (SelectedSale == null)
                {
                    // ── Add new sale ───────────────────────────────────────────
                    bool exists = ctx.Sales.Any(s =>
                        s.StorId   == EditStorId &&
                        s.OrdNum   == EditOrdNum &&
                        s.TitleId  == EditTitleId);

                    if (exists)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                            ErrorMessage = "A sales order with the same Store, Order Number, and Title already exists.");
                        return;
                    }

                    var newSale = new Sale
                    {
                        StorId   = EditStorId.Trim(),
                        OrdNum   = EditOrdNum.Trim(),
                        OrdDate  = EditOrdDate,
                        Qty      = EditQty,
                        PayTerms = EditPayTerms.Trim(),
                        TitleId  = EditTitleId.Trim()
                    };

                    ctx.Sales.Add(newSale);
                    ctx.SaveChanges();
                    StatusMessageChanged?.Invoke("✅  Sales order added successfully.");
                }
                else
                {
                    // ── Update existing sale ───────────────────────────────────
                    var existing = ctx.Sales.Find(SelectedSale.StorId, SelectedSale.OrdNum, SelectedSale.TitleId);
                    if (existing != null)
                    {
                        existing.OrdDate  = EditOrdDate;
                        existing.Qty      = EditQty;
                        existing.PayTerms = EditPayTerms.Trim();
                        ctx.SaveChanges();
                    }

                    StatusMessageChanged?.Invoke("✅  Sales order updated successfully.");
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsEditing    = false;
                    ErrorMessage = string.Empty;
                });

                LoadData();
            });
        }

        private void DeleteSelected()
        {
            if (SelectedSale == null) return;

            var result = MessageBox.Show(
                $"Delete order '{SelectedSale.OrdNum}' for store '{SelectedSale.Store?.StorName ?? SelectedSale.StorId}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            TryExecute(() =>
            {
                using var ctx = DbContextFactory.Create();
                var sale = ctx.Sales.Find(SelectedSale.StorId, SelectedSale.OrdNum, SelectedSale.TitleId);
                if (sale != null)
                {
                    ctx.Sales.Remove(sale);
                    ctx.SaveChanges();
                }

                StatusMessageChanged?.Invoke("🗑  Sales order deleted.");
                LoadData();
            });
        }

        private void CancelEdit()
        {
            IsEditing    = false;
            ErrorMessage = string.Empty;
        }

        private void ExportCsv()
        {
            var saved = CsvExporter.Export(
                SalesOrders,
                ["Store ID", "Store Name", "Order #", "Order Date", "Qty", "Pay Terms", "Title ID", "Book Title"],
                s =>
                [
                    s.StorId,
                    s.Store?.StorName ?? string.Empty,
                    s.OrdNum,
                    s.OrdDate.ToString("yyyy-MM-dd"),
                    s.Qty.ToString(),
                    s.PayTerms,
                    s.TitleId,
                    s.Title?.TitleName ?? string.Empty
                ],
                "sales_orders");

            if (saved)
                StatusMessageChanged?.Invoke($"✅  Exported {SalesOrders.Count} records to CSV.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void PopulateEditForm(Sale sale)
        {
            EditStorId   = sale.StorId;
            EditOrdNum   = sale.OrdNum;
            EditOrdDate  = sale.OrdDate;
            EditQty      = sale.Qty;
            EditPayTerms = sale.PayTerms;
            EditTitleId  = sale.TitleId;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(EditStorId))
            { ErrorMessage = "Store is required."; return false; }

            if (string.IsNullOrWhiteSpace(EditOrdNum))
            { ErrorMessage = "Order number is required."; return false; }

            if (EditOrdNum.Length > 20)
            { ErrorMessage = "Order number must be 20 characters or fewer."; return false; }

            if (EditQty <= 0)
            { ErrorMessage = "Quantity must be greater than zero."; return false; }

            if (string.IsNullOrWhiteSpace(EditPayTerms))
            { ErrorMessage = "Payment terms are required."; return false; }

            if (string.IsNullOrWhiteSpace(EditTitleId))
            { ErrorMessage = "Title is required."; return false; }

            ErrorMessage = string.Empty;
            return true;
        }

        /// <summary>Wraps a synchronous DB call with loading state and error handling.</summary>
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
                ExceptionLogger.Log(nameof(SalesOrderViewModel), ex);
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
