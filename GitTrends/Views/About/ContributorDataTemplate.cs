using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class ContributorDataTemplate : DataTemplate
    {
        const int _textPadding = 5;
        const int _circleDiameter = 62;

        public ContributorDataTemplate() : base(CreateContributorDataTemplate)
        {

        }

        enum ContributorRow { Avatar, Login }
        enum ContributorColumn { LeftText, Image, RightText, RightPadding }

        static Grid CreateContributorDataTemplate() => new()
        {
            RowSpacing = 4,

            RowDefinitions = Rows.Define(
                (ContributorRow.Avatar, _circleDiameter),
                (ContributorRow.Login, 25)),

            ColumnDefinitions = Columns.Define(
                (ContributorColumn.LeftText, _textPadding),
                (ContributorColumn.Image, _circleDiameter),
                (ContributorColumn.RightText, _textPadding),
                (ContributorColumn.RightPadding, 0.5)),

            Children =
                {
                    new AvatarImage(_circleDiameter)
                        .Row(ContributorRow.Avatar).Column(ContributorColumn.Image)
                        .Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime)
                        .DynamicResource(CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),

                    new Label { LineBreakMode = LineBreakMode.TailTruncation }.TextCenterHorizontal().TextTop().Font(FontFamilyConstants.RobotoRegular, 12)
                        .Row(ContributorRow.Login).Column(ContributorColumn.LeftText).ColumnSpan(3)
                        .Bind<Label, string, string>(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime, convert: login => $"@{login}")
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
                }
        };
    }
}
