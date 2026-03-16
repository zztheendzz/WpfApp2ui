using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WpfApp2.command;
using WpfApp2.model;
using WpfApp2.Services;
using WpfApp2.view.pages;
using WpfApp2.viewmodel.common;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace WpfApp2.viewmodel
{
    class ModelViewModel :BaseCrudViewModel<Model>
    {

        public ModelViewModel():base(new BaseService<Model>())
        {

        }


    }
}
