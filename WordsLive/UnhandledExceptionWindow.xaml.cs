﻿using System;
using System.Collections.Generic;
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

namespace WordsLive
{
	/// <summary>
	/// Interaktionslogik für UnhandledExceptionWindow.xaml
	/// </summary>
	public partial class UnhandledExceptionWindow : Window
	{
		public Exception Exception
		{
			get;
			private set;
		}


		public UnhandledExceptionWindow(Exception exception)
		{
			InitializeComponent();

			this.Exception = exception;
			this.DataContext = this;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}
	}
}
