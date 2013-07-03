using System;
using WordsLive.Presentation;

namespace WordsLive.Documents
{
	public interface IDocumentPresentation : IPresentation
	{
		void Load();
		bool IsLoaded { get; }
		int PageCount { get; }
		int CurrentPage { get; }
		void GotoPage(int page);
		void PreviousPage();
		void NextPage();
		void FitToWidth();
		void WholePage();
		event EventHandler DocumentLoaded;
	}
}
