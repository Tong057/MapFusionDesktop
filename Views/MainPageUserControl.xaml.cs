﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.Globe.ClientControl.Models;

namespace MapFusion.ViewModels
{
    /// <summary>
    /// Interaction logic for MainPageUserControl.xaml
    /// </summary>
    public partial class MainPageUserControl : UserControl
    {
        public MainPageUserControl(MainViewModel mainVM)
        {
            InitializeComponent();

            DataContext = new MainPageViewModel(mainVM);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            globe1.Initialise(new WPF.Globe.ClientControl.LayerTypes.Image_Layers.BingMapLayer()
            {
                ID = new Guid().ToString().Replace("-", ""),
                Name = "Bing Layer",
                MapMode = WPF.Globe.ClientControl.LayerTypes.Image_Layers.BingMapLayer.ImageMode.h
            });
        }


    }
}
