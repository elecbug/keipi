namespace KeiPI
{
    public class DataRow
    {
        public int No { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public DateOnly Date { get; set; }
        public string Writer { get; set; }

        public DataRow(int no, string title, string link, DateOnly date, string writer)
        {
            No = no;
            Title = title;
            Link = link;
            Date = date;
            Writer = writer;
        }

        public override string ToString()
        {
            return $"No: {No}\nTitle: {Title}\nLink: {Link}\nDate: {Date}\nWriter: {Writer}\n\n";
        }
    }
}