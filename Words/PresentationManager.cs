using System;
using Words.Presentation;
using Words.Presentation.Wpf;
using System.ComponentModel;

namespace Words
{
	public class PresentationManager : INotifyPropertyChanged
	{
		private readonly PresentationArea area;
		private readonly Words.Presentation.Wpf.Blackscreen blackscreen;
		private IPresentation currentPresentation;
		private PresentationStatus status = PresentationStatus.Hide;

		internal PresentationManager()
		{
			this.area = new PresentationArea();
			this.blackscreen = CreatePresentation<Words.Presentation.Wpf.Blackscreen>();
		}

		public T CreatePresentation<T>() where T : IPresentation, new()
		{
			T pres = new T();
			pres.Init(this.Area);
			return pres;
		}

		public PresentationArea Area
		{
			get
			{ 
				return this.area;
			}
		}

		public IPresentation Blackscreen
		{
			get
			{
				return this.blackscreen;
			}
		}

		public IPresentation CurrentPresentation
		{
			get
			{
				return this.currentPresentation;
			}
			set
			{
				if (Status == PresentationStatus.Show)
				{
					IPresentation previousPresentation = currentPresentation;
					currentPresentation = value;

					Action afterShowing = () =>
						{
							if (previousPresentation != null /*&& !previousPresentation.UsesSamePresentationWindow(currentPresentation)*/)
							{
								previousPresentation.Close();
							}
							if (currentPresentation != null && !currentPresentation.UsesSamePresentationWindow(this.blackscreen))
							{
								this.blackscreen.Hide();
							}
						};

					if (currentPresentation.TransitionPossibleFrom(previousPresentation))
					{
						currentPresentation.Show(Properties.Settings.Default.PresentationTransition, afterShowing);
					}
					else
					{
						currentPresentation.Show();
						afterShowing();
					}
				}
				else
				{
					if (currentPresentation != null)
						currentPresentation.Close();

					currentPresentation = value;
				}

				OnPropertyChanged("CurrentPresentation");
			}
		}

		public PresentationStatus Status
		{
			get
			{
				return status;
			}
			set
			{
				if (value == status)
					return;

				switch (value)
				{
					case PresentationStatus.Hide:
						if (currentPresentation != null)
							currentPresentation.Hide();
						if (this.blackscreen != null)
							this.blackscreen.Hide();
					   
						status = PresentationStatus.Hide;
						break;

					case PresentationStatus.Blackscreen:
						if (status == PresentationStatus.Show && this.currentPresentation.TransitionPossibleFrom(this.blackscreen))
							this.blackscreen.Show(Properties.Settings.Default.PresentationTransition);
						else
							this.blackscreen.Show();

						if (currentPresentation != null && !currentPresentation.UsesSamePresentationWindow(this.blackscreen))
						{
							currentPresentation.Hide();
						}

						status = PresentationStatus.Blackscreen;
						break;

					case PresentationStatus.Show:
						if (currentPresentation == null)
						{
							throw new InvalidOperationException("Can't show presentation when none is active");
						}

						if (status == PresentationStatus.Blackscreen && this.currentPresentation.TransitionPossibleFrom(this.blackscreen))
							this.currentPresentation.Show(Properties.Settings.Default.PresentationTransition);
						else
							this.currentPresentation.Show();

						if (!currentPresentation.UsesSamePresentationWindow(this.blackscreen))
							this.blackscreen.Hide();

						status = PresentationStatus.Show;
						break;
				}

				OnPropertyChanged("Status");
			}
		}

		public void Shutdown()
		{
			Status = PresentationStatus.Hide;
			if (CurrentPresentation != null)
			{
				CurrentPresentation.Close();
				CurrentPresentation = null;
			}
			Blackscreen.Close();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
