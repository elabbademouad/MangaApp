﻿using Application.Entities;
using DataScraping.Config;
using DataScraping.Helpers;
using DataScraping.Model;
using HtmlAgilityPack;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace DataScraping
{
    static class Program
    {
        private static Configuration config;
        private static MangaRepository mangaRepository;
        private static ChapterRepository chapterRepository;
        private static PageRepository pageRepository;
        private static TagRepository tagRepository;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome To Manga Scraping :");
            Console.WriteLine("Initialize");
            config = new Configuration(args[0]);
            Initialize();
            Console.WriteLine("Manga Targeted : " + config["MangaUrl"]);
            Console.WriteLine("Press any key to start scraping !");
            Console.ReadKey();
            var manga = ScrapingManga(config["MangaUrl"]);
            Console.WriteLine("Chapters extracted");
            Console.WriteLine("Manga extracted with success!");
            CreateOrUpdateDataBase(manga);
            Console.WriteLine("Scraping done!");
            Console.ReadKey();
        }

        static void Initialize()
        {
            mangaRepository = new MangaRepository(config);
            chapterRepository = new ChapterRepository(config);
            pageRepository = new PageRepository(config);
            tagRepository = new TagRepository(config);
        }
        static void CreateOrUpdateDataBase(MangaScrapModel manga)
        {
            var created = false;
            created = mangaRepository.Query(m => m.Name == manga.Title).Count != 0;
            if (!created)
            {
                CreateMangaDb(manga, config["MediaPath"]);
            }
            else
            {
                UpdateMangaDb(manga, config["MediaPath"]);
            }
        }
        static MangaScrapModel ScrapingManga(string url)
        {
            MangaScrapModel manga = new MangaScrapModel();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(url);
            // Manga Details
            Console.WriteLine("Load html document");
            var htmlExtract1 = htmlDocument.DocumentNode.SelectSingleNode("//img[@class='manga-cover']").Attributes;
            foreach (var item in htmlExtract1)
            {
                if (item.Name == "alt")
                    manga.Title = item.Value.Replace("manga", "").Trim();
                if (item.Name == "src")
                {
                    manga.CoverUrl = item.Value;
                }
            }
            var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='manga-details-extended']");
            var detailNode = htmlNode.SelectNodes("//h4");
            manga.DateEdition = detailNode[0].InnerHtml;
            manga.State = detailNode[1].InnerHtml;
            manga.Resume = detailNode[2].InnerHtml;
            var texts = htmlNode.SelectNodes("//ul").First().InnerText.Split('\n');
            manga.Tags = new List<string>();
            foreach (var item in texts)
            {
                if (!string.IsNullOrEmpty(item) && !manga.Tags.Contains(item.Trim()))
                    manga.Tags.Add(item.Trim());
            }
            Console.WriteLine("Manga details extracted with success");
            manga.Chapters = new List<ChapterScrapModel>();
            var htmlExtract = htmlDocument.DocumentNode.SelectNodes("//a[@class='chapter']");
            int chNb = htmlExtract.Count;
            foreach (var item in htmlExtract)
            {
                var message = "chapter" + (htmlExtract.Count - chNb + 1) + " extracting...";
                Console.Write(message);
                ChapterScrapModel chapter = new ChapterScrapModel
                {
                    Number = chNb
                };
                string urlch = "";
                var urlPart = item.Attributes["href"].Value.Split('/');
                for (int i = 0; i < urlPart.Length - 2; i++)
                {
                    urlch += urlPart[i] + '/';
                }
                urlch += "0/full";
                chapter.Url = urlch;
                chapter.Pages = GetPages(urlch);
                chapter.Title = item.InnerHtml;
                manga.Chapters.Add(chapter);
                chNb--;
                for (int j = 0; j < message.Length; j++)
                {
                    Console.Write("\b \b");
                }
            }
            manga.Chapters.Reverse();
            return manga;
        }
        static List<PageScrapModel> GetPages(string url)
        {
            List<PageScrapModel> pages = new List<PageScrapModel>();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(url);
            var pagesNode = htmlDocument.DocumentNode.SelectNodes("//img");
            int pageNb = 1;
            foreach (var item in pagesNode)
            {
                if (item.Attributes["alt"].Value != "hiddenmangaimage")
                {
                    var page = new PageScrapModel();
                    page.Number = pageNb;
                    page.Url = item.Attributes["src"].Value;
                    pages.Add(page);
                    pageNb++;
                }
            }
            return pages;
        }

        static void CreateMangaDb(MangaScrapModel manga, string mediaPath)
        {
            if (manga != null)
            {
                Console.WriteLine("Start downloading and saving manga ");
                var mangaGuid = Guid.NewGuid();
                Manga mangaDoc = new Manga();
                mangaDoc.Id = mangaGuid;
                mangaDoc.Name = manga.Title;
                mangaDoc.Resume = manga.Resume;
                mangaDoc.State = manga.State;
                mangaDoc.Date = manga.DateEdition;
                mangaDoc.CoverExteranlUrl = manga.CoverUrl;
                mangaDoc.CoverInternalUrl = ImageHelper.GetPagelocalPath(manga.CoverUrl, mediaPath, "Manga/" + mangaDoc.Name.Replace(" ", "_"), "cover." + manga.CoverUrl.Split(".").Last());
                mangaDoc.Tags = string.Join(",", manga.Tags.ToArray());
                mangaRepository.Create(mangaDoc);
                Console.WriteLine("manga details downloaded and saved succefully ");
                foreach (var tag in manga.Tags)
                {
                    if (tagRepository.Query(t => t.Label == tag.Trim()).Count == 0)
                    {
                        var tagGuid = Guid.NewGuid();
                        tagRepository.Create(new Tag() { Id = tagGuid, Label = tag.Trim() });
                    }
                }
                foreach (var chapter in manga.Chapters)
                {
                    var message = string.Format("Chapter {0} downloaded and saved succefully", chapter.Number);
                    var chapterGuid = Guid.NewGuid();
                    var chapterDoc = new Chapter()
                    {
                        Id = chapterGuid,
                        Title = chapter.Title,
                        Url = chapter.Url,
                        Number = chapter.Number,
                        MangaId = mangaGuid,
                    };
                    chapterRepository.Create(chapterDoc);
                    var process = Process.Start(config["BrowserPath"], chapter.Url);
                    foreach (var page in chapter.Pages)
                    {
                        string internalUrl = ImageHelper.GetPagelocalPath(page.Url, mediaPath, "Manga/" + mangaDoc.Name.Replace(" ", "_") + "/chapter" + chapter.Number, page.Number.ToString() + "." + page.Url.Split(".").Last());
                        for (int i = 0; i < 50; i++)
                        {
                            if (string.IsNullOrEmpty(internalUrl))
                            {
                                internalUrl = ImageHelper.GetPagelocalPath(page.Url, mediaPath, "Manga/" + mangaDoc.Name.Replace(" ", "_") + "/chapter" + chapter.Number, page.Number.ToString() + "." + page.Url.Split(".").Last());
                            }
                            else
                            {
                                i = 51;
                            }

                        }
                        var pageGuid = Guid.NewGuid();
                        var pageDoc = new Page()
                        {
                            Id = pageGuid,
                            Number = page.Number,
                            ExternalUrl = page.Url,
                            InternalUrl = "Manga/" + mangaDoc.Name.Replace(" ", "_") + "/chapter" + chapter.Number + "/" + page.Number.ToString() + "." + page.Url.Split(".").Last(),
                            Pending = string.IsNullOrEmpty(internalUrl),
                            ChapterId = chapterGuid
                        };
                        pageRepository.Create(pageDoc);

                    }
                    process.CloseMainWindow();
                    process.Close();
                    if (chapter != manga.Chapters.First())
                    {
                        for (int j = 0; j < message.Length; j++)
                        {
                            Console.Write("\b \b");
                        }
                    }
                    Console.Write(message);
                }
                Console.WriteLine("\nEnd downloading and saving manga ");

            }
        }
        static void UpdateMangaDb(MangaScrapModel manga, string rootPath)
        {
            if (manga != null)
            {
                Console.WriteLine("Start updating and saving manga ");
                var mangaDoc = mangaRepository.Query(m => m.Name == manga.Title).First();
                int diff = manga.Chapters.Count - (int)chapterRepository.Count(c => c.MangaId == mangaDoc.Id);
                if (diff > 0)
                {
                    var newChapters = manga.Chapters.OrderBy(c => c.Number).TakeLast(diff);
                    foreach (var chapter in newChapters)
                    {
                        var message = string.Format("Chapter {0} downloaded and saved succefully", chapter.Number);
                        var chapterGUID = Guid.NewGuid();
                        var chapterDoc = new Chapter()
                        {
                            Id = chapterGUID,
                            Title = chapter.Title,
                            Url = chapter.Url,
                            Number = chapter.Number,
                            MangaId = mangaDoc.Id
                        };
                        chapterRepository.Create(chapterDoc);
                        var process = Process.Start(config["BrowserPath"], chapter.Url);
                        foreach (var page in chapter.Pages)
                        {
                            string internalUrl = ImageHelper.GetPagelocalPath(page.Url, rootPath, "Manga/" + mangaDoc.Name.Replace(" ", "_") + "/chapter" + chapter.Number, page.Number.ToString() + "." + page.Url.Split(".").Last());
                            for (int i = 0; i < 50; i++)
                            {
                                if (string.IsNullOrEmpty(internalUrl))
                                {
                                    internalUrl = ImageHelper.GetPagelocalPath(page.Url, rootPath, "Manga/" + mangaDoc.Name.Replace(" ", "_") + "/chapter" + chapter.Number, page.Number.ToString() + "." + page.Url.Split(".").Last());
                                }
                                else
                                {
                                    i = 51;
                                }

                            }
                            var pageGuid = Guid.NewGuid();
                            var pageDoc = new Page()
                            {
                                Id = pageGuid,
                                Number = page.Number,
                                ExternalUrl = page.Url,
                                InternalUrl = "Manga/" + mangaDoc.Name.Replace(" ", "_") + "/chapter" + chapter.Number + "/" + page.Number.ToString() + "." + page.Url.Split(".").Last(),
                                Pending = string.IsNullOrEmpty(internalUrl),
                                ChapterId = chapterGUID
                            };
                            pageRepository.Create(pageDoc);

                        }
                        process.CloseMainWindow();
                        process.Close();
                        if (chapter != manga.Chapters.First())
                        {
                            for (int j = 0; j < message.Length; j++)
                            {
                                Console.Write("\b \b");
                            }
                        }
                        Console.Write(message);
                    }
                    Console.WriteLine("\nEnd Updating and saving manga ");
                }

            }
        }

        static void DeleteMangaCascade(string mangaTitle)
        {
            var mangaToDelet = mangaRepository.Query(m => m.Name == mangaTitle).First();
            mangaRepository.Delete(mangaToDelet);
            var chaptersTodelete = chapterRepository.Query(c => c.MangaId == mangaToDelet.Id);
            foreach (var chapterToDelete in chaptersTodelete)
            {
                chapterRepository.Delete(chapterToDelete.Id);
                var pagesToDelete = pageRepository.Query(p => p.ChapterId == chapterToDelete.Id);
                foreach (var pageTodelete in pagesToDelete)
                {
                    pageRepository.Delete(pageTodelete.Id);
                }
            }
        }
        static void CleanDataBase()
        {
            var chaptersIds = chapterRepository.GetAll().Select(c => c.Id);
            var pageTodelete = pageRepository.GetAll().Where(p => !chaptersIds.Contains(p.ChapterId));
            foreach (var item in pageTodelete)
            {
                pageRepository.Delete(item);
            }
        }

    }
}
