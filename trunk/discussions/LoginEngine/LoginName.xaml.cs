﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Discussions.DbModel;
using LoginEngine;

namespace Discussions
{
    public partial class LoginName : Window
    {       
        public bool BackClicked = false;
         
        string _enteredName = "<User name>"; 
        public string EnteredName
        {
            get
            {
                return _enteredName;
            }
            set
            {
                _enteredName = value;
            }
        }

        public LoginName()
        {
            InitializeComponent();

            DataContext = this;

            //SkinManager.ChangeSkin("GreenSkin.xaml", this.Resources);
            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);          
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackClicked = true;
            Close();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (EnteredName.Trim() == "")
                EnteredName = null;
            
            Close();
        }

        private void tbxName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            decorations.SetGreetingName(tbxName.Text);
        }
    }
}
