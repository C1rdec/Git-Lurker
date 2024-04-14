namespace GitLurker.UI.ViewModels;

using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

public abstract class ItemListViewModelBase<T> : PropertyChangedBase
    where T : ItemViewModelBase
{
    private T _selectedViewModel;

    public T SelectedViewModel
    {
        get => _selectedViewModel;
        set
        {
            _selectedViewModel = value;
            NotifyOfPropertyChange();
        }
    }

    public abstract ObservableCollection<T> ItemViewModels { get; }

    protected bool IsMouseHover { get; private set; }

    public void Clear()
    {
        SelectedViewModel = null;
        Execute.OnUIThread(() => ItemViewModels.Clear());
    }

    public void MoveUp()
    {
        if (SelectedViewModel == null)
        {
            return;
        }

        SelectedViewModel = ItemViewModels.FirstOrDefault(g => g.Id == SelectedViewModel.Id);
        var index = ItemViewModels.IndexOf(SelectedViewModel);
        if (index <= 0)
        {
            return;
        }

        index--;
        SelectedViewModel.IsSelected = false;
        SelectedViewModel = ItemViewModels.ElementAt(index);
        SelectedViewModel.IsSelected = true;
    }

    public void MoveDown()
    {
        if (SelectedViewModel == null)
        {
            SelectedViewModel = ItemViewModels.FirstOrDefault();
            if (SelectedViewModel != null)
            {
                SelectedViewModel.IsSelected = true;
            }

            return;
        }

        SelectedViewModel = ItemViewModels.FirstOrDefault(g => g.Id == SelectedViewModel.Id);

        var index = ItemViewModels.IndexOf(SelectedViewModel);
        if (index == -1 || (index + 1) >= ItemViewModels.Count)
        {
            return;
        }

        index++;
        SelectedViewModel.IsSelected = false;
        SelectedViewModel = ItemViewModels.ElementAt(index);
        SelectedViewModel.IsSelected = true;
    }

    public void OnMouseEnter()
    {
        IsMouseHover = true;
    }

    public void OnMouseLeave()
    {
        IsMouseHover = false;
    }
}
