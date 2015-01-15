using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using korgeaux;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Mirko
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string appKey = "WykopApi app key here";
        string appSecret = "WykopApi secret key here";
        string accountKey = "Linked account key here";

        WykopApi a;
        OwnProfile OwnProfile;
        bool isLoading = true;
        int lastPage = 1;

        public MainWindow()
        {
            InitializeComponent();

            a = new WykopApi(appKey, appSecret);

            new Task(() => {
                var p = new RequestParams("user/login");
                p.PostParams.Add("accountkey", accountKey);
                OwnProfile = a.DoRequest<OwnProfile>(p);

                p = new RequestParams("stream/hot");
                p.ApiParams.Add("page", lastPage++.ToString());
                p.ApiParams.Add("userkey", OwnProfile.UserKey);
                var result = a.DoRequest<ObservableCollection<Entry>>(p);
                Dispatcher.Invoke(() => {
                    Mirko.ItemsSource = result;
                    isLoading = false;
                });
            }).Start();
        }

        private void Vote_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var data = (sender as FrameworkElement).DataContext;
            string resource = "entries/";

            if(data is Entry)
            {
                Entry entry = data as Entry;

                if (entry.UserVote == 0)
                    resource += "vote";
                else if (entry.UserVote == 1)
                    resource += "unvote";
                else
                    return;

                new Task(() =>
                {
                    var p = new RequestParams(resource);
                    p.MethodParams.Add("entry");
                    p.MethodParams.Add(entry.Id.ToString());
                    p.ApiParams.Add("userkey", OwnProfile.UserKey);
                    Vote result = a.DoRequest<Vote>(p);
                    Dispatcher.Invoke(() => {
                        entry.VoteCount = result.VoteCount;
                        entry.Voters = result.Voters;
                        entry.UserVote ^= 1;
                    });
                }).Start();
            }else if (data is EntryComment){
                EntryComment entry = data as EntryComment;

                if(entry.UserVote == 0)
                    resource += "vote";
                else if(entry.UserVote == 1)
                    resource += "unvote";
                else
                    return;

                new Task(() => {
                    var p = new RequestParams(resource);
                    p.MethodParams.Add("comment");
                    p.MethodParams.Add(entry.EntryId.ToString());
                    p.MethodParams.Add(entry.Id.ToString());
                    p.ApiParams.Add("userkey", OwnProfile.UserKey);
                    Vote result = a.DoRequest<Vote>(p);
                    Dispatcher.Invoke(() => {
                        entry.VoteCount = result.VoteCount;
                        entry.Voters = result.Voters;
                        entry.UserVote ^= 1;
                    });
                }).Start();
                
            }else{
                return;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var data = (sender as FrameworkElement).DataContext;
            if(e.LeftButton == MouseButtonState.Pressed)
            {   
                if(data is Entry)
                {
                    System.Diagnostics.Process.Start((data as Entry).Embed.Url.ToString());
                }else if (data is EntryComment){
                    System.Diagnostics.Process.Start((data as EntryComment).Embed.Url.ToString());
                }
            }
        }

        private void LoadComments_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var data = (sender as FrameworkElement).DataContext;
            Entry entry = data as Entry;

            new Task(() => {
                var p = new RequestParams("entries/index");
                p.MethodParams.Add(entry.Id.ToString());
                p.ApiParams.Add("userkey", OwnProfile.UserKey);
                Entry result = a.DoRequest<Entry>(p);
                Dispatcher.Invoke(() => {
                    entry.Comments = result.Comments;
                });
            }).Start();

            (sender as TextBlock).Text = "Ładowanie...";
            (sender as TextBlock).Cursor = null;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if((e.VerticalOffset + e.ViewportHeight) >= e.ExtentHeight * 0.95)
            {
                if(!isLoading && lastPage <= 10)
                {
                    isLoading = true;
                    new Task(() => {
                        var p = new RequestParams("stream/hot");
                        p.ApiParams.Add("page", lastPage++.ToString());
                        p.ApiParams.Add("userkey", OwnProfile.UserKey);
                        var result = a.DoRequest<ObservableCollection<Entry>>(p);
                        Dispatcher.Invoke(() => {
                            foreach(Entry entry in result)
                                (Mirko.ItemsSource as ObservableCollection<Entry>).Add(entry);
                            isLoading = false;
                        });
                    }).Start();
                }
            }
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}
