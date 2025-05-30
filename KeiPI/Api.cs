using HtmlAgilityPack;
using System.Diagnostics;
using System.Xml;

namespace KeiPI
{
    /// <summary>
    /// Represents an API for fetching and parsing various types of notices from the Keimyung University website.
    /// </summary>
    public partial class Api
    {
        private const string UNSUPPORTED_API_TYPE_MESSAGE = $"Unsupported API type";
        private const string GREATER_THAN_ZERO_MESSAGE = "The value must be greater than 0";

        /// <summary>
        /// Cache for storing pages of notices to avoid redundant network requests.
        /// </summary>
        private static Dictionary<(ApiType, int), (List<DataRow>, DateTime)> PageCache { get; set; } 
            = new Dictionary<(ApiType, int), (List<DataRow>, DateTime)>();
        /// <summary>
        /// The time duration for which the page cache is valid before it needs to be refreshed.
        /// </summary>
        public static TimeSpan PageCachingTime { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets the name of the API type, which is used for display purposes.
        /// </summary>
        public string Name
        {
            get
            {
                switch (ApiType)
                {
                    case ApiType.GeneralNotice:
                        return "공지사항 - 일반";
                    case ApiType.HaksaNotice:
                        return "공지사항 - 학사";
                    case ApiType.JanghakNotice:
                        return "공지사항 - 장학";
                    case ApiType.MozipNotice:
                        return "공지사항 - 모집";
                    case ApiType.ChuiupNotice:
                        return "공지사항 - 취업";
                    case ApiType.GumeNotice:
                        return "공지사항 - 구매";

                    case ApiType.DeptComputer:
                        return "컴퓨터공학과 공지사항";
                    case ApiType.Teach:
                        return "교무교직팀 공지사항";

                    case ApiType.GraduateSchool:
                        return "대학원 공지사항";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, UNSUPPORTED_API_TYPE_MESSAGE);
                }
            }
        }
        /// <summary>
        /// The type of API to be used for fetching notices.
        /// </summary>
        public ApiType ApiType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Api"/> class with the specified API type.
        /// </summary>
        /// <param name="type"></param>
        public Api(ApiType type)
        {
            ApiType = type;
        }

        /// <summary>
        /// Fetches and parses the notices from the specified API type, returning a list of DataRow objects.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public List<DataRow> GetRowsWithPage(int page = 1)
        {
            if (page <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), page, GREATER_THAN_ZERO_MESSAGE);
            }

            if (PageCache.TryGetValue((ApiType, page), out var cachedRows))
            {
                TimeSpan cacheDuration = DateTime.Now - cachedRows.Item2;

                if (cacheDuration < PageCachingTime)
                {
                    return cachedRows.Item1;
                }
                else if (CheckSync(cachedRows.Item1))
                {
                    PageCache[(ApiType, page)] = (cachedRows.Item1, DateTime.Now);
                    return cachedRows.Item1;
                }
            }

            List<DataRow> rows = new List<DataRow>();

            switch (ApiType)
            {
                case ApiType.DeptComputer:
                case ApiType.Teach:
                    rows = ParseNoRss(page); break;
                case ApiType.GraduateSchool:
                    rows = ParseRss(page, ExtractTotalCount() ?? 0); break;
                case ApiType.GeneralNotice:
                case ApiType.HaksaNotice:
                case ApiType.JanghakNotice:
                case ApiType.MozipNotice:
                case ApiType.ChuiupNotice:
                case ApiType.GumeNotice:
                    rows = ParseNotice(page); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, UNSUPPORTED_API_TYPE_MESSAGE);
            }

            PageCache[(ApiType, page)] = (rows, DateTime.Now);

            return rows;
        }
        /// <summary>
        /// Fetches and parses the notices from the specified API type, returning a list of DataRow objects with a specified count.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<DataRow> GetRowsWithCount(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, GREATER_THAN_ZERO_MESSAGE);
            }

            List<DataRow> result = new List<DataRow>();
            int page = 1;

            while (result.Count < count)
            {
                var pageRows = GetRowsWithPage(page);
                if (pageRows == null || pageRows.Count == 0)
                {
                    break;
                }

                foreach (var row in pageRows)
                {
                    result.Add(row);
                    if (result.Count == count)
                        break;
                }

                page++;
            }

            return result;
        }
        /// <summary>
        /// Gets a specific row by its number from the notices, ensuring that the number is greater than zero.
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DataRow GetRow(int no)
        {
            if (no <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(no), no, GREATER_THAN_ZERO_MESSAGE);
            }

            List<DataRow> rows = GetRowsWithCount(no);

            return rows.Last();
        }

        /// <summary>
        /// Constructs the URI for fetching notices based on the API type and page number.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Uri GetUri(int page = 1)
        {
            switch (ApiType)
            {
                case ApiType.GeneralNotice:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=143&pageNo=" + page);
                case ApiType.HaksaNotice:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=144&pageNo=" + page);
                case ApiType.JanghakNotice:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=145&pageNo=" + page);
                case ApiType.MozipNotice:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=147&pageNo=" + page);
                case ApiType.ChuiupNotice:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=3445&pageNo=" + page);
                case ApiType.GumeNotice:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=148&pageNo=" + page);
                case ApiType.DeptComputer:
                    return new Uri(GetUriPrefix() + "/bbs/computer/265/artclList.do?page=" + page);
                case ApiType.GraduateSchool:
                    return new Uri(GetUriPrefix() + "/bbs/gs/654/rssList.do?page=" + page);
                case ApiType.Teach:
                    return new Uri(GetUriPrefix() + "/bbs/teach/433/artclList.do?page=" + page);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, UNSUPPORTED_API_TYPE_MESSAGE);
            }
        }
        /// <summary>
        /// Gets the base URI prefix for the specified API type, which is used to construct full URIs for fetching notices.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetUriPrefix()
        {
            switch (ApiType)
            {
                case ApiType.GeneralNotice:
                case ApiType.HaksaNotice:
                case ApiType.JanghakNotice:
                case ApiType.MozipNotice:
                case ApiType.ChuiupNotice:
                case ApiType.GumeNotice:
                    return "https://www.kmu.ac.kr";
                case ApiType.DeptComputer:
                    return "https://computer.kmu.ac.kr";
                case ApiType.GraduateSchool:
                    return "https://gs.kmu.ac.kr";
                case ApiType.Teach:
                    return "https://teach.kmu.ac.kr";
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, UNSUPPORTED_API_TYPE_MESSAGE);
            }
        }

        /// <summary>
        /// Parses the notices from the HTML content of the specified page, returning a list of DataRow objects.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private List<DataRow> ParseNotice(int page = 1)
        {
            string html = GetUri(page).Curl();
            List<DataRow> rows = new List<DataRow>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//table[contains(@class, 'board_st')]//tbody/tr");

            if (nodes == null) return rows;

            foreach (var tr in nodes)
            {
                try
                {
                    var tds = tr.SelectNodes("td");
                    if (tds == null || tds.Count < 4) continue;

                    string noText = tds[0].InnerText.Trim();
                    if (!int.TryParse(noText, out int no))
                    {
                        if (page == 1) no = -1;
                        else continue;
                    }

                    string? title = HtmlEntity.DeEntitize(tds[1].InnerText.Trim());
                    string link = HtmlEntity.DeEntitize(tds[1].SelectSingleNode(".//a")?.GetAttributeValue("href", "") ?? "") ?? "";
                    string? writer = HtmlEntity.DeEntitize(tds[2].InnerText.Trim());
                    string date = tds[3].InnerText.Trim();

                    if (title != null && title.EndsWith("\u00A0"))
                    {
                        title = title.Replace("\u00A0", " ").TrimEnd();
                    }

                    rows.Add(new DataRow(no, title ?? "", link, DateOnly.Parse(date), writer ?? ""));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Parsing Error: " + ex.Message);
                    continue;
                }
            }

            return rows;
        }
        /// <summary>
        /// Parses the notices from the HTML content of the specified page when RSS is not available, returning a list of DataRow objects.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private List<DataRow> ParseNoRss(int page = 1)
        {
            string html = GetUri(page).Curl();
            List<DataRow> rows = new List<DataRow>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//table[contains(@class, 'board-table')]//tbody/tr");

            if (nodes == null) return rows;

            foreach (var tr in nodes)
            {
                try
                {
                    var tds = tr.SelectNodes("td");
                    if (tds == null || tds.Count < 5) continue;

                    int no = int.Parse(tds[0].InnerText.Trim());
                    string? title = HtmlEntity.DeEntitize(tds[1].InnerText.Trim());
                    string link = GetUriPrefix() + tds[1].SelectSingleNode(".//a")?.GetAttributeValue("href", "") ?? "";
                    string? writer = HtmlEntity.DeEntitize(tds[2].InnerText.Trim());
                    string date = tds[3].InnerText.Trim();

                    rows.Add(new DataRow(no, title ?? "", link, DateOnly.Parse(date), writer ?? ""));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Parsing Error: " + ex.Message);
                    continue;
                }
            }

            return rows;
        }
        /// <summary>
        /// Parses the RSS feed for the Graduate School notices, returning a list of DataRow objects.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="totalArticleCount"></param>
        /// <returns></returns>
        private List<DataRow> ParseRss(int page, int totalArticleCount)
        {
            string url = GetUri(page).ToString();
            var rows = new List<DataRow>();

            var xml = new XmlDocument();
            xml.Load(url);

            var items = xml.SelectNodes("//item");

            if (items == null || items.Count == 0)
            {
                Debug.WriteLine($"No items found on page {page}.");
                return rows;
            }

            int startNo = totalArticleCount - ((page - 1) * 10);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    Debug.WriteLine($"Item at index {i} is null.");
                    continue;
                }

                XmlNode item = items[i]!;
                int no = startNo - i;

                string title = item["title"]?.InnerText.Trim() ?? "";
                string link = item["link"]?.InnerText.Trim() ?? "";
                string date = item["pubDate"]?.InnerText.Trim() ?? "";
                string writer = item["author"]?.InnerText.Trim() ?? "";

                if (!link.StartsWith("http"))
                {
                    link = GetUriPrefix() + link;
                }

                if (title.EndsWith("}"))
                {
                    title = title.Replace("}", "");
                }

                rows.Add(new DataRow(no, title, link, DateOnly.Parse(date.Split(' ')[0]), writer));
            }

            return rows;
        }

        /// <summary>
        /// Extracts the total count of notices from the specified API type, if applicable.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private int? ExtractTotalCount()
        {
            switch (ApiType)
            {
                case ApiType.GraduateSchool:
                    return ExtractTotalCountWithGraduateSchool();
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, UNSUPPORTED_API_TYPE_MESSAGE);
            }
        }
        /// <summary>
        /// Extracts the total count of notices specifically for the Graduate School from the HTML content.
        /// </summary>
        /// <returns></returns>
        private int? ExtractTotalCountWithGraduateSchool()
        {
            try
            {
                string url = GetUriPrefix() + "/bbs/gs/654/artclList.do";
                var client = new HttpClient();
                var html = client.GetStringAsync(url).Result;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var strongNode = doc.DocumentNode
                    .SelectSingleNode("//div[@class='srch_counts']/strong");

                if (strongNode != null && int.TryParse(strongNode.InnerText.Replace(",", ""), out int count))
                {
                    return count;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to extract total count: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Checks if the first item in the list of notices matches the first item in the cached page, ensuring synchronization.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool CheckSync(List<DataRow> items)
        {
            List<DataRow> rows = new List<DataRow>();

            switch (ApiType)
            {
                case ApiType.DeptComputer:
                case ApiType.Teach:
                    rows = ParseNoRss(1); break;
                case ApiType.GraduateSchool:
                    rows = ParseRss(1, ExtractTotalCount() ?? 0); break;
                case ApiType.GeneralNotice:
                case ApiType.HaksaNotice:
                case ApiType.JanghakNotice:
                case ApiType.MozipNotice:
                case ApiType.ChuiupNotice:
                case ApiType.GumeNotice:
                    rows = ParseNotice(1); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, UNSUPPORTED_API_TYPE_MESSAGE);
            }

            return rows.First().Equals(items.First()) && rows.Last().Equals(items.Last());
        }
    }
}