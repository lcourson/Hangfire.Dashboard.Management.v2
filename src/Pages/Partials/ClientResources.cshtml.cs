#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hangfire.Dashboard.Management.v2.Pages.Partials
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    #line 3 "..\..\Pages\Partials\ClientResources.cshtml"
    using Hangfire.Dashboard;
    
    #line default
    #line hidden
    
    #line 4 "..\..\Pages\Partials\ClientResources.cshtml"
    using Hangfire.Dashboard.Management.v2;
    
    #line default
    #line hidden
    
    #line 5 "..\..\Pages\Partials\ClientResources.cshtml"
    using Newtonsoft.Json;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    internal partial class ClientResources : Hangfire.Dashboard.RazorPage
    {
#line hidden

        public override void Execute()
        {


WriteLiteral("\r\n\r\n");






            
            #line 7 "..\..\Pages\Partials\ClientResources.cshtml"
  
    var assetBaseUrl = Url.To($"{GlobalConfigurationExtension.GetAssetBaseURL()}");
    var version = $"v{GlobalConfigurationExtension.FileSuffix().Replace("_", ".")}";
    /*
     * All JS & CSS Urls that need to be loaded from the UI must be extensionless.
     */


            
            #line default
            #line hidden
WriteLiteral("\r\n<div id=\"hdmConfig\" data-version=\"");


            
            #line 15 "..\..\Pages\Partials\ClientResources.cshtml"
                             Write(version);

            
            #line default
            #line hidden
WriteLiteral("\" data-assetbaseurl=\"");


            
            #line 15 "..\..\Pages\Partials\ClientResources.cshtml"
                                                          Write(assetBaseUrl);

            
            #line default
            #line hidden
WriteLiteral("\"></div>\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"");


            
            #line 16 "..\..\Pages\Partials\ClientResources.cshtml"
                                        Write(assetBaseUrl);

            
            #line default
            #line hidden
WriteLiteral("/libs/FontAwesome/css/fontawesome_min_css\" />\r\n<link rel=\"stylesheet\" type=\"text/" +
"css\" href=\"");


            
            #line 17 "..\..\Pages\Partials\ClientResources.cshtml"
                                        Write(assetBaseUrl);

            
            #line default
            #line hidden
WriteLiteral("/libs/FontAwesome/css/solid_min_css\" />\r\n<link rel=\"stylesheet\" type=\"text/css\" h" +
"ref=\"");


            
            #line 18 "..\..\Pages\Partials\ClientResources.cshtml"
                                        Write(assetBaseUrl);

            
            #line default
            #line hidden
WriteLiteral("/libs/TempusDominus/css/tempus-dominus_min_css\" />\r\n<link rel=\"stylesheet\" type=\"" +
"text/css\" href=\"");


            
            #line 19 "..\..\Pages\Partials\ClientResources.cshtml"
                                        Write(assetBaseUrl);

            
            #line default
            #line hidden
WriteLiteral("/management_css\" />\r\n<script src=\"");


            
            #line 20 "..\..\Pages\Partials\ClientResources.cshtml"
        Write(assetBaseUrl);

            
            #line default
            #line hidden
WriteLiteral("/jsm-init_js\"></script>\r\n");


        }
    }
}
#pragma warning restore 1591