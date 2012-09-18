using System;
using System.Drawing;
using System.Windows.Forms;

namespace WordsLive.Presentation
{
	/// <summary>
	/// Implements a simple black screen on top of WinForms.
	/// </summary>
	public class Blackscreen : IPresentation
	{
		private PresentationForm form;

		/// <summary>
		/// Initializes a new instance of the <see cref="Blackscreen"/> class.
		/// </summary>
		public Blackscreen()
		{
			this.form = new PresentationForm();
		}

		/// <summary>
		/// Shows this presentation (either for the first time or after it has been hidden).
		/// </summary>
		/// <param name="transitionDuration">Duration of the transition in milliseconds.
		/// As this presentation does not support transitions, this parameter will be ignored.</param>
		/// <param name="callback">An optional callback action that will be called after the transition
		/// (or instantly when there is no transition).</param>
		public void Show(int transitionDuration = 0, Action callback = null, IPresentation previous = null)
		{
			this.form.Show();
			if (callback != null)
				callback();
		}

		public void TransitionTo(IPresentation target, int transitionDuration, Action callback)
		{
			// not possible
		}

		/// <summary>
		/// Hides this presentation. Use this if you want to be able to show it again later.
		/// </summary>
		public void Hide()
		{
			this.form.Hide();
		}

		/// <summary>
		/// Gets the preview provider for this presentation.
		/// </summary>
		public IPreviewProvider Preview
		{
			get
			{
				Panel p = new Panel();
				p.Dock = DockStyle.Fill;
				p.BackColor = Color.Black;
				return new BasicPreviewProvider<Panel>(p);
			}
		}

		/// <summary>
		/// Determines whether this presentation useses the same presentation window as another specified presentation.
		/// </summary>
		/// <param name="presentation">The other presentation to check.</param>
		/// <returns>
		///   <c>true</c> if the other presentation uses the same window; otherwise <c>false</c>
		/// </returns>
		public bool UsesSamePresentationWindow(IPresentation presentation)
		{
			return false;
		}

		/// <summary>
		/// Closes this presentation and frees external resources that were used for this presentation.
		/// You will not be able to show it again.
		/// </summary>
		public void Close()
		{
			this.form.Close();
		}

		/// <summary>
		/// Determines whether a transitions is possible from another presentation.
		/// </summary>
		/// <param name="presentation">The other presentation to check.</param>
		/// <returns>
		///   <c>true</c> if a transition is possible from the other presentation; otherwise <c>false</c>
		/// </returns>
		public bool TransitionPossibleFrom(IPresentation presentation)
		{
			return false;
		}

		public bool TransitionPossibleTo(IPresentation presentation)
		{
			return false;
		}

		/// <summary>
		/// Gets the presentation area, that was assigned to this presentation.
		/// </summary>
		public PresentationArea Area
		{
			get
			{
				return this.form.Area;
			}
		}

		/// <summary>
		/// Initializes this presentation with a specified presentation area.
		/// This will be automatically called when creating a presentation.
		/// </summary>
		/// <param name="area">The presentation area that describes, where this presentation should be shown on the screen.</param>
		public void Init(PresentationArea area)
		{
			this.form.Area = area;
		}
	}
}
