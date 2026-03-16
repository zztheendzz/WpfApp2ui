using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.viewmodel.common;

namespace WpfApp2.viewmodel
{

    class EquipmentViewModel : BaseCrudViewModel<Equipment>
    {
        public EquipmentViewModel() : base(new BaseService<Equipment>())
        {

        }
    }

}
