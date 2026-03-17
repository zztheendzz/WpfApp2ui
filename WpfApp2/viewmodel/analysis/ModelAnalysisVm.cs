using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.modelDTO;
using WpfApp2.modelDTO.analysysDto;
using WpfApp2.Services;
using WpfApp2.Services.analysisService;

namespace WpfApp2.viewmodel.analysis
{
    public class ModelAnalysisVm
    {
        private readonly ModelAnalysisSv _service;

        public ICommand AnalyzeModelCommand { get; set; }
        public ObservableCollection<ModelDto> Models { get; set; }

        public ObservableCollection<ModelDto> FilteredModels { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterModel();
            }
        }

        public ModelDto SelectedModel { get; set; }

        public ModelAnalysisVm()
        {
            AnalyzeModelCommand = new RelayCommand(analysisModel);
            var db = new DatabaseService().GetConnection();
            _service = new ModelAnalysisSv(db);

            VendorPrices = new ObservableCollection<ModelAnalysisDto>();
            PurchaseHistory = new ObservableCollection<PurchaseDto>();
        }


        public void analysisModel(Object obj)
        {
            if (SelectedModel == null) return;

            var summary = _service.GetSummary(SelectedModel.Id);

            LastPrice = summary.lastPrice;
            MinPrice = summary.minPrice;
            MaxPrice = summary.maxPrice;
            AvgPrice = summary.avgPrice;

            VendorPrices = new ObservableCollection<ModelAnalysisDto>(
                _service.GetVendorPrices(SelectedModel.Id)
            );

            PurchaseHistory = new ObservableCollection<PurchaseDto>(
                _service.GetPurchaseHistory(SelectedModel.Id)
            );



            OnPropertyChanged(nameof(VendorPrices));
            OnPropertyChanged(nameof(LastPrice));
            OnPropertyChanged(nameof(MinPrice));
            OnPropertyChanged(nameof(MaxPrice));
            OnPropertyChanged(nameof(AvgPrice));
        }


        private void FilterModel()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredModels = new ObservableCollection<ModelDto>(Models);
            }
            else
            {
               FilteredModels = new ObservableCollection<ModelDto>(
                    Models.Where(x => x.ModelName.ToLower().Contains(SearchText.ToLower()))
                );
            }

            OnPropertyChanged(nameof(FilteredModels));
        }

        // Summary
        private decimal _lastPrice;
        public decimal LastPrice
        {
            get => _lastPrice;
            set { _lastPrice = value; OnPropertyChanged(); }
        }

        private decimal _minPrice;
        public decimal MinPrice
        {
            get => _minPrice;
            set { _minPrice = value; OnPropertyChanged(); }
        }

        private decimal _maxPrice;
        public decimal MaxPrice
        {
            get => _maxPrice;
            set { _maxPrice = value; OnPropertyChanged(); }
        }

        private decimal _avgPrice;
        public decimal AvgPrice
        {
            get => _avgPrice;
            set { _avgPrice = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ModelAnalysisDto> VendorPrices { get; set; }

        public ObservableCollection<PurchaseDto> PurchaseHistory { get; set; }

        public void Load(int modelId)
        {
            var summary = _service.GetSummary(modelId);

            LastPrice = summary.lastPrice;
            MinPrice = summary.minPrice;
            MaxPrice = summary.maxPrice;
            AvgPrice = summary.avgPrice;

            VendorPrices.Clear();
            foreach (var v in _service.GetVendorPrices(modelId))
                VendorPrices.Add(v);

            PurchaseHistory.Clear();
            foreach (var p in _service.GetPurchaseHistory(modelId))
                PurchaseHistory.Add(p);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}