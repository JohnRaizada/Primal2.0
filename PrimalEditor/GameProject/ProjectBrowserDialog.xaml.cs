﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PrimalEditor.GameProject
{
	/// <summary>
	/// Interaction logic for ProjectBrowserDialog.xaml
	/// </summary>
	public partial class ProjectBrowserDialog : Window
	{
		private readonly CubicEase _easing = new() { EasingMode = EasingMode.EaseInOut };
		/// <summary>
		/// Determines whether the Project Manager Window should be opened in New Project Tab
		/// </summary>
		public static bool GotoNewProjectTab { get; set; }
		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectBrowserDialog"/> class.
		/// </summary>
		public ProjectBrowserDialog()
		{
			InitializeComponent();
			Loaded += OnProjectBrowserDialogLoaded;
			ForceCursor = true;
		}
		private void OnProjectBrowserDialogLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnProjectBrowserDialogLoaded;
			if (!OpenProject.Projects.Any() || GotoNewProjectTab)
			{
				if (!GotoNewProjectTab)
				{
					openProjectButton.IsEnabled = false;
					openProjectView.Visibility = Visibility.Hidden;
				}
				OnToggleButton_Click(createProjectButton, new RoutedEventArgs());
			}
			GotoNewProjectTab = false;
		}
		private void AnimateToCreateProject()
		{
			DoubleAnimation highlightAnimation = new(200, 400, new Duration(TimeSpan.FromSeconds(0.2)))
			{
				EasingFunction = _easing
			};
			highlightAnimation.Completed += (s, e) =>
			{
				ThicknessAnimation animation = new(new Thickness(0), new Thickness(-1600, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)))
				{
					EasingFunction = _easing
				};
				browserContent.BeginAnimation(MarginProperty, animation);
			};
			highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
		}
		private void AnimateToOpenProject()
		{
			DoubleAnimation highlightAnimation = new(400, 200, new Duration(TimeSpan.FromSeconds(0.2)))
			{
				EasingFunction = _easing
			};
			highlightAnimation.Completed += (s, e) =>
			{
				ThicknessAnimation animation = new(new Thickness(-1600, 0, 0, 0), new Thickness(0), new Duration(TimeSpan.FromSeconds(0.5)))
				{
					EasingFunction = _easing
				};
				browserContent.BeginAnimation(MarginProperty, animation);
			};
			highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
		}
		private void OnToggleButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender == openProjectButton)
			{
				if (createProjectButton.IsChecked == true)
				{
					createProjectButton.IsChecked = false;
					AnimateToOpenProject();
					openProjectView.IsEnabled = true;
					newProjectView.IsEnabled = false;
				}
				openProjectButton.IsChecked = true;
				return;
			}
			if (openProjectButton.IsChecked == true)
			{
				openProjectButton.IsChecked = false;
				AnimateToCreateProject();
				openProjectView.IsEnabled = false;
				newProjectView.IsEnabled = true;
			}
			createProjectButton.IsChecked = true;
		}
	}
}
