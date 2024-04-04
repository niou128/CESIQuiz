using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz.Dialogs
{
    public class ResultPopup : Popup
    {
        public ResultPopup(string message)
        {
            Size = new(300, 200);
            Content = new StackLayout
            {
                Children =
            {
                new Label
                {
                    Text = message,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                },
                new Button
                {
                    Text = "OK",
                    HorizontalOptions = LayoutOptions.Center
                }
            }
            };

            // Attach the Clicked event to the button
            if (Content is StackLayout layout && layout.Children.LastOrDefault() is Button okButton)
            {
                okButton.Clicked += OnOkButtonClicked;
            }
        }

        private void OnOkButtonClicked(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
