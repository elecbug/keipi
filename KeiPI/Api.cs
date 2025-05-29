using HtmlAgilityPack;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace KeiPI
{
    public partial class Api
    {
        public ApiType ApiType { get; set; }

        public Api(ApiType type)
        {
            ApiType = type;
        }

        public string Name
        {
            get
            {
                switch (ApiType)
                {
                    case ApiType.GeneralNotice:
                        return "전체 공지사항 - 일반";
                    case ApiType.HaksaNotice:
                        return "전체 공지사항 - 학사";
                    case ApiType.JanghakNotice:
                        return "전체 공지사항 - 장학";
                    case ApiType.MozipNotice:
                        return "전체 공지사항 - 모집";
                    case ApiType.ChuiupNotice:
                        return "전체 공지사항 - 취업";
                    case ApiType.GumeNotice:
                        return "전체 공지사항 - 구매";

                    case ApiType.DeptComputer:
                        return "컴퓨터공학과 공지사항";
                    case ApiType.GraduateSchool:
                        return "대학원 공지사항";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, "Unsupported KeiPI type");
                }
            }
        }

        public List<DataRow> ToRows(int page = 1)
        {
            switch (ApiType)
            {
                case ApiType.DeptComputer:
                    return ParseDeptComputer(page);
                case ApiType.GraduateSchool:
                    return ParseRss(page, ExtractTotalCount() ?? 0);
                case ApiType.GeneralNotice:
                case ApiType.HaksaNotice:
                case ApiType.JanghakNotice:
                case ApiType.MozipNotice:
                case ApiType.ChuiupNotice:
                case ApiType.GumeNotice:
                    return ParseNotice(page);
                default:
                    throw new NotSupportedException($"KeiPI type {ApiType} is not supported for ToRows method.");
            }

        }

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, "Unsupported KeiPI type");
            }
        }
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(ApiType), ApiType, "Unsupported KeiPI type");
            }
        }

        private List<DataRow> ParseDeptComputer(int page = 1)
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
                        continue;

                    string? title = HtmlEntity.DeEntitize(tds[1].InnerText.Trim());
                    string link = HtmlEntity.DeEntitize(tds[1].SelectSingleNode(".//a")?.GetAttributeValue("href", "") ?? "") ?? "";
                    string? writer = HtmlEntity.DeEntitize(tds[2].InnerText.Trim());
                    string date = tds[3].InnerText.Trim();

                    if (title != null && title.EndsWith("\u00A0"))
                    {
                        title = title.Substring(0, title.Length - 1).TrimEnd();
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

        private int? ExtractTotalCount()
        {
            switch (ApiType)
            {
                case ApiType.GraduateSchool:
                    return ExtractTotalCountWithGraduateSchool();
                default:
                    Debug.WriteLine($"[ERROR] Unsupported KeiPI type for total count extraction: {ApiType}");
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