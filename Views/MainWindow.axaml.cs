using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace data_sets.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void InputFileButtonClick(object sender, RoutedEventArgs e)
        {
            // code here
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            Task.Run(async () =>
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Text File",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
                });

                if (files.Count >= 1)
                {
                    // Open reading stream from the first file.
                    string input_path = files[0].Path.AbsolutePath;
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        txt_input_path.Text = input_path;
                    });
                }
            });

        }

        public void OutputFileButtonClick(object sender, RoutedEventArgs e)
        {
            // code here.
            // code here
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            Task.Run(async () =>
            {
                var folderDialog = new OpenFolderDialog
                {
                    Title = "Select Folder",
                    Directory = "."
                };

                var result = await folderDialog.ShowAsync(this);
                Dispatcher.UIThread.Invoke(() =>
                {
                    txt_output_path.Text = result;
                });
            });
        }

        public void InputSourceButtonClick(object sender, RoutedEventArgs e)
        {
            // code here.
            // code here
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            Task.Run(async () =>
            {
                var folderDialog = new OpenFolderDialog
                {
                    Title = "Select Folder",
                    Directory = "."
                };

                var result = await folderDialog.ShowAsync(this);
                Dispatcher.UIThread.Invoke(() =>
                {
                    input_path.Text = result;
                });
            });
        }

        public void outputDestButtonClick(object sender, RoutedEventArgs e)
        {
            // code here.
            // code here
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            Task.Run(async () =>
            {
                var folderDialog = new OpenFolderDialog
                {
                    Title = "Select Folder",
                    Directory = "."
                };

                var result = await folderDialog.ShowAsync(this);
                Dispatcher.UIThread.Invoke(() =>
                {
                    output_path.Text = result;
                });
            });
        }

        public void StartCurlCmdClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_input_path.Text))
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                string input_path = txt_input_path.Text;
                string output_path = txt_output_path.Text;
                string cookie = txt_curl_cmd.Text.Trim();
                cookie = cookie.Replace("cookie","Cookie").Replace("curl","").TrimStart();
                string[] readText = File.ReadAllLines(input_path);
                pgr_curl.Minimum = 0;
                pgr_curl.Maximum = (double)readText.Length;
                Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(output_path)) return;

                    if (File.Exists(input_path) && string.IsNullOrEmpty(cookie) == false && cookie.Contains("Cookie") && cookie.Contains("https"))
                    {
                        double p = 1;
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        int startindex = cookie.IndexOf("Cookie:");
                        int toindex = cookie.LastIndexOf("-H");
                        string cookie_value = cookie.Substring(startindex + 7, (toindex - startindex) - 2);
                        int indexof = cookie_value.IndexOf("-H",90,StringComparison.CurrentCulture);
                        cookie_value = cookie_value.Substring(0, indexof - 1);
                        startindex = cookie.IndexOf("https");
                        string url = string.Empty;
                        if(cookie.StartsWith("\'"))
                        {
                            toindex = cookie.IndexOf("\'", 30);
                            url = cookie.Substring(startindex, toindex - startindex);
                        }
                        else
                        {
                            toindex = cookie.IndexOf('"', 30);
                            url = cookie.Substring(startindex, toindex - startindex);
                        }


                        foreach (string s in readText)
                        {
                            string symbol = s.Trim();

                            var handler = new HttpClientHandler();
                            handler.UseCookies = false;

                            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
                            handler.AutomaticDecompression = ~DecompressionMethods.None;

                            // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
                            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
                            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
                            using (var httpClient = new HttpClient(handler))
                            {
                                string[] urls = url.Split("stock/");  //url.Substring(0,url.IndexOf("stock/")+6);
                                if (urls.Length > 1)
                                {
                                    string url_with_symbol = urls[0] + "stock/" + symbol + urls[1].Substring(urls[1].IndexOf("/"));

                                    Debug.WriteLine(url_with_symbol);

                                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url_with_symbol))
                                    {
                                        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:127.0) Gecko/20100101 Firefox/127.0");
                                        request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                                        request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
                                        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br, zstd");
                                        request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                                        request.Headers.TryAddWithoutValidation("Referer", $"https://www.set.or.th/th/market/product/stock/quote/{symbol}/financial-statement/company-highlights");
                                        request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                                        request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                                        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                                        request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
                                        request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
                                        request.Headers.TryAddWithoutValidation("TE", "trailers");
                                        request.Headers.TryAddWithoutValidation("Cookie", cookie_value);

                                        var response = await httpClient.SendAsync(request);
                                        using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(output_path, $"{symbol}.json")))
                                        {
                                            string content = await response.Content.ReadAsStringAsync();
                                            await outputFile.WriteAsync(content);
                                        }
                                    }
                                }

                            }

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                pgr_curl.Value = p++;
                            });
                            await Task.Delay(10);
                        }
                    }
                });
            }
        }
        public void  StartExtractDataClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(input_path.Text) || string.IsNullOrEmpty(output_path.Text))
            {
                return;
            }
            else
            {
                pgr_extract.Minimum = 0;
                pgr_extract.Value = 0;
                string src_path = input_path.Text;
                string desc_path = output_path.Text;
                // pe data
                if (cmb_data_type.SelectedIndex == 0)
                {

                }
                // finan
                if (cmb_data_type.SelectedIndex == 1)
                {
                    Task.Run(async () =>
                    {
                         await ExtractFinanAsSql(src_path, desc_path);
                    });
                }
                // yeild
                if (cmb_data_type.SelectedIndex == 2)
                {
                    Task.Run(async () => await ExtractYeildAsSql(src_path, desc_path));
                }
                // last price
                if (cmb_data_type.SelectedIndex == 3)
                {
                    Task.Run(async () => await ExtractLastPriceAsSql(src_path, desc_path));
                }
            }
        }

        private async Task ExtractLastPriceAsSql(string source_folder, string destination_folder)
        {
            string[] filePaths = Directory.GetFiles(source_folder, "*.json",
                                         SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            double prg_inc = 1;
            Dispatcher.UIThread.Invoke(() =>
            {
                pgr_extract.Maximum = (double)filePaths.Length;
            });
            foreach (var filename in filePaths)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    pgr_extract.Value = prg_inc++;
                });
                using (StreamReader reader = new StreamReader(filename))
                {

                    try
                    {
                        string jsonString = reader.ReadToEnd();
                        if (jsonString.Contains("last"))
                        {
                            dynamic fundstocks = JsonConvert.DeserializeObject<dynamic>(jsonString);

                            if (fundstocks != null)
                            {
                                foreach (var stock in fundstocks.relatedProducts)
                                {
                                    const string quote = "\"";
                                    if (stock.last != null)
                                    {
                                        System.Console.WriteLine(stock.symbol);
                                        string insert_xn = @"insert into yeild_stock_lastprice(symbol,price)
                                                    values(" + quote + stock.symbol + quote + "," + stock.last + ")";
                                        sb.Append(insert_xn + ";");
                                    }
                                    sb.AppendLine();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("json wrong format " + jsonString);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine("error at " + filename);
                        throw;
                    }

                }

            }
            //System.Console.WriteLine(sb.ToString());
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(destination_folder, "inser_stock_yeild_lastprice_sql.sql")))
            {
                await outputFile.WriteAsync(sb.ToString());
            }
        }
        private async Task ExtractFinanAsSql(string source_folder, string destination_folder)
        {
            string[] filePaths = Directory.GetFiles(source_folder, "*.json",
                                        SearchOption.TopDirectoryOnly);
            double prg_inc = 1;
            Dispatcher.UIThread.Invoke(() =>
            {
                pgr_extract.Maximum = (double)filePaths.Length;
            });

            StringBuilder sb = new StringBuilder();
            foreach (var filename in filePaths)
            {
                // System.Console.WriteLine(filename);
                Dispatcher.UIThread.Invoke(() =>
                {
                    pgr_extract.Value = prg_inc++;
                });


                using (StreamReader reader = new StreamReader(filename))
                {

                    try
                    {
                        string jsonString = reader.ReadToEnd();
                        if (jsonString.Contains("[{"))
                        {
                            var financial_data = JsonConvert.DeserializeObject<List<dynamic>>(jsonString);

                            if (financial_data != null)
                            {
                                foreach (var f_data in financial_data)
                                {
                                    const string quote = "\"";
                                    if (f_data.symbol != "")
                                    {
                                        double totalAsset = 0, totalLiability = 0, totalRevenue = 0, equity = 0, netProfit = 0, eps = 0, netProfitMargin = 0, roa = 0, roe = 0;
                                        //System.Console.WriteLine(stock.symbol);
                                        double.TryParse(f_data.totalAsset.ToString(), out totalAsset);
                                        double.TryParse(f_data.totalLiability.ToString(), out totalLiability);
                                        double.TryParse(f_data.totalRevenue.ToString(), out totalRevenue);
                                        double.TryParse(f_data.equity.ToString(), out equity);
                                        double.TryParse(f_data.netProfit.ToString(), out netProfit);
                                        double.TryParse(f_data.eps.ToString(), out eps);
                                        double.TryParse(f_data.netProfitMargin.ToString(), out netProfitMargin);
                                        double.TryParse(f_data.roa.ToString(), out roa);
                                        double.TryParse(f_data.roe.ToString(), out roe);

                                        string insert_xn = @"insert into financial(symbol,year,totalAsset,totalLiability,equity,totalRevenue,netProfit,eps,netProfitMargin,roa,roe)
                                                    values(" + quote + f_data.symbol + quote + "," + quote + f_data.year + quote + "," + totalAsset + "," + totalLiability + "," + equity + "," + totalRevenue + "," + netProfit + "," + eps + "," + netProfitMargin + "," + roa + "," + roe + ")";
                                        sb.Append(insert_xn + ";");
                                    }
                                    sb.AppendLine();
                                }
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        System.Console.WriteLine("error at " + filename);
                        throw;
                    }

                }
                await Task.Delay(10);
            }
            //System.Console.WriteLine(sb.ToString());
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(destination_folder, "inser_financial_sql.sql")))
            {
                await outputFile.WriteAsync(sb.ToString());
            }
        }
        private async Task ExtractYeildAsSql(string source_folder, string destination_folder)
        {
            string[] filePaths = Directory.GetFiles(source_folder, "*.json",
                                         SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            double prg_inc = 1;

            Dispatcher.UIThread.Invoke(() =>
            {
                pgr_extract.Maximum = (double)filePaths.Length;
            });

            foreach (var filename in filePaths)
            {
                //System.Console.WriteLine(filename);
                Dispatcher.UIThread.Invoke(() =>
                {
                    pgr_extract.Value = prg_inc++;
                });
                using (StreamReader reader = new StreamReader(filename))
                {

                    try
                    {
                        string jsonString = reader.ReadToEnd();
                        if (jsonString.Contains("[{"))
                        {
                            var fundstocks = JsonConvert.DeserializeObject<List<dynamic>>(jsonString);

                            if (fundstocks != null)
                            {
                                foreach (var stock in fundstocks)
                                {
                                    const string quote = "\"";
                                    if (stock.type == "XN")
                                    {
                                        //System.Console.WriteLine(stock.symbol);
                                        string insert_xn = @"insert into yeild_stock(symbol,type,bookCloseDate,paymentDate,returnAmount,xdate)
                                                    values(" + quote + stock.symbol + quote + "," + quote + stock.type + quote + "," + quote + stock.bookCloseDate + quote + "," + quote + stock.paymentDate + quote + "," + stock.returnAmount + "," + quote + stock.xdate + quote + ")";
                                        sb.Append(insert_xn + ";");
                                    }
                                    if (stock.type == "XD")
                                    {
                                        string insert_xd = @$"insert into yeild_stock(symbol,type,bookCloseDate,paymentDate,dividend,xdate)
                                                values(" + quote + stock.symbol + quote + "," + quote + stock.type + quote + "," + quote + stock.bookCloseDate + quote + "," + quote + stock.paymentDate + quote + "," + stock.dividend + "," + quote + stock.xdate + quote + ")";
                                        sb.Append(insert_xd + ";");
                                    }
                                    sb.AppendLine();
                                }
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        System.Console.WriteLine("error at " + filename);
                        //throw;
                    }

                }
                await Task.Delay(10);
            }
            //System.Console.WriteLine(sb.ToString());
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(destination_folder, "inser_stock_yeild_sql.sql")))
            {
                await outputFile.WriteAsync(sb.ToString());
            }
        }

        public void ComboboxChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine((e.Source as ComboBox).SelectedIndex);
        }
    }
}