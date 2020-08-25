using Hangfire.Dashboard.Pages;

namespace Hangfire.Dashboard.Management.v2.Pages
{
    internal class ManagementPage : RazorPage
    {
        public const string Title = "Management";
        public const string UrlRoute = "/management";

        public override void Execute()
        {
            Layout = new LayoutPage(Title);

            WriteLiteral($@"
    <link rel=""stylesheet"" type=""text/css"" href=""{Url.To($"{UrlRoute}/jsmcss")}"" />
    <div class=""row"">
        <div class=""col-md-3"">
");
            Write(Html.RenderPartial(new CustomSidebarMenu(ManagementSidebarMenu.Items)));

            WriteLiteral($@"
        </div>
        <div class=""col-md-9"">
            <h1 class=""page-header mgmt-title"">{Title}</h1>
            <div class=""visible-md-block visible-lg-block"">
                Select a page from the menu on the left to see the jobs available.
            </div>
            <div class=""hidden-md hidden-lg"">
                Select a page from the tabs at the top to see the jobs available.
            </div>
        </div>
    </div>
");
        }
    }
}