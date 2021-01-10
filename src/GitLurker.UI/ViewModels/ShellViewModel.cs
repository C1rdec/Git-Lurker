namespace GitLurker.UI.ViewModels
{
    using GitLurker.Models;

    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
            this.WorkspaceViewModel = new WorkspaceViewModel(new Workspace(@"C:\Github"));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title => ".NET 5!!!";

        /// <summary>
        /// Gets the workspace view model.
        /// </summary>
        /// <value>The workspace view model.</value>
        public WorkspaceViewModel WorkspaceViewModel { get; private set; }

        #endregion
    }
}
