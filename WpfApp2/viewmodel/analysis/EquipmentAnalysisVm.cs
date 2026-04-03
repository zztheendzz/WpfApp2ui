using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows; // Thêm để dùng MessageBox
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDto;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;
using WpfApp2.Services.exception;

namespace WpfApp2.viewmodel.analysis
{
    public class EquipmentAnalysisVm : INotifyPropertyChanged
    {
        private readonly SearchService _searchService = new SearchService();
        private readonly EquipmentAnalysisSv _service = new EquipmentAnalysisSv();
        private bool _isInternalChange;

        #region ===================== Properties =====================

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                if (_globalSearchText == value) return;
                _globalSearchText = value;
                OnPropertyChanged();

                if (!_isInternalChange)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        SelectedEquipmentId = 0;
                        SearchSuggestions.Clear();
                        IsSearchDropDownOpen = false;
                    }
                    else
                    {
                        UpdateSuggestions();
                    }
                }
            }
        }

        private SearchResultDto _selectedSearchResult;
        public SearchResultDto SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                if (_selectedSearchResult == value) return;
                _selectedSearchResult = value;
                OnPropertyChanged();
            }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set
            {
                if (_isSearchDropDownOpen == value) return;
                _isSearchDropDownOpen = value;
                OnPropertyChanged();
            }
        }

        private int _selectedEquipmentId;
        public int SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set { _selectedEquipmentId = value; OnPropertyChanged(); }
        }

        private EquipmentAnalysisDto _analysis;
        public EquipmentAnalysisDto Analysis
        {
            get => _analysis;
            set { _analysis = value; OnPropertyChanged(); }
        }

        public ICommand AnalyzeCommand { get; set; }
        public ObservableCollection<SearchResultDto> SearchSuggestions { get; } = new ObservableCollection<SearchResultDto>();

        #endregion

        public EquipmentAnalysisVm()
        {
            AnalyzeCommand = new RelayCommand(_ => LoadData());
        }

        #region ===================== Methods =====================

        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText) || GlobalSearchText.Length < 2)
            {
                SearchSuggestions.Clear();
                IsSearchDropDownOpen = false;
                return;
            }

            try
            {
                var results = _searchService.SearchEquipment(GlobalSearchText);

                SearchSuggestions.Clear();
                if (results != null)
                {
                    foreach (var item in results)
                    {
                        SearchSuggestions.Add(item);
                    }
                }

                IsSearchDropDownOpen = SearchSuggestions.Any();
                SelectedSearchResult = SearchSuggestions.FirstOrDefault();
            }
            catch (DatabaseLockedException)
            {
                // Khi đang gõ mà bị lock thì ẩn gợi ý đi để không gây crash hoặc giật lag
                IsSearchDropDownOpen = false;
            }
        }

        public void ConfirmSelection()
        {
            if (SelectedSearchResult == null) return;

            _isInternalChange = true;
            try
            {
                SelectedEquipmentId = SelectedSearchResult.Data switch
                {
                    Equipment e => e.Id,
                    EquipmentDto d => d.Id,
                    _ => SelectedSearchResult.Id
                };

                GlobalSearchText = SelectedSearchResult.Text;
                IsSearchDropDownOpen = false;

                LoadData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting equipment: {ex.Message}");
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        private void LoadData()
        {
            if (SelectedEquipmentId <= 0) return;

            try
            {
                var result = _service.GetEquipmentAnalysis(SelectedEquipmentId);
                if (result != null)
                {
                    Analysis = result;
                }
            }
            catch (DatabaseLockedException)
            {
                MessageBox.Show("Cơ sở dữ liệu đang bận tính toán thông số thiết bị. Vui lòng nhấn nút Thống kê lại sau giây lát.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi phân tích: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}