namespace KeiPI
{
    public class KeiPIRow
    {
        public int No { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }
        public string Writer { get; set; }

        public KeiPIRow(int no, string title, string link, string date, string content, string writer)
        {
            No = no;
            Title = title;
            Link = link;
            Date = date;
            Content = content;
            Writer = writer;
        }

        public override string ToString()
        {
            return $"No: {No}\nTitle: {Title}\nLink: {Link}\nDate: {Date}\nContent: {Content}\nWriter: {Writer}\n\n";
        }
    }
}