using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.viewmodel.common;

namespace WpfApp2.viewmodel
{

    class PurchaseViewModel : BaseCrudViewModel<PurchaseHistory>
    {

        public PurchaseViewModel() : base(new BaseService<PurchaseHistory>())
        {

        }


    }
}

