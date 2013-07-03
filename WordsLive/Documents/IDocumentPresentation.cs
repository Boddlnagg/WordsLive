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
		void GoToPage(int page);
		void PreviousPage();
		void NextPage();
		bool CanGoToPreviousPage { get; }
		bool CanGoToNextPage { get; }
		DocumentPageScale PageScale { get; set; }
		event EventHandler DocumentLoaded;
	}
}
