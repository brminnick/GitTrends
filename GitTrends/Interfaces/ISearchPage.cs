using System;
namespace GitTrends
{
    public interface ISearchPage
    {
        void OnSearchBarTextChanged(in string text);
        event EventHandler<string> SearchBarTextChanged;
    }
}
