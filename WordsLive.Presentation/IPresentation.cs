using System;

namespace WordsLive.Presentation
{
	public interface IPresentation
	{
		/// <summary>
		/// Gets the presentation area, that was assigned to this presentation.
		/// </summary>
		PresentationArea Area { get; }


		/// <summary>
		/// Gets the preview provider for this presentation.
		/// </summary>
		IPreviewProvider Preview { get; }


		/// <summary>
		/// Initializes this presentation with a specified presentation area.
		/// This will be automatically called when creating a presentation.
		/// </summary>
		/// <param name="area">The presentation area that describes, where this presentation should be shown on the screen.</param>
		void Init(PresentationArea area);

		/// <summary>
		/// Shows this presentation (either for the first time or after it has been hidden).
		/// </summary>
		/// <param name="transitionDuration">
		/// Duration of the transition in milliseconds.
		/// If this is greater than zero and the presentation supports transitions,
		/// there will be a transition from the last presentation to this one.
		/// </param>
		/// <param name="callback">
		/// An optional callback action that will be called after the transition
		/// (or instantly when there is no transition).
		/// </param>
		/// <param name="previous">
		/// The previous presentation when there is a transition.
		/// </param>
		void Show(int transitionDuration = 0, Action callback = null, IPresentation previous = null);

		void TransitionTo(IPresentation target, int transitionDuration, Action callback);

		/// <summary>
		/// Hides this presentation. Use this if you want to be able to show it again later.
		/// </summary>
		void Hide();

		/// <summary>
		/// Closes this presentation and frees external resources that were used for this presentation.
		/// You will not be able to show it again.
		/// </summary>
		void Close();

		/// <summary>
		/// Determines whether this presentation useses the same presentation window as another specified presentation.
		/// </summary>
		/// <param name="presentation">The other presentation to check.</param>
		/// <returns><c>true</c> if the other presentation uses the same window; otherwise <c>false</c></returns>
		bool UsesSamePresentationWindow(IPresentation presentation);

		/// <summary>
		/// Determines whether a transitions is possible from another presentation.
		/// </summary>
		/// <param name="presentation">The other presentation to check.</param>
		/// <returns><c>true</c> if a transition is possible from the other presentation; otherwise <c>false</c></returns>
		bool TransitionPossibleFrom(IPresentation presentation);

		bool TransitionPossibleTo(IPresentation presentation);
	}
}
