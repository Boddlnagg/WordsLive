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

		public IPresentation CreatePresentation(Type t)
		{
			if (t.IsClass && !t.IsAbstract && typeof(IPresentation).IsAssignableFrom(t))
			{
				var pres = (IPresentation)Activator.CreateInstance(t);
				pres.Init(this.Area);
				return pres;
			}
			else
			{
				throw new InvalidOperationException("Can't create presentation of non-presentation type or abstract class.");
			}
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
					if (value == null)
						throw new InvalidOperationException("Can't set CurrentPresentation to null when PresentationStatus is set to Show");

					IPresentation previousPresentation = currentPresentation;
					currentPresentation = value;

					Action afterShowing = () =>
						{
							if (previousPresentation != null)
							{
								if (!currentPresentation.UsesSamePresentationWindow(previousPresentation) && previousPresentation.UsesSamePresentationWindow(blackscreen))
								{
									blackscreen.Show();
								}

								previousPresentation.Close();
							}
							if (currentPresentation != null && !currentPresentation.UsesSamePresentationWindow(this.blackscreen))
							{
								this.blackscreen.Hide();
							}
						};

					if (currentPresentation.TransitionPossibleFrom(previousPresentation))
					{
						currentPresentation.Show(Properties.Settings.Default.PresentationTransition, afterShowing, previousPresentation);
					}
					else if (previousPresentation.TransitionPossibleTo(currentPresentation))
					{
						currentPresentation.Show();
						previousPresentation.TransitionTo(currentPresentation, Properties.Settings.Default.PresentationTransition, afterShowing);
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
						Action afterBlackscreen = () =>
							{
								if (currentPresentation != null && !currentPresentation.UsesSamePresentationWindow(this.blackscreen))
								{
									currentPresentation.Hide();
								}
							};

						if (status == PresentationStatus.Show && this.blackscreen.TransitionPossibleFrom(this.currentPresentation))
						{
							this.blackscreen.Show(Properties.Settings.Default.PresentationTransition, afterBlackscreen, this.currentPresentation);
						}
						else
						{
							this.blackscreen.Show();
							afterBlackscreen();
						}

						status = PresentationStatus.Blackscreen;
						break;

					case PresentationStatus.Show:
						if (currentPresentation == null)
						{
							throw new InvalidOperationException("Can't show presentation when none is active");
						}

						Action afterShowing = () => {
							if (!currentPresentation.UsesSamePresentationWindow(this.blackscreen))
								this.blackscreen.Hide();
						};

						if (status == PresentationStatus.Blackscreen && this.currentPresentation.TransitionPossibleFrom(this.blackscreen))
						{
							this.currentPresentation.Show(Properties.Settings.Default.PresentationTransition, afterShowing, this.blackscreen);
						}
						else if (status == PresentationStatus.Blackscreen && this.blackscreen.TransitionPossibleTo(this.currentPresentation))
						{
							this.currentPresentation.Show();
							this.blackscreen.TransitionTo(this.currentPresentation, Properties.Settings.Default.PresentationTransition, afterShowing);
						}
						else
						{
							this.currentPresentation.Show();
							afterShowing();
						}

						

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
