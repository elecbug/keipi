using HtmlAgilityPack;
using System.Diagnostics;
using System.Xml;

namespace KeiPI
{
    public partial class KeiPIWrapper
    {
        public KeiPIType KeiPIType { get; set; }

        public KeiPIWrapper(KeiPIType type)
        {
            KeiPIType = type;
        }

        public string Name
        {
            get
            {
                switch (KeiPIType)
                {
                    case KeiPIType.NoticeGeneral:
                        return "전체 공지사항 - 일반";
                    case KeiPIType.DeptComputer:
                        return "컴퓨터공학과 공지사항";
                    case KeiPIType.GraduateSchool:
                        return "대학원 공지사항";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(KeiPIType), KeiPIType, "Unsupported KeiPI type");
                }
            }
        }

        public List<KeiPIRow> ToRows(int page = 1)
        {
            switch (KeiPIType)
            {
                case KeiPIType.DeptComputer:
                    return ParseDeptComputer(page);
                case KeiPIType.GraduateSchool:
                    return ParseRss(page, ExtractTotalCount()??0);
                default:
                    throw new NotSupportedException($"KeiPI type {KeiPIType} is not supported for ToRows method.");
            }

        }

        private Uri GetUri(int page = 1)
        {
            switch (KeiPIType)
            {
                case KeiPIType.NoticeGeneral:
                    return new Uri(GetUriPrefix() + "/uni/main/page.jsp?mnu_uid=143&pageNo=" + page);
                case KeiPIType.DeptComputer:
                    return new Uri(GetUriPrefix() + "/bbs/computer/265/artclList.do?page=" + page);
                case KeiPIType.GraduateSchool:
                    return new Uri(GetUriPrefix() + "/bbs/gs/654/rssList.do?page=" + page);
                default:
                    throw new ArgumentOutOfRangeException(nameof(KeiPIType), KeiPIType, "Unsupported KeiPI type");
            }
        }
        private string GetUriPrefix()
        {
            switch (KeiPIType)
            {
                case KeiPIType.NoticeGeneral:
                    return "https://www.kmu.ac.kr";
                case KeiPIType.DeptComputer:
                    return "https://computer.kmu.ac.kr";
                case KeiPIType.GraduateSchool:
                    return "https://gs.kmu.ac.kr";
                default:
                    throw new ArgumentOutOfRangeException(nameof(KeiPIType), KeiPIType, "Unsupported KeiPI type");
            }
        }

        private List<KeiPIRow> ParseDeptComputer(int page = 1)
        {
            string html = GetUri(page).Curl();
            Debug.WriteLine(html);

            List<KeiPIRow> rows = new List<KeiPIRow>();

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
                    string link = "https://computer.kmu.ac.kr" + tds[1].SelectSingleNode(".//a")?.GetAttributeValue("href", "") ?? "";
                    string? writer = HtmlEntity.DeEntitize(tds[2].InnerText.Trim());
                    string date = tds[3].InnerText.Trim();
                    string content = "";

                    rows.Add(new KeiPIRow(no, title ?? "", link, date, content, writer ?? ""));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Parsing Error: " + ex.Message);
                    continue;
                }
            }

            return rows;
        }
        private List<KeiPIRow> ParseRss(int page, int totalArticleCount)
        {
            string url = GetUri(page).ToString();
            var rows = new List<KeiPIRow>();

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
                XmlNode item = items[i];
                int no = startNo - i;

                string title = item["title"]?.InnerText.Trim() ?? "";
                string link = item["link"]?.InnerText.Trim() ?? "";
                string date = item["pubDate"]?.InnerText.Trim() ?? "";
                string writer = item["author"]?.InnerText.Trim() ?? "";
                string content = item["description"]?.InnerText.Trim() ?? "";

                if (!link.StartsWith("http"))
                {
                    link = GetUriPrefix() + link;
                }

                rows.Add(new KeiPIRow(no, title, link, date, content, writer));
            }

            return rows;
        }
        public int? ExtractTotalCount()
        {
            switch (KeiPIType) {
                case KeiPIType.GraduateSchool:
                    return ExtractTotalCountWithGraduateSchool();
                default:
                    Debug.WriteLine($"[ERROR] Unsupported KeiPI type for total count extraction: {KeiPIType}");
                    return null;
            }
            
        }
        private int? ExtractTotalCountWithGraduateSchool()
        {
            try
            {
                string url = "https://gs.kmu.ac.kr/bbs/gs/654/artclList.do";
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
                Debug.WriteLine($"[ERROR] Failed to extract total count: {ex.Message}");
            }

            return null;

        }
    }
}