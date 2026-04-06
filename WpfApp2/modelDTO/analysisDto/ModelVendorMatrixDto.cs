using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.modelDTO.analysisDto
{
    public class ModelVendorMatrixDto
    {
        public List<string> Vendors { get; set; } = new();

        public List<ModelVendorMatrixRowDto> Rows { get; set; } = new();
    }
}
