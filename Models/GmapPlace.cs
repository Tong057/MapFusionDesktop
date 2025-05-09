using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace MapFusion.Models
{
    public class GmapPlace : BasePlace
    {

        public GmapPlace(string url) : base(url) { }

        public override async Task ProcessAsync(IBrowser browser)
        {
            var page = await browser.NewPageAsync();

            await page.GotoAsync(URL);
            await BaseProcessor.ClickRejectCookiesIfRequired(page);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var raw = (await ScrollAsync(page)).ToString();

            string prefix = ")]}'";
            if (raw.StartsWith(prefix))
            {
                raw = raw.Substring(prefix.Length);
            }
            raw = raw.Trim();

            EntryFromJSON(raw);

            await page.CloseAsync();
        }


        public void EntryFromJSON(string raw)
        {
            try
            {
                JArray jd = JsonConvert.DeserializeObject<JArray>(raw);

                if (jd == null || jd.Count < 7 || jd[6].Type != JTokenType.Array)
                {
                    throw new Exception("Invalid JSON structure");
                }

                JArray darray = (JArray)jd[6];

                Title = GetNthElementAndCast<string>(darray, 11);
                if (Title == null) throw new Exception("Title is null");

                var categoriesToken = GetNthElementAndCast<JToken>(darray, 13);
                if (categoriesToken == null)
                {
                    throw new Exception("categoriesI is null");
                }
                if (categoriesToken.Type != JTokenType.Array)
                {
                    throw new Exception("categoriesI is not an array");
                }

                var categoriesI = categoriesToken.ToObject<List<object>>();
                Categories = categoriesI.Select(item => item as string).ToList();

                if (Categories.Count > 0)
                {
                    Category = Categories[0];
                }

                Address = GetNthElementAndCast<string>(darray, 18)?.TrimStart(',') ?? throw new Exception("Address is null");
                Cid = GetNthElementAndCast<string>(jd, 25, 3, 0, 13, 0, 0, 1) ?? throw new Exception("Cid is null");
                OpenHours = GetHours(darray);
                PopularTimes = GetPopularTimes(darray);
                Website = GetNthElementAndCast<string>(darray, 7, 0);
                PhoneNumber = GetNthElementAndCast<string>(darray, 178, 0, 0);
                PlusCode = GetNthElementAndCast<string>(darray, 183, 2, 2, 0);
                ReviewCount = (int)GetNthElementAndCast<double>(darray, 4, 8);
                ReviewRating = GetNthElementAndCast<double>(darray, 4, 7);
                Latitude = GetNthElementAndCast<double>(darray, 9, 2);
                Longitude = GetNthElementAndCast<double>(darray, 9, 3);
                Status = GetNthElementAndCast<string>(darray, 34, 4, 4);
                Description = GetNthElementAndCast<string>(darray, 32, 1, 1);
                ReviewsLink = GetNthElementAndCast<string>(darray, 4, 3, 0);
                ThumbnailImage = GetNthElementAndCast<string>(darray, 72, 0, 1, 6, 0);
                Timezone = GetNthElementAndCast<string>(darray, 30);
                PriceRange = GetNthElementAndCast<string>(darray, 4, 2);

                Owner = new Owner
                {
                    ID = GetNthElementAndCast<string>(darray, 57, 2),
                    Name = GetNthElementAndCast<string>(darray, 57, 1)
                };

                if (!string.IsNullOrEmpty(Owner.ID))
                {
                    Owner.Link = string.Format("https://www.google.com/maps/contrib/{0}", Owner.ID);
                }

                CompleteAddress = new Address
                {
                    Borough = GetNthElementAndCast<string>(darray, 183, 1, 0),
                    Street = GetNthElementAndCast<string>(darray, 183, 1, 1),
                    City = GetNthElementAndCast<string>(darray, 183, 1, 3),
                    PostalCode = GetNthElementAndCast<string>(darray, 183, 1, 4),
                    State = GetNthElementAndCast<string>(darray, 183, 1, 5),
                    Country = GetNthElementAndCast<string>(darray, 183, 1, 6)
                };

                var items = GetLinkSource(new GetLinkSourceParams
                {
                    Arr = GetNthElementAndCast<JArray>(darray, 171, 0),
                    Link = new List<int> { 3, 0, 6, 0 },
                    Source = new List<int> { 2 }
                });

                Images = items.Select(item => new Image
                {
                    Title = item.Source,
                    ImageLink = item.Link
                }).ToList();

                Reservations = GetLinkSource(new GetLinkSourceParams
                {
                    Arr = GetNthElementAndCast<JArray>(darray, 46),
                    Link = new List<int> { 0 },
                    Source = new List<int> { 1 }
                });

                Menu = new LinkSource
                {
                    Link = GetNthElementAndCast<string>(darray, 38, 0),
                    Source = GetNthElementAndCast<string>(darray, 38, 1)
                };

                // Todo fix about
                var aboutI = GetNthElementAndCast<JArray>(darray, 100, 1);
                if (aboutI != null)
                {
                    foreach (var aboutItem in aboutI)
                    {
                        var el = GetNthElementAndCast<JArray>(aboutItem as JArray, 0);
                        var about = new About
                        {
                            ID = GetNthElementAndCast<string>(el, 0),
                            Name = GetNthElementAndCast<string>(el, 1),
                            Options = new List<Option>()
                        };

                        var optsI = GetNthElementAndCast<JArray>(el, 2);
                        if (optsI == null)
                            break;

                        foreach (var optItem in optsI)
                        {
                            var opt = new Option
                            {
                                Enabled = GetNthElementAndCast<double>(optItem as JArray, 2, 1, 0, 0) == 1,
                                Name = GetNthElementAndCast<string>(optItem as JArray, 1)
                            };

                            if (!string.IsNullOrEmpty(opt.Name))
                            {
                                about.Options.Add(opt);
                            }
                        }

                        Abouts.Add(about);
                    }
                }

                ReviewsPerRating = new Dictionary<int, int>
        {
            { 1, (int)GetNthElementAndCast<double>(darray, 175, 3, 0) },
            { 2, (int)GetNthElementAndCast<double>(darray, 175, 3, 1) },
            { 3, (int)GetNthElementAndCast<double>(darray, 175, 3, 2) },
            { 4, (int)GetNthElementAndCast<double>(darray, 175, 3, 3) },
            { 5, (int)GetNthElementAndCast<double>(darray, 175, 3, 4) }
        };

                var reviewsI = GetNthElementAndCast<JArray>(darray, 175, 9, 0, 0);
                if (reviewsI != null)
                {
                    foreach (var reviewItem in reviewsI)
                    {
                        var el = GetNthElementAndCast<JArray>(reviewItem as JArray, 0);
                        var time = GetNthElementAndCast<JArray>(el, 2, 2, 0, 1, 21, 6, 7);

                        var review = new Review
                        {
                            Name = GetNthElementAndCast<string>(el, 1, 4, 0, 4),
                            ProfilePicture = GetNthElementAndCast<string>(el, 1, 4, 0, 3),
                            When = time == null || time.Count < 3 ? "" : string.Format("{0}-{1}-{2}", time[0], time[1], time[2]),
                            Rating = (int)GetNthElementAndCast<double>(el, 2, 0, 0),
                            Description = GetNthElementAndCast<string>(el, 2, 15, 0, 0),
                            Images = new List<string>()
                        };

                        if (!string.IsNullOrEmpty(review.Name))
                        {
                            var optsI = GetNthElementAndCast<JArray>(el, 2, 2, 0, 1, 21, 7);
                            if (optsI != null)
                            {
                                foreach (var optItem in optsI)
                                {
                                    var val = GetNthElementAndCast<string>(optItem as JArray);
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        review.Images.Add(val.Substring(2));
                                    }
                                }
                            }
                        }

                        UserReviews.Add(review);
                    }
                }

            }
            catch (Exception ex)
            {
                //throw new Exception("recovered from panic: " + ex.Message);
                Debug.WriteLine("recovered from panic: " + ex.Message);
            }
        }




        //public void EntryFromJSON(string raw)
        //{
        //    try
        //    {
        //        JArray jd = JsonConvert.DeserializeObject<JArray>(raw);

        //        if (jd == null || jd.Count < 7 || jd[6].Type != JTokenType.Array)
        //        {
        //            throw new Exception("Invalid JSON structure");
        //        }

        //        JArray darray = (JArray)jd[6];

        //        Title = GetNthElementAndCast<string>(darray, 11);

        //        var categoriesI = GetNthElementAndCast<List<object>>(darray, 13);

        //        Categories = categoriesI.Select(item => item as string).ToList();

        //        if (Categories.Count > 0)
        //        {
        //            Category = Categories[0];
        //        }

        //        Address = (GetNthElementAndCast<string>(darray, 18)).TrimStart(',');
        //        Cid = GetNthElementAndCast<string>(jd, 25, 3, 0, 13, 0, 0, 1);
        //        OpenHours = GetHours(darray);
        //        PopularTimes = GetPopularTimes(darray);
        //        Website = GetNthElementAndCast<string>(darray, 7, 0);
        //        PhoneNumber = GetNthElementAndCast<string>(darray, 178, 0, 0);
        //        PlusCode = GetNthElementAndCast<string>(darray, 183, 2, 2, 0);
        //        ReviewCount = (int)GetNthElementAndCast<double>(darray, 4, 8);
        //        ReviewRating = GetNthElementAndCast<double>(darray, 4, 7);
        //        Latitude = GetNthElementAndCast<double>(darray, 9, 2);
        //        Longitude = GetNthElementAndCast<double>(darray, 9, 3);
        //        Status = GetNthElementAndCast<string>(darray, 34, 4, 4);
        //        Description = GetNthElementAndCast<string>(darray, 32, 1, 1);
        //        ReviewsLink = GetNthElementAndCast<string>(darray, 4, 3, 0);
        //        ThumbnailImage = GetNthElementAndCast<string>(darray, 72, 0, 1, 6, 0);
        //        Timezone = GetNthElementAndCast<string>(darray, 30);
        //        PriceRange = GetNthElementAndCast<string>(darray, 4, 2);

        //        Owner = new Owner
        //        {
        //            ID = GetNthElementAndCast<string>(darray, 57, 2),
        //            Name = GetNthElementAndCast<string>(darray, 57, 1)
        //        };

        //        if (!string.IsNullOrEmpty(Owner.ID))
        //        {
        //            Owner.Link = string.Format("https://www.google.com/maps/contrib/{0}", Owner.ID);
        //        }

        //        CompleteAddress = new Address
        //        {
        //            Borough = GetNthElementAndCast<string>(darray, 183, 1, 0),
        //            Street = GetNthElementAndCast<string>(darray, 183, 1, 1),
        //            City = GetNthElementAndCast<string>(darray, 183, 1, 3),
        //            PostalCode = GetNthElementAndCast<string>(darray, 183, 1, 4),
        //            State = GetNthElementAndCast<string>(darray, 183, 1, 5),
        //            Country = GetNthElementAndCast<string>(darray, 183, 1, 6)
        //        };

        //        var items = GetLinkSource(new GetLinkSourceParams
        //        {
        //            Arr = GetNthElementAndCast<JArray>(darray, 171, 0),
        //            Link = new List<int> { 3, 0, 6, 0 },
        //            Source = new List<int> { 2 }
        //        });

        //        Images = items.Select(item => new Image
        //        {
        //            Title = item.Source,
        //            ImageLink = item.Link
        //        }).ToList();

        //        Reservations = GetLinkSource(new GetLinkSourceParams
        //        {
        //            Arr = GetNthElementAndCast<JArray>(darray, 46),
        //            Link = new List<int> { 0 },
        //            Source = new List<int> { 1 }
        //        });

        //        Menu = new LinkSource
        //        {
        //            Link = GetNthElementAndCast<string>(darray, 38, 0),
        //            Source = GetNthElementAndCast<string>(darray, 38, 1)
        //        };

        //        // Todo fix about
        //        var aboutI = GetNthElementAndCast<JArray>(darray, 100, 1);
        //        if (aboutI != null)
        //        {
        //            foreach (var aboutItem in aboutI)
        //            {
        //                var el = GetNthElementAndCast<JArray>(aboutItem as JArray, 0);
        //                var about = new About
        //                {
        //                    ID = GetNthElementAndCast<string>(el, 0),
        //                    Name = GetNthElementAndCast<string>(el, 1),
        //                    Options = new List<Option>()
        //                };

        //                var optsI = GetNthElementAndCast<JArray>(el, 2);
        //                if (optsI == null)
        //                    break;

        //                foreach (var optItem in optsI)
        //                {
        //                    var opt = new Option
        //                    {
        //                        Enabled = GetNthElementAndCast<double>(optItem as JArray, 2, 1, 0, 0) == 1,
        //                        Name = GetNthElementAndCast<string>(optItem as JArray, 1)
        //                    };

        //                    if (!string.IsNullOrEmpty(opt.Name))
        //                    {
        //                        about.Options.Add(opt);
        //                    }
        //                }

        //                Abouts.Add(about);
        //            }
        //        }

        //        ReviewsPerRating = new Dictionary<int, int>
        //        {
        //            { 1, (int)GetNthElementAndCast<double>(darray, 175, 3, 0) },
        //            { 2, (int)GetNthElementAndCast<double>(darray, 175, 3, 1) },
        //            { 3, (int)GetNthElementAndCast<double>(darray, 175, 3, 2) },
        //            { 4, (int)GetNthElementAndCast<double>(darray, 175, 3, 3) },
        //            { 5, (int)GetNthElementAndCast<double>(darray, 175, 3, 4) }
        //        };

        //        var reviewsI = GetNthElementAndCast<JArray>(darray, 175, 9, 0, 0);
        //        if (reviewsI != null)
        //        {
        //            foreach (var reviewItem in reviewsI)
        //            {
        //                var el = GetNthElementAndCast<JArray>(reviewItem as JArray, 0);
        //                var time = GetNthElementAndCast<JArray>(el, 2, 2, 0, 1, 21, 6, 7);

        //                var review = new Review
        //                {
        //                    Name = GetNthElementAndCast<string>(el, 1, 4, 0, 4),
        //                    ProfilePicture = GetNthElementAndCast<string>(el, 1, 4, 0, 3),
        //                    When = time == null || time.Count < 3 ? "" : string.Format("{0}-{1}-{2}", time[0], time[1], time[2]),
        //                    Rating = (int)GetNthElementAndCast<double>(el, 2, 0, 0),
        //                    Description = GetNthElementAndCast<string>(el, 2, 15, 0, 0),
        //                    Images = new List<string>()
        //                };

        //                if (!string.IsNullOrEmpty(review.Name))
        //                {
        //                    var optsI = GetNthElementAndCast<JArray>(el, 2, 2, 0, 1, 21, 7);
        //                    if (optsI != null)
        //                    {
        //                        foreach (var optItem in optsI)
        //                        {
        //                            var val = GetNthElementAndCast<string>(optItem as JArray);
        //                            if (!string.IsNullOrEmpty(val))
        //                            {
        //                                review.Images.Add(val.Substring(2));
        //                            }
        //                        }
        //                    }
        //                }

        //                UserReviews.Add(review);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("recovered from panic: " + ex.Message);
        //    }
        //}

        public static T GetNthElementAndCast<T>(JArray arr, params int[] indexes)
        {
            dynamic defaultVal = default(T);
            int idx = 0;

            if (arr == null)
            {
                // обработка случая, когда arr равен null
                return default(T); // или другое поведение в зависимости от контекста
            }

            if (indexes.Length == 0)
            {
                return defaultVal;
            }

            while (indexes.Length > 1)
            {
                idx = indexes[0];
                indexes = indexes.Skip(1).ToArray();

                if (idx >= arr.Count)
                {
                    return defaultVal;
                }

                var next = arr[idx];

                if (next == null || next.Type == JTokenType.Null)
                {
                    return defaultVal;
                }

                arr = next as JArray;
                if (arr == null)
                {
                    return defaultVal;
                }
            }

            if (indexes.Length == 0 || arr.Count == 0)
            {
                return defaultVal;
            }

            var finalElement = arr[indexes[0]];

            if (finalElement is JToken token && token.Type != JTokenType.Null)
            {
                try
                {
                    return token.ToObject<T>();
                }
                catch (Exception)
                {
                    return defaultVal;
                }
            }

            return defaultVal;
        }

        public static List<LinkSource> GetLinkSource(GetLinkSourceParams parameters)
        {
            if (parameters.Arr == null)
            {
                return new List<LinkSource>();  // или другое необходимое поведение при отсутствии данных
            }

            var result = new List<LinkSource>();

            foreach (var item in parameters.Arr)
            {
                var itemArray = item as JArray;
                if (itemArray == null) continue;

                var el = new LinkSource
                {
                    Source = GetNthElementAndCast<string>(itemArray, parameters.Source.ToArray()),
                    Link = GetNthElementAndCast<string>(itemArray, parameters.Link.ToArray())
                };

                if (!string.IsNullOrEmpty(el.Link) && !string.IsNullOrEmpty(el.Source))
                {
                    result.Add(el);
                }
            }

            return result;
        }



        public static Dictionary<string, List<string>> GetHours(JArray darray)
        {
            var items = GetNthElementAndCast<JArray>(darray, 34, 1);
            var hours = new Dictionary<string, List<string>>();

            if (items == null)
                return null;

            foreach (var item in items)
            {
                var day = GetNthElementAndCast<string>((JArray)item, 0);
                var timesI = GetNthElementAndCast<JArray>((JArray)item, 1);
                var times = timesI.Select(t => t.ToString()).ToList();

                hours.Add(day, times);
            }

            return hours;
        }

        public static Dictionary<string, Dictionary<int, int>> GetPopularTimes(JArray darray)
        {
            var items = GetNthElementAndCast<JArray>(darray, 84, 0);
            var popularTimes = new Dictionary<string, Dictionary<int, int>>();
            var dayOfWeek = new Dictionary<int, string>
            {
                { 1, "Monday" },
                { 2, "Tuesday" },
                { 3, "Wednesday" },
                { 4, "Thursday" },
                { 5, "Friday" },
                { 6, "Saturday" },
                { 7, "Sunday" }
            };

            if (items == null)
                return null;

            foreach (var item in items)
            {
                var day = GetNthElementAndCast<double>((JArray)item, 0);
                var timesI = GetNthElementAndCast<JArray>((JArray)item, 1);
                var times = new Dictionary<int, int>();

                foreach (var t in timesI)
                {
                    var time = t as JArray;
                    if (time == null)
                    {
                        return null;
                    }

                    var h = GetNthElementAndCast<double>(time, 0);
                    var v = GetNthElementAndCast<double>(time, 1);

                    times.Add((int)h, (int)v);
                }

                popularTimes.Add(dayOfWeek[(int)day], times);
            }

            return popularTimes;
        }

        public override async Task<JsonElement?> ScrollAsync(IPage page)
        {
            string script = @"
function parse() {
    const inputString = window.APP_INITIALIZATION_STATE[3][6]
    return inputString
}
";

            return await page.EvaluateAsync(script);
        }

    }
}
