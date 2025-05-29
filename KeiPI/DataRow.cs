namespace KeiPI
{
    /// <summary>
    /// Represents a data row containing information about a notice or announcement.
    /// </summary>
    public class DataRow
    {
        /// <summary>
        /// Gets or sets the unique identifier for the data row.
        /// </summary>
        public int No { get; set; }
        /// <summary>
        /// Gets or sets the title of the notice or announcement.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the link to the notice or announcement.
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Gets or sets the date of the notice or announcement.
        /// </summary>
        public DateOnly Date { get; set; }
        /// <summary>
        /// Gets or sets the name of the writer or author of the notice or announcement.
        /// </summary>
        public string Writer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRow"/> class with default values.
        /// </summary>
        /// <param name="no"></param>
        /// <param name="title"></param>
        /// <param name="link"></param>
        /// <param name="date"></param>
        /// <param name="writer"></param>
        public DataRow(int no, string title, string link, DateOnly date, string writer)
        {
            No = no;
            Title = title;
            Link = link;
            Date = date;
            Writer = writer;
        }

        /// <summary>
        /// Returns a string representation of the data row, including its properties.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"No: {(No == -1 ? "[°øÁö]" : $"{No}")}\nTitle: {Title}\nLink: {Link}\nDate: {Date}\nWriter: {Writer}\n\n";
        }
    }
}