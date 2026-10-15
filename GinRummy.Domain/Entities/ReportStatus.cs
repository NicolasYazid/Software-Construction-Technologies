namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A state a report can be in, mapped to the ReportStatus table. The game's code only
    /// ever reads this catalog; it never creates or edits a status.
    /// </summary>
    public class ReportStatus
    {
        /// <summary>
        /// Name of the status of a report that has not been resolved yet.
        /// </summary>
        public const string PendingName = "Pending";

        /// <summary>
        /// Name of the status of a report the automatic check confirmed.
        /// </summary>
        public const string VerifiedName = "Verified";

        /// <summary>
        /// Name of the status of a report the automatic check discarded.
        /// </summary>
        public const string DismissedName = "Dismissed";

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ReportStatus()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int ReportStatusId { get; private set; }

        /// <summary>
        /// Gets the name of the status; one of the three name constants of this class.
        /// </summary>
        public string Name { get; private set; }
    }
}
