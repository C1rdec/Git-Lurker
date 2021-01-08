using System.IO;

namespace GitLurker.Models
{
    public class Repository
    {
        #region Fields

        /// <summary>
        /// The name
        /// </summary>
        private string _name;

        /// <summary>
        /// The configuration
        /// </summary>
        private Configuration _configuration;

        #endregion

        #region Constructors

        public static bool IsValid(string folder)
        {
            if (Path.GetFileName(folder).StartsWith("."))
            {
                return false;
            }

            return true;
        }

        public Repository(string folder)
        {
            this._name = Path.GetFileName(folder);
        }

        #endregion
    }
}
