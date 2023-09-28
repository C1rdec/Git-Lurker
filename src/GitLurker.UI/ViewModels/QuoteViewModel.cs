using Caliburn.Micro;
using GitLurker.Core.Models;

namespace GitLurker.UI.ViewModels
{
	public class QuoteViewModel : PropertyChangedBase
	{
		private string _quote;
		private bool _hasQuote;

		public QuoteViewModel(SettingsFile file)
		{
			_quote = file.Entity.Quote;
			_hasQuote = !string.IsNullOrEmpty(_quote);
		}

		public string Quote => _quote;

		public bool HasQuote
		{
			get => _hasQuote;
			set
			{
				_hasQuote = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(() => HasNoQuote);
			}
		}

		public bool HasNoQuote => !HasQuote;
    }
}
